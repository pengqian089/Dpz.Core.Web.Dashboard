(function (DpzOS) {
    "use strict";

    var ui = DpzOS.ui;
    var util = DpzOS.util;
    var icon = DpzOS.icon;
    var data = DpzOS.data;

    function render(ctx) {
        var state = ctx.state;
        state.keyword = state.keyword || "";
        state.page = state.page || 1;
        var PAGE_SIZE = 6;

        var rows = data.friends.filter(function (friend) {
            return util.matches(state.keyword, [friend.name, friend.url, friend.desc]);
        });
        var totalPages = Math.max(1, Math.ceil(rows.length / PAGE_SIZE));
        state.page = util.clamp(state.page, 1, totalPages);
        var pageRows = rows.slice((state.page - 1) * PAGE_SIZE, state.page * PAGE_SIZE);

        return (
            '<div class="ui-page">' +
            '<div class="ui-head">' +
            '<div class="ui-head__text">' +
            '<div class="ui-eyebrow">站点配置 · FRIENDS</div>' +
            '<h1 class="ui-title">友情链接</h1>' +
            '<div class="ui-subtitle">共 ' +
            data.friends.length +
            " 个友链 · 新增与编辑提供前台卡片实时预览</div>" +
            "</div>" +
            '<div class="ui-head__actions">' +
            ui.btn({ label: "新增友链", icon: "plus", variant: "primary", attrs: 'data-action="add"' }) +
            ui.btn({ label: "刷新", icon: "refresh", attrs: 'data-action="refresh"' }) +
            "</div>" +
            "</div>" +
            ui.toolbar([
                [
                    ui.search({
                        placeholder: "搜索名称、链接或描述，支持拼音…",
                        value: state.keyword,
                        attrs: 'data-role="search-friend"'
                    })
                ],
                [
                    ui.chip("共 " + data.friends.length + " 个", { icon: "link" }),
                    ui.chip(
                        "本月新增 " +
                            data.friends.filter(function (friend) {
                                return Date.now() - friend.createdAt.getTime() < 30 * 86400000;
                            }).length,
                        { icon: "trending" }
                    )
                ]
            ]) +
            '<div class="ui-grid ui-grid--2">' +
            (pageRows.length
                ? pageRows
                      .map(function (friend) {
                          return (
                              '<article class="app-friend" data-id="' +
                              friend.id +
                              '">' +
                              '<span class="app-friend__avatar" style="background-image:' +
                              util.gradient(friend.name) +
                              '">' +
                              util.esc(util.initials(friend.name)) +
                              "</span>" +
                              '<div class="ui-item__main">' +
                              '<div class="ui-item__title">' +
                              util.esc(friend.name) +
                              "</div>" +
                              '<div class="ui-item__sub u-ellipsis">' +
                              util.esc(friend.desc) +
                              "</div>" +
                              '<div class="ui-item__sub u-mono">' +
                              util.esc(friend.url) +
                              "</div>" +
                              "</div>" +
                              '<div class="ui-item__side">' +
                              ui.iconBtn("external", {
                                  label: "访问",
                                  attrs: 'data-action="visit" data-id="' + friend.id + '"'
                              }) +
                              ui.iconBtn("edit", {
                                  label: "编辑",
                                  attrs: 'data-action="edit" data-id="' + friend.id + '"'
                              }) +
                              ui.iconBtn("trash", {
                                  label: "删除",
                                  danger: true,
                                  attrs: 'data-action="delete" data-id="' + friend.id + '"'
                              }) +
                              "</div>" +
                              "</article>"
                          );
                      })
                      .join("")
                : ui.empty({
                      icon: "link",
                      title: "没有匹配的友链",
                      text: "换个关键词，或新增一个友链。"
                  })) +
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

    function openFriendForm(ctx, friend) {
        var draft = friend
            ? { name: friend.name, url: friend.url, desc: friend.desc }
            : { name: "", url: "", desc: "" };
        var promise = ctx.dialog({
            title: friend ? "编辑友链" : "新增友链",
            subtitle: "右侧实时预览前台卡片效果",
            icon: "link",
            confirmText: "保存",
            html:
                '<div class="ui-grid ui-grid--2" style="gap:16px">' +
                '<div class="ui-stack">' +
                ui.field({
                    label: "名称",
                    required: true,
                    control:
                        '<input class="ui-input" data-field="name" value="' +
                        util.esc(draft.name) +
                        '" placeholder="例如：Kaito 的实验室">'
                }) +
                ui.field({
                    label: "链接",
                    required: true,
                    control:
                        '<input class="ui-input u-mono" data-field="url" value="' +
                        util.esc(draft.url) +
                        '" placeholder="https://example.com">'
                }) +
                ui.field({
                    label: "图标地址",
                    required: true,
                    hint: "留空时前台使用名称首字兜底",
                    control:
                        '<input class="ui-input u-mono" data-field="icon" value="" placeholder="https://example.com/avatar.png">'
                }) +
                ui.field({
                    label: "描述",
                    control:
                        '<textarea class="ui-textarea" data-field="desc" placeholder="一句话介绍">' +
                        util.esc(draft.desc) +
                        "</textarea>"
                }) +
                "</div>" +
                '<div class="ui-stack">' +
                '<div class="ui-eyebrow">实时预览</div>' +
                '<div class="app-friend app-friend__preview">' +
                '<span class="app-friend__avatar" data-preview="avatar" style="background-image:' +
                util.gradient(draft.name || "新友链") +
                '">' +
                util.esc(util.initials(draft.name || "新")) +
                "</span>" +
                '<div class="ui-item__main">' +
                '<div class="ui-item__title" data-preview="name">' +
                util.esc(draft.name || "友链名称") +
                "</div>" +
                '<div class="ui-item__sub" data-preview="desc">' +
                util.esc(draft.desc || "一句话描述") +
                "</div>" +
                '<div class="ui-item__sub u-mono" data-preview="url">' +
                util.esc(draft.url || "https://example.com") +
                "</div>" +
                "</div>" +
                "</div>" +
                ui.callout("保存后前台导航页会立即更新，图标建议使用方形 PNG 或 ICO。", {
                    icon: "info"
                }) +
                "</div>" +
                "</div>"
        });

        var modal = document.querySelector("#modal-root .os-modal");
        if (modal) {
            var nameInput = modal.querySelector('[data-field="name"]');
            var urlInput = modal.querySelector('[data-field="url"]');
            var descInput = modal.querySelector('[data-field="desc"]');
            var iconInput = modal.querySelector('[data-field="icon"]');
            var previewName = modal.querySelector('[data-preview="name"]');
            var previewDesc = modal.querySelector('[data-preview="desc"]');
            var previewUrl = modal.querySelector('[data-preview="url"]');
            var previewAvatar = modal.querySelector('[data-preview="avatar"]');

            function syncPreview() {
                draft.name = nameInput.value;
                draft.url = urlInput.value;
                draft.desc = descInput.value;
                draft.icon = iconInput.value;
                previewName.textContent = draft.name || "友链名称";
                previewDesc.textContent = draft.desc || "一句话描述";
                previewUrl.textContent = draft.url || "https://example.com";
                previewAvatar.textContent = util.initials(draft.name || "新");
                previewAvatar.style.backgroundImage = util.gradient(draft.name || "新友链");
            }

            [nameInput, urlInput, descInput, iconInput].forEach(function (input) {
                input.addEventListener("input", syncPreview);
            });
        }

        promise.then(function (ok) {
            if (!ok) {
                return;
            }
            if (friend) {
                friend.name = draft.name || friend.name;
                friend.url = draft.url || friend.url;
                friend.desc = draft.desc;
                friend.updatedAt = new Date();
            } else {
                data.friends.unshift({
                    id: util.uid("f"),
                    name: draft.name || "新友链",
                    url: draft.url || "https://example.com",
                    icon: draft.icon,
                    desc: draft.desc,
                    createdAt: new Date(),
                    updatedAt: new Date()
                });
            }
            ctx.rerender();
            DpzOS.toast({
                title: friend ? "友链已更新" : "友链已新增",
                tone: "success",
                icon: "check-circle"
            });
        });
    }

    function bind(rootNode, ctx) {
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
            var friend = data.friends.find(function (item) {
                return item.id === id;
            });

            if (kind === "add") {
                openFriendForm(ctx, null);
            }
            if (kind === "refresh") {
                ctx.rerender();
                DpzOS.toast({ title: "友链已刷新", tone: "success", icon: "refresh" });
            }
            if (kind === "visit" && friend) {
                DpzOS.toast({ title: "访问 " + friend.name, text: friend.url, tone: "info", icon: "globe" });
            }
            if (kind === "edit" && friend) {
                openFriendForm(ctx, friend);
            }
            if (kind === "delete" && friend) {
                ctx.confirm({
                    title: "删除这个友链？",
                    text: friend.name + "（" + friend.url + "）删除后前台立即隐藏。",
                    confirmText: "删除",
                    tone: "danger",
                    icon: "trash"
                }).then(function (ok) {
                    if (ok) {
                        data.friends = data.friends.filter(function (item) {
                            return item.id !== id;
                        });
                        ctx.rerender();
                        DpzOS.toast({ title: "友链已删除", tone: "success", icon: "check-circle" });
                    }
                });
            }
        });

        var search = rootNode.querySelector('[data-role="search-friend"]');
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
        id: "friends",
        name: "友情链接",
        en: "Friends",
        icon: "link",
        tone: "info",
        group: "站点配置",
        size: { w: 1000, h: 680 },
        singleton: true,
        desc: "友链增删改与前台卡片预览",
        render: render,
        mount: bind
    });
})(window.DpzOS);
