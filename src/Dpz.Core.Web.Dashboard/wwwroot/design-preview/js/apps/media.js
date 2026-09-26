(function (DpzOS) {
    "use strict";

    var ui = DpzOS.ui;
    var util = DpzOS.util;
    var icon = DpzOS.icon;
    var data = DpzOS.data;

    var PAGE_SIZE = 8;

    function filtered(state) {
        return data.pictures.filter(function (picture) {
            if (state.type && state.type !== "全部" && picture.category !== state.type) {
                return false;
            }
            if (state.tag && state.tag !== "全部" && picture.tags.indexOf(state.tag) < 0) {
                return false;
            }
            if (state.desc && picture.desc.toLowerCase().indexOf(state.desc.toLowerCase()) < 0) {
                return false;
            }
            return true;
        });
    }

    function renderGridCard(picture) {
        return (
            '<article class="ui-card app-gallery__card" data-id="' +
            picture.id +
            '">' +
            ui.cover(picture.id, {
                cls: "ui-cover--photo",
                center: icon("image"),
                tag: ui.badge(picture.category, "accent"),
                time: picture.dimensions
            }) +
            '<div class="ui-card__body">' +
            '<div class="ui-card__title u-ellipsis" title="' +
            util.esc(picture.desc) +
            '">' +
            util.esc(picture.desc) +
            "</div>" +
            '<div class="ui-card__meta">' +
            ui.chip(picture.tags[0] || "未标签", { icon: "tag" }) +
            "<span>" +
            util.esc(picture.size) +
            "</span>" +
            "</div>" +
            '<div class="ui-card__meta">' +
            "<span>" +
            util.esc(picture.uploader) +
            "</span>" +
            "<span>" +
            util.esc(util.relativeTime(picture.uploadedAt)) +
            "</span>" +
            "</div>" +
            "</div>" +
            '<div class="ui-card__hover">' +
            ui.btn({
                label: "预览",
                icon: "eye",
                size: "sm",
                variant: "primary",
                attrs: 'data-action="preview" data-id="' + picture.id + '"'
            }) +
            ui.btn({
                label: "编辑",
                icon: "edit",
                size: "sm",
                attrs: 'data-action="edit" data-id="' + picture.id + '"'
            }) +
            ui.btn({
                label: "删除",
                icon: "trash",
                size: "sm",
                variant: "danger",
                attrs: 'data-action="delete" data-id="' + picture.id + '"'
            }) +
            "</div>" +
            "</article>"
        );
    }

    function renderGridView(rows) {
        return (
            '<div class="app-gallery__grid">' +
            rows.map(renderGridCard).join("") +
            "</div>"
        );
    }

    function renderTableView(rows) {
        return ui.table({
            columns: [
                { label: "预览" },
                { label: "描述" },
                { label: "标签" },
                { label: "类型" },
                { label: "尺寸 / 大小" },
                { label: "上传" },
                { label: "操作", align: "right" }
            ],
            rows: rows.map(function (picture) {
                return {
                    cells: [
                        '<div style="width:64px">' +
                            ui.cover(picture.id, { cls: "ui-cover--photo", center: icon("image"), attrs: 'data-action="preview" data-id="' + picture.id + '"' }) +
                            "</div>",
                        '<div class="ui-table__main">' + util.esc(picture.desc) + "</div>" +
                            '<div class="ui-table__sub u-mono">' + util.esc(picture.md5) + "</div>",
                        picture.tags
                            .map(function (tag) {
                                return '<span class="app-article__tag">' + util.esc(tag) + "</span>";
                            })
                            .join(" "),
                        ui.badge(picture.category, "violet"),
                        '<span class="u-mono">' +
                            util.esc(picture.dimensions) +
                            "</span><div class='ui-table__sub'>" +
                            util.esc(picture.size) +
                            "</div>",
                        '<span class="u-muted">' +
                            util.esc(picture.uploader) +
                            '</span><div class="ui-table__sub">' +
                            util.esc(util.relativeTime(picture.uploadedAt)) +
                            "</div>",
                        '<div class="ui-table__actions">' +
                            ui.iconBtn("edit", { label: "编辑", attrs: 'data-action="edit" data-id="' + picture.id + '"' }) +
                            ui.iconBtn("trash", { label: "删除", danger: true, attrs: 'data-action="delete" data-id="' + picture.id + '"' }) +
                            "</div>"
                    ]
                };
            })
        });
    }

    function render(ctx) {
        var state = ctx.state;
        state.page = state.page || 1;
        state.view = state.view || "grid";
        state.type = state.type || "全部";
        state.tag = state.tag || "全部";
        state.desc = state.desc || "";

        var rows = filtered(state);
        var totalPages = Math.max(1, Math.ceil(rows.length / PAGE_SIZE));
        state.page = util.clamp(state.page, 1, totalPages);
        var pageRows = rows.slice((state.page - 1) * PAGE_SIZE, state.page * PAGE_SIZE);
        var tags = ["全部"].concat(
            data.tags.slice(0, 10)
        );

        return (
            '<div class="ui-page">' +
            '<div class="ui-head">' +
            '<div class="ui-head__text">' +
            '<div class="ui-eyebrow">内容创作 · GALLERY</div>' +
            '<h1 class="ui-title">相册管理</h1>' +
            '<div class="ui-subtitle">共 ' +
            data.pictures.length +
            " 张图片 · 单文件上限 100MB · 支持 jpg / png / gif / webp / svg</div>" +
            "</div>" +
            '<div class="ui-head__actions">' +
            ui.btn({ label: "上传图片", icon: "upload", variant: "primary", attrs: 'data-action="upload"' }) +
            ui.btn({ label: "刷新", icon: "refresh", attrs: 'data-action="refresh"' }) +
            "</div>" +
            "</div>" +
            ui.toolbar([
                [
                    '<div class="ui-seg">' +
                        '<button type="button" class="ui-seg__btn' +
                        (state.view === "grid" ? " is-active" : "") +
                        '" data-view="grid">' +
                        icon("grid") +
                        "网格</button>" +
                        '<button type="button" class="ui-seg__btn' +
                        (state.view === "table" ? " is-active" : "") +
                        '" data-view="table">' +
                        icon("list") +
                        "表格</button>" +
                        "</div>"
                ],
                [
                    ui.select({
                        options: data.pictureTypes,
                        value: state.type,
                        attrs: 'data-role="filter-type"'
                    }),
                    ui.select({
                        options: tags,
                        value: state.tag,
                        attrs: 'data-role="filter-tag"'
                    }),
                    ui.search({
                        placeholder: "搜索描述，回车确认",
                        value: state.desc,
                        attrs: 'data-role="search-desc"'
                    })
                ]
            ]) +
            (state.view === "grid" ? renderGridView(pageRows) : renderTableView(pageRows)) +
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
            var view = event.target.closest("[data-view]");
            if (view) {
                state.view = view.getAttribute("data-view");
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
            var picture = data.pictures.find(function (item) {
                return item.id === id;
            });

            if (kind === "upload") {
                openUpload(ctx);
            }
            if (kind === "refresh") {
                ctx.rerender();
                DpzOS.toast({ title: "相册已刷新", tone: "success", icon: "refresh" });
            }
            if (kind === "preview" && picture) {
                DpzOS.lightbox.open({
                    title: picture.name,
                    desc: picture.desc,
                    seed: picture.id,
                    metaHtml:
                        "尺寸：" +
                        util.esc(picture.dimensions) +
                        "<br>大小：" +
                        util.esc(picture.size) +
                        "<br>类型：" +
                        util.esc(picture.category) +
                        "<br>上传人：" +
                        util.esc(picture.uploader) +
                        "<br>MD5：" +
                        util.esc(picture.md5)
                });
            }
            if (kind === "edit" && picture) {
                DpzOS.dialog
                    .prompt({
                        title: "编辑描述",
                        label: picture.name,
                        value: picture.desc,
                        icon: "edit"
                    })
                    .then(function (value) {
                        if (value !== null) {
                            picture.desc = value;
                            ctx.rerender();
                            DpzOS.toast({ title: "已保存", tone: "success", icon: "check-circle" });
                        }
                    });
            }
            if (kind === "delete" && picture) {
                ctx.confirm({
                    title: "删除这张图片？",
                    text: picture.name + "（" + picture.size + "）删除后 CDN 缓存将在下次刷新时失效。",
                    confirmText: "删除",
                    tone: "danger",
                    icon: "trash"
                }).then(function (ok) {
                    if (ok) {
                        data.pictures = data.pictures.filter(function (item) {
                            return item.id !== id;
                        });
                        ctx.rerender();
                        DpzOS.toast({ title: "图片已删除", tone: "success", icon: "check-circle" });
                    }
                });
            }
        });

        var search = rootNode.querySelector('[data-role="search-desc"]');
        if (search) {
            search.addEventListener("keydown", function (event) {
                if (event.key === "Enter") {
                    state.desc = search.value.trim();
                    state.page = 1;
                    ctx.rerender();
                }
            });
        }

        rootNode.addEventListener("change", function (event) {
            if (event.target.matches('[data-role="filter-type"]')) {
                state.type = event.target.value;
                state.page = 1;
                ctx.rerender();
            }
            if (event.target.matches('[data-role="filter-tag"]')) {
                state.tag = event.target.value;
                state.page = 1;
                ctx.rerender();
            }
        });
    }

    function openUpload(ctx) {
        ctx
            .dialog({
                title: "上传图片",
                subtitle: "拖拽或点击选择，单张最大 100MB",
                icon: "upload",
                confirmText: "开始上传",
                html:
                    '<div class="ui-stack">' +
                    '<div class="ui-dropzone">' +
                    icon("upload") +
                    "<div>把图片拖到这里，或点击选择文件</div>" +
                    '<div class="ui-field__hint">客户端会先缩放到 1000×1000 再上传，并显示实时进度</div>' +
                    "</div>" +
                    ui.field({ label: "标签", control: ui.select({ options: ["设计", "壁纸", "生活", "代码"] }) }) +
                    ui.field({
                        label: "描述",
                        control: '<textarea class="ui-textarea" placeholder="补充说明…"></textarea>'
                    }) +
                    "</div>"
            })
            .then(function (ok) {
                if (ok) {
                    DpzOS.toast({ title: "上传完成（演示）", text: "已加入相册列表", tone: "success", icon: "upload" });
                }
            });
    }

    DpzOS.registerApp({
        id: "gallery",
        name: "相册管理",
        en: "Gallery",
        icon: "image",
        tone: "magenta",
        group: "内容创作",
        size: { w: 1160, h: 720 },
        singleton: true,
        desc: "图片网格/表格双视图与上传",
        render: render,
        mount: bind
    });

    function renderVideo(ctx) {
        var state = ctx.state;
        state.page = state.page || 1;
        var videos = data.videos;
        var pageRows = videos.slice((state.page - 1) * 6, state.page * 6);

        return (
            '<div class="ui-page">' +
            '<div class="ui-head">' +
            '<div class="ui-head__text">' +
            '<div class="ui-eyebrow">内容创作 · VIDEO</div>' +
            '<h1 class="ui-title">视频管理</h1>' +
            '<div class="ui-subtitle">云端视频元数据与封面管理，播放走 HLS 自适应码率</div>' +
            "</div>" +
            '<div class="ui-head__actions">' +
            ui.btn({ label: "刷新", icon: "refresh", attrs: 'data-action="refresh"' }) +
            "</div>" +
            "</div>" +
            ui.toolbar([
                [
                    ui.chip("共 " + videos.length + " 个视频", { icon: "video" }),
                    ui.chip(
                        "弹幕 " + videos.reduce(function (sum, item) { return sum + item.danmaku; }, 0),
                        { icon: "comments" }
                    ),
                    ui.chip(
                        "评论 " + videos.reduce(function (sum, item) { return sum + item.comments; }, 0),
                        { icon: "message-circle" }
                    )
                ]
            ]) +
            '<div class="app-video__grid">' +
            pageRows
                .map(function (video) {
                    return (
                        '<article class="ui-card" data-id="' +
                        video.id +
                        '">' +
                        ui.cover(video.id, {
                            cls: "ui-cover--wide app-video__cover",
                            center: "",
                            tag: ui.badge(video.tags[0] || "视频", "accent"),
                            time: util.formatDuration(video.duration)
                        }) +
                        '<div class="app-video__play" data-action="play" data-id="' +
                        video.id +
                        '"><i>' +
                        icon("play") +
                        "</i></div>" +
                        '<div class="ui-card__body">' +
                        '<div class="ui-card__title">' +
                        util.esc(video.title) +
                        "</div>" +
                        '<div class="ui-meta">' +
                        util.esc(video.subtitle) +
                        "</div>" +
                        '<div class="app-video__stats">' +
                        "<span>" +
                        icon("eye") +
                        util.formatCompact(video.views) +
                        "</span>" +
                        "<span>" +
                        icon("comments") +
                        video.danmaku +
                        "</span>" +
                        "<span>" +
                        icon("message-circle") +
                        video.comments +
                        "</span>" +
                        "</div>" +
                        '<div class="ui-row ui-row--tight">' +
                        ui.btn({ label: "编辑", icon: "edit", size: "sm", attrs: 'data-action="edit" data-id="' + video.id + '"' }) +
                        ui.btn({ label: "封面截图", icon: "scissors", size: "sm", attrs: 'data-action="screenshot" data-id="' + video.id + '"' }) +
                        ui.btn({ label: "弹幕", icon: "comments", size: "sm", variant: "ghost", attrs: 'data-action="danmaku" data-id="' + video.id + '"' }) +
                        "</div>" +
                        "</div>" +
                        "</article>"
                    );
                })
                .join("") +
            "</div>" +
            ui.pager({
                page: state.page,
                totalPages: Math.max(1, Math.ceil(videos.length / 6)),
                pageSize: 6,
                count: videos.length
            }) +
            "</div>"
        );
    }

    function bindVideo(rootNode, ctx) {
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
            var video = data.videos.find(function (item) {
                return item.id === id;
            });
            if (!video && kind !== "refresh") {
                return;
            }
            if (kind === "play") {
                ctx.open("video-player", { id: id });
            }
            if (kind === "danmaku") {
                ctx.open("danmaku", { group: video.title });
            }
            if (kind === "refresh") {
                ctx.rerender();
                DpzOS.toast({ title: "视频列表已刷新", tone: "success", icon: "refresh" });
            }
            if (kind === "edit") {
                ctx
                    .dialog({
                        title: "编辑视频信息",
                        subtitle: video.title,
                        icon: "edit",
                        confirmText: "保存",
                        html:
                            '<div class="ui-stack">' +
                            ui.field({ label: "标题", required: true, control: '<input class="ui-input" value="' + util.esc(video.title) + '">' }) +
                            ui.field({ label: "副标题", control: '<input class="ui-input" value="' + util.esc(video.subtitle) + '">' }) +
                            ui.field({
                                label: "标签",
                                hint: "回车或逗号添加，最多 10 个",
                                control:
                                    '<div class="ui-input" style="display:flex;align-items:center;gap:6px;height:auto;min-height:var(--ctl-h);flex-wrap:wrap;padding:6px 8px">' +
                                    video.tags
                                        .map(function (tag) {
                                            return ui.chip(tag, { removable: true, tone: "accent" });
                                        })
                                        .join("") +
                                    '<input style="border:0;background:transparent;flex:1;min-width:80px;outline:none" placeholder="添加标签…">' +
                                    "</div>"
                            }) +
                            ui.field({ label: "描述", control: '<textarea class="ui-textarea">' + util.esc(video.desc) + "</textarea>" }) +
                            "</div>"
                    })
                    .then(function (ok) {
                        if (ok) {
                            DpzOS.toast({ title: "视频信息已保存", tone: "success", icon: "check-circle" });
                        }
                    });
            }
            if (kind === "screenshot") {
                ctx
                    .dialog({
                        title: "设置封面",
                        subtitle: "输入截取时间点（秒），服务端会截帧并生成封面",
                        icon: "scissors",
                        confirmText: "开始截取",
                        html:
                            '<div class="ui-stack">' +
                            ui.field({
                                label: "时间点（0.01 ~ " + video.duration + "s）",
                                control: '<input class="ui-input" type="number" min="0.01" max="' + video.duration + '" step="0.01" value="' + Math.round(video.duration / 3) + '">'
                            }) +
                            ui.cover(video.id, {
                                cls: "ui-cover--wide",
                                center: icon("camera"),
                                time: util.formatDuration(video.duration / 3)
                            }) +
                            "</div>"
                    })
                    .then(function (ok) {
                        if (ok) {
                            DpzOS.toast({ title: "封面已更新", text: "CDN 缓存将在几分钟内刷新", tone: "success", icon: "camera" });
                        }
                    });
            }
        });
    }

    DpzOS.registerApp({
        id: "video",
        name: "视频管理",
        en: "Video",
        icon: "video",
        tone: "violet",
        group: "内容创作",
        size: { w: 1160, h: 720 },
        singleton: true,
        desc: "视频元数据、封面截图与弹幕入口",
        render: renderVideo,
        mount: bindVideo
    });

    function renderPlayer(ctx) {
        var video = data.videos.find(function (item) {
            return item.id === (ctx.params && ctx.params.id);
        });
        if (!video) {
            return ui.empty({ icon: "video", title: "视频不存在" });
        }
        var related = data.danmaku.filter(function (item) {
            return item.group.indexOf(video.title.slice(0, 4)) >= 0;
        });
        if (related.length === 0) {
            related = data.danmaku.slice(0, 5);
        }
        return (
            '<div class="ui-page">' +
            '<div class="app-player">' +
            '<div style="' +
            util.coverStyle(video.id) +
            ';position:absolute;inset:0"></div>' +
            '<div class="app-player__overlay">' +
            '<div class="app-player__bar"><i></i></div>' +
            '<div class="app-player__controls">' +
            icon("play") +
            "<span>" +
            util.formatDuration(video.duration / 3) +
            " / " +
            util.formatDuration(video.duration) +
            "</span>" +
            '<span class="u-grow"></span>' +
            icon("volume") +
            icon("maximize") +
            "</div>" +
            "</div>" +
            "</div>" +
            '<div class="ui-grid ui-grid--2">' +
            ui.panel({
                title: video.title,
                sub: video.subtitle,
                body:
                    '<div class="ui-meta u-mono">' +
                    icon("eye") +
                    util.formatCompact(video.views) +
                    " 播放 · " +
                    icon("comments") +
                    video.danmaku +
                    " 弹幕 · " +
                    icon("message-circle") +
                    video.comments +
                    " 评论</div>" +
                    '<p class="u-muted" style="margin-top:10px;font-size:12.5px">' +
                    util.esc(video.desc) +
                    "</p>" +
                    '<div class="ui-chips" style="margin-top:12px">' +
                    video.tags
                        .map(function (tag) {
                            return ui.chip(tag, { icon: "tag" });
                        })
                        .join("") +
                    "</div>" +
                    '<div class="ui-row" style="margin-top:14px">' +
                    ui.btn({ label: "编辑信息", icon: "edit", size: "sm", attrs: 'data-action="edit"' }) +
                    ui.btn({ label: "设置封面", icon: "scissors", size: "sm", attrs: 'data-action="screenshot"' }) +
                    ui.btn({ label: "打开弹幕管理", icon: "comments", size: "sm", variant: "primary", attrs: 'data-action="danmaku"' }) +
                    "</div>"
            }) +
            ui.panel({
                title: "实时弹幕",
                icon: "message-circle",
                sub: related.length + " 条",
                body:
                    '<div class="ui-list">' +
                    related
                        .map(function (item) {
                            return (
                                '<div class="ui-item">' +
                                '<span class="ui-mono u-dim" style="font-size:11px">' +
                                item.time.toFixed(1) +
                                "s</span>" +
                                '<div class="ui-item__main"><div class="ui-item__title" style="color:' +
                                util.esc(item.color) +
                                '">' +
                                util.esc(item.text) +
                                '</div><div class="ui-item__sub">' +
                                util.esc(item.position + " · " + item.size) +
                                "</div></div>" +
                                "</div>"
                            );
                        })
                        .join("") +
                    "</div>"
            }) +
            "</div>" +
            "</div>"
        );
    }

    function bindPlayer(rootNode, ctx) {
        rootNode.addEventListener("click", function (event) {
            var action = event.target.closest("[data-action]");
            if (!action) {
                return;
            }
            var kind = action.getAttribute("data-action");
            if (kind === "danmaku") {
                ctx.open("danmaku", { group: ctx.params.id });
            }
            if (kind === "edit" || kind === "screenshot") {
                DpzOS.toast({ title: "请在视频管理列表中操作", text: "演示：打开视频管理窗口", tone: "info" });
                ctx.open("video");
            }
        });
    }

    DpzOS.registerApp({
        id: "video-player",
        name: "视频播放",
        en: "Player",
        icon: "play",
        tone: "violet",
        group: "内容创作",
        size: { w: 1040, h: 700 },
        desc: "HLS 播放与实时弹幕",
        render: renderPlayer,
        mount: bindPlayer
    });
})(window.DpzOS);
