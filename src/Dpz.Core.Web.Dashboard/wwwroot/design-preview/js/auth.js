(function (DpzOS) {
    "use strict";

    var util = DpzOS.util;
    var icon = DpzOS.icon;

    var LOGO =
        '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" ' +
        'stroke-linecap="round" stroke-linejoin="round"><path d="M12 2 3 7v10l9 5 9-5V7z"/>' +
        '<path d="M12 22V12"/><path d="m3 7 9 5 9-5"/></svg>';

    var STATES = [
        {
            id: "logging-in",
            label: "跳转认证",
            icon: "log-in",
            tone: "accent",
            title: "正在跳转认证中心…",
            desc: "即将离开当前站点，请在认证中心完成登录后自动返回。",
            progress: true,
            meta: [["Authority", "https://auth.dpangzi.com"], ["ReturnUrl", "/#/dashboard"]],
            actions: [
                { label: "取消", icon: "x", variant: "ghost", run: "cancel" }
            ]
        },
        {
            id: "completing-login",
            label: "完成登录",
            icon: "shield",
            tone: "accent",
            title: "正在完成登录…",
            desc: "正在校验授权码并建立会话，请不要关闭页面。",
            progress: true,
            meta: [["GrantType", "authorization_code"], ["ResponseMode", "query"]],
            actions: []
        },
        {
            id: "login-failed",
            label: "登录失败",
            icon: "x-circle",
            tone: "danger",
            title: "登录失败",
            desc: "认证中心返回了错误，或授权码已过期。你可以重新登录，或联系管理员检查客户端配置。",
            meta: [["Error", "invalid_grant"], ["ClientId", "manage"]],
            actions: [
                { label: "重新登录", icon: "refresh", variant: "primary", run: "login" },
                { label: "返回主站", icon: "external", variant: "ghost", run: "home" }
            ]
        },
        {
            id: "registering",
            label: "注册中",
            icon: "user",
            tone: "accent",
            title: "正在创建账号…",
            desc: "注册流程由认证中心托管，完成后会使用新账号自动登录。",
            progress: true,
            actions: [
                { label: "取消", icon: "x", variant: "ghost", run: "cancel" }
            ]
        },
        {
            id: "user-profile",
            label: "用户信息",
            icon: "id-card",
            tone: "success",
            title: "已登录",
            desc: "认证中心返回的用户信息如下，确认后进入管理后台。",
            profile: true,
            actions: [
                { label: "进入后台", icon: "log-in", variant: "primary", run: "home" },
                { label: "退出登录", icon: "log-out", variant: "ghost", run: "logout" }
            ]
        },
        {
            id: "logged-out",
            label: "已退出",
            icon: "check-circle",
            tone: "success",
            title: "已安全退出",
            desc: "会话已结束，本地令牌已清除。你可以随时重新登录。",
            actions: [
                { label: "重新登录", icon: "log-in", variant: "primary", run: "login" },
                { label: "返回主站", icon: "external", variant: "ghost", run: "home" }
            ]
        },
        {
            id: "completing-logout",
            label: "退出中",
            icon: "log-out",
            tone: "accent",
            title: "正在退出…",
            desc: "正在通知认证中心结束会话，随后返回主站。",
            progress: true,
            actions: []
        },
        {
            id: "logout-failed",
            label: "退出失败",
            icon: "alert",
            tone: "warning",
            title: "退出失败",
            desc: "认证中心未响应，但本地会话已清除。可以返回主站继续浏览。",
            meta: [["Error", "end_session_endpoint_timeout"]],
            actions: [
                { label: "重试", icon: "refresh", variant: "primary", run: "logout" },
                { label: "返回主站", icon: "external", variant: "ghost", run: "home" }
            ]
        },
        {
            id: "session-expired",
            label: "会话过期",
            icon: "clock",
            tone: "warning",
            title: "登录已过期",
            desc: "密码修改或长时间未操作会导致会话失效。请重新登录以继续当前操作。",
            countdown: 8,
            meta: [["ReturnUrl", "/#/article-editor"]],
            actions: [
                { label: "立即登录", icon: "log-in", variant: "primary", run: "login" },
                { label: "取消自动跳转", icon: "x", variant: "ghost", run: "cancel" }
            ]
        },
        {
            id: "no-permission",
            label: "权限不足",
            icon: "shield",
            tone: "danger",
            title: "权限不足",
            desc: "当前账号没有后台访问权限，需要 Permissions.System 标志。请更换账号或联系管理员。",
            meta: [["Required", "Permissions.System"], ["Current", "Permissions.Member"]],
            actions: [
                { label: "退出登录", icon: "log-out", variant: "primary", run: "logout" },
                { label: "返回主站", icon: "external", variant: "ghost", run: "home" }
            ]
        },
        {
            id: "not-found",
            label: "页面不存在",
            icon: "alert-circle",
            tone: "warning",
            title: "页面不存在",
            desc: "你访问的地址可能已被移动或删除。在桌面模式下，它也可能是尚未注册的应用。",
            meta: [["Path", "/#/unknown-app"], ["Code", "404"]],
            actions: [
                { label: "回到桌面", icon: "dashboard", variant: "primary", run: "home" },
                { label: "打开命令面板", icon: "command", variant: "ghost", run: "palette" }
            ]
        }
    ];

    var current = "logging-in";
    var countdownTimer = null;
    var countdownLeft = 0;

    function notice(text) {
        var node = document.getElementById("auth-notice");
        node.innerHTML = icon("sparkles") + "<span>" + util.esc(text) + "</span>";
        node.classList.add("is-on");
        window.clearTimeout(notice.timer);
        notice.timer = window.setTimeout(function () {
            node.classList.remove("is-on");
        }, 2600);
    }

    function renderSwitch() {
        var box = document.getElementById("auth-switch");
        box.innerHTML =
            '<span class="auth-switch__label">认证状态 · 设计走查</span>' +
            STATES.map(function (state) {
                return (
                    '<button type="button" class="auth-switch__chip' +
                    (state.id === current ? " is-active" : "") +
                    '" data-state="' +
                    state.id +
                    '">' +
                    icon(state.icon) +
                    util.esc(state.label) +
                    "</button>"
                );
            }).join("");
        box.onclick = function (event) {
            var chip = event.target.closest("[data-state]");
            if (chip) {
                show(chip.getAttribute("data-state"));
            }
        };
    }

    function renderCard() {
        var state = STATES.find(function (item) {
            return item.id === current;
        });
        var card = document.getElementById("auth-card");
        var metaHtml = state.meta
            ? '<div class="auth-card__meta">' +
              state.meta
                  .map(function (row) {
                      return (
                          "<div>" +
                          util.esc(row[0]) +
                          " · <b>" +
                          util.esc(row[1]) +
                          "</b></div>"
                      );
                  })
                  .join("") +
              "</div>"
            : "";
        var progressHtml = state.progress
            ? '<div class="ui-progress" style="width:100%"><i style="width:42%"></i></div>'
            : "";
        var countdownHtml = state.countdown
            ? '<div class="auth-countdown">' +
              '<div class="auth-countdown__row"><span>自动跳转倒计时</span><span data-role="countdown">' +
              countdownLeft +
              " s</span></div>" +
              '<div class="ui-progress ui-progress--warning"><i data-role="countdown-bar" style="width:' +
              (countdownLeft / state.countdown) * 100 +
              '%"></i></div>' +
              "</div>"
            : "";
        var profileHtml = state.profile
            ? '<div class="auth-profile">' +
              '<span class="ui-avatar ui-avatar--lg" style="background-image:linear-gradient(135deg,#22d3ee,#8b5cf6)">胖</span>' +
              "<div>" +
              '<div class="auth-profile__name">' +
              util.esc(DpzOS.data.profile.name) +
              "</div>" +
              '<div class="auth-profile__sub">@' +
              util.esc(DpzOS.data.profile.account) +
              " · System 管理员</div>" +
              "</div>" +
              '<span class="u-grow"></span>' +
              icon("check-circle") +
              "</div>" +
              '<dl class="auth-claims">' +
              [
                  ["sub", "pengqian089"],
                  ["name", "胖子"],
                  ["permissions", "System, Member"],
                  ["picture", "https://dpangzi.com/avatar.png"]
              ]
                  .map(function (row) {
                      return (
                          "<div class=\"auth-claims__row\"><dt>" +
                          util.esc(row[0]) +
                          "</dt><dd>" +
                          util.esc(row[1]) +
                          "</dd></div>"
                      );
                  })
                  .join("") +
              "</dl>"
            : "";

        card.innerHTML =
            '<div class="auth-card__mark' +
            (state.progress ? " auth-card__mark--spin" : "") +
            '">' +
            LOGO +
            "</div>" +
            '<span class="auth-card__state" data-tone="' +
            state.tone +
            '">' +
            (state.progress ? '<span class="auth-card__spinner"></span>' : "") +
            icon(state.icon) +
            "</span>" +
            '<h1 class="auth-card__title">' +
            util.esc(state.title) +
            "</h1>" +
            '<p class="auth-card__desc">' +
            util.esc(state.desc) +
            "</p>" +
            progressHtml +
            countdownHtml +
            profileHtml +
            metaHtml +
            '<div class="auth-card__actions">' +
            state.actions
                .map(function (action) {
                    return (
                        '<button type="button" class="ui-btn ui-btn--' +
                        action.variant +
                        '" data-run="' +
                        action.run +
                        '">' +
                        icon(action.icon) +
                        "<span>" +
                        util.esc(action.label) +
                        "</span></button>"
                    );
                })
                .join("") +
            "</div>" +
            '<div class="auth-card__hint">深链接：?state=' +
            state.id +
            "</div>";

        card.querySelector(".auth-card__actions").onclick = function (event) {
            var button = event.target.closest("[data-run]");
            if (button) {
                run(button.getAttribute("data-run"));
            }
        };
    }

    function stopCountdown() {
        window.clearInterval(countdownTimer);
        countdownTimer = null;
    }

    function startCountdown() {
        stopCountdown();
        countdownLeft = 8;
        countdownTimer = window.setInterval(function () {
            countdownLeft -= 1;
            var text = document.querySelector('[data-role="countdown"]');
            var bar = document.querySelector('[data-role="countdown-bar"]');
            if (text) {
                text.textContent = countdownLeft + " s";
            }
            if (bar) {
                bar.style.width = (countdownLeft / 8) * 100 + "%";
            }
            if (countdownLeft <= 0) {
                stopCountdown();
                notice("演示：即将跳转认证中心");
                show("logging-in");
            }
        }, 1000);
    }

    function run(action) {
        if (action === "cancel") {
            stopCountdown();
            notice("已取消自动跳转（演示）");
        }
        if (action === "home") {
            notice("演示：返回桌面 / 主站");
        }
        if (action === "palette") {
            notice("演示：桌面内按 Ctrl+K 呼出命令面板");
        }
        if (action === "login") {
            show("logging-in");
            window.setTimeout(function () {
                if (current !== "logging-in") {
                    return;
                }
                show("completing-login");
                window.setTimeout(function () {
                    if (current === "completing-login") {
                        show("user-profile");
                        notice("登录成功（演示）");
                    }
                }, 1500);
            }, 1500);
        }
        if (action === "logout") {
            show("completing-logout");
            window.setTimeout(function () {
                if (current === "completing-logout") {
                    show("logged-out");
                    notice("已安全退出（演示）");
                }
            }, 1500);
        }
    }

    function show(id) {
        var state = STATES.find(function (item) {
            return item.id === id;
        });
        if (!state) {
            return;
        }
        current = id;
        stopCountdown();
        if (state.countdown) {
            countdownLeft = state.countdown;
            startCountdown();
        }
        renderSwitch();
        renderCard();
        if (window.history.replaceState) {
            window.history.replaceState(null, "", "?state=" + id);
        }
    }

    function boot() {
        var query = new URLSearchParams(window.location.search);
        var requested = query.get("state");
        var exists = STATES.some(function (item) {
            return item.id === requested;
        });
        show(exists ? requested : "logging-in");
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", boot);
    } else {
        boot();
    }
})(window.DpzOS || (window.DpzOS = {}));
