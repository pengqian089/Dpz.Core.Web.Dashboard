import { defaultKeymap, history, historyKeymap, indentWithTab } from "@codemirror/commands";
import { css } from "@codemirror/lang-css";
import { html } from "@codemirror/lang-html";
import { javascript } from "@codemirror/lang-javascript";
import { json } from "@codemirror/lang-json";
import { markdown } from "@codemirror/lang-markdown";
import { sql } from "@codemirror/lang-sql";
import { xml } from "@codemirror/lang-xml";
import { StreamLanguage, defaultHighlightStyle, syntaxHighlighting } from "@codemirror/language";
import { csharp } from "@codemirror/legacy-modes/mode/clike";
import { EditorSelection, EditorState, Extension } from "@codemirror/state";
import {
    EditorView,
    drawSelection,
    highlightActiveLine,
    highlightSpecialChars,
    keymap,
    lineNumbers
} from "@codemirror/view";

type CodeEditorOptions = {
    value?: string;
    language?: string;
    readOnly?: boolean;
};

const darkTheme = EditorView.theme(
    {
        "&": {
            backgroundColor: "var(--bg-body)",
            color: "var(--text-primary)",
            height: "100%"
        },
        ".cm-scroller": {
            fontFamily: "'JetBrains Mono', Consolas, monospace"
        },
        ".cm-content": {
            caretColor: "var(--primary)",
            minHeight: "100%"
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
            outline: "1px solid var(--primary)"
        }
    },
    { dark: true }
);

class CodeLanguageResolver {
    public resolve(language?: string): Extension {
        switch (this.normalize(language)) {
            case "html":
            case "htm":
                return html();
            case "css":
            case "scss":
            case "sass":
            case "less":
                return css();
            case "javascript":
            case "js":
            case "jsx":
                return javascript({ jsx: true });
            case "typescript":
            case "ts":
            case "tsx":
                return javascript({ jsx: true, typescript: true });
            case "json":
                return json();
            case "markdown":
            case "md":
                return markdown();
            case "xml":
            case "svg":
                return xml();
            case "sql":
                return sql();
            case "csharp":
            case "cs":
            case "c#":
                return StreamLanguage.define(csharp);
            default:
                return [];
        }
    }

    private normalize(language?: string): string {
        return (language ?? "plaintext").trim().toLowerCase();
    }
}

class CodeEditorRegistry {
    private readonly editors = new Map<string, EditorView>();
    private readonly languageResolver = new CodeLanguageResolver();

    public createEditor(elementId: string, options: CodeEditorOptions = {}): void {
        const parent = document.getElementById(elementId);
        if (!parent) {
            return;
        }

        this.destroy(elementId);
        parent.textContent = "";

        const state = EditorState.create({
            doc: options.value ?? "",
            extensions: this.createExtensions(options)
        });

        const view = new EditorView({ state, parent });
        this.editors.set(elementId, view);
    }

    public updateEditor(elementId: string, options: CodeEditorOptions = {}): void {
        const view = this.editors.get(elementId);
        if (!view) {
            this.createEditor(elementId, options);
            return;
        }

        const currentValue = view.state.doc.toString();
        if ((options.value ?? "") !== currentValue) {
            view.dispatch({
                changes: { from: 0, to: view.state.doc.length, insert: options.value ?? "" }
            });
        }
    }

    public getValue(elementId: string): string {
        return this.editors.get(elementId)?.state.doc.toString() ?? "";
    }

    public insertValue(elementId: string, value: string): void {
        const view = this.editors.get(elementId);
        if (!view) {
            return;
        }

        const changes = view.state.changeByRange((range) => ({
            changes: { from: range.from, to: range.to, insert: value },
            range: EditorSelection.cursor(range.from + value.length)
        }));

        view.dispatch(changes);
        view.focus();
    }

    public destroy(elementId: string): void {
        const view = this.editors.get(elementId);
        if (!view) {
            return;
        }

        view.destroy();
        this.editors.delete(elementId);
    }

    private createExtensions(options: CodeEditorOptions): Extension[] {
        return [
            lineNumbers(),
            highlightSpecialChars(),
            history(),
            drawSelection(),
            highlightActiveLine(),
            syntaxHighlighting(defaultHighlightStyle, { fallback: true }),
            this.languageResolver.resolve(options.language),
            EditorState.readOnly.of(Boolean(options.readOnly)),
            EditorView.editable.of(!options.readOnly),
            keymap.of([indentWithTab, ...defaultKeymap, ...historyKeymap]),
            darkTheme
        ];
    }
}

const registry = new CodeEditorRegistry();

export function createEditor(elementId: string, options: CodeEditorOptions = {}): void {
    registry.createEditor(elementId, options);
}

export function updateEditor(elementId: string, options: CodeEditorOptions = {}): void {
    registry.updateEditor(elementId, options);
}

export function getValue(elementId: string): string {
    return registry.getValue(elementId);
}

export function insertValue(elementId: string, value: string): void {
    registry.insertValue(elementId, value);
}

export function destroy(elementId: string): void {
    registry.destroy(elementId);
}
