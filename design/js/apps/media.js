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
            if (state.tags && state.tags.length) {
                var hit = state.tags.some(function (tag) {
                    return picture.tags.indexOf(tag) >= 0;
                });
                if (!hit) {
                    return false;
                }
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
        state.tags = state.tags || [];
        state.desc = state.desc || "";

        var rows = filtered(state);
        var totalPages = Math.max(1, Math.ceil(rows.length / PAGE_SIZE));
        state.page = util.clamp(state.page, 1, totalPages);
        var pageRows = rows.slice((state.page - 1) * PAGE_SIZE, state.page * PAGE_SIZE);
        var tagPool = [];
        data.pictures.forEach(function (picture) {
            picture.tags.forEach(function (tag) {
                if (tagPool.indexOf(tag) < 0) {
                    tagPool.push(tag);
                }
            });
        });

        var tagFilter =
            '<div class="ui-chips app-gallery__tagfilter">' +
            '<button type="button" class="ui-chip ui-chip--click' +
            (state.tags.length === 0 ? " ui-chip--active" : "") +
            '" data-tag-filter="__all__">' +
            icon("layers") +
            "全部标签</button>" +
            tagPool
                .map(function (tag) {
                    return (
                        '<button type="button" class="ui-chip ui-chip--click' +
                        (state.tags.indexOf(tag) >= 0 ? " ui-chip--active" : "") +
                        '" data-tag-filter="' +
                        util.esc(tag) +
                        '">' +
                        icon("tag") +
                        util.esc(tag) +
                        "</button>"
                    );
                })
                .join("") +
            "</div>";

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
                        "</div>",
                    ui.select({
                        options: data.pictureTypes,
                        value: state.type,
                        attrs: 'data-role="filter-type"'
                    }),
                    ui.search({
                        placeholder: "搜索描述，回车确认",
                        value: state.desc,
                        attrs: 'data-role="search-desc"'
                    })
                ],
                [
                    ui.badge(
                        state.tags.length ? "已选 " + state.tags.length + " 个标签" : "标签可多选",
                        state.tags.length ? "accent" : null,
                        "tag"
                    )
                ]
            ]) +
            tagFilter +
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
            var tagFilter = event.target.closest("[data-tag-filter]");
            if (tagFilter) {
                var value = tagFilter.getAttribute("data-tag-filter");
                if (value === "__all__") {
                    state.tags = [];
                } else if (state.tags.indexOf(value) >= 0) {
                    state.tags = state.tags.filter(function (tag) {
                        return tag !== value;
                    });
                } else {
                    state.tags = state.tags.concat([value]);
                }
                state.page = 1;
                ctx.rerender();
                return;
            }

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
                        "<br>标签：" +
                        util.esc(picture.tags.join("、")) +
                        "<br>上传人：" +
                        util.esc(picture.uploader) +
                        "<br>MD5：" +
                        util.esc(picture.md5)
                });
            }
            if (kind === "edit" && picture) {
                openPictureEditor(ctx, picture);
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
        });
    }

    function openPictureEditor(ctx, picture) {
        var draft = { desc: picture.desc, tags: picture.tags.slice() };
        var promise = ctx.dialog({
            title: "编辑图片",
            subtitle: picture.name + " · " + picture.dimensions + " · " + picture.size,
            icon: "edit",
            confirmText: "保存",
            html:
                '<div class="ui-stack">' +
                ui.cover(picture.id, {
                    cls: "ui-cover--wide",
                    center: icon("image"),
                    time: picture.dimensions
                }) +
                ui.field({
                    label: "描述",
                    control:
                        '<textarea class="ui-textarea" data-role="picture-desc">' +
                        util.esc(picture.desc) +
                        "</textarea>"
                }) +
                ui.field({
                    label: "标签",
                    hint: "点击 × 删除，输入后回车添加",
                    control:
                        '<div class="ui-chips" data-role="picture-tags"></div>' +
                        '<div class="ui-search" style="margin-top:8px">' +
                        icon("tag") +
                        '<input class="ui-input" data-role="picture-tag-input" data-ignore-enter placeholder="新标签，例如：壁纸">' +
                        "</div>"
                }) +
                ui.callout("标签会同步用于筛选器与前台展示，移动端同样支持多选筛选。", {
                    icon: "info"
                }) +
                "</div>"
        });

        var modal = document.querySelector("#modal-root .os-modal");
        if (modal) {
            var tagsBox = modal.querySelector('[data-role="picture-tags"]');
            var tagInput = modal.querySelector('[data-role="picture-tag-input"]');
            var descInput = modal.querySelector('[data-role="picture-desc"]');

            function renderTags() {
                tagsBox.innerHTML = draft.tags.length
                    ? draft.tags
                          .map(function (tag) {
                              return ui.chip(tag, {
                                  removable: true,
                                  tone: "accent",
                                  attrs: 'data-tag="' + util.esc(tag) + '"'
                              });
                          })
                          .join("")
                    : '<span class="u-dim" style="font-size:12px">暂无标签</span>';
            }

            renderTags();
            tagsBox.addEventListener("click", function (event) {
                var chip = event.target.closest(".ui-chip[data-tag]");
                if (chip && event.target.closest(".ui-chip__x")) {
                    var tag = chip.getAttribute("data-tag");
                    draft.tags = draft.tags.filter(function (item) {
                        return item !== tag;
                    });
                    renderTags();
                }
            });
            tagInput.addEventListener("keydown", function (event) {
                if (event.key !== "Enter") {
                    return;
                }
                event.preventDefault();
                var value = tagInput.value.trim();
                if (!value || draft.tags.indexOf(value) >= 0) {
                    return;
                }
                draft.tags.push(value);
                tagInput.value = "";
                renderTags();
            });
            descInput.addEventListener("input", function () {
                draft.desc = descInput.value;
            });
        }

        promise.then(function (ok) {
            if (!ok) {
                return;
            }
            picture.desc = draft.desc;
            picture.tags = draft.tags.slice();
            ctx.rerender();
            DpzOS.toast({ title: "图片信息已保存", tone: "success", icon: "check-circle" });
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
                        '<div class="app-video__cover">' +
                        ui.cover(video.id, {
                            cls: "ui-cover--wide",
                            center: "",
                            tag: ui.badge(video.tags[0] || "视频", "accent"),
                            time: util.formatDuration(video.duration)
                        }) +
                        '<div class="app-video__play" data-action="play" data-id="' +
                        video.id +
                        '"><i>' +
                        icon("play") +
                        "</i></div>" +
                        "</div>" +
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
                        ui.btn({ label: "封面截图", icon: "camera", size: "sm", attrs: 'data-action="screenshot" data-id="' + video.id + '"' }) +
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
                ctx.open("video-edit", { id: id });
            }
            if (kind === "screenshot") {
                ctx
                    .dialog({
                        title: "设置封面",
                        subtitle: "输入截取时间点（秒），服务端会截帧并生成封面",
                        icon: "camera",
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
            video = data.videos[0];
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
                    ui.btn({ label: "设置封面", icon: "camera", size: "sm", attrs: 'data-action="screenshot"' }) +
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
                ctx.open("video-edit", { id: ctx.params.id });
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

    function renderVideoEdit(ctx) {
        var state = ctx.state;
        var video = data.videos.find(function (item) {
            return item.id === (ctx.params && ctx.params.id);
        });
        if (!video) {
            video = data.videos[0];
        }
        state.form = state.form || {
            title: video.title,
            subtitle: video.subtitle,
            tags: video.tags.slice(),
            desc: video.desc,
            frame: Math.round(video.duration / 3),
            coverSeed: video.id
        };
        var form = state.form;
        var frames = [0.15, 0.3, 0.45, 0.6, 0.75, 0.9].map(function (ratio) {
            return {
                ratio: ratio,
                time: Math.round(video.duration * ratio),
                seed: video.id + "-f" + Math.round(ratio * 100)
            };
        });

        return (
            '<div class="ui-page">' +
            '<div class="ui-head">' +
            '<div class="ui-head__text">' +
            '<div class="ui-eyebrow">内容创作 · VIDEO / EDIT</div>' +
            '<h1 class="ui-title">编辑视频</h1>' +
            '<div class="ui-subtitle u-mono">' +
            util.esc(video.id + " · " + util.formatDuration(video.duration) + " · " + util.formatCompact(video.views) + " 播放") +
            "</div>" +
            "</div>" +
            '<div class="ui-head__actions">' +
            ui.btn({ label: "返回列表", icon: "arrow-left", attrs: 'data-action="back"' }) +
            ui.btn({ label: "重置", icon: "eraser", attrs: 'data-action="reset"' }) +
            ui.btn({ label: "保存", icon: "check", variant: "primary", attrs: 'data-action="save"' }) +
            "</div>" +
            "</div>" +
            '<div class="ui-grid ui-grid--2">' +
            '<div class="ui-stack">' +
            ui.panel({
                title: "封面预览",
                icon: "image",
                sub: "16:9",
                body:
                    '<div class="app-video__cover">' +
                    ui.cover(form.coverSeed, {
                        cls: "ui-cover--wide",
                        center: icon("play"),
                        time: util.formatDuration(video.duration)
                    }) +
                    "</div>"
            }) +
            ui.panel({
                title: "封面截图",
                icon: "camera",
                sub: "服务端按时间点截帧",
                body:
                    ui.field({
                        label: "时间点（秒）",
                        hint: "0.01 ~ " + video.duration + " 秒",
                        control:
                            '<div class="ui-row ui-row--tight">' +
                            '<input class="ui-input u-mono" style="width:auto;flex:1;min-width:120px" type="number" min="0.01" max="' +
                            video.duration +
                            '" step="0.01" value="' +
                            form.frame +
                            '" data-role="frame-input">' +
                            '<button type="button" class="ui-btn ui-btn--primary" data-action="capture">' +
                            icon("camera") +
                            "<span>截取</span></button>" +
                            "</div>"
                    }) +
                    '<div class="app-editor__gallery" style="margin-top:12px">' +
                    frames
                        .map(function (frame) {
                            return ui.cover(frame.seed, {
                                cls: "ui-cover--square",
                                center: icon("image"),
                                time: util.formatDuration(frame.time),
                                attrs: 'data-action="pick-frame" data-seed="' + util.esc(frame.seed) + '"'
                            });
                        })
                        .join("") +
                    "</div>"
            }) +
            "</div>" +
            '<div class="ui-stack">' +
            ui.panel({
                title: "基本信息",
                icon: "file",
                body:
                    ui.field({
                        label: "标题",
                        required: true,
                        control:
                            '<input class="ui-input" value="' +
                            util.esc(form.title) +
                            '" data-role="title-input">'
                    }) +
                    ui.field({
                        label: "副标题",
                        control:
                            '<input class="ui-input" value="' +
                            util.esc(form.subtitle) +
                            '" data-role="subtitle-input">'
                    })
            }) +
            ui.panel({
                title: "标签",
                icon: "tag",
                sub: form.tags.length + " / 10",
                body:
                    '<div class="ui-chips" data-role="tag-list">' +
                    form.tags
                        .map(function (tag) {
                            return ui.chip(tag, {
                                removable: true,
                                tone: "accent",
                                attrs: 'data-tag="' + util.esc(tag) + '"'
                            });
                        })
                        .join("") +
                    "</div>" +
                    '<div style="margin-top:10px">' +
                    ui.search({
                        placeholder: "输入标签后回车添加…",
                        attrs: 'data-role="tag-input" data-ignore-enter'
                    }) +
                    "</div>"
            }) +
            ui.panel({
                title: "描述",
                icon: "quote",
                body: '<textarea class="ui-textarea" style="min-height:140px" data-role="desc-input">' +
                    util.esc(form.desc) +
                    "</textarea>" +
                    '<div class="ui-row" style="margin-top:12px">' +
                    ui.btn({ label: "保存修改", icon: "check", variant: "primary", attrs: 'data-action="save"' }) +
                    ui.btn({ label: "取消", icon: "x", variant: "ghost", attrs: 'data-action="back"' }) +
                    "</div>"
            }) +
            ui.callout("保存走 <b>POST /api/Video</b>；封面截图走 <b>PATCH /api/Video/screenshot/{id}</b>，成功后 CDN 缓存会在数分钟内刷新。", {
                tone: "accent",
                icon: "info"
            }) +
            "</div>" +
            "</div>" +
            "</div>"
        );
    }

    function bindVideoEdit(rootNode, ctx) {
        var state = ctx.state;
        var video = data.videos.find(function (item) {
            return item.id === (ctx.params && ctx.params.id);
        });
        if (!video) {
            video = data.videos[0];
        }
        var form = state.form;

        function syncInput(selector, key) {
            var input = rootNode.querySelector(selector);
            if (input) {
                input.addEventListener("input", function () {
                    form[key] = input.value;
                });
            }
        }

        syncInput('[data-role="title-input"]', "title");
        syncInput('[data-role="subtitle-input"]', "subtitle");
        syncInput('[data-role="desc-input"]', "desc");
        syncInput('[data-role="frame-input"]', "frame");

        var tagInput = rootNode.querySelector('[data-role="tag-input"]');
        if (tagInput) {
            tagInput.addEventListener("keydown", function (event) {
                if (event.key !== "Enter") {
                    return;
                }
                var value = tagInput.value.trim();
                if (!value || form.tags.indexOf(value) >= 0 || form.tags.length >= 10) {
                    return;
                }
                form.tags.push(value);
                ctx.rerender();
            });
        }

        rootNode.addEventListener("click", function (event) {
            var chip = event.target.closest(".ui-chip[data-tag]");
            if (chip && event.target.closest(".ui-chip__x")) {
                var tag = chip.getAttribute("data-tag");
                form.tags = form.tags.filter(function (item) {
                    return item !== tag;
                });
                ctx.rerender();
                return;
            }
            var action = event.target.closest("[data-action]");
            if (!action) {
                return;
            }
            var kind = action.getAttribute("data-action");
            if (kind === "back") {
                ctx.open("video");
                ctx.close();
            }
            if (kind === "reset") {
                state.form = null;
                ctx.rerender();
                DpzOS.toast({ title: "已重置为未保存状态", tone: "info", icon: "eraser", timeout: 1600 });
            }
            if (kind === "capture") {
                var input = rootNode.querySelector('[data-role="frame-input"]');
                form.frame = util.clamp(Number(input.value) || 0, 0.01, video.duration);
                form.coverSeed = video.id + "-shot" + form.frame;
                ctx.rerender();
                DpzOS.toast({
                    title: "截图完成（演示）",
                    text: "时间点 " + form.frame.toFixed(2) + "s",
                    tone: "success",
                    icon: "camera"
                });
            }
            if (kind === "pick-frame") {
                form.coverSeed = action.getAttribute("data-seed");
                ctx.rerender();
            }
            if (kind === "save") {
                if (!form.title.trim()) {
                    DpzOS.toast({ title: "标题不能为空", tone: "danger", icon: "alert" });
                    return;
                }
                video.title = form.title.trim();
                video.subtitle = form.subtitle.trim();
                video.tags = form.tags.slice();
                video.desc = form.desc;
                DpzOS.toast({ title: "视频信息已保存", tone: "success", icon: "check-circle" });
                ctx.setSubtitle(video.id + " · 已保存");
            }
        });
    }

    DpzOS.registerApp({
        id: "video-edit",
        name: "编辑视频",
        en: "Video edit",
        icon: "edit",
        tone: "violet",
        group: "内容创作",
        size: { w: 1160, h: 720 },
        desc: "视频元数据、标签与封面截图",
        render: renderVideoEdit,
        mount: bindVideoEdit
    });
})(window.DpzOS);
