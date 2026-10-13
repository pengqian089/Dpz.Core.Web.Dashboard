(function (DpzOS) {
    "use strict";

    var ui = DpzOS.ui;
    var util = DpzOS.util;
    var icon = DpzOS.icon;
    var data = DpzOS.data;

    var README = [
        "# Dpz.Core.Web.Dashboard",
        "",
        "基于 Blazor WebAssembly 的个人网站管理后台，前端资产由 Vite 构建。",
        "",
        "## 桌面外壳",
        "",
        "- `window-manager` 负责窗口创建、拖拽、缩放与吸附",
        "- `registry` 负责应用注册与启动",
        "- `command-palette` 提供全局命令入口",
        "",
        "## 构建",
        "",
        "```powershell",
        ".\\build.ps1 check",
        "dotnet run --project ./Dpz.Core.Web.Dashboard.csproj",
        "```"
    ].join("\n");

    var SCRIPT = [
        "param(",
        "    [ValidateSet(\"prod\", \"build\", \"dev\", \"check\", \"clean\")]",
        "    [string]$Task = \"build\"",
        ")",
        "",
        "switch ($Task) {",
        "    \"check\" { npm run check }",
        "    \"dev\"   { Start-DevLoop }",
        "    default  { Invoke-ProductionBuild }",
        "}"
    ].join("\n");

    function fileIcon(file) {
        if (file.type === "目录") {
            return icon("folder");
        }
        var map = {
            ts: "code",
            js: "code",
            css: "palette",
            md: "book",
            ps1: "terminal",
            html: "page",
            json: "database"
        };
        return icon(map[file.ext] || "file");
    }

    function highlightCode(source) {
        return util
            .esc(source)
            .replace(/("(?:[^"\\]|\\.)*")/g, '<span class="c-str">$1</span>')
            .replace(
                /\b(export|class|const|return|if|this|void|new|extends|implements)\b/g,
                '<span class="c-key">$1</span>'
            )
            .replace(/\b(WindowManager|AppDefinition|WindowHandle|AppParams|SnapTarget)\b/g, '<span class="c-tag">$1</span>')
            .replace(/\/\/.*$/gm, '<span class="c-com">$&</span>');
    }

    function filtered(state) {
        return data.codeFiles.filter(function (file) {
            if (state.keyword && !util.matches(state.keyword, [file.name, file.path, file.note, file.ext])) {
                return false;
            }
            return true;
        });
    }

    function parentPath(path) {
        var index = path.lastIndexOf("/");
        return index <= 0 ? "/" : path.slice(0, index);
    }

    function currentEntries(state) {
        if (state.keyword) {
            return filtered(state);
        }
        return data.codeFiles.filter(function (file) {
            if (file.type === "目录") {
                return parentPath(file.path) === state.currentPath;
            }
            return file.path === state.currentPath;
        });
    }

    function breadcrumb(state, ctx) {
        var segments = state.currentPath.split("/").filter(Boolean);
        var parts = [
            '<button type="button" class="app-code__crumb-link" data-goto="/">' +
                icon("folder") +
                " 根目录</button>"
        ];
        var accumulated = "";
        segments.forEach(function (segment) {
            accumulated += "/" + segment;
            parts.push(
                '<span class="u-dim">/</span><button type="button" class="app-code__crumb-link" data-goto="' +
                    util.esc(accumulated) +
                    '">' +
                    util.esc(segment) +
                    "</button>"
            );
        });
        return parts.join("");
    }

    function renderTree(ctx) {
        var state = ctx.state;
        state.currentPath = state.currentPath || "/";
        var files = currentEntries(state);
        var selected = data.codeFiles.find(function (file) {
            return file.name === state.selected;
        });
        if (!selected || (selected.type === "目录" && !state.keyword)) {
            selected = files.find(function (file) {
                return file.type !== "目录";
            });
        }
        if (selected && selected.type === "目录") {
            selected = null;
        }

        var selectedIsDir = false;

        return (
            ui.toolbar([
                [
                    ui.search({
                        placeholder: "搜索文件或目录，支持拼音…",
                        value: state.keyword,
                        attrs: 'data-role="search-code"'
                    })
                ],
                [
                    '<div class="ui-seg">' +
                        '<button type="button" class="ui-seg__btn' +
                        (state.view === "tree" ? " is-active" : "") +
                        '" data-view="tree">' +
                        icon("git-branch") +
                        "树结构</button>" +
                        '<button type="button" class="ui-seg__btn' +
                        (state.view === "list" ? " is-active" : "") +
                        '" data-view="list">' +
                        icon("list") +
                        "列表视图</button>" +
                        "</div>"
                ]
            ]) +
            '<div class="app-code ui-panel" style="min-height:520px">' +
            '<div class="app-code__list">' +
            '<div class="app-code__crumb" style="padding:10px 13px;border-bottom:1px solid var(--stroke-0)">' +
            breadcrumb(state, ctx) +
            '<span class="u-grow"></span>' +
            (state.keyword ? ui.badge("搜索：" + state.keyword, "accent", "search") : "") +
            "</div>" +
            (files.length
                ? files
                      .map(function (file) {
                          var isSelected = selected && file.name === selected.name;
                          return (
                              '<div class="app-code__row' +
                              (isSelected ? " is-active" : "") +
                              '" data-' +
                              (file.type === "目录" ? "dir" : "file") +
                              '="' +
                              util.esc(file.name) +
                              '">' +
                              '<span class="app-code__icon">' +
                              fileIcon(file) +
                              "</span>" +
                              '<div class="ui-item__main">' +
                              '<div class="app-code__name">' +
                              util.highlight(file.name, state.keyword, "ui-mark") +
                              "</div>" +
                              '<div class="app-code__note">' +
                              util.esc(file.note || file.path) +
                              "</div>" +
                              "</div>" +
                              '<span class="ui-mono u-dim" style="font-size:11px">' +
                              util.esc(file.size) +
                              "</span>" +
                              "</div>"
                          );
                      })
                      .join("")
                : ui.empty({ icon: "folder", title: "该目录为空" })) +
            "</div>" +
            '<div class="app-code__detail">' +
            (selected
                ? '<div class="ui-spread">' +
                  "<div>" +
                  '<div class="ui-title">' +
                  fileIcon(selected) +
                  " " +
                  util.esc(selected.name) +
                  "</div>" +
                  '<div class="ui-subtitle u-mono">' +
                  util.esc(selected.path + "/" + selected.name) +
                  "</div>" +
                  "</div>" +
                  '<div class="ui-row ui-row--tight">' +
                  ui.badge("更新 " + util.relativeTime(selected.write), null, "clock") +
                  (selected.ai
                      ? ui.badge("AI " + util.relativeTime(selected.ai), "violet", "sparkles")
                      : "") +
                  ui.btn({
                      label: "编辑说明",
                      icon: "edit",
                      size: "sm",
                      attrs: 'data-action="note" data-file="' + util.esc(selected.name) + '"'
                  }) +
                  "</div>" +
                  "</div>" +
                  ui.callout("说明：" + (util.esc(selected.note) || "暂无说明"), { icon: "info" }) +
                  (selected.ext === "ts"
                      ? ui.code(data.codeSnippet, highlightCode(data.codeSnippet))
                      : selected.ext === "md"
                        ? ui.markdown(README)
                        : selected.ext === "ps1"
                          ? ui.code(SCRIPT)
                          : selectedIsDir
                            ? ui.empty({
                                  icon: "folder",
                                  title: "目录 " + selected.name,
                                  text: "点击左侧目录进入下一级"
                              })
                            : ui.empty({ icon: "file", title: "不支持预览此类型", text: selected.ext }))
                : ui.empty({ icon: "file", title: "选择左侧文件查看内容" })) +
            "</div>" +
            "</div>"
        );
    }

    function renderList(ctx) {
        var state = ctx.state;
        state.sortKey = state.sortKey || "write";
        state.sortDesc = state.sortDesc !== false;
        var rows = filtered(state).slice().sort(function (a, b) {
            var left = a[state.sortKey];
            var right = b[state.sortKey];
            if (left instanceof Date && right instanceof Date) {
                left = left.getTime();
                right = right.getTime();
            }
            if (left === right) {
                return 0;
            }
            var result = left > right ? 1 : -1;
            return state.sortDesc ? -result : result;
        });

        return (
            ui.toolbar([
                [
                    ui.search({
                        placeholder: "搜索文件名、路径或说明…",
                        value: state.keyword,
                        attrs: 'data-role="search-code"'
                    }),
                    ui.select({
                        options: ["/", "/src", "/src/shell", "/src/styles", "/src/apps"],
                        value: state.path || "/",
                        attrs: 'data-role="filter-path"'
                    })
                ],
                [
                    '<div class="ui-seg">' +
                        '<button type="button" class="ui-seg__btn' +
                        (state.view === "tree" ? " is-active" : "") +
                        '" data-view="tree">' +
                        icon("git-branch") +
                        "树结构</button>" +
                        '<button type="button" class="ui-seg__btn' +
                        (state.view === "list" ? " is-active" : "") +
                        '" data-view="list">' +
                        icon("list") +
                        "列表视图</button>" +
                        "</div>",
                    ui.btn({ label: "重置", icon: "eraser", size: "sm", attrs: 'data-action="reset"' })
                ]
            ]) +
            ui.panel({
                flush: true,
                body: ui.table({
                    columns: [
                        { label: "名称" },
                        { label: "路径" },
                        { label: "类型" },
                        { label: "大小", sortable: true, key: "size" },
                        { label: "说明" },
                        { label: "写入时间", sortable: true, key: "write" },
                        { label: "操作", align: "right" }
                    ],
                    rows: rows.map(function (file) {
                        return {
                            cells: [
                                '<div class="ui-table__main">' +
                                    '<span class="app-code__icon">' +
                                    fileIcon(file) +
                                    "</span>" +
                                    '<span class="app-code__name">' +
                                    util.highlight(file.name, state.keyword, "ui-mark") +
                                    "</span>" +
                                    "</div>",
                                '<span class="u-mono u-muted">' +
                                    util.esc(file.path) +
                                    "</span>",
                                ui.badge(file.type, file.type === "目录" ? "warning" : null),
                                '<span class="u-mono">' +
                                    util.esc(file.size) +
                                    "</span>",
                                '<span class="u-ellipsis" style="display:inline-block;max-width:220px">' +
                                    util.esc(file.note) +
                                    "</span>",
                                '<span class="u-mono u-muted">' +
                                    util.esc(util.relativeTime(file.write)) +
                                    "</span>",
                                '<div class="ui-table__actions">' +
                                    (file.type === "目录"
                                        ? ui.iconBtn("filter", {
                                              label: "按此路径过滤",
                                              attrs: 'data-action="filter-path" data-path="' + util.esc(file.path + "/" + file.name) + '"'
                                          })
                                        : ui.iconBtn("eye", {
                                              label: "查看文件",
                                              attrs: 'data-action="open-file" data-file="' + util.esc(file.name) + '"'
                                          })) +
                                    ui.iconBtn("edit", {
                                        label: "编辑说明",
                                        attrs: 'data-action="note" data-file="' + util.esc(file.name) + '"'
                                    }) +
                                    "</div>"
                            ]
                        };
                    })
                })
            }) +
            ui.pager({ page: 1, totalPages: 1, pageSize: rows.length || 1, count: rows.length })
        );
    }

    function render(ctx) {
        var state = ctx.state;
        state.view = state.view || (ctx.params && ctx.params.view) || "tree";
        state.keyword = state.keyword || "";

        return (
            '<div class="ui-page">' +
            '<div class="ui-head">' +
            '<div class="ui-head__text">' +
            '<div class="ui-eyebrow">开发工具 · SOURCE</div>' +
            '<h1 class="ui-title">源码管理</h1>' +
            '<div class="ui-subtitle">浏览目录、查看源码、维护文件说明与 AI 分析结果</div>' +
            "</div>" +
            '<div class="ui-head__actions">' +
            ui.btn({ label: "刷新", icon: "refresh", attrs: 'data-action="refresh"' }) +
            "</div>" +
            "</div>" +
            (state.view === "tree" ? renderTree(ctx) : renderList(ctx)) +
            "</div>"
        );
    }

    function bind(rootNode, ctx) {
        var state = ctx.state;

        rootNode.addEventListener("click", function (event) {
            var view = event.target.closest("[data-view]");
            if (view) {
                state.view = view.getAttribute("data-view");
                ctx.rerender();
                return;
            }
            var crumb = event.target.closest("[data-goto]");
            if (crumb) {
                state.currentPath = crumb.getAttribute("data-goto");
                state.keyword = "";
                state.selected = null;
                ctx.rerender();
                return;
            }
            var dir = event.target.closest("[data-dir]");
            if (dir && !event.target.closest("[data-action]")) {
                var folder = data.codeFiles.find(function (file) {
                    return file.name === dir.getAttribute("data-dir");
                });
                if (folder) {
                    state.currentPath = folder.path;
                    state.keyword = "";
                    state.selected = null;
                    ctx.rerender();
                }
                return;
            }
            var row = event.target.closest("[data-file]");
            if (row && !event.target.closest("[data-action]")) {
                state.selected = row.getAttribute("data-file");
                ctx.rerender();
                return;
            }
            var sortHeader = event.target.closest("[data-sort-key]");
            if (sortHeader) {
                var key = sortHeader.getAttribute("data-sort-key");
                if (state.sortKey === key) {
                    state.sortDesc = !state.sortDesc;
                } else {
                    state.sortKey = key;
                    state.sortDesc = true;
                }
                ctx.rerender();
                return;
            }
            var action = event.target.closest("[data-action]");
            if (!action) {
                return;
            }
            var kind = action.getAttribute("data-action");
            if (kind === "refresh") {
                ctx.rerender();
                DpzOS.toast({ title: "源码索引已刷新", tone: "success", icon: "refresh" });
            }
            if (kind === "reset") {
                state.keyword = "";
                state.currentPath = "/";
                state.path = "/";
                state.selected = null;
                state.sortKey = "write";
                state.sortDesc = true;
                ctx.rerender();
            }
            if (kind === "filter-path") {
                state.path = action.getAttribute("data-path");
                state.currentPath = state.path;
                state.view = "tree";
                state.keyword = "";
                state.selected = null;
                ctx.rerender();
            }
            if (kind === "open-file") {
                state.view = "tree";
                state.selected = action.getAttribute("data-file");
                ctx.rerender();
            }
            if (kind === "note") {
                var file = data.codeFiles.find(function (item) {
                    return item.name === action.getAttribute("data-file");
                });
                if (!file) {
                    return;
                }
                var noteValue = file.note;
                var notePromise = ctx.dialog({
                    title: "编辑文件说明",
                    subtitle: file.path + "/" + file.name,
                    icon: "edit",
                    confirmText: "保存",
                    html: ui.field({
                        label: "说明",
                        control:
                            '<textarea class="ui-textarea" data-role="note-value">' +
                            util.esc(file.note) +
                            "</textarea>"
                    })
                });
                var noteInput = document.querySelector('#modal-root [data-role="note-value"]');
                if (noteInput) {
                    noteInput.addEventListener("input", function () {
                        noteValue = noteInput.value;
                    });
                }
                notePromise.then(function (ok) {
                    if (!ok) {
                        return;
                    }
                    file.note = noteValue;
                    ctx.rerender();
                    DpzOS.toast({ title: "说明已保存", tone: "success", icon: "check-circle" });
                });
            }
        });

        rootNode.addEventListener("change", function (event) {
            if (event.target.matches('[data-role="filter-path"]')) {
                state.path = event.target.value;
                ctx.rerender();
            }
        });

        var search = rootNode.querySelector('[data-role="search-code"]');
        if (search) {
            search.addEventListener("input", util.debounce(function () {
                state.keyword = search.value.trim();
                ctx.rerender();
                var next = rootNode.querySelector('[data-role="search-code"]');
                if (next) {
                    next.focus();
                    next.setSelectionRange(next.value.length, next.value.length);
                }
            }, 220));
        }
    }

    DpzOS.registerApp({
        id: "code",
        name: "源码管理",
        en: "Source",
        icon: "git-branch",
        tone: "success",
        group: "开发工具",
        size: { w: 1180, h: 720 },
        singleton: true,
        desc: "源码树、列表排序、说明与 AI 分析",
        render: render,
        mount: bind
    });
})(window.DpzOS);
