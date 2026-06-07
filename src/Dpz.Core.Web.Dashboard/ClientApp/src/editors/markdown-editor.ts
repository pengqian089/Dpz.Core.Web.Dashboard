import { languages } from "@codemirror/language-data";
import { oneDark } from "@codemirror/theme-one-dark";
import { EditorView } from "@codemirror/view";
import { Crepe } from "@milkdown/crepe";
import { replaceAll } from "@milkdown/kit/utils";
import { TooltipController } from "../interactions/tooltip";

type DotNetHelper = {
    invokeMethodAsync<T>(methodName: string, ...args: unknown[]): Promise<T>;
};

declare const DotNet: {
    createJSStreamReference(file: Blob): unknown;
};

type MarkdownEditorInstance = {
    crepe: Crepe;
    dotNetHelper: DotNetHelper;
};

type MarkdownViewportMode = "desktop" | "mobile";

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
        this.applyTitles(".milkdown-toolbar .toolbar-item", toolbarTitles);
        this.applyTitles(".milkdown-top-bar .top-bar-item", topBarTitles);
        this.applyStaticTitle(".top-bar-heading-button", "选择标题级别");
        this.applyHeadingOptions();
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
        this.configureToolbar(container);
        this.destroyToolbarHints(elementId);

        const mobile = this.isMobileViewport();
        const crepe = new Crepe({
            root,
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
                    languages,
                    theme: [oneDark, codeBlockTheme],
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

        await crepe.create();
        crepe.setReadonly(editOnly);
        this.editors.set(elementId, { crepe, dotNetHelper });
        this.configureToolbar(container);
        this.startToolbarHints(elementId, container);
    }

    public getMarkdown(elementId: string): string {
        return this.editors.get(elementId)?.crepe.getMarkdown() ?? "";
    }

    public setMarkdown(elementId: string, markdown: string): void {
        const instance = this.editors.get(elementId);
        if (!instance) {
            return;
        }

        instance.crepe.editor.action(replaceAll(markdown ?? "", true));
    }

    public insertValue(elementId: string, value: string): void {
        const instance = this.editors.get(elementId);
        if (!instance) {
            return;
        }

        const nextMarkdown = `${instance.crepe.getMarkdown()}${value}`;
        instance.crepe.editor.action(replaceAll(nextMarkdown, true));
    }

    public setReadonly(elementId: string, readonly: boolean): void {
        this.editors.get(elementId)?.crepe.setReadonly(readonly);
    }

    public async destroy(elementId: string): Promise<void> {
        const instance = this.editors.get(elementId);
        if (!instance) {
            return;
        }

        await instance.crepe.destroy();
        this.editors.delete(elementId);
        this.destroyToolbarHints(elementId);
    }

    private configureToolbar(root: HTMLElement): void {
        const mode = this.getViewportMode();
        root.classList.toggle("markdown-editor-container--mobile", mode === "mobile");
        root.classList.toggle("markdown-editor-container--desktop", mode === "desktop");

        requestAnimationFrame(() => {
            root.querySelectorAll("button:not([type])").forEach((button) => {
                button.setAttribute("type", "button");
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
        const streamRef = DotNet.createJSStreamReference(file);
        return await dotNetHelper.invokeMethodAsync<string>(
            "UploadImage",
            streamRef,
            file.name,
            file.type
        );
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
