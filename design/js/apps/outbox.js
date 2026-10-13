(function (DpzOS) {
    "use strict";

    var ui = DpzOS.ui;
    var util = DpzOS.util;
    var icon = DpzOS.icon;
    var data = DpzOS.data;

    var STATUS = [
        { id: "Pending", label: "待发布", icon: "clock", tone: "warning", desc: "尚未投递到 Broker" },
        { id: "Sent", label: "已发布", icon: "send", tone: "info", desc: "已投递，等待消费" },
        { id: "Consumed", label: "已消费", icon: "check-circle", tone: "success", desc: "消费者已确认" },
        { id: "Failed", label: "发布失败", icon: "x-circle", tone: "danger", desc: "Broker 不可达或超时" },
        { id: "ConsumeFailed", label: "消费失败", icon: "alert", tone: "danger", desc: "消费者处理异常" }
    ];

    function statusMeta(id) {
        return STATUS.find(function (item) {
            return item.id === id;
        });
    }

    function filtered(state) {
        return data.outbox.filter(function (item) {
            if (state.status && item.status !== state.status) {
                return false;
            }
            if (state.type && state.type !== "全部" && item.type !== state.type) {
                return false;
            }
            if (state.exchange && state.exchange !== "全部" && item.exchange !== state.exchange) {
                return false;
            }
            if (state.keyword) {
                var haystack = [item.id, item.type, item.source, item.routingKey, item.error].join(" ");
                if (!util.matches(state.keyword, [haystack])) {
                    return false;
                }
            }
            return true;
        });
    }

    function render(ctx) {
        var state = ctx.state;
        state.page = state.page || 1;
        state.status = state.status || null;
        state.type = state.type || "全部";
        state.exchange = state.exchange || "全部";
        state.keyword = state.keyword || "";

        var rows = filtered(state);
        var totalPages = Math.max(1, Math.ceil(rows.length / 8));
        state.page = util.clamp(state.page, 1, totalPages);
        var pageRows = rows.slice((state.page - 1) * 8, state.page * 8);

        var tiles = STATUS.map(function (item) {
            var count = data.outbox.filter(function (message) {
                return message.status === item.id;
            }).length;
            return (
                '<button type="button" class="app-outbox__tile' +
                (state.status === item.id ? " is-active" : "") +
                '" data-status="' +
                item.id +
                '" title="' +
                util.esc(item.desc) +
                '">' +
                '<span class="app-outbox__tile-top">' +
                icon(item.icon) +
                util.esc(item.label) +
                "</span>" +
                '<span class="app-outbox__tile-value" style="color:var(--' +
                (item.tone === "accent" ? "accent" : item.tone) +
                ')">' +
                count +
                "</span>" +
                "</button>"
            );
        }).join("");

        return (
            '<div class="ui-page">' +
            '<div class="ui-head">' +
            '<div class="ui-head__text">' +
            '<div class="ui-eyebrow">开发工具 · OUTBOX</div>' +
            '<h1 class="ui-title">消息队列</h1>' +
            '<div class="ui-subtitle">RabbitMQ Outbox 模式 · 共 ' +
            data.outbox.length +
            " 条消息，点击状态卡片快速筛选</div>" +
            "</div>" +
            '<div class="ui-head__actions">' +
            ui.btn({ label: "刷新", icon: "refresh", attrs: 'data-action="refresh"' }) +
            "</div>" +
            "</div>" +
            '<div class="app-outbox__status">' +
            tiles +
            "</div>" +
            ui.toolbar([
                [
                    ui.search({
                        placeholder: "搜索消息 ID / 类型 / 路由 / 错误…",
                        value: state.keyword,
                        attrs: 'data-role="search-outbox"'
                    }),
                    ui.select({
                        options: [{ value: "全部", label: "全部类型" }].concat(
                            data.outboxTypes.slice(1).map(function (type) {
                                return { value: type, label: type };
                            })
                        ),
                        value: state.type,
                        attrs: 'data-role="filter-type"'
                    }),
                    ui.select({
                        options: ["全部", "dpz.topic", "dpz.fanout", "dpz.direct"],
                        value: state.exchange,
                        attrs: 'data-role="filter-exchange"'
                    })
                ],
                [
                    ui.btn({
                        label: state.status ? "清除状态筛选" : "仅显示异常",
                        icon: "filter",
                        size: "sm",
                        attrs: 'data-action="quick-filter"'
                    }),
                    ui.btn({ label: "重置", icon: "eraser", size: "sm", variant: "ghost", attrs: 'data-action="reset"' })
                ]
            ]) +
            ui.panel({
                flush: true,
                body: ui.table({
                    columns: [
                        { label: "消息" },
                        { label: "路由" },
                        { label: "状态" },
                        { label: "发布" },
                        { label: "消费" },
                        { label: "创建时间" },
                        { label: "操作", align: "right" }
                    ],
                    rows: pageRows.map(function (item) {
                        var meta = statusMeta(item.status);
                        return {
                            attrs: 'data-id="' + item.id + '"',
                            cells: [
                                '<div class="ui-table__main u-mono">' +
                                    util.highlight(item.type, state.keyword) +
                                    "</div>" +
                                    '<div class="ui-table__sub u-mono">#' +
                                    util.esc(item.id) +
                                    " · " +
                                    util.esc(item.source) +
                                    "</div>",
                                '<div class="u-mono">' +
                                    util.esc(item.exchange) +
                                    '</div><div class="ui-table__sub u-mono">' +
                                    util.esc(item.routingKey || "-") +
                                    "</div>",
                                ui.badge(meta ? meta.label : item.status, meta ? meta.tone : null, meta ? meta.icon : null),
                                '<span class="u-mono">尝试 ' +
                                    item.publishTries +
                                    '</span><div class="ui-table__sub">' +
                                    util.esc(item.publishedAt ? util.relativeTime(item.publishedAt) : "未发布") +
                                    "</div>",
                                '<span class="u-mono">尝试 ' +
                                    item.consumeTries +
                                    '</span><div class="ui-table__sub">' +
                                    util.esc(item.consumedAt ? util.relativeTime(item.consumedAt) : "未消费") +
                                    "</div>",
                                '<span class="u-mono u-muted">' +
                                    util.esc(util.formatDate(item.createdAt) + " " + util.formatTime(item.createdAt)) +
                                    "</span>",
                                '<div class="ui-table__actions">' +
                                    (item.status === "Sent" || item.status === "ConsumeFailed"
                                        ? ui.iconBtn("refresh", {
                                              label: "重新消费",
                                              attrs: 'data-action="reconsume" data-id="' + item.id + '"'
                                          })
                                        : "") +
                                    ui.iconBtn("eye", { label: "详情", attrs: 'data-action="detail" data-id="' + item.id + '"' }) +
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
                pageSize: 8,
                count: rows.length
            }) +
            "</div>"
        );
    }

    function bind(rootNode, ctx) {
        var state = ctx.state;

        rootNode.addEventListener("click", function (event) {
            var tile = event.target.closest("[data-status]");
            if (tile) {
                var status = tile.getAttribute("data-status");
                state.status = state.status === status ? null : status;
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
            if (!action) {
                return;
            }
            var kind = action.getAttribute("data-action");
            var id = action.getAttribute("data-id");
            var message = data.outbox.find(function (item) {
                return item.id === id;
            });

            if (kind === "refresh") {
                ctx.rerender();
                DpzOS.toast({ title: "队列状态已刷新", tone: "success", icon: "refresh" });
            }
            if (kind === "quick-filter") {
                state.status = state.status ? null : "ConsumeFailed";
                state.page = 1;
                ctx.rerender();
            }
            if (kind === "reset") {
                state.status = null;
                state.type = "全部";
                state.exchange = "全部";
                state.keyword = "";
                state.page = 1;
                ctx.rerender();
            }
            if (kind === "reconsume" && message) {
                ctx.confirm({
                    title: "重新投递这条消息？",
                    text: message.type + " 将重新进入消费队列。",
                    confirmText: "重新消费",
                    icon: "refresh"
                }).then(function (ok) {
                    if (!ok) {
                        return;
                    }
                    message.consumeTries += 1;
                    message.status = "Consumed";
                    message.consumedAt = new Date();
                    message.error = "";
                    ctx.rerender();
                    DpzOS.toast({ title: "已重新消费", text: message.id, tone: "success", icon: "check-circle" });
                });
            }
            if (kind === "delete" && message) {
                ctx.confirm({
                    title: "删除这条消息记录？",
                    text: message.type + " · " + message.id,
                    confirmText: "删除",
                    tone: "danger",
                    icon: "trash"
                }).then(function (ok) {
                    if (ok) {
                        data.outbox = data.outbox.filter(function (item) {
                            return item.id !== id;
                        });
                        ctx.rerender();
                        DpzOS.toast({ title: "消息已删除", tone: "success", icon: "check-circle" });
                    }
                });
            }
            if (kind === "detail" && message) {
                ctx.dialog({
                    title: message.type,
                    subtitle: "#" + message.id + " · " + message.source,
                    icon: "inbox",
                    confirmText: "关闭",
                    cancelText: null,
                    html:
                        '<div class="ui-stack">' +
                        '<div class="app-settings__about"><dl>' +
                        '<dt>Exchange</dt><dd>' +
                        util.esc(message.exchange) +
                        "</dd>" +
                        "<dt>RoutingKey</dt><dd>" +
                        util.esc(message.routingKey || "-") +
                        "</dd>" +
                        "<dt>状态</dt><dd>" +
                        util.esc(message.status) +
                        "</dd>" +
                        "<dt>创建时间</dt><dd>" +
                        util.esc(util.formatDate(message.createdAt) + " " + util.formatTime(message.createdAt)) +
                        "</dd></dl></div>" +
                        ui.field({
                            label: "Payload",
                            control: ui.code(
                                '{\n  "id": "' +
                                    message.id +
                                    '",\n  "type": "' +
                                    message.type +
                                    '",\n  "source": "' +
                                    message.source +
                                    '",\n  "occurredAt": "' +
                                    message.createdAt.toISOString() +
                                    '"\n}'
                            )
                        }) +
                        (message.error
                            ? ui.callout("<b>错误信息</b><br>" + util.esc(message.error), {
                                  tone: "danger",
                                  icon: "alert"
                              })
                            : "") +
                        "</div>"
                });
            }
        });

        rootNode.addEventListener("input", function (event) {
            if (event.target.matches('[data-role="search-outbox"]')) {
                state.keyword = event.target.value;
                state.page = 1;
                ctx.rerender();
                var next = rootNode.querySelector('[data-role="search-outbox"]');
                if (next) {
                    next.focus();
                    next.setSelectionRange(next.value.length, next.value.length);
                }
            }
        });

        rootNode.addEventListener("change", function (event) {
            if (event.target.matches('[data-role="filter-type"]')) {
                state.type = event.target.value;
                state.page = 1;
                ctx.rerender();
            }
            if (event.target.matches('[data-role="filter-exchange"]')) {
                state.exchange = event.target.value;
                state.page = 1;
                ctx.rerender();
            }
        });
    }

    DpzOS.registerApp({
        id: "outbox",
        name: "消息队列",
        en: "Outbox",
        icon: "route",
        tone: "warning",
        group: "开发工具",
        size: { w: 1200, h: 720 },
        singleton: true,
        desc: "Outbox 状态筛选、重新消费与详情",
        render: render,
        mount: bind
    });
})(window.DpzOS);
