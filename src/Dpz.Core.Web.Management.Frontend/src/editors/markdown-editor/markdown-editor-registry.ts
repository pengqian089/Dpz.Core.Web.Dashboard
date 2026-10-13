import { languages } from "@codemirror/language-data";
import { oneDark } from "@codemirror/theme-one-dark";
import { Crepe } from "@milkdown/crepe";
import { uploadConfig } from "@milkdown/kit/plugin/upload";
import { replaceAll } from "@milkdown/kit/utils";
import { MermaidPreviewRenderer } from "./mermaid-preview-renderer";
import { MultiImageInputManager } from "./multi-image-input-manager";
import { SourceMarkdownEditor } from "./source-markdown-editor";
import { codeBlockTheme } from "./themes";
import { ToolbarHintManager } from "./toolbar-hint-manager";
import { TopBarTouchGuard } from "./top-bar-touch-guard";
import type {
    CreateJSStreamReference,
    DotNetHelper,
    MarkdownEditMode,
    MarkdownEditorInstance,
    MarkdownImageMode,
    MarkdownViewportMode
} from "./types";

/** 匹配独立成行的 HTML <br> 标签，用于清理 Milkdown 产出的无意义空行。 */
const htmlBreakLinePattern = /^[ \t]*<br\s*\/?>[ \t]*$/i;

/**
 * Markdown 编辑器总注册表。
 *
 * 这是 Blazor JS 模块背后的状态中心：同一页面可能创建多个 MarkdownEditor，
 * 因此所有实例都按 elementId 注册在这里。它负责协调 Crepe 可视化编辑器、
 * SourceMarkdownEditor 源码编辑器、工具栏提示、图片上传和模式切换。
 */
export class MarkdownEditorRegistry {
    private readonly editors = new Map<string, MarkdownEditorInstance>();
    private readonly toolbarHints = new Map<string, ToolbarHintManager>();
    private readonly imageInputManagers = new Map<string, MultiImageInputManager>();
    private readonly topBarTouchGuards = new Map<string, TopBarTouchGuard>();
    private readonly mermaidPreview = new MermaidPreviewRenderer();

    public constructor(private readonly createJSStreamReference: CreateJSStreamReference) {}

    /**
     * 创建或重建编辑器实例。
     *
     * 若 elementId 已存在实例，会先完整销毁旧实例，避免 Milkdown/CodeMirror 的
     * DOM、事件监听和 MutationObserver 残留。editOnly 名称沿用现有 .NET 调用，
     * 实际含义是只读状态。
     */
    public async createEditor(
        elementId: string,
        markdown: string,
        editOnly: boolean,
        dotNetHelper: DotNetHelper,
        imageMode: MarkdownImageMode = "inline"
    ): Promise<void> {
        const root = document.getElementById(elementId);
        const container = document.getElementById(`markdown-container-${elementId}`);
        if (!root || !container) {
            return;
        }

        await this.destroy(elementId);
        root.textContent = "";
        root.classList.add("markdown-editor-workspace");
        container.classList.toggle("markdown-editor-container--gallery", imageMode === "gallery");
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
        const crepe = this.createCrepe(visualHost, markdown, mobile, dotNetHelper);
        this.configureUploadPlugin(crepe, dotNetHelper);

        await crepe.create();
        crepe.setReadonly(editOnly);

        const sourceEditor = new SourceMarkdownEditor(sourceHost, markdown ?? "", editOnly);
        this.editors.set(elementId, {
            crepe,
            dotNetHelper,
            imageMode,
            mode: "visual",
            sourceEditor,
            sourceHost,
            visualHost
        });
        this.applyMode(elementId);
        this.configureToolbar(container);
        this.startToolbarHints(elementId, container);
        this.startImageInputManager(elementId, container);
        this.startTopBarTouchGuard(elementId, container);
    }

    /** 获取当前 Markdown，源码模式直接读 CodeMirror，可视化模式读 Crepe 并清理噪声。 */
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

    /** 外部同步 Markdown 内容时，同时更新可视化编辑器和源码编辑器。 */
    public setMarkdown(elementId: string, markdown: string): void {
        const instance = this.editors.get(elementId);
        if (!instance) {
            return;
        }

        instance.crepe.editor.action(replaceAll(markdown ?? "", true));
        instance.sourceEditor.setValue(markdown ?? "");
    }

    /** 向当前模式插入文本；源码模式插入到光标处，可视化模式追加到文档末尾。 */
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

    /** 动态切换只读状态，需要同时更新 Crepe 和源码编辑器。 */
    public setReadonly(elementId: string, readonly: boolean): void {
        const instance = this.editors.get(elementId);
        if (!instance) {
            return;
        }

        instance.crepe.setReadonly(readonly);
        instance.sourceEditor.setReadonly(readonly);
    }

    /** 销毁指定实例及其附属管理器。 */
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
        this.destroyTopBarTouchGuard(elementId);
    }

    private createCrepe(
        root: HTMLElement,
        markdown: string,
        mobile: boolean,
        dotNetHelper: DotNetHelper
    ): Crepe {
        return new Crepe({
            root,
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
    }

    /**
     * 配置拖拽/粘贴图片的上传处理。
     *
     * Milkdown upload plugin 会接管粘贴和拖放文件；这里把默认 base64 行为替换为
     * 通过 Blazor 上传，并用返回 URL 创建 image-block/image 节点。
     */
    private configureUploadPlugin(crepe: Crepe, dotNetHelper: DotNetHelper): void {
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
                    if (this.getImageMode(dotNetHelper) === "gallery") {
                        this.appendUploadedImages(dotNetHelper, images, urls);
                        return [];
                    }

                    return urls
                        .map((src) => nodeType.createAndFill({ src }))
                        .filter((node) => node !== null);
                }
            }));
        });
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
     * 切换编辑模式。
     *
     * 切换前必须把当前模式的内容同步到目标编辑器，否则用户会看到旧内容。
     * 从可视化切到源码时额外清理 Milkdown 产生的独立 <br> 行。
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

    /** 根据当前模式切换 DOM 可见性和 modebar 按钮选中态。 */
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
     *
     * Milkdown 连续换行可能生成独立 <br> 行；这些行保存为 Markdown 后会污染源码，
     * 因此在读取可视化内容时移除，并压缩过多空行。
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

    /** 根据视口给容器打 class，CSS 和 toolbar hints 都依赖这些 class。 */
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
        const manager = new MultiImageInputManager(
            root,
            async (input, files) => {
                const instance = this.editors.get(elementId);
                if (!instance) {
                    return;
                }

                const urls = await this.uploadImages(instance.dotNetHelper, files);
                if (urls.length === 0) {
                    return;
                }

                if (instance.imageMode === "gallery") {
                    this.clearCurrentImageInput(input);
                    this.appendUploadedImages(instance.dotNetHelper, files, urls);
                    return;
                }

                const firstUrlInserted = this.setCurrentImageUrl(input, urls[0]);
                const remainingUrls = firstUrlInserted ? urls.slice(1) : urls;
                if (remainingUrls.length > 0) {
                    this.insertValue(elementId, this.createImageMarkdown(files, remainingUrls));
                }
            },
            (_, files) => this.editors.get(elementId)?.imageMode === "gallery" || files.length > 1
        );

        manager.start();
        this.imageInputManagers.set(elementId, manager);
    }

    private destroyImageInputManager(elementId: string): void {
        this.imageInputManagers.get(elementId)?.stop();
        this.imageInputManagers.delete(elementId);
    }

    private startTopBarTouchGuard(elementId: string, root: HTMLElement): void {
        const guard = new TopBarTouchGuard(root);
        guard.start();
        this.topBarTouchGuards.set(elementId, guard);
    }

    private destroyTopBarTouchGuard(elementId: string): void {
        this.topBarTouchGuards.get(elementId)?.stop();
        this.topBarTouchGuards.delete(elementId);
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
        if (this.getImageMode(dotNetHelper) === "gallery") {
            this.appendUploadedImages(dotNetHelper, [file], urls);
            return urls[0] ?? "";
        }

        return urls[0] ?? "";
    }

    /**
     * 图片上传链路：File -> JSStreamReference -> .NET UploadImages -> URL[]。
     *
     * 这里不直接读取文件内容，交给 Blazor stream reference 传输，避免大图片在 JS
     * 侧额外复制成 base64。
     */
    private async uploadImages(
        dotNetHelper: DotNetHelper,
        files: readonly File[]
    ): Promise<string[]> {
        const streamRefs = files.map((file) => this.createJSStreamReference(file));
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

    private appendUploadedImages(
        dotNetHelper: DotNetHelper,
        files: readonly File[],
        urls: readonly string[]
    ): void {
        const elementId = this.getElementId(dotNetHelper);
        if (!elementId || urls.length === 0) {
            return;
        }

        const markdown = this.createImageMarkdown(files, urls);
        const instance = this.editors.get(elementId);
        if (!instance) {
            return;
        }

        const nextMarkdown = `${this.getMarkdown(elementId)}${markdown}`;
        instance.crepe.editor.action(replaceAll(nextMarkdown, true));
        instance.sourceEditor.setValue(nextMarkdown);
    }

    private getImageMode(dotNetHelper: DotNetHelper): MarkdownImageMode {
        const elementId = this.getElementId(dotNetHelper);
        return elementId ? (this.editors.get(elementId)?.imageMode ?? "inline") : "inline";
    }

    private getElementId(dotNetHelper: DotNetHelper): string | undefined {
        for (const [elementId, instance] of this.editors) {
            if (instance.dotNetHelper === dotNetHelper) {
                return elementId;
            }
        }

        return undefined;
    }

    /**
     * 将第一张上传后的图片 URL 注入 Milkdown 图片弹窗。
     *
     * 这是对 Milkdown 当前 DOM 结构的适配：找到 .image-edit 内部 URL 输入框，
     * 手动触发 input 和 Enter，让弹窗按“用户确认 URL”的路径完成插入。
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

    private clearCurrentImageInput(input: HTMLInputElement): void {
        const imageEdit = input.closest(".image-edit");
        const linkInput = imageEdit?.querySelector<HTMLInputElement>(".link-input-area");
        if (!linkInput) {
            return;
        }

        linkInput.value = "";
        linkInput.dispatchEvent(new InputEvent("input", { bubbles: true, data: "" }));
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
