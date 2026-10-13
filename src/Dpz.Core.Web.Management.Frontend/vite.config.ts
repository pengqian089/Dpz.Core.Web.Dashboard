import { resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { defineConfig } from "vite";
import { viteIndexSyncPlugin } from "./sync-index.mjs";

const clientAppRoot = fileURLToPath(new URL(".", import.meta.url));

export default defineConfig({
    // Make URLs inside emitted CSS relative to the CSS file. Without this, fonts are
    // requested from site root and Blazor returns 404 for assets/*.woff2 files.
    base: "./",

    // 应用项目的 index.html 需要跟着 manifest 里的 hash 走。
    // MSBuild 构建时通过 MSBUILD_INDEX_HTML / MSBUILD_VERSION 传入目标路径与版本号；
    // 未设置时插件不生效，因此单独跑 Vite 不会有副作用。
    plugins: [viteIndexSyncPlugin()],

    build: {
        // 产物直接输出到应用项目的 wwwroot/assets。
        //
        // 为什么不输出到本项目的 wwwroot 再靠 ProjectReference 合并：
        // 静态 web 资产是由 App 项目的 ResolveProjectStaticWebAssets 从 Content 通配符
        // 发现的，而通配符在项目求值时就已展开。生成于构建期的文件进不了那份清单，
        // 于是 `dotnet build` 能用（开发服务器运行时按清单解析）、但 `dotnet publish`
        // 会静默漏掉整个 assets 目录。输出到 App 自己的 wwwroot 则由它的通配符天然覆盖。
        outDir: "../Dpz.Core.Web.Management/wwwroot/assets",

        // 生产构建不应将旧的 dev/watch 输出与新文件混合。
        // build.ps1 在调用 Vite 之前也会删除此目录。
        emptyOutDir: true,

        // Source map 使浏览器诊断更加方便，同时不会改变公共资源 URL 的形态。
        sourcemap: true,

        // 将 CSS 作为独立的哈希文件提供给 index.html，而非通过 JavaScript 注入。
        cssCodeSplit: true,

        // manifest 是哈希化的 Vite 文件名与 Blazor 互操作导入之间的桥梁。
        // build.ps1 通过它更新 index.html；C# 通过它动态加载 MarkdownEditor、CodeEditor 等模块。
        manifest: "manifest.json",

        rollupOptions: {
            // Blazor 以 ES 模块方式导入这些文件并按名称调用其导出函数，
            // 因此 Rollup 必须保留公共入口导出的签名。
            preserveEntrySignatures: "exports-only",

            // 以下均为 Blazor 或 index.html 使用的公共入口点。
            input: {
                app: resolve(clientAppRoot, "src/app.ts"),
                "code-editor": resolve(clientAppRoot, "src/editors/code-editor.ts"),
                dashboard: resolve(clientAppRoot, "src/pages/dashboard.ts"),
                "markdown-editor": resolve(clientAppRoot, "src/editors/markdown-editor.ts"),
                "markdown-preview": resolve(clientAppRoot, "src/markdown-preview.ts"),
                "photoswipe-gallery": resolve(clientAppRoot, "src/photoswipe-gallery.ts"),
                "upload-interop": resolve(clientAppRoot, "src/interop/upload-interop.ts"),
                "video-player": resolve(clientAppRoot, "src/pages/video-player.ts"),
                "webawesome-dialog": resolve(clientAppRoot, "src/interop/webawesome-dialog.ts")
            },

            output: {
                // 将哈希值放入文件名，因为部分 CDN 在缓存时会忽略查询字符串。
                entryFileNames: "[name].[hash].js",
                chunkFileNames: "chunks/[name].[hash].js",
                assetFileNames: "[name].[hash][extname]"
            }
        }
    }
});
