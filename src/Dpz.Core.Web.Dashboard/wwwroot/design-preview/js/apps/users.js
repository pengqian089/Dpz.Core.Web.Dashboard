(function (DpzOS) {
    "use strict";

    var ui = DpzOS.ui;
    var util = DpzOS.util;
    var icon = DpzOS.icon;
    var data = DpzOS.data;

    function permissionBadge(permissions) {
        if (!permissions || permissions === "未设置") {
            return ui.badge("未设置", "warning", "alert");
        }
        if (permissions.indexOf("System") >= 0) {
            return ui.badge(permissions, "danger", "shield");
        }
        return ui.badge(permissions, "accent", "user");
    }

    function render(ctx) {
        var state = ctx.state;
        state.page = state.page || 1;
        state.keyword = state.keyword || (ctx.params && ctx.params.account) || "";
        var PAGE_SIZE = 8;

        var accounts = data.accounts.filter(function (account) {
            return util.matches(state.keyword, [
                account.account,
                account.name,
                account.signature,
                account.permissions
            ]);
        });
        var totalPages = Math.max(1, Math.ceil(accounts.length / PAGE_SIZE));
        state.page = util.clamp(state.page, 1, totalPages);
        var pageRows = accounts.slice((state.page - 1) * PAGE_SIZE, state.page * PAGE_SIZE);

        return (
            '<div class="ui-page">' +
            '<div class="ui-head">' +
            '<div class="ui-head__text">' +
            '<div class="ui-eyebrow">安全防护 · ACCOUNTS</div>' +
            '<h1 class="ui-title">用户与权限</h1>' +
            '<div class="ui-subtitle">共 ' +
            data.accounts.length +
            " 个账号 · System 账号不可停用或改密</div>" +
            "</div>" +
            '<div class="ui-head__actions">' +
            ui.btn({ label: "新建账号", icon: "plus", variant: "primary", attrs: 'data-action="create"' }) +
            ui.btn({ label: "刷新", icon: "refresh", attrs: 'data-action="refresh"' }) +
            "</div>" +
            "</div>" +
            ui.toolbar([
                [
                    ui.search({
                        placeholder: "搜索账号、昵称或签名，支持拼音…",
                        value: state.keyword,
                        attrs: 'data-role="search-account"'
                    })
                ],
                [
                    ui.chip("启用 " + data.accounts.filter(function (a) { return a.enabled; }).length, { icon: "check-circle" }),
                    ui.chip("停用 " + data.accounts.filter(function (a) { return !a.enabled; }).length, { icon: "lock" }),
                    ui.chip("System " + data.accounts.filter(function (a) { return a.isSystem; }).length, { icon: "shield" })
                ]
            ]) +
            ui.panel({
                flush: true,
                body: ui.table({
                    columns: [
                        { label: "账号" },
                        { label: "性别" },
                        { label: "最近登录" },
                        { label: "签名" },
                        { label: "权限" },
                        { label: "状态" },
                        { label: "操作", align: "right" }
                    ],
                    rows: pageRows.map(function (account) {
                        return {
                            attrs: 'data-id="' + account.id + '"',
                            cells: [
                                '<div class="ui-table__main">' +
                                    ui.avatar(account.name, null, account.account) +
                                    "<div><div>" +
                                    util.esc(account.name) +
                                    "</div>" +
                                    '<div class="ui-table__sub u-mono">@' +
                                    util.esc(account.account) +
                                    "</div></div>" +
                                    "</div>",
                                util.esc(account.gender),
                                '<span class="u-mono u-muted">' +
                                    util.esc(util.relativeTime(account.lastLogin)) +
                                    "</span>",
                                '<span class="u-ellipsis" style="display:inline-block;max-width:220px">' +
                                    util.esc(account.signature) +
                                    "</span>",
                                permissionBadge(account.permissions),
                                account.enabled
                                    ? ui.badge("已启用", "success", "check")
                                    : ui.badge("已停用", "warning", "lock"),
                                '<div class="ui-table__actions">' +
                                    ui.iconBtn("clock", {
                                        label: "登录记录",
                                        attrs: 'data-action="history" data-id="' + account.id + '"'
                                    }) +
                                    '<button type="button" class="ui-iconbtn' +
                                    (account.isSystem ? "" : "") +
                                    '" data-action="toggle" data-id="' +
                                    account.id +
                                    '" title="' +
                                    (account.isSystem ? "System 账号不可操作" : account.enabled ? "停用" : "启用") +
                                    '"' +
                                    (account.isSystem ? " disabled style=\"opacity:.35\"" : "") +
                                    ">" +
                                    icon(account.enabled ? "lock" : "check-circle") +
                                    "</button>" +
                                    '<button type="button" class="ui-iconbtn" data-action="password" data-id="' +
                                    account.id +
                                    '" title="' +
                                    (account.isSystem ? "System 账号不可操作" : "修改密码") +
                                    '"' +
                                    (account.isSystem ? " disabled style=\"opacity:.35\"" : "") +
                                    ">" +
                                    icon("key") +
                                    "</button>" +
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
                count: accounts.length
            }) +
            "</div>"
        );
    }

    function accountForm(ctx, account) {
        var creating = !account;
        return ctx
            .dialog({
                title: creating ? "新建账号" : "修改密码",
                subtitle: creating ? "创建后立即生效" : "@" + account.account + " · " + account.name,
                icon: "user",
                confirmText: creating ? "创建" : "保存",
                html:
                    '<div class="ui-stack">' +
                    ui.field({
                        label: "账号",
                        required: true,
                        control:
                            '<input class="ui-input u-mono" value="' +
                            util.esc(account ? account.account : "") +
                            '"' +
                            (creating ? "" : " readonly") +
                            ">"
                    }) +
                    ui.field({
                        label: "昵称",
                        control:
                            '<input class="ui-input" value="' +
                            util.esc(account ? account.name : "") +
                            '"' +
                            (creating ? "" : " readonly") +
                            ">"
                    }) +
                    ui.field({
                        label: "密码",
                        required: creating,
                        hint: "建议 12 位以上，包含大小写与符号",
                        control:
                            '<div class="ui-search" style="position:relative"><input class="ui-input u-mono" type="password" value=""><button type="button" class="ui-iconbtn" style="position:absolute;right:4px" data-action="toggle-password">' +
                            icon("eye") +
                            "</button></div>"
                    }) +
                    (creating
                        ? ui.field({
                              label: "初始权限",
                              control:
                                  '<div class="ui-chips">' +
                                  ui.chip("Member", { active: true, icon: "user", click: true }) +
                                  ui.chip("System", { icon: "shield", click: true }) +
                                  "</div>"
                          })
                        : "") +
                    "</div>"
            })
            .then(function (ok) {
                if (ok) {
                    DpzOS.toast({
                        title: creating ? "账号已创建（演示）" : "密码已修改（演示）",
                        tone: "success",
                        icon: "check-circle"
                    });
                }
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
            if (event.target.closest("[data-action='toggle-password']")) {
                var input = event.target.closest(".ui-search").querySelector("input");
                input.type = input.type === "password" ? "text" : "password";
                return;
            }
            var action = event.target.closest("[data-action]");
            if (!action) {
                return;
            }
            var kind = action.getAttribute("data-action");
            var id = action.getAttribute("data-id");
            var account = data.accounts.find(function (item) {
                return item.id === id;
            });

            if (kind === "create") {
                accountForm(ctx, null);
            }
            if (kind === "refresh") {
                ctx.rerender();
                DpzOS.toast({ title: "账号列表已刷新", tone: "success", icon: "refresh" });
            }
            if (kind === "history") {
                ctx.open("token-history", { account: account ? account.account : "" });
            }
            if (kind === "toggle" && account && !account.isSystem) {
                ctx.confirm({
                    title: account.enabled ? "停用这个账号？" : "启用这个账号？",
                    text: "@" + account.account + " " + (account.enabled ? "将无法再登录后台。" : "将恢复登录权限。"),
                    confirmText: account.enabled ? "停用" : "启用",
                    tone: account.enabled ? "danger" : "accent",
                    icon: account.enabled ? "lock" : "check-circle"
                }).then(function (ok) {
                    if (ok) {
                        account.enabled = !account.enabled;
                        ctx.rerender();
                        DpzOS.toast({
                            title: account.enabled ? "账号已启用" : "账号已停用",
                            tone: "success",
                            icon: "check-circle"
                        });
                    }
                });
            }
            if (kind === "password" && account && !account.isSystem) {
                accountForm(ctx, account);
            }
        });

        var search = rootNode.querySelector('[data-role="search-account"]');
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
        id: "users",
        name: "用户与权限",
        en: "Accounts",
        icon: "users",
        tone: "warning",
        group: "安全防护",
        size: { w: 1120, h: 700 },
        singleton: true,
        desc: "账号、权限、登录记录与密码管理",
        render: render,
        mount: bind
    });

    function renderHistory(ctx) {
        var state = ctx.state;
        state.page = state.page || 1;
        var PAGE_SIZE = 8;
        var account = ctx.params && ctx.params.account;
        var rows = data.tokenHistory.filter(function (item) {
            return !account || item.account === account;
        });
        var totalPages = Math.max(1, Math.ceil(rows.length / PAGE_SIZE));
        state.page = util.clamp(state.page, 1, totalPages);
        var pageRows = rows.slice((state.page - 1) * PAGE_SIZE, state.page * PAGE_SIZE);

        return (
            '<div class="ui-page">' +
            '<div class="ui-head">' +
            '<div class="ui-head__text">' +
            '<div class="ui-eyebrow">登录审计 · TOKENS</div>' +
            '<h1 class="ui-title">登录记录' +
            (account ? " · @" + util.esc(account) : "") +
            "</h1>" +
            '<div class="ui-subtitle">共 ' +
            rows.length +
            " 条，失败尝试会同时写入安全中心</div>" +
            "</div>" +
            "</div>" +
            ui.panel({
                flush: true,
                body: ui.table({
                    columns: [
                        { label: "账号" },
                        { label: "IP" },
                        { label: "User-Agent" },
                        { label: "SessionId" },
                        { label: "方式" },
                        { label: "结果" },
                        { label: "时间" }
                    ],
                    rows: pageRows.map(function (item) {
                        return {
                            cells: [
                                ui.badge(item.account, "accent", "user"),
                                '<span class="u-mono">' + util.esc(item.ip) + "</span>",
                                '<span class="u-ellipsis" style="display:inline-block;max-width:220px">' +
                                    util.esc(item.agent) +
                                    "</span>",
                                '<span class="u-mono u-muted">' +
                                    util.esc(item.session) +
                                    "</span>",
                                util.esc(item.method),
                                ui.badge(
                                    item.result,
                                    item.resultTone,
                                    item.resultTone === "success" ? "check-circle" : "alert"
                                ),
                                '<span class="u-mono u-muted">' +
                                    util.esc(util.formatDate(item.createdAt) + " " + util.formatTime(item.createdAt)) +
                                    "</span>"
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

    function bindHistory(rootNode, ctx) {
        rootNode.addEventListener("click", function (event) {
            var pageButton = event.target.closest("[data-page]");
            if (pageButton && !pageButton.disabled) {
                ctx.state.page = Number(pageButton.getAttribute("data-page"));
                ctx.rerender();
            }
        });
    }

    DpzOS.registerApp({
        id: "token-history",
        name: "登录记录",
        en: "Token history",
        icon: "clock",
        tone: "info",
        group: "安全防护",
        size: { w: 1080, h: 640 },
        desc: "账号登录审计与失败记录",
        render: renderHistory,
        mount: bindHistory
    });
})(window.DpzOS);
