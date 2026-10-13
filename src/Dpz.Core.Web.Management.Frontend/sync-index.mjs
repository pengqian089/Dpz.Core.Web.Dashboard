// 把 Vite manifest 里带 hash 的产物名写回应用项目的 index.html。
//
// 这里是唯一的同步实现：Vite 构建通过 viteIndexSyncPlugin 调用它，
// build.ps1 也通过 CLI 调用它。不要在其他地方重复这段正则逻辑。
//
// 设计要点：
// - 只用 Node 标准库，无第三方依赖（Vite 配置与构建脚本都能直接引用）。
// - 内容未变化时不写文件，避免刷新 mtime 造成 MSBuild 反复判定项目过期。

import { readFileSync, writeFileSync } from "node:fs";
import { resolve } from "node:path";

const LINK_PATTERN = /<link data-vite-entry="app" href="[^"]+" rel="stylesheet" \/>/;
const SCRIPT_PATTERN = /<script data-vite-entry="app" type="module" src="[^"]+"><\/script>/;
const VERSION_PATTERN = /(<div class="app-loading__version">)v[\d.]+(<\/div>)/;
const APP_ENTRY = "src/app.ts";

/**
 * 读取 Vite manifest 中应用的入口产物路径。
 * @param {string} manifestPath manifest.json 的绝对路径
 * @returns {{ script: string, style: string }}
 */
export function readAppEntry(manifestPath) {
    const manifest = JSON.parse(readFileSync(manifestPath, "utf8"));
    const entry = manifest[APP_ENTRY];
    if (!entry || !entry.file) {
        throw new Error(`Vite manifest has no entry '${APP_ENTRY}' (${manifestPath})`);
    }
    if (!Array.isArray(entry.css) || entry.css.length === 0) {
        throw new Error(
            `Vite manifest entry '${APP_ENTRY}' has no CSS asset; index.html needs a stylesheet link.`
        );
    }

    return {
        script: `assets/${entry.file}`,
        style: `assets/${entry.css[0]}`
    };
}

/**
 * 按 manifest 同步 index.html。返回是否真的写入了内容。
 * @param {{ indexHtmlPath: string, manifestPath: string, version: string }} options
 * @returns {boolean} 内容有变化并已写回时为 true
 */
export function syncIndexHtml({ indexHtmlPath, manifestPath, version }) {
    if (!indexHtmlPath) {
        throw new Error("indexHtmlPath is required.");
    }

    const { script, style } = readAppEntry(manifestPath);
    const before = readFileSync(indexHtmlPath, "utf8");

    if (!LINK_PATTERN.test(before)) {
        throw new Error(
            `index.html is missing the '<link data-vite-entry="app" ...>' tag (${indexHtmlPath})`
        );
    }
    if (!SCRIPT_PATTERN.test(before)) {
        throw new Error(
            `index.html is missing the '<script data-vite-entry="app" ...>' tag (${indexHtmlPath})`
        );
    }

    let after = before
        .replace(LINK_PATTERN, `<link data-vite-entry="app" href="${style}" rel="stylesheet" />`)
        .replace(
            SCRIPT_PATTERN,
            `<script data-vite-entry="app" type="module" src="${script}"></script>`
        );

    if (version) {
        if (!VERSION_PATTERN.test(after)) {
            throw new Error(
                `index.html is missing the app-loading__version element (${indexHtmlPath})`
            );
        }
        after = after.replace(VERSION_PATTERN, `$1v${version}$2`);
    }

    if (after === before) {
        return false;
    }

    writeFileSync(indexHtmlPath, after, "utf8");
    return true;
}

/**
 * Vite 插件：在每次构建（含 watch 重建）后同步 index.html。
 * 仅当 MSBUILD_INDEX_HTML 环境变量存在时启用，因此纯前端使用 Vite 时不受影响。
 * @returns {import("vite").Plugin}
 */
export function viteIndexSyncPlugin() {
    // 从 Vite 解析后的配置取输出目录，避免这里再硬编码一份 outDir。
    let manifestPath = "";

    return {
        name: "dpz-index-html-sync",
        apply: "build",
        configResolved(config) {
            manifestPath = resolve(config.root, config.build.outDir, "manifest.json");
        },
        closeBundle() {
            const indexHtmlPath = process.env.MSBUILD_INDEX_HTML;
            if (!indexHtmlPath) {
                return;
            }

            const changed = syncIndexHtml({
                indexHtmlPath,
                manifestPath,
                version: process.env.MSBUILD_VERSION ?? ""
            });

            this.info(
                changed
                    ? `index.html synced to v${process.env.MSBUILD_VERSION ?? ""}`
                    : "index.html already in sync"
            );
        }
    };
}

// CLI 入口：node sync-index.mjs --index <path> --manifest <path> --version <v>
if (process.argv[1] && import.meta.url === new URL(`file://${process.argv[1]}`).href) {
    const args = new Map();
    for (let i = 2; i < process.argv.length; i += 2) {
        args.set(process.argv[i].replace(/^--/, ""), process.argv[i + 1]);
    }

    const changed = syncIndexHtml({
        indexHtmlPath: args.get("index"),
        manifestPath: args.get("manifest"),
        version: args.get("version") ?? ""
    });

    console.log(changed ? "index.html synced" : "index.html already in sync");
}
