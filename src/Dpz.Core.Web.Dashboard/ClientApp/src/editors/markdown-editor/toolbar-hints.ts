import type { ToolbarHint } from "./types";

/**
 * 选择区浮动工具栏的中文提示。
 *
 * 该工具栏只在选中文本后出现，仅包含行内样式操作。顺序必须与 Milkdown
 * Toolbar 实际渲染的按钮顺序一致。
 */
export const selectionToolbarHints: readonly ToolbarHint[] = [
    { key: "bold", title: "加粗" },
    { key: "italic", title: "斜体" },
    { key: "strikethrough", title: "删除线" },
    { key: "code", title: "行内代码" },
    { key: "link", title: "链接" },
    { key: "ai", title: "AI" }
];

/**
 * 顶部固定工具栏的中文提示。
 *
 * 包含行内样式和块级操作。表格功能在移动端创建 Crepe 时被关闭，因此
 * table 项只在 desktop 参与顺序匹配，避免后续按钮提示整体错位。
 */
export const topBarHints: readonly ToolbarHint[] = [
    { key: "bold", title: "加粗" },
    { key: "italic", title: "斜体" },
    { key: "strikethrough", title: "删除线" },
    { key: "code", title: "行内代码" },
    { key: "bullet-list", title: "无序列表" },
    { key: "ordered-list", title: "有序列表" },
    { key: "task-list", title: "任务列表" },
    { key: "link", title: "链接" },
    { key: "image", title: "图片" },
    { key: "table", title: "表格", visibleIn: ["desktop"] },
    { key: "code-block", title: "代码块" },
    { key: "quote", title: "引用" },
    { key: "hr", title: "分割线" }
];
