const loadedStylesheets = new Set<string>();

export function ensureStylesheet(href: string): void {
    if (loadedStylesheets.has(href)) {
        return;
    }

    const existingLink = document.querySelector<HTMLLinkElement>(
        `link[rel="stylesheet"][href="${href}"]`
    );
    if (existingLink) {
        loadedStylesheets.add(href);
        return;
    }

    const link = document.createElement("link");
    link.rel = "stylesheet";
    link.href = href;
    document.head.append(link);
    loadedStylesheets.add(href);
}
