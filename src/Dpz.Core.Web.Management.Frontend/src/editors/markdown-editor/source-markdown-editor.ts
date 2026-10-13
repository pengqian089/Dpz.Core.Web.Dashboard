import { defaultKeymap, history, historyKeymap, indentWithTab } from "@codemirror/commands";
import { markdown as markdownLanguage } from "@codemirror/lang-markdown";
import { Compartment, EditorSelection, EditorState } from "@codemirror/state";
import {
    drawSelection,
    EditorView,
    highlightActiveLine,
    highlightSpecialChars,
    keymap,
    lineNumbers
} from "@codemirror/view";
import { sourceEditorTheme, sourceMarkdownHighlightExtensions } from "./themes";

/**
 * 源码 Markdown 编辑器，基于 CodeMirror 6。
 *
 * 它只负责“源码”模式下的文本编辑能力，不知道 Crepe、上传或模式切换。
 * MarkdownEditorRegistry 会在切换模式时调用 setValue/getValue 同步内容。
 *
 * Compartment 是 CodeMirror 的动态配置容器。只读状态需要同时切换
 * EditorState.readOnly 和 EditorView.editable，否则 UI 与命令状态可能不一致。
 */
export class SourceMarkdownEditor {
    private readonly readOnlyCompartment = new Compartment();
    private readonly editableCompartment = new Compartment();
    private readonly view: EditorView;

    /** 创建 CodeMirror 编辑器并挂载到指定 DOM 节点。 */
    public constructor(parent: HTMLElement, value: string, readonly: boolean) {
        const state = EditorState.create({
            doc: value,
            extensions: [
                lineNumbers(),
                highlightSpecialChars(),
                history(),
                drawSelection(),
                highlightActiveLine(),
                ...sourceMarkdownHighlightExtensions,
                markdownLanguage(),
                EditorView.lineWrapping,
                this.readOnlyCompartment.of(EditorState.readOnly.of(readonly)),
                this.editableCompartment.of(EditorView.editable.of(!readonly)),
                keymap.of([indentWithTab, ...defaultKeymap, ...historyKeymap]),
                sourceEditorTheme
            ]
        });

        this.view = new EditorView({ state, parent });
    }

    /** 读取当前源码内容。 */
    public getValue(): string {
        return this.view.state.doc.toString();
    }

    /** 用新 Markdown 替换整个文档，内容相同时不派发无意义更新。 */
    public setValue(value: string): void {
        if (value === this.getValue()) {
            return;
        }

        this.view.dispatch({
            changes: { from: 0, to: this.view.state.doc.length, insert: value }
        });
    }

    /** 在当前选区插入文本，并把光标移动到插入内容之后。 */
    public insertValue(value: string): void {
        const changes = this.view.state.changeByRange((range) => ({
            changes: { from: range.from, to: range.to, insert: value },
            range: EditorSelection.cursor(range.from + value.length)
        }));

        this.view.dispatch(changes);
        this.view.focus();
    }

    /** 动态切换只读/可编辑状态，无需销毁并重建编辑器。 */
    public setReadonly(readonly: boolean): void {
        this.view.dispatch({
            effects: [
                this.readOnlyCompartment.reconfigure(EditorState.readOnly.of(readonly)),
                this.editableCompartment.reconfigure(EditorView.editable.of(!readonly))
            ]
        });
    }

    /** 源码面板从 hidden 切换为可见后，要求 CodeMirror 重新测量布局并聚焦。 */
    public reveal(): void {
        this.view.requestMeasure();
        this.view.focus();
    }

    /** 释放 CodeMirror 持有的 DOM、插件和事件监听。 */
    public destroy(): void {
        this.view.destroy();
    }
}
