import Cherry from 'https://dpangzi.com/library/cherry-markdown/cherry-markdown.esm.js';

let cherryInstance = null;

export function createEditor(elementId, markdown, editOnly, dotNetHelper) {    
    const element = document.getElementById(elementId);
    if (!element) {        
        return;
    }
    
    const editorMode = editOnly ? 'editOnly' : 'edit&preview';
    
    try {
        cherryInstance = new Cherry({
            id: elementId,
            value: markdown,
            height: "100%",
            themeSettings: {
                mainTheme: "dark",
                codeBlockTheme: 'one-dark',
            },
            editor: {
                defaultModel: editorMode,
            },
            engine: {
                syntax: {
                    codeBlock: {
                        editCode: false,
                        changeLang: false,
                    },
                }
            },
            toolbars: {
                toolbar: ['bold', 'italic', 'size', '|', 'color', 'header', '|', 'theme',
                    {insert: ['image', 'link', 'hr', 'br', 'code', 'table']}
                ],
                sidebar: ['mobilePreview', 'copy', 'theme'],
            },
            fileUpload: async (file, callback) => {
                if (file.type.startsWith('image/')) {
                    const streamRef = DotNet.createJSStreamReference(file);
                    try {
                        const url = await dotNetHelper.invokeMethodAsync('UploadImage', streamRef, file.name, file.type);
                        if (url) callback(url);
                    } catch (e) {
                        console.error('Upload error:', e);
                    }
                }
            },
            callback: {
                afterChange: () => {},
                afterInit: () => {
                    console.log('Cherry editor initialized');
                },
                beforeImageMounted: (srcProp, src) => srcProp,
            }
        });
        console.log('Cherry instance created successfully');
    } catch (e) {
        console.error('Failed to create Cherry editor:', e);
    }
}

export function getMarkdown() {
    return cherryInstance?.getMarkdown() ?? "";
}

export function setMarkdown(markdown) {
    if (cherryInstance) {
        cherryInstance.setMarkdown(markdown);
    }
}

export function insertValue(value, isSelect = false) {
    if (cherryInstance) {
        cherryInstance.insert(value, isSelect);
    }
}

export function destroy() {
    if (cherryInstance) {
        cherryInstance.destroy();
        cherryInstance = null;
    }
}
