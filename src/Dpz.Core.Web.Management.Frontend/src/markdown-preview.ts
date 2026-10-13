import Prism from "prismjs";
import "prismjs/components/prism-bash";
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

    public highlightAll(force = false): void {
        if (force) {
            document.querySelectorAll<HTMLElement>("pre code").forEach((block) => {
                block.removeAttribute(highlightedAttribute);
            });
        }

        Prism.highlightAll();
    }
}

const highlighter = new MarkdownPreviewHighlighter();

export function highlightCodeBlocks(container: HTMLElement | null): void {
    highlighter.highlightCodeBlocks(container);
}

export function highlightAll(force = false): void {
    highlighter.highlightAll(force);
}
