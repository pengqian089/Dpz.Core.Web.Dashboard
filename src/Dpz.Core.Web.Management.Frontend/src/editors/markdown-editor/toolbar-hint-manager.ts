import { TooltipController } from "../../interactions/tooltip";
import { selectionToolbarHints, topBarHints } from "./toolbar-hints";
import type { MarkdownViewportMode, ToolbarHint } from "./types";

/**
 * 工具栏提示管理器。
 *
 * Milkdown 的工具栏按钮由 Vue 组件动态渲染，编辑器生命周期中会反复增删。
 * 因此这里通过 MutationObserver 监听容器变化，在按钮出现后补齐中文 tooltip
 * 和 button type，避免按钮落在表单中时触发提交。
 */
export class ToolbarHintManager {
    private readonly observer: MutationObserver;
    private readonly tooltips = new TooltipController();

    public constructor(private readonly root: HTMLElement) {
        this.observer = new MutationObserver(() => this.apply());
    }

    /** 立即应用一次提示，并开始监听后续 Milkdown DOM 变化。 */
    public start(): void {
        this.apply();
        this.observer.observe(this.root, {
            childList: true,
            subtree: true
        });
    }

    /** 停止监听并移除已创建的 WebAwesome tooltip 节点。 */
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

    /**
     * 按当前视口过滤 hints 后，根据 Milkdown 实际 DOM 顺序绑定 tooltip。
     *
     * Milkdown 没有给按钮输出稳定 key，因此当前最稳妥的做法是用数据配置维护
     * “实际渲染顺序”。table 在移动端不渲染，会先从配置中移除再匹配 index。
     */
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
