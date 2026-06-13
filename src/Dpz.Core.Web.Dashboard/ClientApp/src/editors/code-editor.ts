import { defaultKeymap, history, historyKeymap, indentWithTab } from "@codemirror/commands";
import { css } from "@codemirror/lang-css";
import { html } from "@codemirror/lang-html";
import { javascript } from "@codemirror/lang-javascript";
import { json } from "@codemirror/lang-json";
import { markdown } from "@codemirror/lang-markdown";
import { sql } from "@codemirror/lang-sql";
import { xml } from "@codemirror/lang-xml";
import { HighlightStyle, StreamLanguage, syntaxHighlighting } from "@codemirror/language";
import { csharp } from "@codemirror/legacy-modes/mode/clike";
import { Compartment, EditorSelection, EditorState, Extension } from "@codemirror/state";
import {
    EditorView,
    drawSelection,
    highlightActiveLine,
    highlightSpecialChars,
    keymap,
    lineNumbers
} from "@codemirror/view";
import { tags } from "@lezer/highlight";
import "../styles/feature-code-editor.css";

type CodeEditorOptions = {
    value?: string;
    language?: string;
    readOnly?: boolean;
};

const darkTheme = EditorView.theme(
    {
        "&": {
            backgroundColor: "#07101f",
            color: "#dbeafe",
            height: "100%"
        },
        ".cm-scroller": {
            fontFamily: "'JetBrains Mono', Consolas, monospace",
            fontSize: "0.875rem",
            lineHeight: "1.65"
        },
        ".cm-content": {
            caretColor: "#93c5fd",
            minHeight: "100%",
            padding: "12px 0"
        },
        ".cm-line": {
            padding: "0 16px"
        },
        ".cm-gutters": {
            backgroundColor: "#0b1424",
            color: "#7891a8",
            borderRightColor: "#1d2d44"
        },
        ".cm-lineNumbers .cm-gutterElement": {
            minWidth: "3.1rem",
            padding: "0 12px 0 16px"
        },
        ".cm-activeLine": {
            backgroundColor: "rgba(59, 130, 246, 0.1)"
        },
        ".cm-activeLineGutter": {
            backgroundColor: "rgba(59, 130, 246, 0.14)",
            color: "#bfdbfe"
        },
        "&.cm-focused > .cm-scroller > .cm-selectionLayer .cm-selectionBackground, .cm-selectionBackground, .cm-content ::selection":
            {
                backgroundColor: "rgba(59, 130, 246, 0.34)"
            },
        ".cm-cursor, .cm-dropCursor": {
            borderLeftColor: "#bfdbfe"
        },
        ".cm-matchingBracket": {
            backgroundColor: "rgba(14, 165, 233, 0.2)",
            outline: "1px solid rgba(125, 211, 252, 0.32)"
        },
        ".cm-nonmatchingBracket": {
            backgroundColor: "rgba(239, 68, 68, 0.2)",
            outline: "1px solid rgba(248, 113, 113, 0.34)"
        },
        "&.cm-focused": {
            outline: "1px solid rgba(59, 130, 246, 0.6)"
        }
    },
    { dark: true }
);

const readableHighlightStyle = HighlightStyle.define([
    { tag: tags.keyword, color: "#93c5fd", fontWeight: "600" },
    { tag: [tags.atom, tags.bool, tags.null], color: "#fbbf24" },
    { tag: [tags.number, tags.integer, tags.float], color: "#fcd34d" },
    { tag: [tags.string, tags.special(tags.string)], color: "#86efac" },
    { tag: [tags.character, tags.escape], color: "#5eead4" },
    { tag: [tags.regexp, tags.url], color: "#67e8f9" },
    { tag: [tags.comment, tags.docComment], color: "#7891a8", fontStyle: "italic" },
    { tag: [tags.variableName, tags.name], color: "#dbeafe" },
    {
        tag: [tags.definition(tags.variableName), tags.function(tags.variableName)],
        color: "#bfdbfe"
    },
    { tag: [tags.propertyName, tags.attributeName], color: "#7dd3fc" },
    { tag: [tags.typeName, tags.className, tags.namespace], color: "#c4b5fd" },
    { tag: [tags.operator, tags.operatorKeyword, tags.punctuation], color: "#cbd5e1" },
    { tag: [tags.heading, tags.strong], color: "#f8fafc", fontWeight: "700" },
    { tag: tags.emphasis, color: "#dbeafe", fontStyle: "italic" },
    { tag: [tags.link, tags.labelName], color: "#60a5fa" },
    { tag: [tags.deleted, tags.invalid], color: "#fca5a5" },
    { tag: tags.inserted, color: "#86efac" }
]);

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
    private readonly languageCompartment = new Compartment();
    private readonly readOnlyCompartment = new Compartment();
    private readonly editableCompartment = new Compartment();

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

        view.dispatch({
            effects: [
                this.languageCompartment.reconfigure(
                    this.languageResolver.resolve(options.language)
                ),
                this.readOnlyCompartment.reconfigure(
                    EditorState.readOnly.of(Boolean(options.readOnly))
                ),
                this.editableCompartment.reconfigure(EditorView.editable.of(!options.readOnly))
            ]
        });
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
            syntaxHighlighting(readableHighlightStyle, { fallback: true }),
            this.languageCompartment.of(this.languageResolver.resolve(options.language)),
            this.readOnlyCompartment.of(EditorState.readOnly.of(Boolean(options.readOnly))),
            this.editableCompartment.of(EditorView.editable.of(!options.readOnly)),
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
