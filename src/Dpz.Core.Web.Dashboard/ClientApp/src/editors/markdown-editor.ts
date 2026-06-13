import { defaultKeymap, history, historyKeymap, indentWithTab } from "@codemirror/commands";
import { markdown as markdownLanguage } from "@codemirror/lang-markdown";
import {
    defaultHighlightStyle,
    LanguageDescription,
    LanguageSupport,
    StreamLanguage,
    syntaxHighlighting
} from "@codemirror/language";
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
import { Crepe } from "@milkdown/crepe";
import { uploadConfig } from "@milkdown/kit/plugin/upload";
import { replaceAll } from "@milkdown/kit/utils";
import { csharp } from "@codemirror/legacy-modes/mode/clike";
import { TooltipController } from "../interactions/tooltip";
import "../styles/feature-markdown-editor.css";

type DotNetHelper = {
    invokeMethodAsync<T>(methodName: string, ...args: unknown[]): Promise<T>;
};

declare const DotNet: {
    createJSStreamReference(file: Blob): unknown;
};

type MarkdownEditorInstance = {
    crepe: Crepe;
    dotNetHelper: DotNetHelper;
    mode: MarkdownEditMode;
    sourceEditor: SourceMarkdownEditor;
    sourceHost: HTMLElement;
    visualHost: HTMLElement;
};

type MarkdownViewportMode = "desktop" | "mobile";

type MarkdownEditMode = "visual" | "source";

type MermaidApi = typeof import("mermaid").default;

const markdownCodeLanguages = [
    LanguageDescription.of({
        name: "C#",
        alias: ["csharp", "cs"],
        extensions: ["cs"],
        load: () => Promise.resolve(new LanguageSupport(StreamLanguage.define(csharp)))
    }),
    LanguageDescription.of({
        name: "CSS",
        extensions: ["css"],
        load: () => import("@codemirror/lang-css").then((module) => module.css())
    }),
    LanguageDescription.of({
        name: "HTML",
        alias: ["htm"],
        extensions: ["html", "htm"],
        load: () => import("@codemirror/lang-html").then((module) => module.html())
    }),
    LanguageDescription.of({
        name: "JavaScript",
        alias: ["js"],
        extensions: ["js", "mjs", "cjs"],
        load: () => import("@codemirror/lang-javascript").then((module) => module.javascript())
    }),
    LanguageDescription.of({
        name: "JSON",
        extensions: ["json"],
        load: () => import("@codemirror/lang-json").then((module) => module.json())
    }),
    LanguageDescription.of({
        name: "Markdown",
        alias: ["md"],
        extensions: ["md", "markdown"],
        load: () => import("@codemirror/lang-markdown").then((module) => module.markdown())
    }),
    LanguageDescription.of({
        name: "SQL",
        extensions: ["sql"],
        load: () => import("@codemirror/lang-sql").then((module) => module.sql())
    }),
    LanguageDescription.of({
        name: "TypeScript",
        alias: ["ts", "tsx"],
        extensions: ["ts", "tsx"],
        load: () =>
            import("@codemirror/lang-javascript").then((module) =>
                module.javascript({ jsx: true, typescript: true })
            )
    }),
    LanguageDescription.of({
        name: "XML",
        alias: ["svg"],
        extensions: ["xml", "svg"],
        load: () => import("@codemirror/lang-xml").then((module) => module.xml())
    })
];

const toolbarTitles = ["加粗", "斜体", "删除线", "行内代码", "链接", "AI"];

const topBarTitles = [
    "加粗",
    "斜体",
    "删除线",
    "行内代码",
    "无序列表",
    "有序列表",
    "任务列表",
    "引用",
    "分割线",
    "代码块",
    "图片",
    "表格"
];

const htmlBreakLinePattern = /^[ \t]*<br\s*\/?>[ \t]*$/i;

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
        this.applyTitles(".milkdown-toolbar .toolbar-item", toolbarTitles);
        this.applyTitles(".milkdown-top-bar .top-bar-item", topBarTitles);
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

    private applyTitles(selector: string, titles: string[]): void {
        this.root.querySelectorAll<HTMLElement>(selector).forEach((item, index) => {
            const title = titles[index];
            if (!title) {
                return;
            }

            this.setTitle(item, title);
        });
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

class MarkdownEditorRegistry {
    private readonly editors = new Map<string, MarkdownEditorInstance>();
    private readonly toolbarHints = new Map<string, ToolbarHintManager>();
    private readonly imageInputManagers = new Map<string, MultiImageInputManager>();
    private readonly mermaidPreview = new MermaidPreviewRenderer();

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
                [Crepe.Feature.TopBar]: !mobile
            },
            featureConfigs: {
                [Crepe.Feature.BlockEdit]: this.createBlockEditConfig(),
                [Crepe.Feature.CodeMirror]: {
                    languages: markdownCodeLanguages,
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
