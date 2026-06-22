import { defaultKeymap, history, historyKeymap, indentWithTab } from "@codemirror/commands";
import { markdown as markdownLanguage } from "@codemirror/lang-markdown";
import { defaultHighlightStyle, HighlightStyle, syntaxHighlighting } from "@codemirror/language";
import { languages } from "@codemirror/language-data";
import { Compartment, EditorSelection, EditorState } from "@codemirror/state";
import { oneDark } from "@codemirror/theme-one-dark";
import {
    drawSelection,
    EditorView,
    highlightActiveLine,
    highlightSpecialChars,
    keymap,
    lineNumbers
} from "@codemirror/view";
import { tags } from "@lezer/highlight";
import { Crepe } from "@milkdown/crepe";
import { uploadConfig } from "@milkdown/kit/plugin/upload";
import { replaceAll } from "@milkdown/kit/utils";
import { TooltipController } from "../interactions/tooltip";

/** Blazor JS 互操作辅助对象，用于调用 .NET 实例方法 */
type DotNetHelper = {
    invokeMethodAsync<T>(methodName: string, ...args: unknown[]): Promise<T>;
};

/** Blazor 全局 JS 互操作入口（由 Blazor 运行时注入） */
declare const DotNet: {
    createJSStreamReference(file: Blob): unknown;
};

/** 单个编辑器实例的运行时状态 */
type MarkdownEditorInstance = {
    crepe: Crepe;
    dotNetHelper: DotNetHelper;
    mode: MarkdownEditMode;
    sourceEditor: SourceMarkdownEditor;
    sourceHost: HTMLElement;
    visualHost: HTMLElement;
};

/** 视口类型：桌面端显示完整工具栏，移动端隐藏部分功能 */
type MarkdownViewportMode = "desktop" | "mobile";

/** 编辑模式：可视化（所见即所得）/ 源码（原始 Markdown） */
type MarkdownEditMode = "visual" | "source";

/** Mermaid 库的默认导出类型（动态 import 使用） */
type MermaidApi = typeof import("mermaid").default;

/**
 * 工具栏按钮的中文提示映射。
 * key 对应 Milkdown 内部按钮的固定顺序位置，
 * visibleIn 控制该按钮在特定视口下是否可见（未设置则始终显示）。
 */
type ToolbarHint = {
    key: string;
    title: string;
    visibleIn?: readonly MarkdownViewportMode[];
};

/**
 * 选择区浮动工具栏的中文提示（选中文本后出现在光标附近的弹出菜单）。
 * 仅包含行内样式（粗/斜/删除线/代码/链接），不包含块级操作。
 * 按钮顺序必须与 Milkdown Toolbar 功能按钮的实际渲染顺序一一对应。
 */
const selectionToolbarHints: readonly ToolbarHint[] = [
    { key: "bold", title: "加粗" },
    { key: "italic", title: "斜体" },
    { key: "strikethrough", title: "删除线" },
    { key: "code", title: "行内代码" },
    { key: "link", title: "链接" },
    { key: "ai", title: "AI" }
];

/**
 * 顶部固定工具栏的中文提示（编辑器顶部始终可见的完整工具栏）。
 * 包含行内样式 + 块级操作（列表/表格/代码块/引用/分割线/图片）。
 * 表格按钮仅在桌面端显示（visibleIn: ["desktop"]），移动端隐藏。
 * 按钮顺序必须与 Milkdown TopBar 功能按钮的实际渲染顺序一一对应。
 */
const topBarHints: readonly ToolbarHint[] = [
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

/**
 * 匹配独立成行的 HTML <br> 标签。
 * Milkdown 在可视化模式下连续回车会产生 <br> 行，切回源码模式时需要清理。
 */
const htmlBreakLinePattern = /^[ \t]*<br\s*\/?>[ \t]*$/i;

/** 源码编辑器中链接的高亮样式（浅蓝色 + 下划线） */
const markdownSourceHighlightStyle = HighlightStyle.define([
    {
        tag: [tags.link, tags.url],
        color: "#7dd3fc",
        textDecoration: "underline",
        textUnderlineOffset: "2px"
    }
]);

/** 源码编辑器（CodeMirror）的整体主题样式 */
const sourceEditorTheme = EditorView.theme(
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

/**
 * Mermaid 图表预览渲染器。
 * 在代码块预览中检测 mermaid/mmd 语言，动态加载 Mermaid 库并渲染为 SVG。
 * Mermaid 库通过动态 import() 按需加载，首次使用时才请求。
 */
class MermaidPreviewRenderer {
    private initialized = false;
    private mermaidModule: Promise<MermaidApi> | null = null;
    private nextDiagramId = 0;

    public render(
        language: string,
        content: string,
        applyPreview: (value: null | string | HTMLElement) => void
    ): void | null {
        if (!this.isMermaidLanguage(language)) {
            return null;
        }

        const diagram = content.trim();
        if (!diagram) {
            applyPreview(this.createMessage("请输入 Mermaid 图表内容"));
            return;
        }

        this.nextDiagramId += 1;
        const diagramId = `markdown-mermaid-${this.nextDiagramId}`;
        void this.renderDiagram(diagramId, diagram, applyPreview);
    }

    private async renderDiagram(
        diagramId: string,
        diagram: string,
        applyPreview: (value: null | string | HTMLElement) => void
    ): Promise<void> {
        try {
            const mermaid = await this.getMermaid();
            this.ensureInitialized(mermaid);
            const { svg } = await mermaid.render(diagramId, diagram);
            applyPreview(svg);
        } catch (error: unknown) {
            applyPreview(this.createMessage(this.getErrorMessage(error)));
        }
    }

    private getMermaid(): Promise<MermaidApi> {
        this.mermaidModule ??= import("mermaid").then((module) => module.default);
        return this.mermaidModule;
    }

    private ensureInitialized(mermaid: MermaidApi): void {
        if (this.initialized) {
            return;
        }

        mermaid.initialize({
            startOnLoad: false,
            securityLevel: "strict",
            theme: "dark",
            fontFamily: "JetBrains Mono, Microsoft YaHei UI, sans-serif",
            themeVariables: {
                background: "#0f172a",
                darkMode: true,
                fontFamily: "JetBrains Mono, Microsoft YaHei UI, sans-serif",
                primaryColor: "#1e293b",
                primaryTextColor: "#f8fafc",
                primaryBorderColor: "#60a5fa",
                lineColor: "#93c5fd",
                secondaryColor: "#0f766e",
                tertiaryColor: "#334155"
            }
        });
        this.initialized = true;
    }

    private isMermaidLanguage(language: string): boolean {
        const normalizedLanguage = language.trim().toLowerCase();
        return normalizedLanguage === "mermaid" || normalizedLanguage === "mmd";
    }

    private createMessage(message: string): HTMLElement {
        const element = document.createElement("div");
        element.className = "markdown-mermaid-message";
        element.textContent = message;
        return element;
    }

    private getErrorMessage(error: unknown): string {
        if (error instanceof Error) {
            return `Mermaid 渲染失败：${error.message}`;
        }

        return "Mermaid 渲染失败，请检查图表语法";
    }
}

/** 代码块内部的编辑器主题（CodeMirror oneDark 变体） */
const codeBlockTheme = EditorView.theme(
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

/**
 * 源码 Markdown 编辑器，基于 CodeMirror 6。
 * 用于"源码"编辑模式，提供语法高亮、行号、历史记录等基础编辑能力。
 *
 * Compartment（隔离舱）概念：CodeMirror 动态配置的容器。
 * readOnlyCompartment / editableCompartment 分别包装只读状态和可编辑状态，
 * 通过 reconfigure() 替换配置值而无需重建整个 EditorState。
 * 两个 Compartment 必须同时切换，否则编辑器会停留在旧状态。
 */
class SourceMarkdownEditor {
    private readonly readOnlyCompartment = new Compartment();
    private readonly editableCompartment = new Compartment();
    private readonly view: EditorView;

    public constructor(parent: HTMLElement, value: string, readonly: boolean) {
        const state = EditorState.create({
            doc: value,
            extensions: [
                lineNumbers(),
                highlightSpecialChars(),
                history(),
                drawSelection(),
                highlightActiveLine(),
                syntaxHighlighting(defaultHighlightStyle, { fallback: true }),
                syntaxHighlighting(markdownSourceHighlightStyle),
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

    public getValue(): string {
        return this.view.state.doc.toString();
    }

    public setValue(value: string): void {
        if (value === this.getValue()) {
            return;
        }

        this.view.dispatch({
            changes: { from: 0, to: this.view.state.doc.length, insert: value }
        });
    }

    public insertValue(value: string): void {
        const changes = this.view.state.changeByRange((range) => ({
            changes: { from: range.from, to: range.to, insert: value },
            range: EditorSelection.cursor(range.from + value.length)
        }));

        this.view.dispatch(changes);
        this.view.focus();
    }

    public setReadonly(readonly: boolean): void {
        this.view.dispatch({
            effects: [
                this.readOnlyCompartment.reconfigure(EditorState.readOnly.of(readonly)),
                this.editableCompartment.reconfigure(EditorView.editable.of(!readonly))
            ]
        });
    }

    public reveal(): void {
        this.view.requestMeasure();
        this.view.focus();
    }

    public destroy(): void {
        this.view.destroy();
    }
}

/**
 * 工具栏提示管理器。
 *
 * 由于 Milkdown 的工具栏按钮是动态渲染的（会在编辑器生命周期中增删），
 * 本管理器使用 MutationObserver 监听 DOM 变化，在新按钮出现时自动：
 * 1. 按 hints 数组的顺序为按钮绑定中文 tooltip
 * 2. 确保所有 <button> 都有 type="button"（防止触发表单提交）
 * 3. 为标题下拉选项绑定 tooltip
 *
 * hint 数组的顺序必须与 Milkdown 实际渲染的按钮 DOM 顺序严格一致，
 * 因为 applyHints 按 index 一一对应绑定。
 */
class ToolbarHintManager {
    private readonly observer: MutationObserver;
    private readonly tooltips = new TooltipController();

    public constructor(private readonly root: HTMLElement) {
        this.observer = new MutationObserver(() => this.apply());
    }

    public start(): void {
        this.apply();
        this.observer.observe(this.root, {
            childList: true,
            subtree: true
        });
    }

    public stop(): void {
        this.observer.disconnect();
        this.tooltips.destroy();
    }

    private apply(): void {
        this.applyButtonTypes();
        this.applyHints(".milkdown-toolbar .toolbar-item", selectionToolbarHints);
        this.applyHints(".milkdown-top-bar .top-bar-item", topBarHints);
        this.applyStaticTitle(".top-bar-heading-button", "选择标题级别");
        this.applyHeadingOptions();
    }

    private applyButtonTypes(): void {
        this.root.querySelectorAll<HTMLButtonElement>("button").forEach((button) => {
            if (button.hasAttribute("type")) {
                return;
            }

            button.type = "button";
        });
    }

    private applyHints(selector: string, hints: readonly ToolbarHint[]): void {
        const visibleHints = this.getVisibleHints(hints);

        this.root.querySelectorAll<HTMLElement>(selector).forEach((item, index) => {
            const hint = visibleHints[index];
            if (!hint) {
                return;
            }

            item.dataset.toolbarHint = hint.key;
            this.setTitle(item, hint.title);
        });
    }

    private getVisibleHints(hints: readonly ToolbarHint[]): readonly ToolbarHint[] {
        const mode = this.getViewportMode();
        return hints.filter((hint) => !hint.visibleIn || hint.visibleIn.includes(mode));
    }

    private getViewportMode(): MarkdownViewportMode {
        return this.root.classList.contains("markdown-editor-container--mobile")
            ? "mobile"
            : "desktop";
    }

    private applyStaticTitle(selector: string, title: string): void {
        this.root.querySelectorAll<HTMLElement>(selector).forEach((item) => {
            this.setTitle(item, title);
        });
    }

    private applyHeadingOptions(): void {
        this.root.querySelectorAll<HTMLElement>(".top-bar-heading-option").forEach((item) => {
            const label = item.textContent?.trim();
            if (!label) {
                return;
            }

            this.setTitle(item, label);
        });
    }

    private setTitle(item: HTMLElement, title: string): void {
        this.tooltips.register(item, title, {
            placement: "top",
            showDelay: 220,
            hideDelay: 80
        });
    }
}

/**
 * Markdown 编辑器总注册表（单例）。
 *
 * 核心职责：
 * - 管理多个编辑器实例（按 elementId 区分，同一页面可有多个编辑器）
 * - 创建 Crepe（Milkdown 可视化编辑器）和 SourceMarkdownEditor（源码编辑器）
 * - 切换可视化/源码编辑模式
 * - 上传图片到 .NET 后端（通过 Blazor JS 互操作）
 * - 管理工具栏提示和图片多选增强功能
 *
 * 关键设计：
 * - Crepe 负责可视化 WYSIWYG 编辑（基于 ProseMirror）
 * - SourceMarkdownEditor 负责原始 Markdown 编辑（基于 CodeMirror 6）
 * - 两种模式通过 modeBar 切换，切换时同步内容
 * - 图片上传路径：File -> DotNet.createJSStreamReference -> dotNetHelper.invokeMethodAsync("UploadImages")
 */
class MarkdownEditorRegistry {
    /** 所有编辑器实例的映射表（elementId -> instance） */
    private readonly editors = new Map<string, MarkdownEditorInstance>();
    /** 每个编辑器的工具栏提示管理器 */
    private readonly toolbarHints = new Map<string, ToolbarHintManager>();
    /** 每个编辑器的图片多选增强管理器 */
    private readonly imageInputManagers = new Map<string, MultiImageInputManager>();
    /** Mermaid 图表渲染器（全局共享，仅初始化一次） */
    private readonly mermaidPreview = new MermaidPreviewRenderer();

    /**
     * 创建或重建编辑器。
     * 如果 elementId 已有编辑器实例，会先销毁再重建。
     *
     * 布局结构：
     * root（编辑器容器）
     *   ├── modeBar（可视化/源码切换按钮）
     *   ├── visualHost（Crepe 可视化编辑器）
     *   └── sourceHost（CodeMirror 源码编辑器，初始隐藏）
     *
     * container（工具栏容器，独立于 root，由页面提供）
     *   用于包裹 Milkdown 的工具栏 DOM。
     */
    public async createEditor(
        elementId: string,
        markdown: string,
        editOnly: boolean,
        dotNetHelper: DotNetHelper
    ): Promise<void> {
        const root = document.getElementById(elementId);
        const container = document.getElementById(`markdown-container-${elementId}`);
        if (!root || !container) {
            return;
        }

        await this.destroy(elementId);
        root.textContent = "";
        root.classList.add("markdown-editor-workspace");
        this.configureToolbar(container);
        this.destroyToolbarHints(elementId);

        const modeBar = this.createModeBar(elementId);
        const visualHost = document.createElement("div");
        const sourceHost = document.createElement("div");
        visualHost.className = "markdown-editor-visual";
        sourceHost.className = "markdown-editor-source";
        sourceHost.hidden = true;
        root.append(modeBar, visualHost, sourceHost);

        const mobile = this.isMobileViewport();
        const crepe = new Crepe({
            root: visualHost,
            defaultValue: markdown ?? "",
            features: {
                [Crepe.Feature.BlockEdit]: !mobile,
                [Crepe.Feature.Latex]: false,
                [Crepe.Feature.Table]: !mobile,
                [Crepe.Feature.TopBar]: true
            },
            featureConfigs: {
                [Crepe.Feature.BlockEdit]: this.createBlockEditConfig(),
                [Crepe.Feature.CodeMirror]: {
                    languages,
                    theme: [oneDark, codeBlockTheme],
                    renderPreview: (
                        language: string,
                        content: string,
                        applyPreview: (value: null | string | HTMLElement) => void
                    ) => this.mermaidPreview.render(language, content, applyPreview),
                    searchPlaceholder: "搜索语言",
                    noResultText: "没有匹配的语言",
                    copyText: "复制",
                    previewLabel: "预览",
                    previewLoading: "预览加载中...",
                    previewToggleText: (previewOnlyMode: boolean) =>
                        previewOnlyMode ? "编辑" : "隐藏预览"
                },
                [Crepe.Feature.ImageBlock]: {
                    onUpload: async (file: File) => this.uploadImage(dotNetHelper, file)
                },
                [Crepe.Feature.Placeholder]: {
                    text: "开始写作..."
                },
                [Crepe.Feature.TopBar]: this.createTopBarConfig(),
                [Crepe.Feature.Toolbar]: this.createToolbarConfig()
            }
        });
        // 配置拖拽/粘贴图片的上传处理。
        // Milkdown 的 upload plugin 拦截图片拖放和粘贴操作，
        // 替换默认行为改为自定义上传到 .NET 后端。
        crepe.editor.config((ctx) => {
            ctx.update(uploadConfig.key, (prev) => ({
                ...prev,
                uploader: async (files, schema) => {
                    const nodeType = schema.nodes["image-block"] ?? schema.nodes.image;
                    if (!nodeType) {
                        return [];
                    }

                    const images = this.getImageFiles(files);
                    if (images.length === 0) {
                        return [];
                    }

                    const urls = await this.uploadImages(dotNetHelper, images);
                    return urls
                        .map((src) => nodeType.createAndFill({ src }))
                        .filter((node) => node !== null);
                }
            }));
        });

        await crepe.create();
        crepe.setReadonly(editOnly);
        const sourceEditor = new SourceMarkdownEditor(sourceHost, markdown ?? "", editOnly);
        this.editors.set(elementId, {
            crepe,
            dotNetHelper,
            mode: "visual",
            sourceEditor,
            sourceHost,
            visualHost
        });
        this.applyMode(elementId);
        this.configureToolbar(container);
        this.startToolbarHints(elementId, container);
        this.startImageInputManager(elementId, container);
    }

    public getMarkdown(elementId: string): string {
        const instance = this.editors.get(elementId);
        if (!instance) {
            return "";
        }

        if (instance.mode === "source") {
            return instance.sourceEditor.getValue();
        }

        return this.sanitizeVisualMarkdown(instance.crepe.getMarkdown());
    }

    public setMarkdown(elementId: string, markdown: string): void {
        const instance = this.editors.get(elementId);
        if (!instance) {
            return;
        }

        instance.crepe.editor.action(replaceAll(markdown ?? "", true));
        instance.sourceEditor.setValue(markdown ?? "");
    }

    public insertValue(elementId: string, value: string): void {
        const instance = this.editors.get(elementId);
        if (!instance) {
            return;
        }

        if (instance.mode === "source") {
            instance.sourceEditor.insertValue(value);
            return;
        }

        const nextMarkdown = `${instance.crepe.getMarkdown()}${value}`;
        instance.crepe.editor.action(replaceAll(nextMarkdown, true));
    }

    public setReadonly(elementId: string, readonly: boolean): void {
        const instance = this.editors.get(elementId);
        if (!instance) {
            return;
        }

        instance.crepe.setReadonly(readonly);
        instance.sourceEditor.setReadonly(readonly);
    }

    public async destroy(elementId: string): Promise<void> {
        const instance = this.editors.get(elementId);
        if (!instance) {
            return;
        }

        instance.sourceEditor.destroy();
        await instance.crepe.destroy();
        this.editors.delete(elementId);
        this.destroyToolbarHints(elementId);
        this.destroyImageInputManager(elementId);
    }

    private createModeBar(elementId: string): HTMLElement {
        const bar = document.createElement("div");
        bar.className = "markdown-editor-modebar";

        bar.append(
            this.createModeButton(elementId, "visual", "可视化"),
            this.createModeButton(elementId, "source", "源码")
        );

        return bar;
    }

    private createModeButton(
        elementId: string,
        mode: MarkdownEditMode,
        label: string
    ): HTMLButtonElement {
        const button = document.createElement("button");
        button.type = "button";
        button.className = "markdown-editor-modebar__button";
        button.dataset.mode = mode;
        button.textContent = label;
        button.addEventListener("click", () => this.setEditMode(elementId, mode));
        return button;
    }

    /**
     * 切换编辑模式（可视化 ↔ 源码）。
     * 切换前将当前模式的内容同步到目标模式的编辑器。
     * 从可视化切到源码时，会先清理 HTML <br> 行再写入。
     */
    private setEditMode(elementId: string, mode: MarkdownEditMode): void {
        const instance = this.editors.get(elementId);
        if (!instance || instance.mode === mode) {
            return;
        }

        if (mode === "source") {
            const visualMarkdown = this.sanitizeVisualMarkdown(instance.crepe.getMarkdown());
            instance.sourceEditor.setValue(visualMarkdown);
        } else {
            instance.crepe.editor.action(replaceAll(instance.sourceEditor.getValue(), true));
        }

        instance.mode = mode;
        this.applyMode(elementId);
    }

    private applyMode(elementId: string): void {
        const instance = this.editors.get(elementId);
        if (!instance) {
            return;
        }

        const sourceMode = instance.mode === "source";
        const root = instance.visualHost.parentElement;
        instance.visualHost.hidden = sourceMode;
        instance.sourceHost.hidden = !sourceMode;
        root?.classList.toggle("markdown-editor-workspace--source", sourceMode);
        root?.classList.toggle("markdown-editor-workspace--visual", !sourceMode);

        root?.querySelectorAll<HTMLButtonElement>(".markdown-editor-modebar__button").forEach(
            (button) => {
                const active = button.dataset.mode === instance.mode;
                button.classList.toggle("is-active", active);
                button.setAttribute("aria-pressed", active ? "true" : "false");
            }
        );

        if (sourceMode) {
            requestAnimationFrame(() => instance.sourceEditor.reveal());
        }
    }

    /**
     * 清理可视化编辑器产出的 Markdown。
     * Milkdown 在可视化模式下连续换行可能产生独立的 <br> 行，
     * 这些 <br> 行在源码模式或最终保存时没有意义，需要过滤掉。
     * 过滤后连续的空行也会压缩为最多两个换行。
     */
    private sanitizeVisualMarkdown(markdown: string): string {
        const normalizedMarkdown = markdown.replace(/\r\n?/g, "\n");
        if (!normalizedMarkdown.split("\n").some((line) => htmlBreakLinePattern.test(line))) {
            return markdown;
        }

        return normalizedMarkdown
            .split("\n")
            .filter((line) => !htmlBreakLinePattern.test(line))
            .join("\n")
            .replace(/\n{3,}/g, "\n\n");
    }

    /**
     * 配置工具栏容器的视口模式。
     * 移动端（≤700px）会隐藏部分功能（如 BlockEdit、Table），
     * 并通过 CSS 类名控制工具栏布局。
     */
    private configureToolbar(root: HTMLElement): void {
        const mode = this.getViewportMode();
        root.classList.toggle("markdown-editor-container--mobile", mode === "mobile");
        root.classList.toggle("markdown-editor-container--desktop", mode === "desktop");

        requestAnimationFrame(() => {
            root.querySelectorAll<HTMLButtonElement>("button:not([type])").forEach((button) => {
                button.type = "button";
            });
        });
    }

    private getViewportMode(): MarkdownViewportMode {
        return this.isMobileViewport() ? "mobile" : "desktop";
    }

    private isMobileViewport(): boolean {
        return window.matchMedia("(max-width: 700px)").matches;
    }

    private startToolbarHints(elementId: string, root: HTMLElement): void {
        const hints = new ToolbarHintManager(root);
        hints.start();
        this.toolbarHints.set(elementId, hints);
    }

    private destroyToolbarHints(elementId: string): void {
        this.toolbarHints.get(elementId)?.stop();
        this.toolbarHints.delete(elementId);
    }

    private startImageInputManager(elementId: string, root: HTMLElement): void {
        const manager = new MultiImageInputManager(root, async (input, files) => {
            const instance = this.editors.get(elementId);
            if (!instance) {
                return;
            }

            const urls = await this.uploadImages(instance.dotNetHelper, files);
            if (urls.length === 0) {
                return;
            }

            const firstUrlInserted = this.setCurrentImageUrl(input, urls[0]);
            const remainingUrls = firstUrlInserted ? urls.slice(1) : urls;
            if (remainingUrls.length > 0) {
                this.insertValue(elementId, this.createImageMarkdown(files, remainingUrls));
            }
        });

        manager.start();
        this.imageInputManagers.set(elementId, manager);
    }

    private destroyImageInputManager(elementId: string): void {
        this.imageInputManagers.get(elementId)?.stop();
        this.imageInputManagers.delete(elementId);
    }

    private createBlockEditConfig(): object {
        return {
            textGroup: {
                label: "文本",
                text: { label: "正文" },
                h1: { label: "一级标题" },
                h2: { label: "二级标题" },
                h3: { label: "三级标题" },
                h4: { label: "四级标题" },
                h5: { label: "五级标题" },
                h6: { label: "六级标题" },
                quote: { label: "引用" },
                divider: { label: "分割线" }
            },
            listGroup: {
                label: "列表",
                bulletList: { label: "无序列表" },
                orderedList: { label: "有序列表" },
                taskList: { label: "任务列表" }
            },
            advancedGroup: {
                label: "插入",
                image: { label: "图片" },
                codeBlock: { label: "代码块" },
                table: { label: "表格" },
                math: null
            }
        };
    }

    private createTopBarConfig(): object {
        return {
            headingOptions: [
                { label: "正文", level: null },
                { label: "标题 1", level: 1 },
                { label: "标题 2", level: 2 },
                { label: "标题 3", level: 3 }
            ],
            advancedGroup: {
                label: "插入",
                math: null
            }
        };
    }

    private createToolbarConfig(): object {
        return {
            advancedGroup: {
                label: "插入",
                math: null,
                ai: null
            }
        };
    }

    private async uploadImage(dotNetHelper: DotNetHelper, file: File): Promise<string> {
        const urls = await this.uploadImages(dotNetHelper, [file]);
        return urls[0] ?? "";
    }

    /**
     * 图片上传路径：File -> JSStreamReference -> .NET UploadImages 方法。
     *
     * DotNet.createJSStreamReference() 将 File 转换为 Blazor 可读的流引用，
     * 然后通过 dotNetHelper.invokeMethodAsync 调用 .NET 端的 UploadImages 方法，
     * 返回上传后的图片 URL 数组。
     */
    private async uploadImages(
        dotNetHelper: DotNetHelper,
        files: readonly File[]
    ): Promise<string[]> {
        const streamRefs = files.map((file) => DotNet.createJSStreamReference(file));
        const fileNames = files.map((file) => file.name);
        const contentTypes = files.map((file) => file.type || "application/octet-stream");
        return await dotNetHelper.invokeMethodAsync<string[]>(
            "UploadImages",
            streamRefs,
            fileNames,
            contentTypes
        );
    }

    private getImageFiles(files: FileList): File[] {
        return Array.from(files).filter((file) => file.type.includes("image"));
    }

    /**
     * 将第一个上传的图片 URL 注入到 Milkdown 的图片编辑弹窗中。
     *
     * 由于 Milkdown 的图片上传弹窗不会自动获取上传结果，
     * 这里通过查询 DOM 找到 .image-edit > .link-input-area 输入框，
     * 填入上传后的 URL 并触发 input + Enter 事件来模拟用户确认。
     * 返回 true 表示成功注入，false 表示未找到弹窗（如拖拽上传场景）。
     */
    private setCurrentImageUrl(input: HTMLInputElement, url: string): boolean {
        const imageEdit = input.closest(".image-edit");
        const linkInput = imageEdit?.querySelector<HTMLInputElement>(".link-input-area");
        if (!linkInput) {
            return false;
        }

        linkInput.value = url;
        linkInput.dispatchEvent(new InputEvent("input", { bubbles: true, data: url }));
        linkInput.dispatchEvent(new KeyboardEvent("keydown", { bubbles: true, key: "Enter" }));
        return true;
    }

    private createImageMarkdown(files: readonly File[], urls: readonly string[]): string {
        const offset = files.length - urls.length;
        const markdown = urls
            .map((url, index) => {
                const alt = this.getImageAlt(files[index + offset]);
                return `![${alt}](${url})`;
            })
            .join("\n\n");

        return `\n\n${markdown}`;
    }

    private getImageAlt(file: File | undefined): string {
        const name = file?.name.replace(/\.[^.]+$/, "").trim() || "image";
        return name.replace(/[[\]]/g, "");
    }
}

/**
 * 多图片上传增强管理器。
 *
 * Milkdown 默认的图片上传 input 不支持多选（multiple），
 * 本管理器拦截所有 image 类型的 file input 的 change 事件：
 * 1. 通过 MutationObserver 监听 DOM，自动将匹配的 input 设为 multiple
 * 2. 当用户选择 ≥2 张图片时，截获事件（preventDefault + stopImmediatePropagation）
 * 3. 第一张图片注入到当前弹窗的 URL 输入框，其余图片以 Markdown 语法追加到编辑器末尾
 *
 * 使用 capture 阶段（addEventListener(..., true)）确保在 Milkdown 之前拦截。
 */
class MultiImageInputManager {
    private readonly observer: MutationObserver;
    private readonly onChange = (event: Event) => {
        const input = event.target;
        if (!(input instanceof HTMLInputElement) || input.type !== "file") {
            return;
        }

        const files = input.files;
        if (!this.isImageInput(input) || !files || files.length < 2) {
            return;
        }

        event.preventDefault();
        event.stopPropagation();
        event.stopImmediatePropagation();

        const images = Array.from(files).filter((file) => file.type.includes("image"));
        if (images.length === 0) {
            input.value = "";
            return;
        }

        this.onUpload(input, images)
            .catch((error: unknown) => {
                console.error("An error occurred while uploading images");
                console.error(error);
            })
            .finally(() => {
                input.value = "";
            });
    };

    public constructor(
        private readonly root: HTMLElement,
        private readonly onUpload: (input: HTMLInputElement, files: File[]) => Promise<void>
    ) {
        this.observer = new MutationObserver(() => this.apply());
    }

    public start(): void {
        this.apply();
        this.root.addEventListener("change", this.onChange, true);
        this.observer.observe(this.root, {
            childList: true,
            subtree: true
        });
    }

    public stop(): void {
        this.root.removeEventListener("change", this.onChange, true);
        this.observer.disconnect();
    }

    private apply(): void {
        this.root.querySelectorAll<HTMLInputElement>('input[type="file"]').forEach((input) => {
            if (this.isImageInput(input)) {
                input.multiple = true;
            }
        });
    }

    private isImageInput(input: HTMLInputElement): boolean {
        return input.accept
            .split(",")
            .map((value) => value.trim().toLowerCase())
            .some((value) => value === "image/*" || value.startsWith("image/"));
    }
}

const registry = new MarkdownEditorRegistry();

export async function createEditor(
    elementId: string,
    markdown: string,
    editOnly: boolean,
    dotNetHelper: DotNetHelper
): Promise<void> {
    await registry.createEditor(elementId, markdown, editOnly, dotNetHelper);
}

export function getMarkdown(elementId: string): string {
    return registry.getMarkdown(elementId);
}

export function setMarkdown(elementId: string, markdown: string): void {
    registry.setMarkdown(elementId, markdown);
}

export function insertValue(elementId: string, value: string): void {
    registry.insertValue(elementId, value);
}

export function setReadonly(elementId: string, readonly: boolean): void {
    registry.setReadonly(elementId, readonly);
}

export async function destroy(elementId: string): Promise<void> {
    await registry.destroy(elementId);
}
