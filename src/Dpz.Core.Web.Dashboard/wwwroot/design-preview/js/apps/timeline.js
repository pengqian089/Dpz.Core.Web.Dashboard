(function (DpzOS) {
    "use strict";

    var ui = DpzOS.ui;
    var util = DpzOS.util;
    var icon = DpzOS.icon;
    var data = DpzOS.data;

    var PAGE_SIZE = 8;

    function renderList(ctx) {
        var state = ctx.state;
        state.page = state.page || 1;
        state.keyword = state.keyword || "";

        var rows = data.timelines
            .slice()
            .sort(function (a, b) {
                return b.date - a.date;
            })
            .filter(function (item) {
                return util.matches(state.keyword, [item.title, item.author, item.content]);
            });
        var totalPages = Math.max(1, Math.ceil(rows.length / PAGE_SIZE));
        state.page = util.clamp(state.page, 1, totalPages);
        var pageRows = rows.slice((state.page - 1) * PAGE_SIZE, state.page * PAGE_SIZE);

        return (
            '<div class="ui-page">' +
            '<div class="ui-head">' +
            '<div class="ui-head__text">' +
            '<div class="ui-eyebrow">内容创作 · TIMELINE</div>' +
            '<h1 class="ui-title">时间轴</h1>' +
            '<div class="ui-subtitle">共有 ' +
            data.timelines.length +
            " 个节点 · 节点按时间倒序排列，可关联外部链接</div>" +
            "</div>" +
            '<div class="ui-head__actions">' +
            ui.btn({ label: "发布节点", icon: "plus", variant: "primary", attrs: 'data-action="create"' }) +
            ui.btn({ label: "刷新", icon: "refresh", attrs: 'data-action="refresh"' }) +
            "</div>" +
            "</div>" +
            ui.toolbar([
                [
                    ui.search({
                        placeholder: "搜索标题或内容，支持拼音…",
                        value: state.keyword,
                        attrs: 'data-role="search-timeline"'
                    })
                ],
                [ui.chip("最早节点 " + util.formatDate(data.timelines[data.timelines.length - 1].date).slice(0, 10), { icon: "activity" })]
            ]) +
            ui.panel({
                flush: true,
                body: ui.table({
                    columns: [
                        { label: "时间节点" },
                        { label: "标题" },
                        { label: "链接" },
                        { label: "作者" },
                        { label: "最后更新" },
                        { label: "操作", align: "right" }
                    ],
                    rows: pageRows.map(function (item) {
                        return {
                            attrs: 'data-id="' + item.id + '"',
                            cells: [
                                '<div class="u-mono" style="color:var(--accent)">' +
                                    util.esc(util.formatDate(item.date).slice(0, 10)) +
                                    "</div>" +
                                    '<div class="ui-table__sub">' +
                                    util.esc(util.formatClock(item.date)) +
                                    "</div>",
                                '<div class="ui-table__main">' +
                                    util.esc(item.title) +
                                    "</div>" +
                                    '<div class="ui-table__sub u-clamp-2">' +
                                    util.esc(item.content.replace(/[#\n]/g, " ").slice(0, 46)) +
                                    "</div>",
                                item.more
                                    ? '<span class="u-mono u-ellipsis" style="display:inline-block;max-width:200px">' +
                                      util.esc(item.more) +
                                      "</span>"
                                    : '<span class="u-dim">无</span>',
                                util.esc(item.author),
                                '<span class="u-mono u-muted">' +
                                    util.esc(util.relativeTime(item.updatedAt)) +
                                    "</span>",
                                '<div class="ui-table__actions">' +
                                    ui.iconBtn("eye", { label: "查看内容", attrs: 'data-action="view" data-id="' + item.id + '"' }) +
                                    ui.iconBtn("edit", { label: "编辑", attrs: 'data-action="edit" data-id="' + item.id + '"' }) +
                                    ui.iconBtn("trash", { label: "删除", danger: true, attrs: 'data-action="delete" data-id="' + item.id + '"' }) +
                                    "</div>"
                            ]
                        };
                    })
                })
            }) +
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
            var id = action.getAttribute("data-id");
            var item = data.timelines.find(function (entry) {
                return entry.id === id;
            });

            if (kind === "create") {
                ctx.open("timeline-editor", { mode: "create" });
            }
            if (kind === "refresh") {
                ctx.rerender();
                DpzOS.toast({ title: "时间轴已刷新", tone: "success", icon: "refresh" });
            }
            if (kind === "view" && item) {
                ctx.dialog({
                    title: item.title,
                    subtitle: util.formatDate(item.date) + " · " + item.author,
                    icon: "activity",
                    confirmText: "关闭",
                    cancelText: null,
                    html: ui.markdown(item.content)
                });
            }
            if (kind === "edit" && item) {
                ctx.open("timeline-editor", { id: item.id });
            }
            if (kind === "delete" && item) {
                ctx.confirm({
                    title: "删除这个时间节点？",
                    text: item.title,
                    confirmText: "删除",
                    tone: "danger",
                    icon: "trash"
                }).then(function (ok) {
                    if (ok) {
                        data.timelines = data.timelines.filter(function (entry) {
                            return entry.id !== id;
                        });
                        ctx.rerender();
                        DpzOS.toast({ title: "节点已删除", tone: "success", icon: "check-circle" });
                    }
                });
            }
        });

        var search = rootNode.querySelector('[data-role="search-timeline"]');
        if (search) {
            search.addEventListener("keydown", function (event) {
                if (event.key === "Enter") {
                    state.keyword = search.value.trim();
                    state.page = 1;
                    ctx.rerender();
                }
            });
        }
    }

    DpzOS.registerApp({
        id: "timeline",
        name: "时间轴",
        en: "Timeline",
        icon: "activity",
        tone: "violet",
        group: "内容创作",
        size: { w: 1120, h: 700 },
        singleton: true,
        desc: "时间节点、外链与 Markdown 内容",
        render: renderList,
        mount: bindList
    });

    function renderEditor(ctx) {
        var state = ctx.state;
        var item = null;
        if (ctx.params && ctx.params.id) {
            item = data.timelines.find(function (entry) {
                return entry.id === ctx.params.id;
            });
        }
        state.draft = state.draft || {
            title: item ? item.title : "",
            more: item ? item.more : "",
            date: item
                ? item.date.toISOString().slice(0, 10)
                : new Date().toISOString().slice(0, 10),
            content: item ? item.content : ""
        };
        var draft = state.draft;

        return (
            '<div class="ui-page ui-page--narrow">' +
            '<div class="ui-head">' +
            '<div class="ui-head__text">' +
            '<div class="ui-eyebrow">内容创作 · TIMELINE / EDIT</div>' +
            '<h1 class="ui-title">' +
            (item ? "编辑时间节点" : "发布时间节点") +
            "</h1>" +
            '<div class="ui-subtitle">节点会出现在前台时间轴页面</div>' +
            "</div>" +
            '<div class="ui-head__actions">' +
            ui.btn({ label: "返回列表", icon: "arrow-left", attrs: 'data-action="back"' }) +
            ui.btn({ label: "预览", icon: "eye", attrs: 'data-action="preview"' }) +
            ui.btn({ label: "保存", icon: "check", variant: "primary", attrs: 'data-action="save"' }) +
            "</div>" +
            "</div>" +
            ui.panel({
                title: "节点信息",
                icon: "activity",
                body:
                    '<div class="ui-grid ui-grid--2">' +
                    ui.field({
                        label: "标题",
                        required: true,
                        control:
                            '<input class="ui-input" data-role="tl-title" value="' +
                            util.esc(draft.title) +
                            '" placeholder="例如：发布赛博桌面后台 2.7.0">'
                    }) +
                    ui.field({
                        label: "时间节点",
                        required: true,
                        control:
                            '<input class="ui-input u-mono" type="date" data-role="tl-date" value="' +
                            util.esc(draft.date) +
                            '">'
                    }) +
                    "</div>" +
                    ui.field({
                        label: "链接 More",
                        hint: "可留空；一般为文章详情页相对路径",
                        control:
                            '<input class="ui-input u-mono" data-role="tl-more" value="' +
                            util.esc(draft.more) +
                            '" placeholder="/article/read/xxx.html">'
                    })
            }) +
            ui.panel({
                title: "内容",
                icon: "edit",
                sub: "Markdown",
                body:
                    '<textarea class="ui-textarea u-mono" style="min-height:260px" data-role="tl-content" placeholder="记录这个节点…">' +
                    util.esc(draft.content) +
                    "</textarea>" +
                    '<div class="ui-panel__foot" style="margin:12px -15px -15px">' +
                    '<span class="ui-meta">支持 Markdown · 图片上传走 /api/Timeline/upload</span>' +
                    '<div class="ui-row ui-row--tight">' +
                    ui.btn({ label: "保存并返回", icon: "check", variant: "primary", size: "sm", attrs: 'data-action="save"' }) +
                    ui.btn({ label: "取消", icon: "x", size: "sm", variant: "ghost", attrs: 'data-action="back"' }) +
                    "</div>" +
                    "</div>"
            }) +
            "</div>"
        );
    }

    function bindEditor(rootNode, ctx) {
        var state = ctx.state;
        var draft = state.draft;

        ["title", "date", "more", "content"].forEach(function (key) {
            var input = rootNode.querySelector('[data-role="tl-' + key + '"]');
            if (input) {
                input.addEventListener("input", function () {
                    draft[key] = input.value;
                });
            }
        });

        rootNode.addEventListener("click", function (event) {
            var action = event.target.closest("[data-action]");
            if (!action) {
                return;
            }
            var kind = action.getAttribute("data-action");
            if (kind === "back") {
                ctx.open("timeline");
                ctx.close();
            }
            if (kind === "preview") {
                ctx.dialog({
                    title: draft.title || "未命名节点",
                    subtitle: draft.date,
                    icon: "eye",
                    confirmText: "关闭",
                    cancelText: null,
                    html: ui.markdown(draft.content || "*还没有内容*")
                });
            }
            if (kind === "save") {
                if (!draft.title.trim() || !draft.date) {
                    DpzOS.toast({ title: "标题与时间节点为必填项", tone: "danger", icon: "alert" });
                    return;
                }
                var date = new Date(draft.date + "T09:00:00");
                var target = ctx.params && ctx.params.id
                    ? data.timelines.find(function (entry) {
                          return entry.id === ctx.params.id;
                      })
                    : null;
                if (target) {
                    target.title = draft.title.trim();
                    target.date = date;
                    target.more = draft.more.trim();
                    target.content = draft.content;
                    target.updatedAt = new Date();
                } else {
                    data.timelines.unshift({
                        id: util.uid("t"),
                        title: draft.title.trim(),
                        date: date,
                        more: draft.more.trim(),
                        author: data.profile.name,
                        content: draft.content,
                        createdAt: new Date(),
                        updatedAt: new Date()
                    });
                }
                DpzOS.toast({ title: target ? "节点已更新" : "节点已发布", tone: "success", icon: "check-circle" });
                ctx.open("timeline");
                ctx.close();
            }
        });
    }

    DpzOS.registerApp({
        id: "timeline-editor",
        name: "时间轴编辑",
        en: "Timeline editor",
        icon: "edit",
        tone: "violet",
        group: "内容创作",
        size: { w: 1040, h: 720 },
        desc: "节点信息、日期与 Markdown 正文",
        render: renderEditor,
        mount: bindEditor
    });
})(window.DpzOS);
