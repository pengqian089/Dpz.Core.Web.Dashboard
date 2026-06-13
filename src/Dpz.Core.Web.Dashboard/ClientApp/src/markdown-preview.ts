import markdownPreviewStylesUrl from "./styles/feature-markdown-preview.css?url";
import { ensureStylesheet } from "./interop/stylesheet";

const highlightedAttribute = "data-highlighted";
type PrismApi = typeof import("prismjs");

class MarkdownPreviewHighlighter {
    private prismModule: Promise<PrismApi> | null = null;

    public async highlightCodeBlocks(container: HTMLElement | null): Promise<void> {
        if (!container) {
            return;
        }

        const prism = await this.getPrism();
        container.querySelectorAll<HTMLElement>("pre code").forEach((block) => {
            if (block.hasAttribute(highlightedAttribute)) {
                return;
            }

            prism.highlightElement(block);
            block.setAttribute(highlightedAttribute, "true");
        });
    }

    public async highlightAll(): Promise<void> {
        const prism = await this.getPrism();
        prism.highlightAll();
    }

    private async getPrism(): Promise<PrismApi> {
        ensureStylesheet(markdownPreviewStylesUrl);
        this.prismModule ??= Promise.all([
            import("prismjs"),
            import("prismjs/components/prism-csharp"),
            import("prismjs/components/prism-css"),
            import("prismjs/components/prism-json"),
            import("prismjs/components/prism-markdown"),
            import("prismjs/components/prism-powershell"),
            import("prismjs/components/prism-sql"),
            import("prismjs/components/prism-typescript")
        ]).then(([module]) => module);

        return await this.prismModule;
    }
}

const highlighter = new MarkdownPreviewHighlighter();

export async function highlightCodeBlocks(container: HTMLElement | null): Promise<void> {
    await highlighter.highlightCodeBlocks(container);
}

export async function highlightAll(): Promise<void> {
    await highlighter.highlightAll();
}
