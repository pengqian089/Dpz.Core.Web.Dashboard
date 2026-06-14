import Prism from "prismjs";
import "prismjs/components/prism-csharp";
import "prismjs/components/prism-css";
import "prismjs/components/prism-json";
import "prismjs/components/prism-markdown";
import "prismjs/components/prism-powershell";
import "prismjs/components/prism-sql";
import "prismjs/components/prism-typescript";

const highlightedAttribute = "data-highlighted";

class MarkdownPreviewHighlighter {
    public highlightCodeBlocks(container: HTMLElement | null): void {
        if (!container) {
            return;
        }

        container.querySelectorAll<HTMLElement>("pre code").forEach((block) => {
            if (block.hasAttribute(highlightedAttribute)) {
                return;
            }

            Prism.highlightElement(block);
            block.setAttribute(highlightedAttribute, "true");
        });
    }

    public highlightAll(): void {
        Prism.highlightAll();
    }
}

const highlighter = new MarkdownPreviewHighlighter();

export function highlightCodeBlocks(container: HTMLElement | null): void {
    highlighter.highlightCodeBlocks(container);
}

export function highlightAll(): void {
    highlighter.highlightAll();
}
