(function (DpzOS) {
    "use strict";

    var ui = DpzOS.ui;
    var util = DpzOS.util;
    var icon = DpzOS.icon;
    var data = DpzOS.data;

    function pageHeader(counts) {
        return (
            '<div class="ui-head">' +
            '<div class="ui-head__text">' +
            '<div class="ui-eyebrow">安全防护 · SECURITY</div>' +
            '<h1 class="ui-title">安全中心</h1>' +
            '<div class="ui-subtitle">黑名单累计 ' +
            counts.blacklist +
            " 条 · 当前封禁 " +
            counts.blocked +
            " 个 IP · 拦截规则 " +
            counts.rules +
            " 条</div>" +
            "</div>" +
            '<div class="ui-head__actions">' +
            ui.btn({ label: "刷新", icon: "refresh", attrs: 'data-action="refresh"' }) +
            "</div>" +
            "</div>"
        );
    }

    function renderBlacklist(ctx) {
        var state = ctx.state;
        state.blacklistKeyword = state.blacklistKeyword || "";
        state.expanded = state.expanded || {};
        var rows = data.blacklist.filter(function (item) {
            return util.matches(state.blacklistKeyword, [
                item.method,
                item.path,
                item.ips.join(" "),
                item.uas.join(" ")
            ]);
        });

        return (
            ui.toolbar([
                [
                    ui.search({
                        placeholder: "搜索方法 / 路径 / IP / UA，自动高亮…",
                        value: state.blacklistKeyword,
                        attrs: 'data-role="search-blacklist"'
                    })
                ],
                [ui.chip("匹配 " + rows.length + " 条", { icon: "filter" })]
            ]) +
            '<div class="ui-stack">' +
            (rows.length
                ? rows
                      .map(function (item) {
                          var expanded = !!state.expanded[item.id];
                          var ips = expanded ? item.ips : item.ips.slice(0, 6);
                          var uas = expanded ? item.uas : item.uas.slice(0, 2);
                          return (
                              '<article class="app-blacklist__card" data-id="' +
                              item.id +
                              '">' +
                              '<div class="ui-spread">' +
                              '<div class="app-blacklist__path u-grow">' +
                              ui.badge(item.method, item.method === "GET" ? "success" : "warning") +
                              "<code>" +
                              util.highlight(item.path, state.blacklistKeyword) +
                              "</code>" +
                              "</div>" +
                              '<div class="ui-row ui-row--tight">' +
                              ui.btn({
                                  label: expanded ? "收起" : "展开",
                                  icon: expanded ? "chevron-up" : "chevron-down",
                                  size: "sm",
                                  variant: "ghost",
                                  attrs: 'data-action="toggle-expand" data-id="' + item.id + '"'
                              }) +
                              ui.btn({
                                  label: "删除",
                                  icon: "trash",
                                  size: "sm",
                                  variant: "danger",
                                  attrs: 'data-action="delete-blacklist" data-id="' + item.id + '"'
                              }) +
                              "</div>" +
                              "</div>" +
                              '<div class="app-blacklist__counts">' +
                              "<span>访问 <b>" +
                              util.formatNumber(item.visits) +
                              "</b> 次</span>" +
                              "<span>IP <b>" +
                              item.ips.length +
                              "</b> 个</span>" +
                              "<span>UA <b>" +
                              item.uas.length +
                              "</b> 种</span>" +
                              "<span>最近 <b>" +
                              util.esc(util.relativeTime(item.lastSeen)) +
                              "</b></span>" +
                              "</div>" +
                              '<div class="app-ipchips">' +
                              ips
                                  .map(function (ip) {
                                      return (
                                          '<span class="app-ipchip">' +
                                          util.highlight(ip, state.blacklistKeyword, "ui-mark") +
                                          "</span>"
                                      );
                                  })
                                  .join("") +
                              (item.ips.length > 6 && !expanded
                                  ? '<span class="app-ipchip">+' + (item.ips.length - 6) + "</span>"
                                  : "") +
                              "</div>" +
                              '<div class="app-ipchips">' +
                              uas
                                  .map(function (ua) {
                                      return (
                                          '<span class="app-ipchip" style="border-radius:8px">' +
                                          util.esc(ua) +
                                          "</span>"
                                      );
                                  })
                                  .join("") +
                              (item.uas.length > 2 && !expanded
                                  ? '<span class="app-ipchip">+' + (item.uas.length - 2) + "</span>"
                                  : "") +
                              "</div>" +
                              "</article>"
                          );
                      })
                      .join("")
                : ui.empty({ icon: "shield", title: "没有匹配的黑名单记录" })) +
            "</div>"
        );
    }

    function renderBlocked(ctx) {
        var state = ctx.state;
        state.blockIp = state.blockIp || "";
        state.blockMinutes = state.blockMinutes || 30;
        return (
            '<div class="ui-grid ui-grid--2">' +
            '<div class="ui-stack">' +
            data.blockedIps
                .map(function (item) {
                    return (
                        '<article class="app-blacklist__card" data-ip="' +
                        util.esc(item.ip) +
                        '">' +
                        '<div class="ui-spread">' +
                        '<div class="ui-row">' +
                        icon("shield") +
                        '<span class="u-mono" style="color:var(--text-0);font-size:13px">' +
                        util.esc(item.ip) +
                        "</span>" +
                        ui.badge("封禁中", "danger", "lock") +
                        "</div>" +
                        ui.btn({
                            label: "解封",
                            icon: "lock",
                            size: "sm",
                            variant: "danger",
                            attrs: 'data-action="unblock" data-ip="' + util.esc(item.ip) + '"'
                        }) +
                        "</div>" +
                        '<div class="app-blacklist__counts">' +
                        "<span>窗口事件 <b>" +
                        util.formatNumber(item.events) +
                        "</b></span>" +
                        "<span>原因 <b>" +
                        util.esc(item.reason) +
                        "</b></span>" +
                        "<span>截止 <b>" +
                        util.esc(util.formatDate(item.until) + " " + util.formatTime(item.until)) +
                        "</b></span>" +
                        "</div>" +
                        "</article>"
                    );
                })
                .join("") +
            "</div>" +
            ui.panel({
                title: "手动封禁",
                icon: "lock",
                sub: "最长 24 小时",
                accent: true,
                body:
                    ui.field({
                        label: "IP 地址",
                        required: true,
                        control:
                            '<input class="ui-input u-mono" placeholder="例如 203.0.113.10" value="' +
                            util.esc(state.blockIp) +
                            '" data-role="block-ip">'
                    }) +
                    ui.field({
                        label: "封禁时长（分钟）",
                        hint: "范围 1 - 1440，默认 30",
                        control:
                            '<input class="ui-input u-mono" type="number" min="1" max="1440" value="' +
                            state.blockMinutes +
                            '" data-role="block-minutes">'
                    }) +
                    ui.btn({
                        label: "立即封禁",
                        icon: "shield",
                        variant: "primary",
                        attrs: 'data-action="block-ip"'
                    }) +
                    ui.callout("封禁会立即作用于所有节点，解封后 1 分钟内全量生效。", { icon: "info" })
            }) +
            "</div>"
        );
    }

    function renderRules(ctx) {
        var state = ctx.state;
        state.ruleDraft = state.ruleDraft || { type: "URI 路径", key: "", pattern: "" };
        var types = ["URI 路径", "请求方法", "客户端 IP", "User-Agent", "查询参数", "请求头"];
        return (
            '<div class="ui-grid ui-grid--2">' +
            '<div class="ui-stack">' +
            (data.interceptRules.length
                ? data.interceptRules
                      .map(function (rule) {
                          return (
                              '<article class="app-blacklist__card" data-id="' +
                              rule.id +
                              '">' +
                              '<div class="ui-spread">' +
                              '<div class="ui-row">' +
                              ui.badge(rule.type, "accent", "filter") +
                              '<code class="u-mono" style="color:var(--text-0)">' +
                              util.esc(rule.pattern) +
                              "</code>" +
                              (rule.key
                                  ? ui.badge("key: " + rule.key, null, "hash")
                                  : "") +
                              "</div>" +
                              '<div class="ui-row ui-row--tight">' +
                              ui.iconBtn("edit", { label: "编辑", attrs: 'data-action="edit-rule" data-id="' + rule.id + '"' }) +
                              ui.iconBtn("trash", { label: "删除", danger: true, attrs: 'data-action="delete-rule" data-id="' + rule.id + '"' }) +
                              "</div>" +
                              "</div>" +
                              '<div class="ui-item__sub">' +
                              util.esc(rule.desc) +
                              "</div>" +
                              "</article>"
                          );
                      })
                      .join("")
                : ui.empty({ icon: "filter", title: "还没有拦截规则" })) +
            "</div>" +
            ui.panel({
                title: "新增拦截规则",
                icon: "plus",
                sub: "支持 * 与 ? 通配符",
                body:
                    ui.field({
                        label: "拦截类型",
                        control: ui.select({
                            options: types,
                            value: state.ruleDraft.type,
                            attrs: 'data-role="rule-type"'
                        })
                    }) +
                    (state.ruleDraft.type === "查询参数" || state.ruleDraft.type === "请求头"
                        ? ui.field({
                              label: "参数 / 请求头名称",
                              control:
                                  '<input class="ui-input u-mono" value="' +
                                  util.esc(state.ruleDraft.key) +
                                  '" data-role="rule-key" placeholder="例如 token">'
                          })
                        : "") +
                    ui.field({
                        label: "匹配模式",
                        required: true,
                        hint: "例如 /admin/* · POST · 203.0.113.* · BadBot*",
                        control:
                            '<input class="ui-input u-mono" value="' +
                            util.esc(state.ruleDraft.pattern) +
                            '" data-role="rule-pattern" placeholder="' +
                            (state.ruleDraft.type === "请求方法" ? "POST" : "/admin/*") +
                            '">'
                    }) +
                    ui.btn({
                        label: "保存规则",
                        icon: "check",
                        variant: "primary",
                        attrs: 'data-action="save-rule"'
                    }) +
                    ui.callout("命中规则的请求会被服务端直接拒绝，并计入黑名单统计。", {
                        tone: "warning",
                        icon: "alert"
                    })
            }) +
            "</div>"
        );
    }

    function render(ctx) {
        var state = ctx.state;
        state.tab = state.tab || (ctx.params && ctx.params.tab) || "blacklist";
        var counts = {
            blacklist: data.blacklist.length,
            blocked: data.blockedIps.length,
            rules: data.interceptRules.length
        };
        var body = "";
        if (state.tab === "blacklist") {
            body = renderBlacklist(ctx);
        }
        if (state.tab === "blocked") {
            body = renderBlocked(ctx);
        }
        if (state.tab === "rules") {
            body = renderRules(ctx);
        }
        return (
            '<div class="ui-page">' +
            pageHeader(counts) +
            ui.tabs(
                [
                    { id: "blacklist", label: "请求黑名单", icon: "shield", count: counts.blacklist },
                    { id: "blocked", label: "封禁 IP", icon: "lock", count: counts.blocked },
                    { id: "rules", label: "拦截规则", icon: "filter", count: counts.rules }
                ],
                state.tab
            ) +
            body +
            "</div>"
        );
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
            var action = event.target.closest("[data-action]");
            if (!action) {
                return;
            }
            var kind = action.getAttribute("data-action");
            var id = action.getAttribute("data-id");

            if (kind === "refresh") {
                ctx.rerender();
                DpzOS.toast({ title: "安全数据已刷新", tone: "success", icon: "refresh" });
            }
            if (kind === "toggle-expand") {
                state.expanded[id] = !state.expanded[id];
                ctx.rerender();
            }
            if (kind === "delete-blacklist") {
                ctx.confirm({
                    title: "移除这条黑名单记录？",
                    text: "移除后该请求模式将不再被统计。",
                    confirmText: "移除",
                    tone: "danger",
                    icon: "trash"
                }).then(function (ok) {
                    if (ok) {
                        data.blacklist = data.blacklist.filter(function (item) {
                            return item.id !== id;
                        });
                        ctx.rerender();
                        DpzOS.toast({ title: "已移除", tone: "success", icon: "check-circle" });
                    }
                });
            }
            if (kind === "unblock") {
                var ip = action.getAttribute("data-ip");
                ctx.confirm({
                    title: "解除封禁？",
                    text: "IP " + ip + " 将立即恢复访问。",
                    confirmText: "解封",
                    icon: "lock"
                }).then(function (ok) {
                    if (ok) {
                        data.blockedIps = data.blockedIps.filter(function (item) {
                            return item.ip !== ip;
                        });
                        ctx.rerender();
                        DpzOS.toast({ title: "已解封 " + ip, tone: "success", icon: "check-circle" });
                    }
                });
            }
            if (kind === "block-ip") {
                if (!state.blockIp.trim()) {
                    DpzOS.toast({ title: "请输入 IP 地址", tone: "danger", icon: "alert" });
                    return;
                }
                var until = new Date(Date.now() + state.blockMinutes * 60000);
                data.blockedIps.unshift({
                    ip: state.blockIp.trim(),
                    until: until,
                    events: 0,
                    reason: "手动封禁"
                });
                DpzOS.toast({
                    title: "已封禁 " + state.blockIp.trim(),
                    text: state.blockMinutes + " 分钟内拒绝访问",
                    tone: "success",
                    icon: "shield"
                });
                state.blockIp = "";
                ctx.rerender();
            }
            if (kind === "save-rule") {
                if (!state.ruleDraft.pattern.trim()) {
                    DpzOS.toast({ title: "请填写匹配模式", tone: "danger", icon: "alert" });
                    return;
                }
                data.interceptRules.unshift({
                    id: util.uid("r"),
                    type: state.ruleDraft.type,
                    key: state.ruleDraft.key,
                    pattern: state.ruleDraft.pattern,
                    desc: "手动创建"
                });
                state.ruleDraft = { type: "URI 路径", key: "", pattern: "" };
                ctx.rerender();
                DpzOS.toast({ title: "规则已保存", tone: "success", icon: "check-circle" });
            }
            if (kind === "edit-rule") {
                var rule = data.interceptRules.find(function (item) {
                    return item.id === id;
                });
                if (rule) {
                    state.ruleDraft = { type: rule.type, key: rule.key, pattern: rule.pattern };
                    state.tab = "rules";
                    ctx.rerender();
                    DpzOS.toast({ title: "已载入到右侧表单", tone: "info", icon: "edit", timeout: 1600 });
                }
            }
            if (kind === "delete-rule") {
                ctx.confirm({
                    title: "删除这条拦截规则？",
                    confirmText: "删除",
                    tone: "danger",
                    icon: "trash"
                }).then(function (ok) {
                    if (ok) {
                        data.interceptRules = data.interceptRules.filter(function (item) {
                            return item.id !== id;
                        });
                        ctx.rerender();
                        DpzOS.toast({ title: "规则已删除", tone: "success", icon: "check-circle" });
                    }
                });
            }
        });

        rootNode.addEventListener("input", function (event) {
            if (event.target.matches('[data-role="search-blacklist"]')) {
                state.blacklistKeyword = event.target.value;
                ctx.rerender();
                var next = rootNode.querySelector('[data-role="search-blacklist"]');
                if (next) {
                    next.focus();
                    next.setSelectionRange(next.value.length, next.value.length);
                }
            }
            if (event.target.matches('[data-role="block-ip"]')) {
                state.blockIp = event.target.value;
            }
            if (event.target.matches('[data-role="block-minutes"]')) {
                state.blockMinutes = util.clamp(Number(event.target.value) || 30, 1, 1440);
            }
            if (event.target.matches('[data-role="rule-key"]')) {
                state.ruleDraft.key = event.target.value;
            }
            if (event.target.matches('[data-role="rule-pattern"]')) {
                state.ruleDraft.pattern = event.target.value;
            }
        });

        rootNode.addEventListener("change", function (event) {
            if (event.target.matches('[data-role="rule-type"]')) {
                state.ruleDraft.type = event.target.value;
                ctx.rerender();
            }
        });
    }

    DpzOS.registerApp({
        id: "security",
        name: "安全中心",
        en: "Security",
        icon: "shield",
        tone: "danger",
        group: "安全防护",
        size: { w: 1120, h: 700 },
        singleton: true,
        desc: "黑名单、封禁 IP 与拦截规则",
        render: render,
        mount: bind
    });
})(window.DpzOS);
