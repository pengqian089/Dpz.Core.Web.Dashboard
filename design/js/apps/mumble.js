(function (DpzOS) {
    "use strict";

    var ui = DpzOS.ui;
    var util = DpzOS.util;
    var icon = DpzOS.icon;
    var data = DpzOS.data;

    var PAGE_SIZE = 5;

    function renderList(ctx) {
        var state = ctx.state;
        state.page = state.page || 1;
        state.keyword = state.keyword || "";
        state.activeId = state.activeId || null;

        var rows = data.mumbles.filter(function (mumble) {
            return util.matches(state.keyword, [mumble.content, mumble.author]);
        });
        var totalPages = Math.max(1, Math.ceil(rows.length / PAGE_SIZE));
        state.page = util.clamp(state.page, 1, totalPages);
        var pageRows = rows.slice((state.page - 1) * PAGE_SIZE, state.page * PAGE_SIZE);

        return (
            '<div class="ui-page ui-page--narrow">' +
            '<div class="ui-head">' +
            '<div class="ui-head__text">' +
            '<div class="ui-eyebrow">内容创作 · MUMBLE</div>' +
            '<h1 class="ui-title">碎碎念</h1>' +
            '<div class="ui-subtitle">共 ' +
            data.mumbles.length +
            " 条 · 发布支持 Markdown 与 Gallery 图库模式</div>" +
            "</div>" +
            '<div class="ui-head__actions">' +
            ui.btn({ label: "发布碎碎念", icon: "plus", variant: "primary", attrs: 'data-action="create"' }) +
            ui.btn({ label: "刷新", icon: "refresh", attrs: 'data-action="refresh"' }) +
            "</div>" +
            "</div>" +
            ui.toolbar([
                [
                    ui.search({
                        placeholder: "搜索内容，支持拼音…",
                        value: state.keyword,
                        attrs: 'data-role="search-mumble"'
                    })
                ],
                [
                    ui.chip("带图 " + data.mumbles.filter(function (m) { return m.images > 0; }).length, { icon: "image" }),
                    ui.chip(
                        "今日 " +
                            data.mumbles.filter(function (m) {
                                return Date.now() - m.createdAt.getTime() < 86400000;
                            }).length,
                        { icon: "clock" }
                    )
                ]
            ]) +
            '<div class="ui-stack">' +
            (pageRows.length
                ? pageRows
                      .map(function (mumble) {
                          return (
                              '<article class="ui-card" data-id="' +
                              mumble.id +
                              '">' +
                              '<div class="ui-card__body">' +
                              '<div class="ui-row">' +
                              ui.avatar(mumble.author, null, mumble.author) +
                              "<div class=\"ui-grow\">" +
                              '<div class="ui-item__title">' +
                              util.esc(mumble.author) +
                              (Date.now() - mumble.updatedAt.getTime() > 60000
                                  ? ui.badge("已编辑", null, "edit")
                                  : "") +
                              "</div>" +
                              '<div class="ui-item__sub">' +
                              util.esc(util.relativeTime(mumble.createdAt)) +
                              "</div>" +
                              "</div>" +
                              '<div class="ui-item__side">' +
                              ui.chip(mumble.likes + " 赞", { icon: "star" }) +
                              ui.chip(mumble.comments + " 评", { icon: "message-circle" }) +
                              "</div>" +
                              "</div>" +
                              '<div class="ui-clamp-2" style="font-size:13px;line-height:1.7;white-space:pre-line">' +
                              util.esc(mumble.content) +
                              "</div>" +
                              (mumble.images
                                  ? '<div class="app-editor__gallery app-mumble__gallery">' +
                                    data.pictures
                                        .slice(0, mumble.images)
                                        .map(function (picture) {
                                            return ui.cover(picture.id, {
                                                cls: "ui-cover--square",
                                                center: icon("image")
                                            });
                                        })
                                        .join("") +
                                    "</div>"
                                  : "") +
                              '<div class="ui-row ui-row--tight">' +
                              ui.btn({
                                  label: "预览",
                                  icon: "eye",
                                  size: "sm",
                                  attrs: 'data-action="preview" data-id="' + mumble.id + '"'
                              }) +
                              ui.btn({
                                  label: "编辑",
                                  icon: "edit",
                                  size: "sm",
                                  attrs: 'data-action="edit" data-id="' + mumble.id + '"'
                              }) +
                              '<span class="u-grow"></span>' +
                              ui.btn({
                                  label: "删除",
                                  icon: "trash",
                                  size: "sm",
                                  variant: "danger",
                                  attrs: 'data-action="delete" data-id="' + mumble.id + '"'
                              }) +
                              "</div>" +
                              "</div>" +
                              "</article>"
                          );
                      })
                      .join("")
                : ui.empty({ icon: "message", title: "还没有碎碎念" })) +
            "</div>" +
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
            var mumble = data.mumbles.find(function (item) {
                return item.id === id;
            });

            if (kind === "create") {
                ctx.open("mumble-editor", { mode: "create" });
            }
            if (kind === "refresh") {
                ctx.rerender();
                DpzOS.toast({ title: "碎碎念已刷新", tone: "success", icon: "refresh" });
            }
            if (kind === "preview" && mumble) {
                ctx.dialog({
                    title: mumble.author + " 的碎碎念",
                    subtitle: util.formatDate(mumble.createdAt) + " " + util.formatTime(mumble.createdAt),
                    icon: "message",
                    confirmText: "关闭",
                    cancelText: null,
                    html: ui.markdown(mumble.content)
                });
            }
            if (kind === "edit" && mumble) {
                ctx.open("mumble-editor", { id: mumble.id });
            }
            if (kind === "delete" && mumble) {
                ctx.confirm({
                    title: "删除这条碎碎念？",
                    text: mumble.content.slice(0, 40) + "…",
                    confirmText: "删除",
                    tone: "danger",
                    icon: "trash"
                }).then(function (ok) {
                    if (ok) {
                        data.mumbles = data.mumbles.filter(function (item) {
                            return item.id !== id;
                        });
                        ctx.rerender();
                        DpzOS.toast({ title: "已删除", tone: "success", icon: "check-circle" });
                    }
                });
            }
        });

        var search = rootNode.querySelector('[data-role="search-mumble"]');
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
        id: "mumble",
        name: "碎碎念",
        en: "Mumble",
        icon: "message",
        tone: "success",
        group: "内容创作",
        size: { w: 1000, h: 720 },
        singleton: true,
        desc: "短内容时间流、图库模式与预览",
        render: renderList,
        mount: bindList
    });

    function renderEditor(ctx) {
        var state = ctx.state;
        var mumble = null;
        if (ctx.params && ctx.params.id) {
            mumble = data.mumbles.find(function (item) {
                return item.id === ctx.params.id;
            });
        }
        state.draft = state.draft || {
            content: mumble ? mumble.content : "",
            gallery: mumble ? mumble.images : 0,
            allowComment: true,
            savedAt: null
        };
        var draft = state.draft;

        return (
            '<div class="app-editor" style="height:100%">' +
            '<div class="app-editor__main">' +
            '<div class="app-editor__top">' +
            '<div class="ui-spread">' +
            '<div class="ui-row ui-row--tight">' +
            ui.btn({ label: "返回列表", icon: "arrow-left", size: "sm", variant: "ghost", attrs: 'data-action="back"' }) +
            ui.badge(mumble ? "编辑" : "新碎碎念", mumble ? "accent" : "success", mumble ? "edit" : "plus") +
            (draft.savedAt ? ui.badge("已暂存 " + draft.savedAt, null, "archive") : "") +
            "</div>" +
            '<div class="ui-row ui-row--tight">' +
            ui.btn({ label: "预览", icon: "eye", size: "sm", attrs: 'data-action="preview"' }) +
            ui.btn({ label: "暂存", icon: "archive", size: "sm", attrs: 'data-action="draft"' }) +
            ui.btn({ label: "发布", icon: "send", size: "sm", variant: "primary", attrs: 'data-action="publish"' }) +
            "</div>" +
            "</div>" +
            '<div class="ui-meta">' +
            ui.avatar(data.profile.name, "sm", data.profile.account) +
            "<span>" +
            util.esc(data.profile.name) +
            " · 将以你的身份发布</span>" +
            "</div>" +
            "</div>" +
            '<div class="app-editor__toolbar">' +
            ["bold", "italic", "quote", "list", "code", "link"].map(function (name) {
                return (
                    '<button type="button" class="app-editor__tbtn" data-format="' +
                    name +
                    '" title="' +
                    name +
                    '">' +
                    icon(name) +
                    "</button>"
                );
            }).join("") +
            '<span class="app-editor__tsep"></span>' +
            '<label class="app-editor__tbtn" title="插入图片" style="cursor:pointer">' +
            icon("image") +
            "</label>" +
            '<span class="ui-toolbar__grow"></span>' +
            ui.badge("Markdown · Gallery", "violet", "sparkles") +
            "</div>" +
            '<textarea class="app-editor__pane u-mono" style="border:0;resize:none;outline:none;background:transparent" data-role="mumble-input" placeholder="此刻在想什么…">' +
            util.esc(draft.content) +
            "</textarea>" +
            "</div>" +
            '<aside class="app-editor__side">' +
            '<div class="ui-section">' +
            '<div class="ui-section__head"><h3 class="ui-section__title">' +
            icon("image") +
            "图库（Gallery）</h3><span class=\"ui-section__line\"></span></div>" +
            '<div class="app-editor__gallery">' +
            data.pictures
                .slice(0, Math.max(3, draft.gallery))
                .map(function (picture) {
                    return ui.cover(picture.id, { cls: "ui-cover--square", center: icon("image") });
                })
                .join("") +
            "</div>" +
            '<button type="button" class="ui-dropzone" style="padding:18px" data-action="add-image">' +
            icon("upload") +
            "<div>拖入或点击上传</div>" +
            "</button>" +
            '<div class="ui-field__hint">Gallery 模式：图片只进入图库，发布时自动追加到正文末尾。</div>' +
            "</div>" +
            '<div class="ui-section">' +
            '<div class="ui-section__head"><h3 class="ui-section__title">' +
            icon("settings") +
            "发布设置</h3><span class=\"ui-section__line\"></span></div>" +
            '<label class="ui-switch"><input type="checkbox"' +
            (draft.allowComment ? " checked" : "") +
            " data-role=\"allow-comment\">允许评论</label>" +
            "</div>" +
            ui.callout("发布时会用 Markdig 把 Markdown 转成 HTML 一并提交，前台直接渲染。", {
                icon: "info"
            }) +
            "</aside>" +
            "</div>"
        );
    }

    function bindEditor(rootNode, ctx) {
        var state = ctx.state;
        var draft = state.draft;
        var textarea = rootNode.querySelector('[data-role="mumble-input"]');
        if (textarea) {
            textarea.addEventListener("input", function () {
                draft.content = textarea.value;
            });
        }
        var allow = rootNode.querySelector('[data-role="allow-comment"]');
        if (allow) {
            allow.addEventListener("change", function () {
                draft.allowComment = allow.checked;
            });
        }

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
            var action = event.target.closest("[data-action]");
            if (!action) {
                return;
            }
            var kind = action.getAttribute("data-action");
            if (kind === "back") {
                ctx.open("mumble");
                ctx.close();
            }
            if (kind === "add-image") {
                draft.gallery += 1;
                ctx.rerender();
                DpzOS.toast({ title: "已加入图库（演示）", tone: "success", icon: "image", timeout: 1600 });
            }
            if (kind === "preview") {
                ctx.dialog({
                    title: "预览",
                    subtitle: "Markdig 渲染结果",
                    icon: "eye",
                    confirmText: "关闭",
                    cancelText: null,
                    html: ui.markdown(draft.content || "*还没有内容*")
                });
            }
            if (kind === "draft") {
                draft.savedAt = util.formatClock(new Date());
                ctx.rerender();
                DpzOS.toast({ title: "已暂存到本地", tone: "success", icon: "archive" });
            }
            if (kind === "publish") {
                if (!draft.content.trim()) {
                    DpzOS.toast({ title: "内容不能为空", tone: "danger", icon: "alert" });
                    return;
                }
                var isEdit = ctx.params && ctx.params.id;
                if (isEdit) {
                    var target = data.mumbles.find(function (item) {
                        return item.id === ctx.params.id;
                    });
                    if (target) {
                        target.content = draft.content;
                        target.updatedAt = new Date();
                    }
                } else {
                    data.mumbles.unshift({
                        id: util.uid("m"),
                        author: data.profile.name,
                        content: draft.content,
                        images: Math.max(0, draft.gallery - 0),
                        likes: 0,
                        comments: 0,
                        createdAt: new Date(),
                        updatedAt: new Date()
                    });
                }
                ctx.notify({
                    tone: "success",
                    title: isEdit ? "碎碎念已更新" : "碎碎念已发布",
                    text: draft.content.slice(0, 40),
                    time: new Date(),
                    icon: "message"
                });
                ctx.open("mumble");
                ctx.close();
            }
        });
    }

    DpzOS.registerApp({
        id: "mumble-editor",
        name: "碎碎念编辑",
        en: "Mumble editor",
        icon: "edit",
        tone: "success",
        group: "内容创作",
        size: { w: 1160, h: 700 },
        desc: "Markdown 编辑与 Gallery 图库模式",
        render: renderEditor,
        mount: bindEditor
    });
})(window.DpzOS);
