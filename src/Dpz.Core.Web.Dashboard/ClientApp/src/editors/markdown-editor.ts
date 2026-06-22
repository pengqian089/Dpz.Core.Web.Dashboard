import { MarkdownEditorRegistry } from "./markdown-editor/markdown-editor-registry";
import type { DotNetHelper } from "./markdown-editor/types";

/** Blazor 全局 JS 互操作入口，由 Blazor 运行时注入。 */
declare const DotNet: {
    createJSStreamReference(file: Blob): unknown;
};

/**
 * Markdown 编辑器 JS interop 入口。
 *
 * Vite 仍以本文件作为 markdown-editor 公共入口；Blazor 侧通过导入此模块调用
 * 下方导出的函数。具体实现委托给 MarkdownEditorRegistry，避免入口文件继续膨胀。
 */
const registry = new MarkdownEditorRegistry((file) => DotNet.createJSStreamReference(file));

/** 创建或重建指定 elementId 对应的 Markdown 编辑器。 */
export async function createEditor(
    elementId: string,
    markdown: string,
    editOnly: boolean,
    dotNetHelper: DotNetHelper
): Promise<void> {
    await registry.createEditor(elementId, markdown, editOnly, dotNetHelper);
}

/** 获取当前编辑器 Markdown 内容。 */
export function getMarkdown(elementId: string): string {
    return registry.getMarkdown(elementId);
}

/** 从 .NET 侧同步 Markdown 内容到编辑器。 */
export function setMarkdown(elementId: string, markdown: string): void {
    registry.setMarkdown(elementId, markdown);
}

/** 向当前编辑模式插入一段 Markdown 文本。 */
export function insertValue(elementId: string, value: string): void {
    registry.insertValue(elementId, value);
}

/** 切换指定编辑器的只读状态。 */
export function setReadonly(elementId: string, readonly: boolean): void {
    registry.setReadonly(elementId, readonly);
}

/** 销毁指定编辑器实例及其关联资源。 */
export async function destroy(elementId: string): Promise<void> {
    await registry.destroy(elementId);
}
