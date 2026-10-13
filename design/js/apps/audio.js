(function (DpzOS) {
    "use strict";

    var ui = DpzOS.ui;
    var util = DpzOS.util;
    var icon = DpzOS.icon;
    var data = DpzOS.data;

    var PAGE_SIZE = 6;

    function render(ctx) {
        var state = ctx.state;
        state.page = state.page || 1;
        state.playingId = state.playingId || null;
        state.progress = state.progress || 0;

        var rows = data.recordings;
        var totalPages = Math.max(1, Math.ceil(rows.length / PAGE_SIZE));
        state.page = util.clamp(state.page, 1, totalPages);
        var pageRows = rows.slice((state.page - 1) * PAGE_SIZE, state.page * PAGE_SIZE);

        return (
            '<div class="ui-page">' +
            '<div class="ui-head">' +
            '<div class="ui-head__text">' +
            '<div class="ui-eyebrow">内容创作 · AUDIO</div>' +
            '<h1 class="ui-title">录音管理</h1>' +
            '<div class="ui-subtitle">共 ' +
            data.recordings.length +
            " 条录音 · 列表内嵌播放器，同一时间只播放一条</div>" +
            "</div>" +
            '<div class="ui-head__actions">' +
            ui.btn({ label: "刷新", icon: "refresh", attrs: 'data-action="refresh"' }) +
            "</div>" +
            "</div>" +
            ui.toolbar([
                [
                    ui.chip("总时长 " + util.formatDuration(
                        data.recordings.reduce(function (sum, item) {
                            return sum + item.duration;
                        }, 0)
                    ), { icon: "clock" }),
                    ui.chip(
                        "总大小 " +
                            util.formatBytes(
                                data.recordings.reduce(function (sum, item) {
                                    return sum + parseFloat(item.size) * 1024 * 1024;
                                }, 0)
                            ),
                        { icon: "database" }
                    )
                ],
                [
                    ui.badge("上传入口在移动端 App，仅管理端", null, "info")
                ]
            ]) +
            ui.panel({
                flush: true,
                body: ui.table({
                    columns: [
                        { label: "试听" },
                        { label: "名称" },
                        { label: "时长 / 大小" },
                        { label: "上传人" },
                        { label: "上传时间" },
                        { label: "操作", align: "right" }
                    ],
                    rows: pageRows.map(function (item) {
                        var playing = state.playingId === item.id;
                        var progress = playing ? (state.progress / item.duration) * 100 : 0;
                        return {
                            attrs: 'data-id="' + item.id + '"',
                            cells: [
                                '<div class="app-audio__player" data-player="' +
                                    item.id +
                                    '">' +
                                    '<button type="button" class="ui-iconbtn" data-action="toggle" data-id="' +
                                    item.id +
                                    '" aria-label="' +
                                    (playing ? "暂停" : "播放") +
                                    '">' +
                                    icon(playing ? "pause" : "play") +
                                    "</button>" +
                                    '<div class="app-audio__track">' +
                                    ui.progress(progress) +
                                    "</div>" +
                                    '<span class="u-mono u-dim app-audio__time">' +
                                    util.formatDuration(playing ? state.progress : 0) +
                                    "</span>" +
                                    "</div>",
                                '<div class="ui-table__main">' +
                                    util.esc(item.name) +
                                    "</div>",
                                '<div class="u-mono" style="color:var(--text-1)">' +
                                    util.esc(util.formatDuration(item.duration)) +
                                    '</div><div class="ui-table__sub u-mono">' +
                                    util.esc(item.size) +
                                    "</div>",
                                util.esc(item.uploader),
                                '<span class="u-mono u-muted">' +
                                    util.esc(util.formatDate(item.uploadedAt) + " " + util.formatTime(item.uploadedAt)) +
                                    "</span>",
                                '<div class="ui-table__actions">' +
                                    ui.iconBtn("download", {
                                        label: "下载",
                                        attrs: 'data-action="download" data-id="' + item.id + '"'
                                    }) +
                                    ui.iconBtn("trash", {
                                        label: "删除",
                                        danger: true,
                                        attrs: 'data-action="delete" data-id="' + item.id + '"'
                                    }) +
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

    function bind(rootNode, ctx) {
        var state = ctx.state;
        var timer = null;

        if (state.playingId) {
            var playing = data.recordings.find(function (item) {
                return item.id === state.playingId;
            });
            if (playing) {
                timer = window.setInterval(function () {
                    state.progress += 0.5;
                    if (state.progress >= playing.duration) {
                        state.progress = 0;
                        state.playingId = null;
                        window.clearInterval(timer);
                        timer = null;
                        ctx.rerender();
                        return;
                    }
                    var player = rootNode.querySelector('[data-player="' + playing.id + '"]');
                    if (player) {
                        var bar = player.querySelector(".ui-progress i");
                        if (bar) {
                            bar.style.width = ((state.progress / playing.duration) * 100).toFixed(2) + "%";
                        }
                        var time = player.querySelector(".app-audio__time");
                        if (time) {
                            time.textContent = util.formatDuration(state.progress);
                        }
                    }
                }, 500);
            }
        }

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
            var item = data.recordings.find(function (entry) {
                return entry.id === id;
            });

            if (kind === "toggle" && item) {
                if (state.playingId === id) {
                    state.playingId = null;
                    state.progress = 0;
                } else {
                    state.playingId = id;
                    state.progress = 0;
                }
                ctx.rerender();
            }
            if (kind === "refresh") {
                ctx.rerender();
                DpzOS.toast({ title: "录音列表已刷新", tone: "success", icon: "refresh" });
            }
            if (kind === "download" && item) {
                DpzOS.toast({ title: "开始下载", text: item.name, tone: "info", icon: "download" });
            }
            if (kind === "delete" && item) {
                ctx.confirm({
                    title: "删除这条录音？",
                    text: item.name + "（" + item.size + "）",
                    confirmText: "删除",
                    tone: "danger",
                    icon: "trash"
                }).then(function (ok) {
                    if (ok) {
                        data.recordings = data.recordings.filter(function (entry) {
                            return entry.id !== id;
                        });
                        if (state.playingId === id) {
                            state.playingId = null;
                        }
                        ctx.rerender();
                        DpzOS.toast({ title: "录音已删除", tone: "success", icon: "check-circle" });
                    }
                });
            }
        });

        return function () {
            if (timer) {
                window.clearInterval(timer);
                timer = null;
            }
        };
    }

    DpzOS.registerApp({
        id: "audio-app",
        name: "录音管理",
        en: "Audio",
        icon: "mic",
        tone: "info",
        group: "内容创作",
        size: { w: 1120, h: 660 },
        singleton: true,
        desc: "录音列表与内嵌试听",
        render: render,
        mount: bind
    });
})(window.DpzOS);
