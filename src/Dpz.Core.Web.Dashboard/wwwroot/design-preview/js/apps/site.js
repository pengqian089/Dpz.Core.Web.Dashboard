(function (DpzOS) {
    "use strict";

    var ui = DpzOS.ui;
    var util = DpzOS.util;
    var icon = DpzOS.icon;
    var data = DpzOS.data;

    function tabsFor(ctx, active) {
        return ui.tabs(
            [
                { id: "footer", label: "页脚内容", icon: "info" },
                { id: "robots", label: "Robots.txt", icon: "bot" },
                { id: "seo", label: "SEO 管理", icon: "search", count: data.seoPages.length },
                { id: "notification", label: "系统通知", icon: "bell" }
            ],
            active
        );
    }

    function pageHeader() {
        return (
            '<div class="ui-head">' +
            '<div class="ui-head__text">' +
            '<div class="ui-eyebrow">站点配置 · SITE</div>' +
            '<h1 class="ui-title">网站配置</h1>' +
            '<div class="ui-subtitle">页脚、抓取规则、SEO 与全站通知集中管理（友链见独立应用）</div>' +
            "</div>" +
            '<div class="ui-head__actions">' +
            ui.btn({ label: "打开友链应用", icon: "link", attrs: 'data-action="open-friends"' }) +
            ui.btn({ label: "前台预览", icon: "external", attrs: 'data-action="preview-site"' }) +
            "</div>" +
            "</div>"
        );
    }

    function renderFooter(ctx) {
        var state = ctx.state;
        state.footerEditing = state.footerEditing || false;
        state.footerDraft = state.footerDraft || data.footerContent;
        return (
            ui.toolbar([
                [
                    state.footerEditing
                        ? ui.btn({ label: "取消", icon: "x", attrs: 'data-action="cancel-footer"' }) +
                          ui.btn({
                              label: "保存页脚",
                              icon: "check",
                              variant: "primary",
                              attrs: 'data-action="save-footer"'
                          })
                        : ui.btn({ label: "编辑内容", icon: "edit", variant: "primary", attrs: 'data-action="edit-footer"' }) +
                          ui.btn({ label: "重新加载", icon: "refresh", attrs: 'data-action="reload-footer"' })
                ],
                [ui.badge("CodeMirror · HTML", "violet", "code")]
            ]) +
            ui.callout("页脚会渲染在每篇文章底部，建议保持简洁：版权、运行时间与备案号。", {
                icon: "info"
            }) +
            (state.footerEditing
                ? '<div class="ui-panel"><div class="ui-panel__body">' +
                  '<div style="display:flex;gap:12px">' +
                  '<pre class="u-mono u-dim" style="margin:0;line-height:1.7;font-size:12px;text-align:right">' +
                  state.footerDraft
                      .split("\n")
                      .map(function (line, index) {
                          return util.pad(index + 1);
                      })
                      .join("<br>") +
                  "</pre>" +
                  '<textarea class="ui-textarea u-mono" style="min-height:320px;flex:1" data-role="footer-editor">' +
                  util.esc(state.footerDraft) +
                  "</textarea>" +
                  "</div></div></div>"
                : ui.panel({
                      title: "当前页脚（渲染预览）",
                      icon: "info",
                      actions: ui.badge("HTML", "accent", "code"),
                      body:
                          '<div class="ui-md">' +
                          '<p class="u-center u-muted">© 2024-2026 叫我阿胖 · Powered by <a href="#">Dpz.Core</a></p>' +
                          '<p class="u-center u-muted" style="font-size:12px">本站已运行 <span class="u-mono">1024</span> 天 · 备案号 京ICP备00000000号</p>' +
                          "</div>" +
                          ui.code(data.footerContent)
                  }))
        );
    }

    function renderRobots(ctx) {
        var state = ctx.state;
        state.robotsMode = state.robotsMode || "form";
        var robots = data.robots;

        var text =
            "# Generated at " +
            util.formatDate(new Date()) +
            " " +
            util.formatClock(new Date()) +
            "\n\n" +
            robots.groups
                .map(function (group) {
                    return (
                        "User-agent: " +
                        group.agent +
                        "\n" +
                        group.rules
                            .map(function (rule) {
                                return rule.type + ": " + rule.value;
                            })
                            .join("\n") +
                        (group.crawlDelay ? "\nCrawl-delay: " + group.crawlDelay : "")
                    );
                })
                .join("\n\n") +
            "\n\n" +
            robots.sitemaps
                .map(function (map) {
                    return "Sitemap: " + map;
                })
                .join("\n");

        return (
            ui.toolbar([
                [
                    '<div class="ui-seg">' +
                        '<button type="button" class="ui-seg__btn' +
                        (state.robotsMode === "form" ? " is-active" : "") +
                        '" data-robots-mode="form">' +
                        icon("settings") +
                        "表单模式</button>" +
                        '<button type="button" class="ui-seg__btn' +
                        (state.robotsMode === "text" ? " is-active" : "") +
                        '" data-robots-mode="text">' +
                        icon("code") +
                        "文本模式</button>" +
                        "</div>"
                ],
                [
                    ui.btn({ label: "重新加载", icon: "refresh", attrs: 'data-action="reload-robots"' }),
                    ui.btn({ label: "保存", icon: "check", variant: "primary", attrs: 'data-action="save-robots"' })
                ]
            ]) +
            '<div class="app-robots">' +
            (state.robotsMode === "form"
                ? '<div class="ui-stack">' +
                  robots.groups
                      .map(function (group, groupIndex) {
                          return (
                              '<div class="app-robots__group" data-group="' + groupIndex + '">' +
                              '<div class="ui-spread">' +
                              '<span class="app-robots__key">User-agent</span>' +
                              '<input class="ui-input" value="' + util.esc(group.agent) + '">' +
                              ui.iconBtn("trash", { label: "删除规则组", danger: true, attrs: 'data-action="remove-group"' }) +
                              "</div>" +
                              group.rules
                                  .map(function (rule) {
                                      return (
                                          '<div class="app-robots__line">' +
                                          '<span class="app-robots__key">' +
                                          util.esc(rule.type) +
                                          "</span>" +
                                          '<input class="ui-input u-mono" value="' +
                                          util.esc(rule.value) +
                                          '">' +
                                          "</div>"
                                      );
                                  })
                                  .join("") +
                              '<div class="app-robots__line">' +
                              '<span class="app-robots__key">Crawl-delay</span>' +
                              '<input class="ui-input u-mono" value="' +
                              util.esc(group.crawlDelay) +
                              '" placeholder="留空表示不设置">' +
                              "</div>" +
                              "</div>"
                          );
                      })
                      .join("") +
                  '<div class="ui-row">' +
                  ui.btn({ label: "添加规则组", icon: "plus", size: "sm", attrs: 'data-action="add-group"' }) +
                  ui.btn({ label: "添加 Sitemap", icon: "plus", size: "sm", attrs: 'data-action="add-sitemap"' }) +
                  "</div>" +
                  '<div class="app-robots__group">' +
                  '<div class="app-robots__key" style="width:auto">Sitemap</div>' +
                  robots.sitemaps
                      .map(function (map) {
                          return (
                              '<div class="app-robots__line">' +
                              '<input class="ui-input u-mono" value="' + util.esc(map) + '">' +
                              ui.iconBtn("trash", { label: "删除", danger: true, attrs: 'data-action="remove-sitemap"' }) +
                              "</div>"
                          );
                      })
                      .join("") +
                  "</div>" +
                  "</div>"
                : ui.panel({
                      title: "文本模式",
                      icon: "code",
                      flush: true,
                      body: '<textarea class="ui-textarea u-mono" style="min-height:480px;border:0;border-radius:0">' + util.esc(text) + "</textarea>"
                  })) +
            ui.panel({
                title: "实时预览",
                icon: "eye",
                actions: ui.badge("只读", null, "lock"),
                body: ui.code(text)
            }) +
            "</div>"
        );
    }

    function renderSeo(ctx) {
        var state = ctx.state;
        state.seoKeyword = state.seoKeyword || "";
        var pages = data.seoPages.filter(function (page) {
            return util.matches(state.seoKeyword, [page.route, page.title, page.desc]);
        });
        return (
            ui.toolbar([
                [
                    ui.search({
                        placeholder: "搜索路由或标题…",
                        value: state.seoKeyword,
                        attrs: 'data-role="search-seo"'
                    }),
                    ui.btn({ label: "新增页面 SEO", icon: "plus", variant: "primary", attrs: 'data-action="add-seo"' })
                ],
                [
                    ui.btn({ label: "刷新缓存", icon: "refresh", attrs: 'data-action="refresh-seo-cache"' }),
                    ui.chip("公共基底已配置", { icon: "check-circle" })
                ]
            ]) +
            ui.panel({
                flush: true,
                body: ui.table({
                    columns: [
                        { label: "路由" },
                        { label: "参数" },
                        { label: "继承" },
                        { label: "页面标题" },
                        { label: "关键词" },
                        { label: "Meta" },
                        { label: "操作", align: "right" }
                    ],
                    rows: pages.map(function (page) {
                        return {
                            attrs: 'data-id="' + page.id + '"',
                            cells: [
                                '<div class="ui-table__main u-mono">' +
                                    util.esc(page.route) +
                                    "</div>" +
                                    '<div class="ui-table__sub">' +
                                    page.methods
                                        .map(function (method) {
                                            return ui.badge(method, method === "POST" ? "warning" : "success");
                                        })
                                        .join(" ") +
                                    "</div>",
                                '<span class="u-mono u-muted">' + util.esc(page.param) + "</span>",
                                page.inherit
                                    ? ui.badge("继承公共", "accent", "check")
                                    : ui.badge("不继承", "warning", "x"),
                                '<span class="u-ellipsis" style="display:inline-block;max-width:260px">' +
                                    util.esc(page.title) +
                                    "</span>",
                                page.keywords.length
                                    ? page.keywords
                                          .slice(0, 3)
                                          .map(function (keyword) {
                                              return '<span class="app-article__tag">' + util.esc(keyword) + "</span>";
                                          })
                                          .join(" ")
                                    : '<span class="u-dim">未设置</span>',
                                '<span class="u-mono">' + page.metas + "</span>",
                                '<div class="ui-table__actions">' +
                                    ui.iconBtn("eye", { label: "查看", attrs: 'data-action="view-seo" data-id="' + page.id + '"' }) +
                                    ui.iconBtn("edit", { label: "编辑", attrs: 'data-action="edit-seo" data-id="' + page.id + '"' }) +
                                    ui.iconBtn("trash", { label: "删除", danger: true, attrs: 'data-action="delete-seo" data-id="' + page.id + '"' }) +
                                    "</div>"
                            ]
                        };
                    })
                })
            })
        );
    }

    function renderNotification(ctx) {
        var state = ctx.state;
        state.noticeText = state.noticeText || "";
        var max = 500;
        var ratio = Math.round((state.noticeText.length / max) * 100);
        return (
            '<div class="ui-grid ui-grid--2">' +
            ui.panel({
                title: "编写系统通知",
                icon: "send",
                sub: "通过 SignalR 广播到所有在线访客",
                body:
                    ui.field({
                        label: "通知内容（" + state.noticeText.length + " / " + max + "）",
                        control:
                            '<textarea class="ui-textarea" style="min-height:140px" maxlength="' +
                            max +
                            '" data-role="notice-text" placeholder="例如：站点将于今晚 23:00 进行维护…">' +
                            util.esc(state.noticeText) +
                            "</textarea>"
                    }) +
                    ui.progress(ratio, ratio > 90 ? "warning" : null) +
                    '<div class="ui-row" style="margin-top:12px">' +
                    ui.btn({ label: "维护公告", size: "sm", attrs: 'data-action="tpl-maintenance"' }) +
                    ui.btn({ label: "服务恢复", size: "sm", attrs: 'data-action="tpl-restore"' }) +
                    ui.btn({ label: "功能提醒", size: "sm", attrs: 'data-action="tpl-feature"' }) +
                    '<span class="u-grow"></span>' +
                    ui.btn({ label: "清空", size: "sm", variant: "ghost", attrs: 'data-action="clear-notice"' }) +
                    ui.btn({ label: "发送通知", icon: "send", size: "sm", variant: "primary", attrs: 'data-action="send-notice"' }) +
                    "</div>" +
                    ui.callout(
                        "前台地址：<b>" + util.esc(data.profile.server) + "</b> · Hub：<b>" + util.esc(data.profile.server) + "/notification</b>",
                        { tone: "accent", icon: "wifi" }
                    )
            }) +
            ui.panel({
                title: "最近发送",
                icon: "clock",
                sub: "保留最近 30 天",
                flush: true,
                body:
                    '<div class="ui-list" style="padding:14px">' +
                    data.notifications
                        .slice(0, 4)
                        .map(function (notice) {
                            return (
                                '<div class="os-notice" data-tone="' +
                                util.esc(notice.tone) +
                                '">' +
                                '<span class="os-notice__icon">' +
                                icon(notice.icon) +
                                "</span>" +
                                "<div>" +
                                '<div class="os-notice__title">' +
                                util.esc(notice.title) +
                                "</div>" +
                                '<div class="os-notice__text">' +
                                util.esc(notice.text) +
                                "</div>" +
                                '<div class="os-notice__time">' +
                                util.esc(util.relativeTime(notice.time)) +
                                "</div>" +
                                "</div>" +
                                '<span class="u-grow"></span>' +
                                ui.iconBtn("trash", { label: "删除", danger: true, attrs: 'data-action="delete-notice"' }) +
                                "</div>"
                            );
                        })
                        .join("") +
                    "</div>"
            }) +
            "</div>"
        );
    }

    function render(ctx) {
        var state = ctx.state;
        state.tab = state.tab || (ctx.params && ctx.params.tab) || "footer";
        var body = "";
        if (state.tab === "footer") {
            body = renderFooter(ctx);
        }
        if (state.tab === "robots") {
            body = renderRobots(ctx);
        }
        if (state.tab === "seo") {
            body = renderSeo(ctx);
        }
        if (state.tab === "notification") {
            body = renderNotification(ctx);
        }
        return (
            '<div class="ui-page">' +
            pageHeader() +
            tabsFor(ctx, state.tab) +
            body +
            "</div>"
        );
    }

    function seoForm(ctx, page) {
        var draft = page || {
            route: "",
            param: "未限定参数",
            inherit: true,
            title: "",
            keywords: [],
            desc: "",
            metas: 0
        };
        return ctx
            .dialog({
                title: page ? "编辑页面 SEO" : "新增页面 SEO",
                subtitle: "从路由目录选择目标，再配置元数据",
                icon: "search",
                confirmText: "保存",
                html:
                    '<div class="ui-stack">' +
                    ui.field({
                        label: "目标路由",
                        control: ui.select({
                            options: data.routeCatalog.map(function (route) {
                                return { value: route.key, label: route.key + "  " + route.template };
                            }),
                            value: draft.route
                        })
                    }) +
                    ui.field({
                        label: "路由参数",
                        hint: "Route 参数会参与 URL 拼装，Query 参数只影响匹配",
                        control:
                            '<div class="ui-row ui-row--tight">' +
                            (draft.param
                                ? ui.chip(draft.param, { icon: "hash", tone: "accent" })
                                : "") +
                            ui.chip("未限定参数", { icon: "at-sign" }) +
                            "</div>"
                    }) +
                    '<label class="ui-switch"><input type="checkbox"' +
                    (draft.inherit ? " checked" : "") +
                    ">继承公共 SEO 元数据</label>" +
                    ui.field({
                        label: "页面标题",
                        hint: "最多 100 字",
                        control: '<input class="ui-input" maxlength="100" value="' + util.esc(draft.title) + '">'
                    }) +
                    ui.field({
                        label: "关键词",
                        control:
                            '<div class="ui-input" style="display:flex;align-items:center;gap:6px;height:auto;min-height:var(--ctl-h);flex-wrap:wrap;padding:6px 8px">' +
                            draft.keywords
                                .map(function (keyword) {
                                    return ui.chip(keyword, { removable: true, tone: "accent" });
                                })
                                .join("") +
                            '<input style="border:0;background:transparent;flex:1;min-width:80px;outline:none" placeholder="回车添加…">' +
                            "</div>"
                    }) +
                    ui.field({
                        label: "描述",
                        hint: "最多 200 字",
                        control: '<textarea class="ui-textarea" maxlength="200">' + util.esc(draft.desc) + "</textarea>"
                    }) +
                    ui.callout("合并预览：保存前可调用 /api/Seo/preview 查看最终合并结果与预览 URL。", {
                        tone: "accent",
                        icon: "eye"
                    }) +
                    "</div>"
            })
            .then(function (ok) {
                if (ok) {
                    DpzOS.toast({ title: "SEO 配置已保存（演示）", tone: "success", icon: "check-circle" });
                }
            });
    }

    function bind(rootNode, ctx) {
        var state = ctx.state;

        rootNode.addEventListener("click", function (event) {
            var tab = event.target.closest("[data-tab]");
            if (tab) {
                state.tab = tab.getAttribute("data-tab");
                ctx.rerender();
                return;
            }
            var mode = event.target.closest("[data-robots-mode]");
            if (mode) {
                state.robotsMode = mode.getAttribute("data-robots-mode");
                ctx.rerender();
                return;
            }
            var action = event.target.closest("[data-action]");
            if (!action) {
                return;
            }
            var kind = action.getAttribute("data-action");
            var id = action.getAttribute("data-id");

            if (kind === "preview-site") {
                DpzOS.toast({
                    title: "已打开前台",
                    text: data.profile.server,
                    tone: "info",
                    icon: "external"
                });
            }
            if (kind === "open-friends") {
                ctx.open("friends");
            }
            if (kind === "edit-footer") {
                state.footerEditing = true;
                ctx.rerender();
            }
            if (kind === "cancel-footer") {
                state.footerEditing = false;
                ctx.rerender();
            }
            if (kind === "reload-footer" || kind === "cancel-footer") {
                state.footerDraft = data.footerContent;
                ctx.rerender();
            }
            if (kind === "save-footer") {
                data.footerContent = state.footerDraft;
                state.footerEditing = false;
                DpzOS.toast({ title: "页脚已保存", tone: "success", icon: "check-circle" });
                ctx.rerender();
            }
            if (kind === "add-group") {
                data.robots.groups.push({
                    agent: "*",
                    rules: [{ type: "Disallow", value: "/" }],
                    crawlDelay: ""
                });
                ctx.rerender();
            }
            if (kind === "remove-group") {
                var groupNode = action.closest("[data-group]");
                if (groupNode) {
                    data.robots.groups.splice(Number(groupNode.getAttribute("data-group")), 1);
                    ctx.rerender();
                }
            }
            if (kind === "add-sitemap") {
                data.robots.sitemaps.push("https://dpangzi.com/sitemap.xml");
                ctx.rerender();
            }
            if (kind === "remove-sitemap") {
                var line = action.closest(".app-robots__line");
                if (line) {
                    var input = line.querySelector("input");
                    data.robots.sitemaps = data.robots.sitemaps.filter(function (map) {
                        return map !== input.value;
                    });
                    ctx.rerender();
                }
            }
            if (kind === "reload-robots") {
                ctx.rerender();
                DpzOS.toast({ title: "已重新加载", tone: "success", icon: "refresh" });
            }
            if (kind === "save-robots") {
                DpzOS.toast({ title: "robots.txt 已保存", tone: "success", icon: "check-circle" });
            }
            if (kind === "refresh-seo-cache") {
                DpzOS.toast({ title: "SEO 缓存已刷新", text: "影响后续请求的元数据合并", tone: "success", icon: "refresh" });
            }
            if (kind === "add-seo") {
                seoForm(ctx, null);
            }
            if (kind === "edit-seo" || kind === "view-seo") {
                seoForm(
                    ctx,
                    data.seoPages.find(function (item) {
                        return item.id === id;
                    })
                );
            }
            if (kind === "delete-seo") {
                ctx.confirm({
                    title: "删除这条 SEO 配置？",
                    text: "删除后将回退到公共元数据。",
                    confirmText: "删除",
                    tone: "danger",
                    icon: "trash"
                }).then(function (ok) {
                    if (ok) {
                        data.seoPages = data.seoPages.filter(function (item) {
                            return item.id !== id;
                        });
                        ctx.rerender();
                        DpzOS.toast({ title: "SEO 配置已删除", tone: "success", icon: "check-circle" });
                    }
                });
            }
            if (kind === "tpl-maintenance" || kind === "tpl-restore" || kind === "tpl-feature") {
                var templates = {
                    "tpl-maintenance": "【维护公告】站点将于今晚 23:00 - 23:30 进行例行维护，期间可能无法访问，敬请谅解。",
                    "tpl-restore": "【服务恢复】站点维护已完成，所有服务已恢复正常，感谢你的等待。",
                    "tpl-feature": "【功能提醒】新版桌面后台已上线，欢迎体验窗口化管理与命令面板。"
                };
                state.noticeText = templates[kind];
                ctx.rerender();
            }
            if (kind === "clear-notice") {
                state.noticeText = "";
                ctx.rerender();
            }
            if (kind === "send-notice") {
                if (!state.noticeText.trim()) {
                    DpzOS.toast({ title: "通知内容不能为空", tone: "danger", icon: "alert" });
                    return;
                }
                ctx.confirm({
                    title: "广播这条通知？",
                    text: "所有在线访客都会立即收到。",
                    confirmText: "发送",
                    icon: "send"
                }).then(function (ok) {
                    if (!ok) {
                        return;
                    }
                    ctx.notify({
                        tone: "info",
                        title: "系统通知已发送",
                        text: state.noticeText.slice(0, 40),
                        time: new Date(),
                        icon: "bell"
                    });
                    state.noticeText = "";
                    ctx.rerender();
                    DpzOS.toast({ title: "通知已广播", tone: "success", icon: "send" });
                });
            }
            if (kind === "delete-notice") {
                DpzOS.toast({ title: "通知记录已删除（演示）", tone: "success", icon: "check-circle" });
            }
        });

        rootNode.addEventListener("input", function (event) {
            if (event.target.matches('[data-role="notice-text"]')) {
                state.noticeText = event.target.value;
                var counter = event.target.closest(".ui-field").querySelector(".ui-field__label");
                if (counter) {
                    counter.textContent = "通知内容（" + state.noticeText.length + " / 500）";
                }
                var bar = rootNode.querySelector(".ui-progress i");
                if (bar) {
                    bar.style.width = Math.min(100, (state.noticeText.length / 500) * 100) + "%";
                }
            }
            if (event.target.matches('[data-role="footer-editor"]')) {
                state.footerDraft = event.target.value;
            }
            if (event.target.matches('[data-role="search-seo"]')) {
                state.seoKeyword = event.target.value;
                rootNode.querySelectorAll("tbody tr").forEach(function (row) {
                    row.style.display = util.matches(state.seoKeyword, [row.textContent]) ? "" : "none";
                });
            }
        });
    }

    DpzOS.registerApp({
        id: "site",
        name: "网站配置",
        en: "Site",
        icon: "globe",
        tone: "info",
        group: "站点配置",
        size: { w: 1160, h: 720 },
        singleton: true,
        desc: "友链、页脚、Robots、SEO 与系统通知",
        render: render,
        mount: bind
    });
})(window.DpzOS);
