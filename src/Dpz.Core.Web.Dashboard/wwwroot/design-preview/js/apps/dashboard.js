(function (DpzOS) {
    "use strict";

    var ui = DpzOS.ui;
    var util = DpzOS.util;
    var icon = DpzOS.icon;

    function renderStat() {
        var stats = DpzOS.data.stats;
        return (
            '<div class="ui-grid ui-grid--6">' +
            ui.stat({
                label: "今日访问",
                value: util.formatNumber(stats.todayVisits),
                icon: "eye",
                delta: stats.deltaVisits,
                hint: "对比昨日"
            }) +
            ui.stat({
                label: "七日平均",
                value: util.formatNumber(stats.weekAverage),
                icon: "chart",
                delta: stats.deltaWeek,
                hint: "整体趋势",
                tone: "info"
            }) +
            ui.stat({
                label: "今日文章",
                value: stats.todayArticles,
                icon: "article",
                hint: "已发布",
                tone: "success"
            }) +
            ui.stat({
                label: "文章总量",
                value: util.formatNumber(stats.totalArticles),
                icon: "layers",
                hint: "含草稿",
                tone: "accent"
            }) +
            ui.stat({
                label: "TOP 路径访问",
                value: util.formatCompact(stats.topPathVisits),
                icon: "trending",
                hint: stats.topPath,
                tone: "warning"
            }) +
            ui.stat({
                label: "最慢请求",
                value: stats.slowestMs + " ms",
                icon: "clock",
                hint: "GET /api/Article/page",
                tone: "danger"
            }) +
            "</div>"
        );
    }

    function renderBrowsers() {
        var browsers = DpzOS.data.browsers;
        var total = browsers.reduce(function (sum, item) {
            return sum + item.value;
        }, 0);
        var ring = browsers
            .slice(0, 3)
            .map(function (item, index) {
                return "--p" + (index + 1) + ":" + Math.round((item.value / total) * 100);
            })
            .join(";");
        var colors = ["var(--c1)", "var(--c2)", "var(--c3)", "var(--c4)", "var(--c5)"];
        return (
            '<div class="app-donut">' +
            '<div class="app-donut__ring" style="' +
            ring +
            '">' +
            '<div class="app-donut__center">' +
            '<div class="ui-stat__value">' +
            total +
            "%</div>" +
            '<div class="ui-meta">浏览器样本</div>' +
            "</div>" +
            "</div>" +
            '<div class="app-donut__legend">' +
            browsers
                .map(function (item, index) {
                    return (
                        '<div class="app-donut__row"><i style="background:' +
                        colors[index % colors.length] +
                        '"></i>' +
                        util.esc(item.name) +
                        "<b>" +
                        item.value +
                        "%</b></div>"
                    );
                })
                .join("") +
            "</div>" +
            "</div>"
        );
    }

    function renderHourly() {
        var hourly = DpzOS.data.hourly;
        var max = Math.max.apply(null, hourly.values);
        return (
            '<div class="app-bars">' +
            hourly.values
                .map(function (value, index) {
                    var height = Math.round((value / max) * 100);
                    return (
                        '<div class="app-bars__col" title="' +
                        util.esc(hourly.labels[index] + ":00 · " + util.formatNumber(value)) +
                        '">' +
                        '<div class="app-bars__bar" style="height:' +
                        Math.max(4, height) +
                        '%"></div>' +
                        '<div class="app-bars__label">' +
                        util.esc(hourly.labels[index]) +
                        "</div>" +
                        "</div>"
                    );
                })
                .join("") +
            "</div>"
        );
    }

    function renderReferrers() {
        var referrers = DpzOS.data.referrers;
        var max = Math.max.apply(null, referrers.map(function (item) { return item.value; }));
        return ui.barList(
            referrers.map(function (item) {
                return {
                    name: item.name,
                    sub: item.raw,
                    value: item.value,
                    display: util.formatNumber(item.value),
                    percent: Math.round((item.value / max) * 100)
                };
            })
        );
    }

    function renderTopPages() {
        var pages = DpzOS.data.topPages;
        var max = Math.max.apply(null, pages.map(function (item) { return item.value; }));
        return ui.barList(
            pages.map(function (item) {
                return {
                    name: item.path,
                    value: item.value,
                    display: util.formatNumber(item.value),
                    percent: Math.round((item.value / max) * 100)
                };
            })
        );
    }

    function renderSlowRequests() {
        return ui.table({
            columns: [
                { label: "方法" },
                { label: "路径" },
                { label: "耗时", align: "right" },
                { label: "状态" },
                { label: "设备" }
            ],
            rows: DpzOS.data.slowRequests.map(function (item) {
                var tone = item.status >= 500 ? "danger" : item.status >= 400 ? "warning" : "success";
                return {
                    cells: [
                        '<code class="u-mono">' + util.esc(item.method) + "</code>",
                        '<span class="u-ellipsis" style="max-width:240px;display:inline-block">' +
                            util.esc(item.path) +
                            "</span>",
                        '<span class="u-mono" style="color:var(--warning)">' +
                            item.ms +
                            " ms</span>",
                        ui.badge(String(item.status), tone),
                        '<span class="u-muted">' + util.esc(item.browser + " / " + item.device) + "</span>"
                    ]
                };
            })
        });
    }

    function renderBanners() {
        return (
            '<div class="app-banner" data-role="banner">' +
            DpzOS.data.banners
                .map(function (banner, index) {
                    return (
                        '<div class="app-banner__slide' +
                        (index === 0 ? " is-active" : "") +
                        '" style="' +
                        util.coverStyle(banner.seed) +
                        '">' +
                        "<h4>" +
                        util.esc(banner.title) +
                        "</h4>" +
                        "<p>" +
                        util.esc(banner.desc) +
                        "</p>" +
                        "</div>"
                    );
                })
                .join("") +
            '<div class="app-banner__dots">' +
            DpzOS.data.banners
                .map(function (banner, index) {
                    return '<span class="app-banner__dot' + (index === 0 ? " is-active" : "") + '"></span>';
                })
                .join("") +
            "</div>" +
            "</div>"
        );
    }

    function renderLatest() {
        return (
            '<div class="ui-list">' +
            DpzOS.data.latestArticles
                .map(function (article) {
                    return (
                        '<button type="button" class="ui-item" data-action="edit-article" data-id="' +
                        article.id +
                        '">' +
                        ui.cover(String(article.id), { cls: "ui-cover--square", center: icon("article") }) +
                        '<div class="ui-item__main">' +
                        '<div class="ui-item__title u-ellipsis">' +
                        util.esc(article.title) +
                        "</div>" +
                        '<div class="ui-item__sub">' +
                        util.esc(article.author) +
                        " · " +
                        util.esc(util.relativeTime(article.time)) +
                        " · " +
                        util.formatNumber(article.views) +
                        " 阅读</div>" +
                        "</div>" +
                        '<div class="ui-item__side">' +
                        icon("chevron-right") +
                        "</div>" +
                        "</button>"
                    );
                })
                .join("") +
            "</div>"
        );
    }

    function renderLogs() {
        return (
            '<div class="app-log">' +
            ui.terminalLines(DpzOS.data.logs) +
            "</div>"
        );
    }

    function greeting() {
        var hour = new Date().getHours();
        if (hour < 6) {
            return "凌晨好";
        }
        if (hour < 11) {
            return "早上好";
        }
        if (hour < 14) {
            return "中午好";
        }
        if (hour < 18) {
            return "下午好";
        }
        return "晚上好";
    }

    function render() {
        var stats = DpzOS.data.stats;
        return (
            '<div class="ui-page app-dash">' +
            '<section class="app-dash__hero">' +
            '<div class="app-dash__hello">' +
            '<div class="ui-eyebrow">系统概览 · SYS.OVERVIEW</div>' +
            "<h2>" +
            greeting() +
            "，<em>" +
            util.esc(DpzOS.data.profile.name) +
            "</em>。一切运行正常。</h2>" +
            "<p>今日本站共 " +
            util.formatNumber(stats.todayVisits) +
            " 次访问，发布了 " +
            stats.todayArticles +
            " 篇文章。所有服务在线，消息队列积压 1 条。</p>" +
            "</div>" +
            '<div class="app-dash__stamp">最后更新<br><b>' +
            util.formatTime(stats.updatedAt) +
            "</b><br>" +
            util.formatDate(stats.updatedAt) +
            "</div>" +
            '<div class="ui-head__actions">' +
            ui.btn({ label: "刷新缓存", icon: "refresh", attrs: 'data-action="refresh"' }) +
            ui.btn({ label: "打开命令面板", icon: "command", variant: "primary", attrs: 'data-action="palette"' }) +
            "</div>" +
            "</section>" +
            renderStat() +
            '<div class="ui-grid ui-grid--2">' +
            ui.panel({
                title: "七日访问趋势",
                icon: "trending",
                sub: "近 7 天",
                actions: ui.badge("+12.4%", "success", "trending"),
                body: ui.lineChart(DpzOS.data.visits7)
            }) +
            ui.panel({
                title: "浏览器占比",
                icon: "globe",
                sub: "最近 7 天",
                body: renderBrowsers()
            }) +
            "</div>" +
            '<div class="ui-grid ui-grid--2">' +
            ui.panel({
                title: "每小时流量",
                icon: "chart",
                sub: "最近 24 个时间点",
                body: renderHourly()
            }) +
            ui.panel({
                title: "访问来源 Top 5",
                icon: "route",
                sub: "含直接访问",
                body: renderReferrers()
            }) +
            "</div>" +
            '<div class="ui-grid ui-grid--2">' +
            ui.panel({
                title: "热门页面 Top 6",
                icon: "eye",
                body: renderTopPages()
            }) +
            ui.panel({
                title: "慢请求 Top 6",
                icon: "clock",
                body: renderSlowRequests()
            }) +
            "</div>" +
            '<div class="ui-grid ui-grid--2">' +
            ui.panel({ title: "横幅轮播", icon: "image", body: renderBanners() }) +
            ui.panel({
                title: "最新文章",
                icon: "article",
                actions: ui.btn({
                    label: "全部",
                    size: "sm",
                    variant: "ghost",
                    attrs: 'data-action="open-articles"'
                }),
                body: renderLatest()
            }) +
            "</div>" +
            ui.panel({
                title: "系统日志",
                icon: "terminal",
                sub: "最近 100 条",
                body: renderLogs()
            }) +
            "</div>"
        );
    }

    function mount(rootNode, ctx) {
        var banner = rootNode.querySelector('[data-role="banner"]');
        var timer = null;
        if (banner) {
            var slides = banner.querySelectorAll(".app-banner__slide");
            var dots = banner.querySelectorAll(".app-banner__dot");
            var index = 0;
            timer = window.setInterval(function () {
                slides[index].classList.remove("is-active");
                dots[index].classList.remove("is-active");
                index = (index + 1) % slides.length;
                slides[index].classList.add("is-active");
                dots[index].classList.add("is-active");
            }, 5000);
        }

        rootNode.addEventListener("click", function (event) {
            var target = event.target.closest("[data-action]");
            if (!target) {
                return;
            }
            var action = target.getAttribute("data-action");
            if (action === "refresh") {
                target.classList.add("is-loading");
                window.setTimeout(function () {
                    DpzOS.data.stats.updatedAt = new Date();
                    ctx.rerender();
                    DpzOS.toast({
                        title: "概览缓存已刷新",
                        text: "Community summary 更新完成",
                        tone: "success",
                        icon: "refresh"
                    });
                }, 520);
            }
            if (action === "palette") {
                DpzOS.palette.open();
            }
            if (action === "open-articles") {
                ctx.open("article-list");
            }
            if (action === "edit-article") {
                ctx.open("article-editor", { id: Number(target.getAttribute("data-id")) });
            }
        });

        ctx.onClose(function () {
            if (timer) {
                window.clearInterval(timer);
            }
        });
    }

    DpzOS.registerApp({
        id: "dashboard",
        name: "态势总览",
        en: "Overview",
        icon: "dashboard",
        tone: "accent",
        group: "工作台",
        size: { w: 1180, h: 720 },
        pinned: true,
        singleton: true,
        desc: "访问统计、趋势图表与系统日志",
        render: render,
        mount: mount
    });
})(window.DpzOS);
