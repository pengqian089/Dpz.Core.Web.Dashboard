(function (DpzOS) {
    "use strict";

    var ui = DpzOS.ui;
    var util = DpzOS.util;
    var icon = DpzOS.icon;
    var data = DpzOS.data;

    var PAGE_SIZE = 6;

    function filteredComments(state) {
        return data.comments.filter(function (comment) {
            if (state.node && state.node !== "全部" && comment.node !== state.node) {
                return false;
            }
            if (state.relation && state.relation !== "全部" && comment.relation !== state.relation) {
                return false;
            }
            if (state.keyword) {
                var haystack = [comment.name, comment.content, comment.relation].join(" ");
                if (!util.matches(state.keyword, [haystack])) {
                    return false;
                }
            }
            return true;
        });
    }

    function relationOptions(node) {
        var values = {};
        data.comments.forEach(function (comment) {
            if (!node || node === "全部" || comment.node === node) {
                values[comment.relation] = true;
            }
        });
        return ["全部"].concat(Object.keys(values));
    }

    function renderComment(comment) {
        return (
            '<article class="app-comment' +
            (comment.deleted ? " app-comment--deleted" : "") +
            '" data-id="' +
            comment.id +
            '">' +
            ui.avatar(comment.name, "lg", comment.email || comment.name) +
            '<div class="app-comment__body">' +
            '<div class="app-comment__head">' +
            '<span class="app-comment__name">' +
            util.esc(comment.name) +
            "</span>" +
            (comment.anonymous
                ? ui.badge("匿名", "warning", "eye-off")
                : ui.badge(comment.identity, "accent", "user")) +
            (comment.deleted ? ui.badge("已删除", "danger", "trash") : "") +
            '<span class="u-grow"></span>' +
            '<span class="u-mono u-dim" style="font-size:11px">' +
            util.esc(util.relativeTime(comment.time)) +
            "</span>" +
            "</div>" +
            '<div class="app-comment__text">' +
            util.esc(comment.content) +
            "</div>" +
            '<div class="app-comment__foot">' +
            "<span>" +
            icon("link") +
            " " +
            util.esc(comment.node) +
            " · " +
            util.esc(comment.relation) +
            "</span>" +
            (comment.site ? "<span>" + icon("globe") + " " + util.esc(comment.site) + "</span>" : "") +
            '<span class="u-grow"></span>' +
            '<button type="button" class="ui-btn ui-btn--sm ui-btn--ghost" data-action="view">' +
            icon("eye") +
            "查看</button>" +
            '<button type="button" class="ui-btn ui-btn--sm ui-btn--danger" data-action="delete">' +
            icon("trash") +
            "删除</button>" +
            "</div>" +
            "</div>" +
            "</article>"
        );
    }

    function renderComments(ctx) {
        var state = ctx.state;
        state.page = state.page || 1;
        state.node = state.node || "全部";
        state.relation = state.relation || "全部";
        state.keyword = state.keyword || "";

        var rows = filteredComments(state);
        var totalPages = Math.max(1, Math.ceil(rows.length / PAGE_SIZE));
        state.page = util.clamp(state.page, 1, totalPages);
        var pageRows = rows.slice((state.page - 1) * PAGE_SIZE, state.page * PAGE_SIZE);

        var counts = {};
        data.comments.forEach(function (comment) {
            counts[comment.node] = (counts[comment.node] || 0) + 1;
        });

        var tabs = ui.tabs(
            data.commentNodes.map(function (node) {
                var count = node === "全部" ? data.comments.length : counts[node] || 0;
                return { id: node, label: node, count: count };
            }),
            state.node
        );

        return (
            '<div class="ui-page">' +
            '<div class="ui-head">' +
            '<div class="ui-head__text">' +
            '<div class="ui-eyebrow">互动管理 · COMMENTS</div>' +
            '<h1 class="ui-title">评论管理</h1>' +
            '<div class="ui-subtitle">按节点聚合评论，支持关联筛选、全文查看与删除</div>' +
            "</div>" +
            '<div class="ui-head__actions">' +
            ui.btn({ label: "刷新", icon: "refresh", attrs: 'data-action="refresh"' }) +
            "</div>" +
            "</div>" +
            tabs +
            ui.toolbar([
                [
                    ui.select({
                        options: relationOptions(state.node),
                        value: state.relation,
                        attrs: 'data-role="filter-relation"'
                    }),
                    ui.search({
                        placeholder: "搜索用户或内容，支持拼音…",
                        value: state.keyword,
                        attrs: 'data-role="search-comment"'
                    })
                ],
                [
                    ui.chip("匿名 " + data.comments.filter(function (c) { return c.anonymous; }).length, { icon: "eye-off" }),
                    ui.chip("已删除 " + data.comments.filter(function (c) { return c.deleted; }).length, { icon: "trash" })
                ]
            ]) +
            '<div class="ui-stack" data-role="comment-list">' +
            (pageRows.length
                ? pageRows.map(renderComment).join("")
                : ui.empty({ icon: "comments", title: "没有符合条件的评论" })) +
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

    function bindComments(rootNode, ctx) {
        var state = ctx.state;

        rootNode.addEventListener("click", function (event) {
            var tab = event.target.closest("[data-tab]");
            if (tab) {
                state.node = tab.getAttribute("data-tab");
                state.relation = "全部";
                state.page = 1;
                ctx.rerender();
                return;
            }
            var pageButton = event.target.closest("[data-page]");
            if (pageButton && !pageButton.disabled) {
                state.page = Number(pageButton.getAttribute("data-page"));
                ctx.rerender();
                return;
            }
            var action = event.target.closest("[data-action]");
            if (!action || action.getAttribute("data-action") === "refresh") {
                if (action) {
                    ctx.rerender();
                    DpzOS.toast({ title: "评论已刷新", tone: "success", icon: "refresh" });
                }
                return;
            }
            var card = action.closest("[data-id]");
            var comment = data.comments.find(function (item) {
                return item.id === (card ? card.getAttribute("data-id") : null);
            });
            if (!comment) {
                return;
            }
            var kind = action.getAttribute("data-action");
            if (kind === "view") {
                ctx.dialog({
                    title: comment.name + " 的评论",
                    subtitle: comment.node + " · " + comment.relation,
                    icon: "message-square",
                    confirmText: "关闭",
                    cancelText: null,
                    html:
                        '<div class="ui-stack">' +
                        '<div class="ui-md">' +
                        util.esc(comment.content) +
                        "</div>" +
                        '<div class="ui-meta">' +
                        util.esc(util.formatDate(comment.time) + " " + util.formatTime(comment.time)) +
                        " · " +
                        util.esc(comment.identity) +
                        "</div>" +
                        "</div>"
                });
            }
            if (kind === "delete") {
                ctx.confirm({
                    title: "删除这条评论？",
                    text: comment.name + "：" + comment.content.slice(0, 40) + "…",
                    confirmText: "删除",
                    tone: "danger",
                    icon: "trash"
                }).then(function (ok) {
                    if (ok) {
                        data.comments = data.comments.filter(function (item) {
                            return item.id !== comment.id;
                        });
                        ctx.rerender();
                        DpzOS.toast({ title: "评论已删除", tone: "success", icon: "check-circle" });
                    }
                });
            }
        });

        rootNode.addEventListener("change", function (event) {
            if (event.target.matches('[data-role="filter-relation"]')) {
                state.relation = event.target.value;
                state.page = 1;
                ctx.rerender();
            }
        });

        var search = rootNode.querySelector('[data-role="search-comment"]');
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
        id: "comments",
        name: "评论管理",
        en: "Comments",
        icon: "comments",
        tone: "success",
        group: "互动管理",
        size: { w: 1000, h: 720 },
        singleton: true,
        desc: "评论区审核、节点筛选与删除",
        render: renderComments,
        mount: bindComments
    });

    var PAGE_SIZE_DANMAKU = 9;

    function filteredDanmaku(state) {
        return data.danmaku.filter(function (item) {
            if (state.group && state.group !== "全部" && item.group !== state.group) {
                return false;
            }
            if (state.keyword && !util.matches(state.keyword, [item.text, item.group])) {
                return false;
            }
            return true;
        });
    }

    function renderDanmaku(ctx) {
        var state = ctx.state;
        state.page = state.page || 1;
        state.group = state.group || "全部";
        state.keyword = state.keyword || "";
        state.selected = state.selected || {};
        if (ctx.params && ctx.params.group) {
            state.keyword = String(ctx.params.group);
        }

        var rows = filteredDanmaku(state);
        var totalPages = Math.max(1, Math.ceil(rows.length / PAGE_SIZE_DANMAKU));
        state.page = util.clamp(state.page, 1, totalPages);
        var pageRows = rows.slice((state.page - 1) * PAGE_SIZE_DANMAKU, state.page * PAGE_SIZE_DANMAKU);
        var selectedCount = Object.keys(state.selected).filter(function (key) {
            return state.selected[key];
        }).length;

        return (
            '<div class="ui-page">' +
            '<div class="ui-head">' +
            '<div class="ui-head__text">' +
            '<div class="ui-eyebrow">互动管理 · DANMAKU</div>' +
            '<h1 class="ui-title">弹幕管理</h1>' +
            '<div class="ui-subtitle">共 ' +
            data.danmaku.length +
            " 条弹幕，支持 AcFun JSON / Bilibili XML 导入</div>" +
            "</div>" +
            '<div class="ui-head__actions">' +
            ui.btn({ label: "导入弹幕", icon: "upload", variant: "primary", attrs: 'data-action="import"' }) +
            ui.btn({ label: "刷新", icon: "refresh", attrs: 'data-action="refresh"' }) +
            "</div>" +
            "</div>" +
            ui.toolbar([
                [
                    ui.select({
                        options: data.danmakuGroups,
                        value: state.group,
                        attrs: 'data-role="filter-group"'
                    }),
                    ui.search({
                        placeholder: "搜索弹幕内容，支持拼音…",
                        value: state.keyword,
                        attrs: 'data-role="search-danmaku"'
                    })
                ],
                [
                    ui.badge("点击卡片可多选", null, "check-circle"),
                    ui.badge("已选 " + selectedCount, selectedCount ? "accent" : null, "layers")
                ]
            ]) +
            '<div class="app-danmaku__grid">' +
            (pageRows.length
                ? pageRows
                      .map(function (item) {
                          var selected = !!state.selected[item.id];
                          return (
                              '<article class="app-danmaku__card' +
                              (selected ? " is-selected" : "") +
                              '" data-id="' +
                              item.id +
                              '">' +
                              '<span class="ui-checkbadge' +
                              (selected ? " is-on" : "") +
                              '">' +
                              icon("check") +
                              "</span>" +
                              '<div class="app-danmaku__text" style="color:' +
                              util.esc(item.color) +
                              '">' +
                              util.highlight(item.text, state.keyword) +
                              "</div>" +
                              '<div class="app-danmaku__meta">' +
                              "<span>" +
                              icon("clock") +
                              " " +
                              item.time.toFixed(1) +
                              "s</span>" +
                              "<span>" +
                              icon("hash") +
                              " " +
                              util.esc(item.position) +
                              "</span>" +
                              "<span>" +
                              icon("heading") +
                              " " +
                              util.esc(item.size) +
                              "</span>" +
                              "</div>" +
                              '<div class="ui-meta">' +
                              icon("video") +
                              " " +
                              util.esc(item.group) +
                              "</div>" +
                              "</article>"
                          );
                      })
                      .join("")
                : ui.empty({ icon: "message-circle", title: "没有匹配的弹幕" })) +
            "</div>" +
            (selectedCount
                ? '<div class="app-danmaku__bar">' +
                  "<span>已选择 <b class=\"u-mono\">" +
                  selectedCount +
                  "</b> 条弹幕</span>" +
                  '<div class="ui-row ui-row--tight">' +
                  ui.btn({ label: "取消选择", size: "sm", variant: "ghost", attrs: 'data-action="clear-selection"' }) +
                  ui.btn({
                      label: "删除选中 (" + selectedCount + ")",
                      icon: "trash",
                      size: "sm",
                      variant: "danger",
                      attrs: 'data-action="delete-selected"'
                  }) +
                  "</div>" +
                  "</div>"
                : "") +
            ui.pager({
                page: state.page,
                totalPages: totalPages,
                pageSize: PAGE_SIZE_DANMAKU,
                count: rows.length
            }) +
            "</div>"
        );
    }

    function bindDanmaku(rootNode, ctx) {
        var state = ctx.state;

        rootNode.addEventListener("click", function (event) {
            var pageButton = event.target.closest("[data-page]");
            if (pageButton && !pageButton.disabled) {
                state.page = Number(pageButton.getAttribute("data-page"));
                ctx.rerender();
                return;
            }

            var action = event.target.closest("[data-action]");
            if (action) {
                var kind = action.getAttribute("data-action");
                if (kind === "import") {
                    openImport(ctx);
                    return;
                }
                if (kind === "refresh") {
                    state.selected = {};
                    ctx.rerender();
                    DpzOS.toast({ title: "弹幕已刷新", tone: "success", icon: "refresh" });
                    return;
                }
                if (kind === "clear-selection") {
                    state.selected = {};
                    ctx.rerender();
                    return;
                }
                if (kind === "delete-selected") {
                    var ids = Object.keys(state.selected).filter(function (key) {
                        return state.selected[key];
                    });
                    ctx.confirm({
                        title: "删除选中的 " + ids.length + " 条弹幕？",
                        text: "删除后无法恢复，需要重新导入。",
                        confirmText: "删除",
                        tone: "danger",
                        icon: "trash"
                    }).then(function (ok) {
                        if (!ok) {
                            return;
                        }
                        data.danmaku = data.danmaku.filter(function (item) {
                            return ids.indexOf(item.id) < 0;
                        });
                        state.selected = {};
                        ctx.rerender();
                        DpzOS.toast({
                            title: "已删除 " + ids.length + " 条弹幕",
                            tone: "success",
                            icon: "check-circle"
                        });
                    });
                    return;
                }
            }

            var card = event.target.closest(".app-danmaku__card");
            if (card) {
                var id = card.getAttribute("data-id");
                state.selected[id] = !state.selected[id];
                ctx.rerender();
            }
        });

        rootNode.addEventListener("change", function (event) {
            if (event.target.matches('[data-role="filter-group"]')) {
                state.group = event.target.value;
                state.page = 1;
                ctx.rerender();
            }
        });

        var search = rootNode.querySelector('[data-role="search-danmaku"]');
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

    function openImport(ctx) {
        ctx
            .dialog({
                title: "导入弹幕",
                subtitle: "支持 AcFun JSON 与 Bilibili XML，单文件最大 100MB",
                icon: "upload",
                confirmText: "开始导入",
                html:
                    '<div class="ui-stack">' +
                    ui.tabs(
                        [
                            { id: "bilibili", label: "Bilibili XML" },
                            { id: "acfun", label: "AcFun JSON" }
                        ],
                        "bilibili"
                    ) +
                    '<div class="ui-dropzone">' +
                    icon("upload") +
                    "<div>选择或拖入弹幕文件</div>" +
                    '<div class="ui-field__hint">导入后会自动匹配视频分组并刷新列表</div>' +
                    "</div>" +
                    ui.field({
                        label: "目标视频分组",
                        control: ui.select({ options: data.danmakuGroups.slice(1) })
                    }) +
                    ui.callout("正式版会显示上传进度与导入结果统计。", { icon: "info" }) +
                    "</div>"
            })
            .then(function (ok) {
                if (ok) {
                    DpzOS.toast({
                        title: "导入完成（演示）",
                        text: "新增 1280 条弹幕",
                        tone: "success",
                        icon: "upload"
                    });
                    ctx.notify({
                        tone: "success",
                        title: "弹幕导入完成",
                        text: "bilibili.xml 共 1280 条已入库。",
                        time: new Date(),
                        icon: "upload"
                    });
                }
            });
    }

    DpzOS.registerApp({
        id: "danmaku",
        name: "弹幕管理",
        en: "Danmaku",
        icon: "message-circle",
        tone: "magenta",
        group: "互动管理",
        size: { w: 1080, h: 700 },
        singleton: true,
        desc: "弹幕多选批量删除与导入",
        render: renderDanmaku,
        mount: bindDanmaku
    });
})(window.DpzOS);
