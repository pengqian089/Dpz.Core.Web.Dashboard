/** Mermaid 库的默认导出类型，动态 import 时用于保持类型安全。 */
type MermaidApi = typeof import("mermaid").default;

/**
 * Mermaid 图表预览渲染器。
 *
 * Crepe 的代码块预览会把语言名、内容和 applyPreview 回调交给这里。
 * 只有语言为 mermaid/mmd 时才接管渲染；其他语言返回 null，让 Milkdown
 * 继续使用默认预览逻辑。Mermaid 按需动态加载，避免拖慢普通 Markdown 编辑。
 */
export class MermaidPreviewRenderer {
    private initialized = false;
    private mermaidModule: Promise<MermaidApi> | null = null;
    private nextDiagramId = 0;

    /** 尝试渲染 Mermaid 内容；非 Mermaid 语言返回 null。 */
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
