let vditorInstance = null;

export function createEditor(elementId, markdown, editOnly, dotNetHelper) {
    const element = document.getElementById(elementId);
    if (!element) {
        return;
    }

    try {
        vditorInstance = new Vditor(elementId, {
            value: markdown,
            minHeight: 500,
            height: '100%',
            width: '100%',
            theme: 'dark',
            mode: 'ir',
            cache: {
                enable: false,
            },
            preview: {
                mode: editOnly ? 'editor' : 'both',
                theme: {
                    current: 'dark',
                    path: 'https://dpangzi.com/library/vditor/css/content-theme'
                },
                hljs: {
                    enable: true,
                    style: 'vs2015',
                    lineNumber: true
                }
            },
            counter: {
                enable: true,
            },
            link: {
                isOpen: false,
            },
            tab: '    ',
            toolbar: [
                'emoji', 'headings', 'bold', 'italic', 'strike', 'link', '|',
                'list', 'ordered-list', 'check', 'outdent', 'indent', '|',
                'quote', 'line', 'code', 'inline-code', 'insert-before', 'insert-after', '|',
                'upload', 'table', '|',
                'undo', 'redo', '|',
                'edit-mode', 'content-theme', 'code-theme', '|',
                'both', 'preview', 'fullscreen'
            ],
            upload: {
                accept: 'image/*',
                handler: async (files) => {
                    if (!files || files.length === 0) return;

                    const file = files[0];
                    if (file.type.startsWith('image/')) {
                        const streamRef = DotNet.createJSStreamReference(file);
                        try {
                            const url = await dotNetHelper.invokeMethodAsync('UploadImage', streamRef, file.name, file.type);
                            if (url) {
                                const imageMarkdown = `![${file.name}](${url})`;
                                vditorInstance.insertValue(imageMarkdown);
                            }
                        } catch (e) {
                            console.error('Upload error:', e);
                        }
                    } else {
                        console.error('Only image uploads are supported via this handler.');
                    }
                }
            },
            after: () => {
                console.log('Vditor editor initialized');
            }
        });
        console.log('Vditor instance created successfully');
    } catch (e) {
        console.error('Failed to create Vditor editor:', e);
    }
}

export function getMarkdown() {
    return vditorInstance?.getValue() ?? "";
}

export function setMarkdown(markdown) {
    if (vditorInstance) {
        vditorInstance.setValue(markdown);
    }
}

export function insertValue(value, isSelect = false) {
    if (vditorInstance) {
        vditorInstance.insertValue(value, true);
    }
}

export function destroy() {
    if (vditorInstance) {
        vditorInstance.destroy();
        vditorInstance = null;
    }
}
