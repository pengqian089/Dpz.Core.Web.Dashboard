import { defaultHighlightStyle, HighlightStyle, syntaxHighlighting } from "@codemirror/language";
import { EditorView } from "@codemirror/view";
import { tags } from "@lezer/highlight";

/** 源码编辑器中链接的高亮样式，避免暗色背景下 URL 过暗。 */
const markdownSourceHighlightStyle = HighlightStyle.define([
    {
        tag: [tags.link, tags.url],
        color: "#7dd3fc",
        textDecoration: "underline",
        textUnderlineOffset: "2px"
    }
]);

/**
 * 源码编辑器（CodeMirror）的整体主题样式。
 *
 * 这里仅控制 CodeMirror 容器、行号、选区、光标等基础外观；Markdown token
 * 的语法高亮由 sourceMarkdownHighlightExtensions 提供。
 */
export const sourceEditorTheme = EditorView.theme(
    {
        "&": {
            backgroundColor: "var(--bg-body)",
            color: "var(--text-primary)",
            height: "100%"
        },
        ".cm-scroller": {
            fontFamily: "'JetBrains Mono', Consolas, monospace",
            lineHeight: "1.65"
        },
        ".cm-content": {
            caretColor: "var(--primary)",
            minHeight: "100%",
            padding: "24px 0"
        },
        ".cm-line": {
            padding: "0 24px"
        },
        ".cm-gutters": {
            backgroundColor: "var(--bg-surface)",
            color: "var(--text-muted)",
            borderRightColor: "var(--border-color)"
        },
        ".cm-activeLine": {
            backgroundColor: "rgba(59, 130, 246, 0.12)"
        },
        ".cm-activeLineGutter": {
            backgroundColor: "rgba(59, 130, 246, 0.16)"
        },
        ".cm-selectionBackground, &.cm-focused .cm-selectionBackground": {
            backgroundColor: "rgba(59, 130, 246, 0.35)"
        },
        "&.cm-focused": {
            outline: "none"
        }
    },
    { dark: true }
);

/** 源码 Markdown 编辑器的语法高亮扩展。 */
export const sourceMarkdownHighlightExtensions = [
    syntaxHighlighting(defaultHighlightStyle, { fallback: true }),
    syntaxHighlighting(markdownSourceHighlightStyle)
];

/** 代码块内部编辑器主题（Crepe 的 CodeMirror oneDark 变体）。 */
export const codeBlockTheme = EditorView.theme(
    {
        "&": {
            backgroundColor: "var(--bg-surface)",
            color: "var(--text-primary)"
        },
        ".cm-scroller": {
            fontFamily: "'JetBrains Mono', Consolas, monospace"
        },
        ".cm-content": {
            caretColor: "var(--primary)"
        },
        ".cm-gutters": {
            backgroundColor: "var(--bg-body)",
            color: "var(--text-muted)",
            borderRightColor: "var(--border-color)"
        },
        ".cm-activeLine": {
            backgroundColor: "rgba(59, 130, 246, 0.12)"
        },
        ".cm-selectionBackground, &.cm-focused .cm-selectionBackground": {
            backgroundColor: "rgba(59, 130, 246, 0.32)"
        }
    },
    { dark: true }
);
