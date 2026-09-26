(function (DpzOS) {
    "use strict";

    var util = DpzOS.util;
    var icon = DpzOS.icon;
    var wm = DpzOS.wm;

    var pinned = [
        "dashboard",
        "article-list",
        "gallery",
        "video",
        "danmaku",
        "comments",
        "security",
        "users",
        "outbox",
        "code",
        "terminal",
        "settings-app"
    ];

    function render() {
        var container = document.getElementById("taskbar-apps");
        if (!container) {
            return;
        }
        var running = {};
        wm.list().forEach(function (record) {
            running[record.appId] = record;
        });

        var ids = pinned.slice();
        wm.list().forEach(function (record) {
            if (ids.indexOf(record.appId) < 0) {
                ids.push(record.appId);
            }
        });

        container.innerHTML = ids
            .map(function (appId) {
                var app = DpzOS.getApp(appId);
                if (!app) {
                    return "";
                }
                var record = running[appId];
                var active = record && wm.getActive() && wm.getActive().appId === appId && !record.minimized;
                return (
                    '<button type="button" class="os-app-btn' +
                    (record ? " is-running" : "") +
                    (active ? " is-active" : "") +
                    '" data-app="' +
                    util.esc(appId) +
                    '" aria-label="' +
                    util.esc(app.name) +
                    '">' +
                    icon(app.icon) +
                    '<span class="os-app-btn__label">' +
                    util.esc(app.name) +
                    "</span>" +
                    "</button>"
                );
            })
            .join("");

        container.onclick = function (event) {
            var button = event.target.closest("[data-app]");
            if (!button) {
                return;
            }
            var appId = button.getAttribute("data-app");
            var record = wm.findOpenByApp(appId);
            if (!record) {
                wm.open(appId);
                return;
            }
            if (record.minimized) {
                wm.restore(record.id);
                return;
            }
            if (wm.getActive() && wm.getActive().id === record.id) {
                wm.minimize(record.id);
                return;
            }
            wm.focus(record.id);
        };
    }

    var userMenuItems = [
        { label: "个人资料", icon: "user", action: openAccount },
        { label: "偏好设置", icon: "settings", action: function () { wm.open("settings-app"); } },
        { sep: true },
        {
            label: "锁定桌面",
            icon: "lock",
            action: function () {
                DpzOS.toast({ title: "已锁定（演示）", text: "正式版将回到登录页", tone: "info", icon: "lock" });
            }
        },
        {
            label: "退出登录",
            icon: "log-out",
            action: function () {
                DpzOS.dialog
                    .confirm({
                        title: "退出登录？",
                        text: "将跳转到认证中心并结束当前会话。",
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
        }
    ];

    function openAccount() {
        wm.open("users", { account: DpzOS.data.profile.account });
    }

    function tickClock() {
        var now = new Date();
        var time = document.getElementById("tray-time");
        var date = document.getElementById("tray-date");
        if (time) {
            time.textContent = util.formatClock(now);
        }
        if (date) {
            date.textContent = util.formatDate(now).slice(5);
        }
        var bigTime = document.getElementById("desktop-clock-time");
        var bigDate = document.getElementById("desktop-clock-date");
        if (bigTime) {
            var clock = util.formatClock(now).split(":");
            bigTime.innerHTML = clock[0] + '<span>:</span>' + clock[1];
        }
        if (bigDate) {
            bigDate.textContent = util.formatDate(now) + " · " + util.pad(now.getSeconds());
        }
    }

    function bind() {
        var startButton = document.getElementById("start-button");
        startButton.addEventListener("click", function () {
            DpzOS.startMenu.toggle();
        });

        document.getElementById("taskbar-search").addEventListener("click", function () {
            DpzOS.palette.open();
        });

        document.getElementById("tray-notifications").addEventListener("click", function () {
            DpzOS.notifications.toggle();
        });

        document.getElementById("tray-palette").addEventListener("click", function () {
            wm.open("settings-app", { section: "personalize" });
        });

        document.getElementById("tray-user").addEventListener("click", function (event) {
            var rect = event.currentTarget.getBoundingClientRect();
            DpzOS.contextMenu.open(
                { clientX: rect.left - 140, clientY: rect.top - 8 },
                userMenuItems
            );
        });

        wm.onChange(function () {
            render();
        });

        render();
        tickClock();
        window.setInterval(tickClock, 1000);
    }

    DpzOS.taskbar = {
        bind: bind,
        render: render
    };
})(window.DpzOS || (window.DpzOS = {}));
