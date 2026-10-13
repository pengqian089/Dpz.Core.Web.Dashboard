(function (DpzOS) {
    "use strict";

    var util = DpzOS.util;
    var icon = DpzOS.icon;

    var open = false;

    var pinnedIds = [
        "dashboard",
        "article-list",
        "gallery",
        "video",
        "music-app",
        "danmaku",
        "comments",
        "security",
        "users",
        "outbox",
        "code",
        "settings-app"
    ];

    function render(keyword) {
        var menu = document.getElementById("start-menu");
        var query = keyword || "";
        var pins = pinnedIds
            .map(function (id) {
                return DpzOS.getApp(id);
            })
            .filter(Boolean)
            .filter(function (app) {
                return util.matches(query, [app.name, app.en, app.id, app.group]);
            });

        var groups = DpzOS.appsByGroup()
            .map(function (group) {
                var apps = group.apps.filter(function (app) {
                    return util.matches(query, [app.name, app.en, app.id, app.group]);
                });
                return { name: group.name, apps: apps };
            })
            .filter(function (group) {
                return group.apps.length > 0;
            });

        menu.innerHTML =
            '<label class="os-start__search">' +
            icon("search") +
            '<input type="search" placeholder="搜索应用、功能或拼音首字母…" value="' +
            util.esc(query) +
            '" data-role="start-search">' +
            '<kbd>Enter</kbd>' +
            "</label>" +
            '<div class="os-start__section">' +
            '<div class="os-start__head"><span>已固定</span><span>' +
            pins.length +
            " 个应用</span></div>" +
            '<div class="os-start__pins">' +
            (pins.length
                ? pins
                      .map(function (app) {
                          return (
                              '<button type="button" class="os-pin" data-app="' +
                              util.esc(app.id) +
                              '">' +
                              '<span class="os-pin__glyph">' +
                              icon(app.icon) +
                              "</span>" +
                              "<span>" +
                              util.esc(app.name) +
                              "</span>" +
                              "</button>"
                          );
                      })
                      .join("")
                : '<div class="os-start__empty" style="grid-column:1/-1">没有匹配的应用</div>') +
            "</div>" +
            "</div>" +
            '<div class="os-start__section">' +
            '<div class="os-start__head"><span>所有应用</span><span>' +
            util.formatNumber(DpzOS.listApps().length) +
            " 个</span></div>" +
            '<div class="os-start__list">' +
            (groups.length
                ? groups
                      .map(function (group) {
                          return (
                              '<div>' +
                              '<div class="os-start__group-title">' +
                              util.esc(group.name) +
                              "</div>" +
                              '<div class="os-start__items">' +
                              group.apps
                                  .map(function (app) {
                                      return (
                                          '<button type="button" class="os-start__item" data-app="' +
                                          util.esc(app.id) +
                                          '">' +
                                          icon(app.icon) +
                                          "<span>" +
                                          util.esc(app.name) +
                                          "</span>" +
                                          "<small>" +
                                          util.esc(app.en || "") +
                                          "</small>" +
                                          "</button>"
                                      );
                                  })
                                  .join("") +
                              "</div>" +
                              "</div>"
                          );
                      })
                      .join("")
                : '<div class="os-start__empty">没有找到相关应用，试试拼音首字母</div>') +
            "</div>" +
            "</div>" +
            '<div class="os-start__footer">' +
            '<div class="os-start__user">' +
            DpzOS.ui.avatar(DpzOS.data.profile.name, "lg", DpzOS.data.profile.account) +
            "<div>" +
            '<div class="os-start__user-name">' +
            util.esc(DpzOS.data.profile.name) +
            "</div>" +
            '<div class="os-start__user-role">' +
            util.esc(DpzOS.data.profile.role) +
            "</div>" +
            "</div>" +
            "</div>" +
            '<div class="ui-row ui-row--tight">' +
            '<button type="button" class="ui-btn ui-btn--sm" data-act="palette">' +
            icon("command") +
            "<span>命令面板</span></button>" +
            '<button type="button" class="ui-btn ui-btn--sm" data-act="power" aria-label="电源">' +
            icon("power") +
            "</button>" +
            "</div>" +
            "</div>";

        var input = menu.querySelector('[data-role="start-search"]');
        input.addEventListener("input", function () {
            render(input.value);
            var next = document.querySelector('[data-role="start-search"]');
            next.focus();
            next.setSelectionRange(next.value.length, next.value.length);
        });
        input.addEventListener("keydown", function (event) {
            if (event.key === "Enter") {
                var first = menu.querySelector("[data-app]");
                if (first) {
                    launch(first.getAttribute("data-app"));
                }
            }
        });

        menu.onclick = function (event) {
            var appButton = event.target.closest("[data-app]");
            if (appButton) {
                launch(appButton.getAttribute("data-app"));
                return;
            }
            var action = event.target.closest("[data-act]");
            if (!action) {
                return;
            }
            var act = action.getAttribute("data-act");
            if (act === "palette") {
                close();
                DpzOS.palette.open();
            }
            if (act === "power") {
                close();
                DpzOS.dialog
                    .confirm({
                        title: "退出登录？",
                        text: "演示环境不会真正退出，正式版将跳转认证中心。",
                        confirmText: "退出登录",
                        tone: "danger",
                        icon: "log-out"
                    })
                    .then(function (ok) {
                        if (ok) {
                            DpzOS.toast({ title: "已发起退出（演示）", tone: "success", icon: "check-circle" });
                        }
                    });
            }
        };
    }

    function launch(appId) {
        close();
        DpzOS.wm.open(appId);
    }

    function toggle() {
        if (open) {
            close();
        } else {
            show();
        }
    }

    function show() {
        open = true;
        var menu = document.getElementById("start-menu");
        render("");
        menu.hidden = false;
        document.getElementById("start-button").classList.add("is-open");
        DpzOS.notifications.close();
        DpzOS.contextMenu.close();
        var input = menu.querySelector('[data-role="start-search"]');
        if (input) {
            input.focus();
        }
    }

    function close() {
        open = false;
        document.getElementById("start-menu").hidden = true;
        document.getElementById("start-button").classList.remove("is-open");
    }

    DpzOS.startMenu = {
        toggle: toggle,
        open: show,
        close: close,
        isOpen: function () {
            return open;
        }
    };
})(window.DpzOS || (window.DpzOS = {}));
