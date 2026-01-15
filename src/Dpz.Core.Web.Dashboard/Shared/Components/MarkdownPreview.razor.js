/**
 * MarkdownPreview.razor.js
 * JavaScript interop for Markdown preview component
 */

const HIGHLIGHTED_ATTRIBUTE = 'data-highlighted';

/**
 * Highlight code blocks that haven't been highlighted yet
 * @param {HTMLElement} container - The markdown preview container
 */
export function highlightCodeBlocks(container) {
    if (!container || !window.Prism) {
        return;
    }

    const codeBlocks = container.querySelectorAll('pre code');
    
    codeBlocks.forEach(block => {
        if (!block.hasAttribute(HIGHLIGHTED_ATTRIBUTE)) {
            window.Prism.highlightElement(block);
            block.setAttribute(HIGHLIGHTED_ATTRIBUTE, 'true');
        }
    });
}
