// sync-index.mjs 是纯 ESM 的 Node 模块：Vite 配置与构建脚本共用它，
// 因此不能写成 .ts（Node 需要额外的类型剥离能力才能直接执行）。
// 这里声明它对外暴露的接口，让 vite.config.ts 得到真实类型而非 any。

export interface AppEntry {
    script: string;
    style: string;
}

export interface SyncIndexHtmlOptions {
    indexHtmlPath: string;
    manifestPath: string;
    version?: string;
}

export function readAppEntry(manifestPath: string): AppEntry;

export function syncIndexHtml(options: SyncIndexHtmlOptions): boolean;

export function viteIndexSyncPlugin(): import("vite").Plugin;
