let cherryInstance = null;

/**
 * @param {string} elementId 编辑器ID
 * @param {string} markdown 初始化markdown
 * */
function createNewEditor(elementId, markdown) {
    cherryInstance = new Cherry({
        id: elementId,
        value: markdown,
        // 目前应用的主题
        themeSettings: {
            mainTheme: "dark",
            // 目前应用的代码块主题
            codeBlockTheme: 'one-dark',
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
            // 配置切换主题的按钮到顶部工具栏里
            toolbar: ['bold', 'italic', 'size', '|', 'color', 'header', '|', 'theme',
                {insert: ['image', 'link', 'hr', 'br', 'code', 'table']}
            ],
            // 配置切换主题的按钮到侧边栏里
            sidebar: ['mobilePreview', 'copy', 'theme'],
        },
        fileUpload: async function (file, callback) {
            await fileUpload(file, callback, elementId);
        }
    });
}

function getMarkdown() {
    return cherryInstance?.getMarkdown() ?? "";
}

/**
 * 上传文件函数
 * @param file 上传文件的文件对象
 * @param callback 回调函数，回调函数接收两个参数，第一个参数为文件上传后的url，第二个参数可选，为额外配置信息
 * @param elementId
 */
async function fileUpload(file, callback, elementId) {
    const formData = new FormData();
    formData.append("image", file);
    const postAction = document.getElementById("txt" + elementId).value;
    let identity = JSON.parse(localStorage.getItem("Identity"));
    let response = await fetch(postAction, {
        method: "post",
        body: formData,
        headers: {Authorization: `Bearer ${identity["Token"]}`}
    });
    let result = await response.json();


    callback(result["url"]);

}