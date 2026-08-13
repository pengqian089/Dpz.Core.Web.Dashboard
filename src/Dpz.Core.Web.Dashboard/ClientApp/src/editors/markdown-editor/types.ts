import type { Crepe } from "@milkdown/crepe";
import type { SourceMarkdownEditor } from "./source-markdown-editor";

/** Blazor JS 互操作辅助对象，用于调用 .NET 实例方法。 */
export type DotNetHelper = {
    invokeMethodAsync<T>(methodName: string, ...args: unknown[]): Promise<T>;
};

/** 将浏览器 File/Blob 包装为 Blazor 可读取流引用的函数。 */
export type CreateJSStreamReference = (file: Blob) => unknown;

/** 视口类型：桌面端显示完整工具栏，移动端会关闭部分 Milkdown 功能。 */
export type MarkdownViewportMode = "desktop" | "mobile";

/** 编辑模式：可视化（所见即所得）/ 源码（原始 Markdown）。 */
export type MarkdownEditMode = "visual" | "source";

/** 图片处理模式：编辑器内联显示 / 独立画廊显示。 */
export type MarkdownImageMode = "inline" | "gallery";

/** 单个编辑器实例的运行时状态。 */
export type MarkdownEditorInstance = {
    crepe: Crepe;
    dotNetHelper: DotNetHelper;
    imageMode: MarkdownImageMode;
    mode: MarkdownEditMode;
    sourceEditor: SourceMarkdownEditor;
    sourceHost: HTMLElement;
    visualHost: HTMLElement;
};

/**
 * 工具栏按钮的中文提示映射。
 *
 * key 仅用于调试和 DOM dataset 标记；按钮绑定仍按 Milkdown 的实际渲染顺序进行。
 * visibleIn 控制该按钮只在特定视口中参与顺序匹配，未设置则始终显示。
 */
export type ToolbarHint = {
    key: string;
    title: string;
    visibleIn?: readonly MarkdownViewportMode[];
};
