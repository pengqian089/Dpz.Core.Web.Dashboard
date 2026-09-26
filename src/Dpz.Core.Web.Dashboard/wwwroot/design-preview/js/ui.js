(function (DpzOS) {
    "use strict";

    var util = DpzOS.util;
    var esc = util.esc;
    var icon = DpzOS.icon;

    function avatar(name, size, colorSeed) {
        var cls = "ui-avatar" + (size ? " ui-avatar--" + size : "");
        return (
            '<span class="' +
            cls +
            '" style="background-image:' +
            esc(util.gradient(colorSeed || name, 135)) +
            '">' +
            esc(util.initials(name)) +
            "</span>"
        );
    }

    function badge(text, tone, iconName) {
        var cls = "ui-badge" + (tone ? " ui-badge--" + tone : "");
        return (
            '<span class="' +
            cls +
            '">' +
            (iconName ? icon(iconName) : "") +
            esc(text) +
            "</span>"
        );
    }

    function chip(text, options) {
        var opts = options || {};
        var cls = "ui-chip";
        if (opts.active) {
            cls += " ui-chip--active";
        }
        if (opts.tone) {
            cls += " ui-chip--" + opts.tone;
        }
        if (opts.click) {
            cls += " ui-chip--click";
        }
        var attrs = opts.attrs || "";
        var close = opts.removable
            ? '<span class="ui-chip__x" data-action="remove-chip">' + icon("x") + "</span>"
            : "";
        return (
            '<span class="' +
            cls +
            '" ' +
            attrs +
            ">" +
            (opts.icon ? icon(opts.icon) : "") +
            esc(text) +
            close +
            "</span>"
        );
    }

    function btn(options) {
        var opts = options || {};
        var cls = "ui-btn";
        cls += opts.variant ? " ui-btn--" + opts.variant : "";
        cls += opts.size ? " ui-btn--" + opts.size : "";
        if (opts.iconOnly) {
            cls += " ui-btn--icon";
        }
        return (
            '<button type="button" class="' +
            cls +
            '" ' +
            (opts.attrs || "") +
            (opts.disabled ? " disabled" : "") +
            ">" +
            (opts.icon ? icon(opts.icon) : "") +
            (opts.label ? "<span>" + esc(opts.label) + "</span>" : "") +
            "</button>"
        );
    }

    function iconBtn(name, options) {
        var opts = options || {};
        return (
            '<button type="button" class="ui-iconbtn' +
            (opts.danger ? " ui-iconbtn--danger" : "") +
            '" ' +
            (opts.attrs || "") +
            ' aria-label="' +
            esc(opts.label || "") +
            '" title="' +
            esc(opts.title || opts.label || "") +
            '">' +
            icon(name) +
            "</button>"
        );
    }

    function search(options) {
        var opts = options || {};
        return (
            '<label class="ui-search">' +
            icon("search") +
            '<input type="search" class="ui-input" placeholder="' +
            esc(opts.placeholder || "搜索") +
            '" value="' +
            esc(opts.value || "") +
            '" ' +
            (opts.attrs || "") +
            ">" +
            "</label>"
        );
    }

    function select(options) {
        var opts = options || {};
        var items = (opts.options || [])
            .map(function (option) {
                var value = typeof option === "string" ? option : option.value;
                var label = typeof option === "string" ? option : option.label;
                var selected = String(value) === String(opts.value) ? " selected" : "";
                return (
                    '<option value="' +
                    esc(value) +
                    '"' +
                    selected +
                    ">" +
                    esc(label) +
                    "</option>"
                );
            })
            .join("");
        return (
            '<select class="ui-select" ' +
            (opts.attrs || "") +
            ">" +
            items +
            "</select>"
        );
    }

    function field(options) {
        var opts = options || {};
        return (
            '<label class="ui-field">' +
            '<span class="ui-field__label">' +
            esc(opts.label || "") +
            (opts.required ? " <b>*</b>" : "") +
            "</span>" +
            (opts.control || "") +
            (opts.hint ? '<span class="ui-field__hint">' + esc(opts.hint) + "</span>" : "") +
            "</label>"
        );
    }

    function toolbar(groups) {
        var body = groups
            .map(function (group, index) {
                var content = Array.isArray(group) ? group.join("") : group;
                return (
                    '<div class="ui-toolbar__group">' +
                    content +
                    "</div>" +
                    (index < groups.length - 1 ? '<span class="ui-toolbar__sep"></span>' : "")
                );
            })
            .join("");
        return '<div class="ui-toolbar">' + body + "</div>";
    }

    function panel(options) {
        var opts = options || {};
        var head = "";
        if (opts.title || opts.actions) {
            head =
                '<div class="ui-panel__head">' +
                (opts.icon ? icon(opts.icon) : "") +
                '<div class="ui-panel__title">' +
                esc(opts.title || "") +
                (opts.sub ? ' <span class="ui-panel__sub">' + esc(opts.sub) + "</span>" : "") +
                "</div>" +
                (opts.actions
                    ? '<div class="ui-panel__actions">' + opts.actions + "</div>"
                    : "") +
                "</div>";
        }
        var body =
            '<div class="ui-panel__body' +
            (opts.flush ? " ui-panel__body--flush" : "") +
            '">' +
            (opts.body || "") +
            "</div>";
        var foot = opts.foot ? '<div class="ui-panel__foot">' + opts.foot + "</div>" : "";
        return (
            '<section class="ui-panel' +
            (opts.accent ? " ui-panel--accent" : "") +
            " " +
            (opts.cls || "") +
            '">' +
            head +
            body +
            foot +
            "</section>"
        );
    }

    function section(title, body, options) {
        var opts = options || {};
        return (
            '<section class="ui-section ' +
            (opts.cls || "") +
            '">' +
            '<div class="ui-section__head">' +
            '<h3 class="ui-section__title">' +
            (opts.icon ? icon(opts.icon) : "") +
            esc(title) +
            "</h3>" +
            '<span class="ui-section__line"></span>' +
            (opts.actions ? '<div class="ui-row ui-row--tight">' + opts.actions + "</div>" : "") +
            "</div>" +
            body +
            "</section>"
        );
    }

    function stat(options) {
        var opts = options || {};
        var delta = "";
        if (opts.delta !== undefined && opts.delta !== null) {
            var up = opts.delta >= 0;
            delta =
                '<span class="ui-delta ' +
                (up ? "ui-delta--up" : "ui-delta--down") +
                '">' +
                icon(up ? "trending" : "arrow-down") +
                (up ? "+" : "") +
                opts.delta +
                "%</span>";
        }
        return (
            '<article class="ui-stat" data-tone="' +
            (opts.tone || "accent") +
            '">' +
            '<div class="ui-stat__head">' +
            (opts.icon ? '<span class="ui-stat__icon">' + icon(opts.icon) + "</span>" : "") +
            esc(opts.label || "") +
            "</div>" +
            '<div class="ui-stat__value">' +
            esc(opts.value) +
            "</div>" +
            '<div class="ui-stat__foot">' +
            delta +
            '<span>' +
            esc(opts.hint || "") +
            "</span>" +
            "</div>" +
            "</article>"
        );
    }

    function table(options) {
        var opts = options || {};
        var head = (opts.columns || [])
            .map(function (column) {
                return (
                    "<th" +
                    (column.align === "right" ? ' class="u-right"' : "") +
                    (column.sortable ? " data-sortable data-sort-key=\"" + esc(column.key) + '"' : "") +
                    ">" +
                    esc(column.label) +
                    (column.sortable
                        ? icon(opts.sortKey === column.key && opts.sortDesc ? "arrow-down" : "arrow-up-down")
                        : "") +
                    "</th>"
                );
            })
            .join("");
        var body = (opts.rows || [])
            .map(function (row) {
                var cells = row.cells || row;
                var attrs = row.attrs || "";
                return (
                    "<tr " +
                    attrs +
                    ">" +
                    cells
                        .map(function (cell, index) {
                            var label = (opts.columns[index] && opts.columns[index].label) || "";
                            var align = opts.columns[index] && opts.columns[index].align === "right";
                            return (
                                "<td" +
                                (label ? ' data-label="' + esc(label) + '"' : ' data-label=""') +
                                (align ? ' class="u-right"' : "") +
                                ">" +
                                (cell === null || cell === undefined ? "" : cell) +
                                "</td>"
                            );
                        })
                        .join("") +
                    "</tr>"
                );
            })
            .join("");
        return (
            '<div class="ui-tablewrap"><table class="ui-table"><thead><tr>' +
            head +
            "</tr></thead><tbody>" +
            (body || '<tr><td colspan="' + (opts.columns ? opts.columns.length : 1) + '">' + empty({ icon: "inbox", title: "暂无数据" }) + "</td></tr>") +
            "</tbody></table></div>"
        );
    }

    function pager(options) {
        var opts = options || {};
        var page = opts.page || 1;
        var totalPages = Math.max(1, opts.totalPages || 1);
        var pageSize = opts.pageSize || 10;
        var count = opts.count || 0;
        var start = count === 0 ? 0 : (page - 1) * pageSize + 1;
        var end = Math.min(count, page * pageSize);
        var pages = [];
        var from = Math.max(1, page - 2);
        var to = Math.min(totalPages, from + 4);
        from = Math.max(1, to - 4);
        if (from > 1) {
            pages.push(pageBtn(1, page));
            if (from > 2) {
                pages.push('<span class="ui-pagebtn" style="pointer-events:none">…</span>');
            }
        }
        for (var index = from; index <= to; index++) {
            pages.push(pageBtn(index, page));
        }
        if (to < totalPages) {
            if (to < totalPages - 1) {
                pages.push('<span class="ui-pagebtn" style="pointer-events:none">…</span>');
            }
            pages.push(pageBtn(totalPages, page));
        }
        return (
            '<div class="ui-pager">' +
            '<div class="ui-pager__info">共 <b>' +
            util.formatNumber(count) +
            "</b> 条 · 当前 " +
            start +
            "-" +
            end +
            "</div>" +
            '<div class="ui-pager__pages">' +
            '<button type="button" class="ui-pagebtn" data-page="' +
            (page - 1) +
            '"' +
            (page <= 1 ? " disabled" : "") +
            ">" +
            icon("chevron-left") +
            "</button>" +
            pages.join("") +
            '<button type="button" class="ui-pagebtn" data-page="' +
            (page + 1) +
            '"' +
            (page >= totalPages ? " disabled" : "") +
            ">" +
            icon("chevron-right") +
            "</button>" +
            "</div>" +
            "</div>"
        );
    }

    function pageBtn(value, current) {
        return (
            '<button type="button" class="ui-pagebtn' +
            (value === current ? " is-active" : "") +
            '" data-page="' +
            value +
            '">' +
            value +
            "</button>"
        );
    }

    function tabs(items, activeId) {
        return (
            '<nav class="ui-tabs" role="tablist">' +
            items
                .map(function (item) {
                    return (
                        '<button type="button" role="tab" class="ui-tab' +
                        (item.id === activeId ? " is-active" : "") +
                        '" data-tab="' +
                        esc(item.id) +
                        '">' +
                        (item.icon ? icon(item.icon) : "") +
                        esc(item.label) +
                        (item.count !== undefined
                            ? '<span class="ui-tab__count">' + esc(item.count) + "</span>"
                            : "") +
                        "</button>"
                    );
                })
                .join("") +
            "</nav>"
        );
    }

    function progress(value, tone) {
        var width = util.clamp(Number(value) || 0, 0, 100);
        return (
            '<div class="ui-progress' +
            (tone ? " ui-progress--" + tone : "") +
            '"><i style="width:' +
            width +
            '%"></i></div>'
        );
    }

    function barList(items) {
        return (
            '<div class="ui-bar-list">' +
            items
                .map(function (item) {
                    return (
                        '<div class="ui-bar-item">' +
                        '<div class="ui-bar-item__top"><b>' +
                        esc(item.name) +
                        "</b><span>" +
                        esc(item.display || util.formatNumber(item.value)) +
                        "</span></div>" +
                        progress(item.percent, item.tone) +
                        (item.sub ? '<div class="ui-bar-item__sub">' + esc(item.sub) + "</div>" : "") +
                        "</div>"
                    );
                })
                .join("") +
            "</div>"
        );
    }

    function callout(text, options) {
        var opts = options || {};
        return (
            '<div class="ui-callout' +
            (opts.tone ? " ui-callout--" + opts.tone : "") +
            '">' +
            icon(opts.icon || "info") +
            "<div>" +
            text +
            "</div>" +
            "</div>"
        );
    }

    function empty(options) {
        var opts = options || {};
        return (
            '<div class="ui-empty">' +
            '<span class="ui-empty__icon">' +
            icon(opts.icon || "inbox") +
            "</span>" +
            '<div class="ui-empty__title">' +
            esc(opts.title || "暂无数据") +
            "</div>" +
            (opts.text ? '<div class="ui-empty__text">' + esc(opts.text) + "</div>" : "") +
            (opts.action ? "<div>" + opts.action + "</div>" : "") +
            "</div>"
        );
    }

    function skeleton(lines) {
        var out = [];
        for (var i = 0; i < (lines || 3); i++) {
            out.push(
                '<div class="ui-skeleton ui-skeleton--line' +
                    (i === (lines || 3) - 1 ? " ui-skeleton--sm" : "") +
                    '"></div>'
            );
        }
        return '<div class="ui-stack">' + out.join("") + "</div>";
    }

    function cover(seed, options) {
        var opts = options || {};
        return (
            '<div class="ui-cover ' +
            (opts.cls || "ui-cover--wide") +
            '" style="' +
            util.coverStyle(seed) +
            '" ' +
            (opts.attrs || "") +
            ">" +
            (opts.tag ? '<span class="ui-cover__tag">' + opts.tag + "</span>" : "") +
            (opts.center ? '<span class="ui-cover__center">' + opts.center + "</span>" : "") +
            (opts.time ? '<span class="ui-cover__time">' + esc(opts.time) + "</span>" : "") +
            "</div>"
        );
    }

    function markdown(source) {
        var lines = String(source || "").split("\n");
        var out = [];
        var inCode = false;
        var inList = false;
        var inQuote = false;

        function inline(text) {
            var value = esc(text);
            value = value.replace(/`([^`]+)`/g, "<code>$1</code>");
            value = value.replace(/\*\*([^*]+)\*\*/g, "<strong>$1</strong>");
            value = value.replace(/\[([^\]]+)\]\(([^)]+)\)/g, '<a href="$2" target="_blank" rel="noopener">$1</a>');
            return value;
        }

        function closeBlocks() {
            if (inList) {
                out.push("</ul>");
                inList = false;
            }
            if (inQuote) {
                out.push("</blockquote>");
                inQuote = false;
            }
        }

        lines.forEach(function (line) {
            if (line.indexOf("```") === 0) {
                closeBlocks();
                if (!inCode) {
                    out.push("<pre><code>");
                    inCode = true;
                } else {
                    out.push("</code></pre>");
                    inCode = false;
                }
                return;
            }
            if (inCode) {
                out.push(esc(line) + "\n");
                return;
            }
            if (!line.trim()) {
                closeBlocks();
                return;
            }
            if (line.indexOf("### ") === 0) {
                closeBlocks();
                out.push("<h3>" + inline(line.slice(4)) + "</h3>");
                return;
            }
            if (line.indexOf("## ") === 0) {
                closeBlocks();
                out.push("<h2>" + inline(line.slice(3)) + "</h2>");
                return;
            }
            if (line.indexOf("# ") === 0) {
                closeBlocks();
                out.push("<h1>" + inline(line.slice(2)) + "</h1>");
                return;
            }
            if (line.indexOf("> ") === 0) {
                if (inList) {
                    out.push("</ul>");
                    inList = false;
                }
                if (!inQuote) {
                    out.push("<blockquote>");
                    inQuote = true;
                }
                out.push("<p>" + inline(line.slice(2)) + "</p>");
                return;
            }
            if (line.indexOf("- ") === 0) {
                if (inQuote) {
                    out.push("</blockquote>");
                    inQuote = false;
                }
                if (!inList) {
                    out.push("<ul>");
                    inList = true;
                }
                out.push("<li>" + inline(line.slice(2)) + "</li>");
                return;
            }
            closeBlocks();
            out.push("<p>" + inline(line) + "</p>");
        });

        closeBlocks();
        if (inCode) {
            out.push("</code></pre>");
        }
        return '<div class="ui-md">' + out.join("") + "</div>";
    }

    function code(source, highlightHtml) {
        return '<pre class="ui-code">' + (highlightHtml || esc(source)) + "</pre>";
    }

    function terminalLines(lines) {
        return (
            '<div class="ui-terminal">' +
            lines
                .map(function (line) {
                    return (
                        '<div class="ui-terminal__line">' +
                        '<span class="ui-terminal__time">' +
                        esc(line.time) +
                        "</span>" +
                        '<span class="ui-terminal__level" data-level="' +
                        esc(line.level) +
                        '">' +
                        esc(line.level) +
                        "</span>" +
                        '<span class="ui-terminal__text">' +
                        esc(line.text) +
                        "</span>" +
                        "</div>"
                    );
                })
                .join("") +
            "</div>"
        );
    }

    function lineChart(data, options) {
        var opts = options || {};
        var width = 640;
        var height = 190;
        var padding = { top: 16, right: 12, bottom: 26, left: 34 };
        var values = data.values;
        var min = Math.min.apply(null, values) * 0.9;
        var max = Math.max.apply(null, values) * 1.04;
        var innerW = width - padding.left - padding.right;
        var innerH = height - padding.top - padding.bottom;
        var points = values.map(function (value, index) {
            var x = padding.left + (innerW * index) / Math.max(1, values.length - 1);
            var y = padding.top + innerH - ((value - min) / (max - min || 1)) * innerH;
            return [x, y];
        });
        var path = points
            .map(function (point, index) {
                return (index === 0 ? "M" : "L") + point[0].toFixed(1) + " " + point[1].toFixed(1);
            })
            .join(" ");
        var area = path + " L" + points[points.length - 1][0] + " " + (padding.top + innerH) +
            " L" + points[0][0] + " " + (padding.top + innerH) + " Z";
        var gridLines = "";
        for (var i = 0; i <= 4; i++) {
            var y = padding.top + (innerH * i) / 4;
            gridLines +=
                '<line x1="' + padding.left + '" x2="' + (width - padding.right) +
                '" y1="' + y.toFixed(1) + '" y2="' + y.toFixed(1) + '"/>';
        }
        var labels = data.labels
            .map(function (label, index) {
                var x = padding.left + (innerW * index) / Math.max(1, data.labels.length - 1);
                return (
                    '<text class="app-chart__label" x="' +
                    x.toFixed(1) +
                    '" y="' +
                    (height - 6) +
                    '" text-anchor="middle">' +
                    esc(label) +
                    "</text>"
                );
            })
            .join("");
        var dots = points
            .map(function (point, index) {
                return (
                    '<circle class="app-chart__dot" cx="' +
                    point[0].toFixed(1) +
                    '" cy="' +
                    point[1].toFixed(1) +
                    '" r="' +
                    (index === points.length - 1 ? 5 : 3) +
                    '"><title>' +
                    esc(data.labels[index] + " · " + util.formatNumber(values[index])) +
                    "</title></circle>"
                );
            })
            .join("");
        return (
            '<div class="app-chart"><svg viewBox="0 0 ' +
            width +
            " " +
            height +
            '" preserveAspectRatio="none">' +
            '<defs><linearGradient id="dashArea" x1="0" y1="0" x2="0" y2="1">' +
            '<stop offset="0%" stop-color="var(--accent)" stop-opacity="0.35"/>' +
            '<stop offset="100%" stop-color="var(--accent)" stop-opacity="0"/>' +
            "</linearGradient></defs>" +
            '<g class="app-chart__grid">' +
            gridLines +
            "</g>" +
            '<path class="app-chart__area" d="' + area + '"/>' +
            '<path class="app-chart__line" d="' + path + '"/>' +
            dots +
            labels +
            "</svg></div>"
        );
    }

    DpzOS.ui = {
        avatar: avatar,
        badge: badge,
        chip: chip,
        btn: btn,
        iconBtn: iconBtn,
        search: search,
        select: select,
        field: field,
        toolbar: toolbar,
        panel: panel,
        section: section,
        stat: stat,
        table: table,
        pager: pager,
        tabs: tabs,
        progress: progress,
        barList: barList,
        callout: callout,
        empty: empty,
        skeleton: skeleton,
        cover: cover,
        markdown: markdown,
        code: code,
        terminalLines: terminalLines,
        lineChart: lineChart
    };
})(window.DpzOS || (window.DpzOS = {}));
