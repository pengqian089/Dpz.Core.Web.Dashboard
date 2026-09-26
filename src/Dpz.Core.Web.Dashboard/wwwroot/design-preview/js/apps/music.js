(function (DpzOS) {
    "use strict";

    var ui = DpzOS.ui;
    var util = DpzOS.util;
    var icon = DpzOS.icon;
    var data = DpzOS.data;

    var PAGE_SIZE = 8;

    function render(ctx) {
        var state = ctx.state;
        state.page = state.page || 1;
        state.keyword = state.keyword || "";
        state.group = state.group || "全部";

        var rows = data.musicTracks.filter(function (track) {
            if (state.group !== "全部" && track.groups.indexOf(state.group) < 0) {
                return false;
            }
            return util.matches(state.keyword, [track.title, track.artist, track.fileName, track.groups.join(" ")]);
        });
        var totalPages = Math.max(1, Math.ceil(rows.length / PAGE_SIZE));
        state.page = util.clamp(state.page, 1, totalPages);
        var pageRows = rows.slice((state.page - 1) * PAGE_SIZE, state.page * PAGE_SIZE);

        return (
            '<div class="ui-page">' +
            '<div class="ui-head">' +
            '<div class="ui-head__text">' +
            '<div class="ui-eyebrow">内容创作 · MUSIC</div>' +
            '<h1 class="ui-title">音乐管理</h1>' +
            '<div class="ui-subtitle">共 ' +
            data.musicTracks.length +
            " 首 · 支持 mp3 / flac / ogg，单文件上限 100MB</div>" +
            "</div>" +
            '<div class="ui-head__actions">' +
            ui.btn({ label: "上传音乐", icon: "upload", variant: "primary", attrs: 'data-action="upload"' }) +
            ui.btn({ label: "刷新", icon: "refresh", attrs: 'data-action="refresh"' }) +
            "</div>" +
            "</div>" +
            ui.toolbar([
                [
                    ui.search({
                        placeholder: "搜索歌曲、歌手或文件名…",
                        value: state.keyword,
                        attrs: 'data-role="search-music"'
                    }),
                    ui.select({
                        options: data.musicGroups,
                        value: state.group,
                        attrs: 'data-role="filter-group"'
                    })
                ],
                [
                    ui.chip("有歌词 " + data.musicTracks.filter(function (t) { return t.hasLyrics; }).length, {
                        icon: "file"
                    }),
                    ui.chip("原创 " + data.musicTracks.filter(function (t) { return t.source === "原创"; }).length, {
                        icon: "star"
                    })
                ]
            ]) +
            ui.panel({
                flush: true,
                body: ui.table({
                    columns: [
                        { label: "歌曲" },
                        { label: "歌手" },
                        { label: "分组" },
                        { label: "时长" },
                        { label: "来源" },
                        { label: "歌词" },
                        { label: "上传 / 修改" },
                        { label: "操作", align: "right" }
                    ],
                    rows: pageRows.map(function (track) {
                        return {
                            attrs: 'data-id="' + track.id + '"',
                            cells: [
                                '<div class="ui-table__main">' +
                                    util.esc(track.title) +
                                    "</div>" +
                                    '<div class="ui-table__sub u-mono">' +
                                    util.esc(track.fileName) +
                                    "</div>",
                                util.esc(track.artist),
                                '<div class="app-article__tags">' +
                                    track.groups
                                        .map(function (group) {
                                            return '<span class="app-article__tag">' + util.esc(group) + "</span>";
                                        })
                                        .join("") +
                                    "</div>",
                                '<span class="u-mono">' + util.esc(util.formatDuration(track.duration)) + "</span>",
                                ui.badge(track.source, track.source === "原创" ? "accent" : null),
                                track.hasLyrics
                                    ? ui.badge("有", "success", "check")
                                    : ui.badge("无", "warning", "x"),
                                '<span class="u-mono u-muted">' +
                                    util.esc(util.relativeTime(track.uploadedAt)) +
                                    '</span><div class="ui-table__sub u-mono">' +
                                    util.esc(track.size) +
                                    "</div>",
                                '<div class="ui-table__actions">' +
                                    ui.iconBtn("eye", {
                                        label: "详情",
                                        attrs: 'data-action="detail" data-id="' + track.id + '"'
                                    }) +
                                    ui.iconBtn("trash", {
                                        label: "删除",
                                        danger: true,
                                        attrs: 'data-action="delete" data-id="' + track.id + '"'
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
            var track = data.musicTracks.find(function (item) {
                return item.id === id;
            });

            if (kind === "upload") {
                openUpload(ctx);
            }
            if (kind === "refresh") {
                ctx.rerender();
                DpzOS.toast({ title: "音乐库已刷新", tone: "success", icon: "refresh" });
            }
            if (kind === "detail" && track) {
                ctx.open("music-detail", { id: track.id });
            }
            if (kind === "delete" && track) {
                ctx.confirm({
                    title: "删除这首音乐？",
                    text: track.title + " - " + track.artist,
                    confirmText: "删除",
                    tone: "danger",
                    icon: "trash"
                }).then(function (ok) {
                    if (ok) {
                        data.musicTracks = data.musicTracks.filter(function (item) {
                            return item.id !== id;
                        });
                        ctx.rerender();
                        DpzOS.toast({ title: "音乐已删除", tone: "success", icon: "check-circle" });
                    }
                });
            }
        });

        rootNode.addEventListener("change", function (event) {
            if (event.target.matches('[data-role="filter-group"]')) {
                state.group = event.target.value;
                state.page = 1;
                ctx.rerender();
            }
        });

        var search = rootNode.querySelector('[data-role="search-music"]');
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

    function openUpload(ctx) {
        var draft = { groups: ["电子"], title: "" };
        var promise = ctx.dialog({
            title: "上传音乐",
            subtitle: "音乐文件必填，封面与歌词可选",
            icon: "upload",
            confirmText: "开始上传",
            html:
                '<div class="ui-stack">' +
                ui.field({
                    label: "音乐文件",
                    required: true,
                    control:
                        '<div class="ui-dropzone" style="padding:20px">' +
                        icon("music") +
                        "<div>拖入或点击选择 .mp3 / .flac / .ogg</div>" +
                        "</div>"
                }) +
                ui.field({
                    label: "封面（可选）",
                    hint: "客户端缩放到 800×800 后转 base64 预览",
                    control: '<div class="ui-dropzone" style="padding:16px">' + icon("image") + "<div>选择封面图片</div></div>"
                }) +
                ui.field({
                    label: "歌词 .lrc（可选）",
                    control: '<div class="ui-dropzone" style="padding:16px">' + icon("file") + "<div>选择歌词文件并预览</div></div>"
                }) +
                ui.field({
                    label: "分组",
                    control:
                        '<div class="ui-chips" data-role="music-groups">' +
                        data.musicGroups
                            .slice(1)
                            .map(function (group) {
                                return ui.chip(group, {
                                    tone: "accent",
                                    attrs: 'data-group="' + util.esc(group) + '"',
                                    icon: "tag"
                                });
                            })
                            .join("") +
                        "</div>"
                }) +
                ui.callout("上传通过 XHR 显示百分比进度，字段为 Music / Lyrics / Cover 与多值 Group。", {
                    tone: "accent",
                    icon: "info"
                }) +
                "</div>"
        });

        var modal = document.querySelector("#modal-root .os-modal");
        if (modal) {
            var groupBox = modal.querySelector('[data-role="music-groups"]');
            groupBox.addEventListener("click", function (event) {
                var chip = event.target.closest("[data-group]");
                if (!chip) {
                    return;
                }
                var group = chip.getAttribute("data-group");
                if (draft.groups.indexOf(group) >= 0) {
                    draft.groups = draft.groups.filter(function (item) {
                        return item !== group;
                    });
                    chip.classList.remove("ui-chip--active");
                } else {
                    draft.groups.push(group);
                    chip.classList.add("ui-chip--active");
                }
                void groupBox;
            });
        }

        promise.then(function (ok) {
            if (!ok) {
                return;
            }
            DpzOS.toast({
                title: "上传完成（演示）",
                text: "已加入音乐库",
                tone: "success",
                icon: "upload"
            });
        });
    }

    DpzOS.registerApp({
        id: "music-app",
        name: "音乐管理",
        en: "Music",
        icon: "music",
        tone: "magenta",
        group: "内容创作",
        size: { w: 1180, h: 700 },
        singleton: true,
        desc: "音乐上传、歌词封面与在线试听",
        render: render,
        mount: bind
    });

    var LYRIC = [
        "[00:12.30]霓虹在雨里融化",
        "[00:18.60]代码像河流穿过指缝",
        "[00:25.10]我把夜晚折叠成一行注释",
        "[00:32.40]写给未来的自己",
        "",
        "[00:41.00]Neon drive, neon drive",
        "[00:47.50]穿过没有星星的夜",
        "[00:54.80]Neon drive, neon drive",
        "[01:01.20]直到天亮之前"
    ].join("\n");

    function renderDetail(ctx) {
        var state = ctx.state;
        var track = data.musicTracks.find(function (item) {
            return item.id === (ctx.params && ctx.params.id);
        });
        if (!track) {
            track = data.musicTracks[0];
        }
        state.playing = state.playing === undefined ? true : state.playing;
        state.groups = state.groups || track.groups.slice();

        return (
            '<div class="ui-page">' +
            '<div class="ui-head">' +
            '<div class="ui-head__text">' +
            '<div class="ui-eyebrow">内容创作 · MUSIC / DETAIL</div>' +
            '<h1 class="ui-title">' +
            util.esc(track.title) +
            "</h1>" +
            '<div class="ui-subtitle">' +
            util.esc(track.artist) +
            " · " +
            util.esc(track.fileName) +
            "</div>" +
            "</div>" +
            '<div class="ui-head__actions">' +
            ui.btn({ label: "返回列表", icon: "arrow-left", attrs: 'data-action="back"' }) +
            ui.btn({ label: "保存", icon: "check", variant: "primary", attrs: 'data-action="save"' }) +
            "</div>" +
            "</div>" +
            '<div class="ui-grid ui-grid--2">' +
            '<div class="ui-stack">' +
            ui.panel({
                title: "封面",
                icon: "image",
                sub: track.hasLyrics ? "含歌词" : "无歌词",
                body:
                    ui.cover(track.id, {
                        cls: "ui-cover--square",
                        center: icon("music"),
                        tag: ui.badge(track.groups[0] || "未分组", "accent")
                    }) +
                    '<div class="ui-row" style="margin-top:12px">' +
                    ui.btn({ label: "更换封面", icon: "upload", size: "sm", attrs: 'data-action="cover"' }) +
                    ui.btn({ label: "更换歌词", icon: "file", size: "sm", attrs: 'data-action="lyric"' }) +
                    "</div>"
            }) +
            ui.panel({
                title: "分组",
                icon: "tag",
                body:
                    '<div class="ui-chips" data-role="detail-groups">' +
                    data.musicGroups
                        .slice(1)
                        .map(function (group) {
                            return ui.chip(group, {
                                tone: state.groups.indexOf(group) >= 0 ? "accent" : null,
                                icon: "tag",
                                click: true,
                                attrs: 'data-group="' + util.esc(group) + '"'
                            });
                        })
                        .join("") +
                    "</div>"
            }) +
            "</div>" +
            '<div class="ui-stack">' +
            ui.panel({
                title: "在线试听",
                icon: "play",
                sub: util.formatDuration(track.duration),
                body:
                    '<div class="app-music__player">' +
                    '<div class="app-music__bar"><i style="width:' +
                    (state.playing ? "38" : "0") +
                    '%"></i></div>' +
                    '<div class="app-music__controls">' +
                    '<button type="button" class="ui-iconbtn" data-action="toggle" aria-label="播放/暂停">' +
                    icon(state.playing ? "pause" : "play") +
                    "</button>" +
                    "<span>" +
                    util.formatDuration(state.playing ? track.duration * 0.38 : 0) +
                    " / " +
                    util.formatDuration(track.duration) +
                    "</span>" +
                    '<span class="u-grow"></span>' +
                    icon("volume") +
                    "</div>" +
                    "</div>" +
                    '<div class="app-settings__about" style="margin-top:14px"><dl>' +
                    "<dt>来源</dt><dd>" +
                    util.esc(track.source) +
                    "</dd>" +
                    "<dt>文件大小</dt><dd>" +
                    util.esc(track.size) +
                    "</dd>" +
                    "<dt>上传时间</dt><dd>" +
                    util.esc(util.formatDate(track.uploadedAt) + " " + util.formatTime(track.uploadedAt)) +
                    "</dd>" +
                    "<dt>最后修改</dt><dd>" +
                    util.esc(util.formatDate(track.updatedAt) + " " + util.formatTime(track.updatedAt)) +
                    "</dd></dl></div>"
            }) +
            ui.panel({
                title: "歌词",
                icon: "file",
                actions: ui.badge(track.hasLyrics ? "LRC" : "无", track.hasLyrics ? "success" : "warning"),
                body: '<div class="app-music__lyric">' + util.esc(track.hasLyrics ? LYRIC : "尚未上传歌词") + "</div>"
            }) +
            ui.callout("保存走 <b>PATCH /api/Music/information</b>（multipart：Lyric / Cover / Id / Group）。", {
                tone: "accent",
                icon: "info"
            }) +
            "</div>" +
            "</div>" +
            "</div>"
        );
    }

    function bindDetail(rootNode, ctx) {
        var state = ctx.state;
        var track = data.musicTracks.find(function (item) {
            return item.id === (ctx.params && ctx.params.id);
        });
        if (!track) {
            track = data.musicTracks[0];
        }

        rootNode.addEventListener("click", function (event) {
            var groupChip = event.target.closest("[data-group]");
            if (groupChip) {
                var group = groupChip.getAttribute("data-group");
                if (state.groups.indexOf(group) >= 0) {
                    state.groups = state.groups.filter(function (item) {
                        return item !== group;
                    });
                } else {
                    state.groups.push(group);
                }
                ctx.rerender();
                return;
            }
            var action = event.target.closest("[data-action]");
            if (!action) {
                return;
            }
            var kind = action.getAttribute("data-action");
            if (kind === "back") {
                ctx.open("music-app");
                ctx.close();
            }
            if (kind === "toggle") {
                state.playing = !state.playing;
                ctx.rerender();
            }
            if (kind === "cover" || kind === "lyric") {
                DpzOS.toast({
                    title: kind === "cover" ? "选择新的封面" : "选择新的歌词文件",
                    text: "演示环境不会真正上传",
                    tone: "info",
                    icon: "upload"
                });
            }
            if (kind === "save") {
                track.groups = state.groups.slice();
                track.updatedAt = new Date();
                DpzOS.toast({ title: "音乐信息已保存", tone: "success", icon: "check-circle" });
                ctx.setSubtitle(util.formatDuration(track.duration) + " · 已保存");
            }
        });
    }

    DpzOS.registerApp({
        id: "music-detail",
        name: "音乐详情",
        en: "Music detail",
        icon: "music",
        tone: "magenta",
        group: "内容创作",
        size: { w: 1040, h: 720 },
        desc: "试听、歌词、封面与分组",
        render: renderDetail,
        mount: bindDetail
    });
})(window.DpzOS);
