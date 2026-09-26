(function (DpzOS) {
    "use strict";

    var ui = DpzOS.ui;
    var util = DpzOS.util;
    var icon = DpzOS.icon;
    var data = DpzOS.data;

    var PAGE_SIZE = 10;

    function filtered(state) {
        return data.articles.filter(function (article) {
            if (state.tag && state.tag !== "全部" && article.tags.indexOf(state.tag) < 0) {
                return false;
            }
            if (state.author && state.author !== "全部" && article.author !== state.author) {
                return false;
            }
            if (state.title && article.title.toLowerCase().indexOf(state.title.toLowerCase()) < 0) {
                return false;
            }
            return true;
        });
    }

    function renderList(ctx) {
        var state = ctx.state;
        state.page = state.page || 1;
        state.title = state.title || "";
        state.tag = state.tag || "全部";
        state.author = state.author || "全部";
        if (ctx.params && ctx.params.tag) {
            state.tag = ctx.params.tag;
        }

        var rows = filtered(state);
        var totalPages = Math.max(1, Math.ceil(rows.length / PAGE_SIZE));
        state.page = util.clamp(state.page, 1, totalPages);
        var pageRows = rows.slice((state.page - 1) * PAGE_SIZE, state.page * PAGE_SIZE);

        var table = ui.table({
            columns: [
                { label: "标题" },
                { label: "作者" },
                { label: "互动", align: "right" },
                { label: "来源" },
                { label: "发布时间" },
                { label: "操作", align: "right" }
            ],
            rows: pageRows.map(function (article) {
                return {
                    attrs: 'data-id="' + article.id + '"',
                    cells: [
                        '<div class="ui-table__main u-ellipsis">' +
                            "<span>" +
                            util.esc(article.title) +
                            "</span>" +
                            (article.images > 0
                                ? ui.badge(article.images + " 图", null, "image")
                                : "") +
                            "</div>" +
                            '<div class="ui-table__sub">#' +
                            article.id +
                            " · " +
                            article.tags
                                .map(function (tag) {
                                    return '<span class="app-article__tag">' + util.esc(tag) + "</span>";
                                })
                                .join(" ") +
                            "</div>",
                        util.esc(article.author),
                        '<span class="u-mono">' +
                            util.formatNumber(article.views) +
                            "</span> <span class='u-dim'>阅</span> · " +
                            '<span class="u-mono">' +
                            article.replies +
                            "</span> <span class='u-dim'>评</span>",
                        ui.badge(article.source, article.source === "原创" ? "accent" : "violet"),
                        '<span class="u-mono u-muted">' + util.esc(util.relativeTime(article.publishedAt)) + "</span>",
                        '<div class="ui-table__actions">' +
                            ui.iconBtn("edit", {
                                label: "编辑",
                                title: "编辑文章",
                                attrs: 'data-action="edit" data-id="' + article.id + '"'
                            }) +
                            ui.iconBtn("external", {
                                label: "前台查看",
                                attrs: 'data-action="view" data-id="' + article.id + '"'
                            }) +
                            ui.iconBtn("trash", {
                                label: "删除",
                                danger: true,
                                attrs: 'data-action="delete" data-id="' + article.id + '"'
                            }) +
                            "</div>"
                    ]
                };
            })
        });

        return (
            '<div class="ui-page">' +
            '<div class="ui-head">' +
            '<div class="ui-head__text">' +
            '<div class="ui-eyebrow">内容创作 · ARTICLES</div>' +
            '<h1 class="ui-title">文章管理</h1>' +
            '<div class="ui-subtitle">共 ' +
            util.formatNumber(data.articles.length) +
            " 篇文章，当前视图显示 " +
            rows.length +
            " 条</div>" +
            "</div>" +
            '<div class="ui-head__actions">' +
            ui.btn({
                label: "发布文章",
                icon: "plus",
                variant: "primary",
                attrs: 'data-action="create"'
            }) +
            ui.btn({ label: "刷新", icon: "refresh", attrs: 'data-action="refresh"' }) +
            "</div>" +
            "</div>" +
            ui.toolbar([
                [
                    ui.search({
                        placeholder: "搜索标题，支持拼音…",
                        value: state.title,
                        attrs: 'data-role="search-title"'
                    }),
                    ui.select({
                        options: ["全部"].concat(state.tags || data.tags),
                        value: state.tag,
                        attrs: 'data-role="filter-tag"'
                    }),
                    ui.select({
                        options: ["全部"].concat(data.authors),
                        value: state.author,
                        attrs: 'data-role="filter-author"'
                    })
                ],
                [
                    ui.chip("原创 " + data.articles.filter(function (a) { return a.source === "原创"; }).length, {
                        icon: "check-circle"
                    }),
                    ui.chip("转载 " + data.articles.filter(function (a) { return a.source !== "原创"; }).length, {
                        icon: "copy"
                    })
                ]
            ]) +
            table +
            ui.pager({
                page: state.page,
                totalPages: totalPages,
                pageSize: PAGE_SIZE,
                count: rows.length
            }) +
            "</div>"
        );
    }

    function bindList(rootNode, ctx) {
        var state = ctx.state;

        var search = rootNode.querySelector('[data-role="search-title"]');
        if (search) {
            search.addEventListener("keydown", function (event) {
                if (event.key === "Enter") {
                    state.title = search.value.trim();
                    state.page = 1;
                    ctx.rerender();
                }
            });
        }

        rootNode.addEventListener("change", function (event) {
            if (event.target.matches('[data-role="filter-tag"]')) {
                state.tag = event.target.value;
                state.page = 1;
                ctx.rerender();
            }
            if (event.target.matches('[data-role="filter-author"]')) {
                state.author = event.target.value;
                state.page = 1;
                ctx.rerender();
            }
        });

        rootNode.addEventListener("click", function (event) {
            var pageButton = event.target.closest("[data-page]");
            if (pageButton && !pageButton.disabled) {
                state.page = Number(pageButton.getAttribute("data-page"));
                ctx.rerender();
                return;
            }

            var action = event.target.closest("[data-action]");
            if (!action) {
                return;
            }
            var kind = action.getAttribute("data-action");
            var id = Number(action.getAttribute("data-id"));

            if (kind === "create") {
                ctx.open("article-editor", { mode: "create" });
            }
            if (kind === "refresh") {
                ctx.rerender();
                DpzOS.toast({ title: "列表已刷新", tone: "success", icon: "refresh" });
            }
            if (kind === "edit") {
                ctx.open("article-editor", { id: id });
            }
            if (kind === "view") {
                DpzOS.toast({
                    title: "已在新窗口打开前台页面",
                    text: DpzOS.data.profile.server + "/article/read/" + id + ".html",
                    tone: "info",
                    icon: "external"
                });
            }
            if (kind === "delete") {
                var article = data.articles.find(function (item) {
                    return item.id === id;
                });
                ctx.confirm({
                    title: "删除这篇文章？",
                    text: "《" + (article ? article.title : id) + "》删除后无法恢复，前台将立即 404。",
                    confirmText: "删除",
                    tone: "danger",
                    icon: "trash"
                }).then(function (ok) {
                    if (!ok) {
                        return;
                    }
                    data.articles = data.articles.filter(function (item) {
                        return item.id !== id;
                    });
                    ctx.rerender();
                    DpzOS.toast({ title: "文章已删除", tone: "success", icon: "check-circle" });
                });
            }
        });

        rootNode.addEventListener("dblclick", function (event) {
            if (event.target.closest("button") || event.target.closest(".ui-pagebtn")) {
                return;
            }
            var row = event.target.closest("tr[data-id]");
            if (row) {
                ctx.open("article-editor", { id: Number(row.getAttribute("data-id")) });
            }
        });
    }

    DpzOS.registerApp({
        id: "article-list",
        name: "文章管理",
        en: "Articles",
        icon: "article",
        tone: "accent",
        group: "内容创作",
        size: { w: 1120, h: 700 },
        singleton: true,
        desc: "文章列表、筛选与发布入口",
        render: renderList,
        mount: bindList
    });

    function renderEditor(ctx) {
        var state = ctx.state;
        var article = null;
        if (ctx.params && ctx.params.id) {
            article = data.articles.find(function (item) {
                return item.id === ctx.params.id;
            });
        }
        state.draft = state.draft || {
            title: article ? article.title : "",
            summary: article ? "" : "",
            tags: article ? article.tags.slice() : ["设计"],
            body: article ? data.articleDetail.body : "# 新文章\n\n开始写作吧……",
            status: article ? "已发布" : "草稿"
        };
        var draft = state.draft;

        var toolbarButtons = ["bold", "italic", "heading", "quote", "list", "list-ordered", "code", "link", "image", "divider"];
        var toolbar = toolbarButtons
            .map(function (name) {
                return (
                    '<button type="button" class="app-editor__tbtn" data-format="' +
                    name +
                    '" title="' +
                    name +
                    '">' +
                    icon(name) +
                    "</button>"
                );
            })
            .join('<span class="app-editor__tsep"></span>');

        var tagChips = draft.tags
            .map(function (tag) {
                return ui.chip(tag, { removable: true, attrs: 'data-tag="' + util.esc(tag) + '"' });
            })
            .join("");

        return (
            '<div class="app-editor" style="height:100%">' +
            '<div class="app-editor__main">' +
            '<div class="app-editor__top">' +
            '<div class="ui-spread">' +
            "<div class=\"ui-row ui-row--tight\">" +
            ui.btn({
                label: "返回列表",
                icon: "arrow-left",
                size: "sm",
                variant: "ghost",
                attrs: 'data-action="back"'
            }) +
            ui.badge(draft.status, draft.status === "已发布" ? "success" : "warning", draft.status === "已发布" ? "check-circle" : "edit") +
            (article ? ui.badge("#" + article.id, null, "hash") : ui.badge("新文章", "accent", "plus")) +
            "</div>" +
            '<div class="ui-row ui-row--tight">' +
            ui.btn({ label: "预览", icon: "eye", size: "sm", attrs: 'data-action="preview"' }) +
            ui.btn({ label: "存为草稿", icon: "archive", size: "sm", attrs: 'data-action="draft"' }) +
            ui.btn({
                label: "发布",
                icon: "send",
                size: "sm",
                variant: "primary",
                attrs: 'data-action="publish"'
            }) +
            "</div>" +
            "</div>" +
            '<input class="app-editor__title" value="' +
            util.esc(draft.title) +
            '" placeholder="输入文章标题…" data-role="editor-title">' +
            '<textarea class="ui-textarea" style="min-height:60px" placeholder="写一段简介（必填）…" data-role="editor-summary">' +
            util.esc(draft.summary) +
            "</textarea>" +
            "</div>" +
            '<div class="app-editor__toolbar">' +
            toolbar +
            '<span class="ui-toolbar__grow"></span>' +
            ui.badge("Markdown · Milkdown", "violet", "sparkles") +
            "</div>" +
            '<div class="app-editor__pane" data-role="editor-pane">' +
            util.esc(draft.body)
                .replace(/^(#{1,3}) (.*)$/gm, '<span class="h">$1 $2</span>')
                .replace(/\*\*(.+?)\*\*/g, '<span class="strong">**$1**</span>')
                .replace(/^&gt; (.*)$/gm, '<span class="quote">&gt; $1</span>') +
            '<br><span class="ui-terminal__cursor"></span>' +
            "</div>" +
            "</div>" +
            '<aside class="app-editor__side">' +
            '<div class="ui-section">' +
            '<div class="ui-section__head"><h3 class="ui-section__title">' +
            icon("tag") +
            "标签</h3><span class=\"ui-section__line\"></span></div>" +
            '<div class="ui-chips" data-role="editor-tags">' +
            tagChips +
            '<button type="button" class="ui-chip ui-chip--click" data-action="add-tag">' +
            icon("plus") +
            "添加</button>" +
            "</div>" +
            "</div>" +
            '<div class="ui-section">' +
            '<div class="ui-section__head"><h3 class="ui-section__title">' +
            icon("image") +
            "图片画廊</h3><span class=\"ui-section__line\"></span></div>" +
            '<div class="app-editor__gallery">' +
            data.pictures
                .slice(0, 6)
                .map(function (picture) {
                    return ui.cover(picture.id, {
                        cls: "ui-cover--square",
                        center: icon("image"),
                        attrs: 'data-action="preview-image" data-id="' + picture.id + '"'
                    });
                })
                .join("") +
            "</div>" +
            '<div class="ui-field__hint" style="margin-top:8px">Gallery 模式：上传图片只进入画廊，发布时自动追加到正文末尾。</div>' +
            "</div>" +
            '<div class="ui-section">' +
            '<div class="ui-section__head"><h3 class="ui-section__title">' +
            icon("settings") +
            "发布设置</h3><span class=\"ui-section__line\"></span></div>" +
            '<label class="ui-switch"><input type="checkbox" checked data-role="allow-comment">允许评论</label>' +
            '<label class="ui-switch"><input type="checkbox" checked data-role="sync-source">同步到主站</label>' +
            ui.field({
                label: "来源",
                control: ui.select({
                    options: ["原创", "转载", "翻译"],
                    value: "原创"
                })
            }) +
            "</div>" +
            ui.callout("左侧编辑区在正式版中由 Milkdown Crepe 与 CodeMirror 双模式驱动，支持拖拽上传、Mermaid 预览与画廊模式。", {
                tone: "accent",
                icon: "sparkles"
            }) +
            "</aside>" +
            "</div>"
        );
    }

    function bindEditor(rootNode, ctx) {
        var state = ctx.state;
        var draft = state.draft;

        var titleInput = rootNode.querySelector('[data-role="editor-title"]');
        var summaryInput = rootNode.querySelector('[data-role="editor-summary"]');
        titleInput.addEventListener("input", function () {
            draft.title = titleInput.value;
        });
        summaryInput.addEventListener("input", function () {
            draft.summary = summaryInput.value;
        });

        rootNode.addEventListener("click", function (event) {
            var format = event.target.closest("[data-format]");
            if (format) {
                DpzOS.toast({
                    title: "工具栏（演示）",
                    text: "格式：" + format.getAttribute("data-format"),
                    tone: "info",
                    timeout: 1400
                });
                return;
            }

            var chip = event.target.closest(".ui-chip[data-tag]");
            if (chip && event.target.closest(".ui-chip__x")) {
                var tag = chip.getAttribute("data-tag");
                draft.tags = draft.tags.filter(function (item) {
                    return item !== tag;
                });
                ctx.rerender();
                return;
            }

            var action = event.target.closest("[data-action]");
            if (!action) {
                return;
            }
            var kind = action.getAttribute("data-action");

            if (kind === "back") {
                ctx.open("article-list");
                ctx.close();
            }
            if (kind === "add-tag") {
                DpzOS.dialog
                    .prompt({ title: "添加标签", label: "标签名称", placeholder: "例如：赛博朋克" })
                    .then(function (value) {
                        if (value && value.trim() && draft.tags.indexOf(value.trim()) < 0) {
                            draft.tags.push(value.trim());
                            ctx.rerender();
                            DpzOS.toast({ title: "标签已添加", text: value.trim(), tone: "success", icon: "tag" });
                        }
                    });
            }
            if (kind === "preview") {
                ctx.open("markdown-preview-app", { title: draft.title || "未命名文章" });
            }
            if (kind === "draft") {
                draft.status = "草稿";
                ctx.rerender();
                DpzOS.toast({ title: "已保存草稿", tone: "success", icon: "archive" });
            }
            if (kind === "publish") {
                if (!draft.title.trim() || !summaryInput.value.trim()) {
                    DpzOS.toast({ title: "发布失败", text: "标题与简介为必填项", tone: "danger", icon: "alert" });
                    return;
                }
                draft.status = "已发布";
                ctx.rerender();
                ctx.notify({
                    tone: "success",
                    title: "文章已发布",
                    text: "《" + draft.title.trim() + "》已同步到主站。",
                    time: new Date(),
                    icon: "send"
                });
                DpzOS.toast({ title: "发布成功", text: "前台已可访问", tone: "success", icon: "send" });
            }
            if (kind === "preview-image") {
                var picture = data.pictures.find(function (item) {
                    return item.id === action.getAttribute("data-id");
                });
                if (picture) {
                    DpzOS.lightbox.open({
                        title: picture.name,
                        desc: picture.desc,
                        seed: picture.id,
                        metaHtml:
                            "尺寸：" +
                            util.esc(picture.dimensions) +
                            "<br>大小：" +
                            util.esc(picture.size) +
                            "<br>MD5：" +
                            util.esc(picture.md5)
                    });
                }
            }
        });
    }

    DpzOS.registerApp({
        id: "article-editor",
        name: "文章编辑器",
        en: "Editor",
        icon: "edit",
        tone: "violet",
        group: "内容创作",
        size: { w: 1280, h: 780 },
        sizeHint: "wide",
        desc: "Markdown 双模式编辑器与发布设置",
        render: renderEditor,
        mount: bindEditor
    });

    DpzOS.registerApp({
        id: "markdown-preview-app",
        name: "预览",
        en: "Preview",
        icon: "eye",
        tone: "info",
        group: "内容创作",
        size: { w: 780, h: 640 },
        desc: "Markdown 渲染预览",
        render: function (ctx) {
            var title = (ctx.params && ctx.params.title) || "预览";
            return (
                '<div class="ui-page ui-page--narrow">' +
                '<div class="ui-head"><div class="ui-head__text">' +
                '<div class="ui-eyebrow">PREVIEW</div>' +
                '<h1 class="ui-title">' +
                util.esc(title) +
                "</h1>" +
                '<div class="ui-subtitle">Markdig + Prism 渲染结果</div>' +
                "</div></div>" +
                ui.panel({ body: ui.markdown(data.articleDetail.body) }) +
                "</div>"
            );
        }
    });
})(window.DpzOS);
