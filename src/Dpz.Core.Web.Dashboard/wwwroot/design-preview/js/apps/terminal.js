(function (DpzOS) {
    "use strict";

    var ui = DpzOS.ui;
    var util = DpzOS.util;
    var icon = DpzOS.icon;
    var settings = DpzOS.settings;

    var BANNER = [
        "  ██████╗ ██████╗ ███████╗     ██████╗ ███████╗",
        "  ██╔══██╗██╔══██╗╚══███╔╝    ██╔═══██╗██╔════╝",
        "  ██║  ██║██████╔╝  ███╔╝     ██║   ██║███████╗",
        "  ██║  ██║██╔═══╝  ███╔╝      ██║   ██║╚════██║",
        "  ██████╔╝██║     ███████╗    ╚██████╔╝███████║",
        "  ╚═════╝ ╚═╝     ╚══════╝     ╚═════╝ ╚══════╝"
    ].join("\n");

    function bootLines() {
        return [
            { cls: "app-term__line--ok", text: "DPZ OS v2.7.0-preview (NEXUS) — 静态设计预览" },
            { text: "输入 help 查看可用命令，↑↓ 浏览历史。" },
            { cls: "app-term__line--warn", text: "提示：所有数据均为演示数据，不会调用真实接口。" }
        ];
    }

    function render(ctx) {
        return (
            '<div class="app-term">' +
            '<div class="app-term__out" data-role="term-out">' +
            bootLines()
                .map(function (line) {
                    return '<div class="app-term__line ' + (line.cls || "") + '">' + util.esc(line.text) + "</div>";
                })
                .join("") +
            "</div>" +
            '<div class="app-term__input">' +
            '<span class="app-term__prompt">pengqian089@nexus:~$</span>' +
            '<input type="text" data-role="term-input" autocomplete="off" spellcheck="false" placeholder="输入命令…">' +
            "</div>" +
            "</div>"
        );
    }

    function mount(rootNode, ctx) {
        var out = rootNode.querySelector('[data-role="term-out"]');
        var input = rootNode.querySelector('[data-role="term-input"]');
        var history = [];
        var historyIndex = -1;

        function print(text, cls) {
            var line = document.createElement("div");
            line.className = "app-term__line " + (cls || "");
            line.textContent = text;
            out.appendChild(line);
            out.scrollTop = out.scrollHeight;
        }

        function printCommand(command) {
            var line = document.createElement("div");
            line.className = "app-term__line app-term__line--cmd";
            line.innerHTML =
                '<b>pengqian089@nexus:~$</b> ' + util.esc(command) + '<span class="ui-terminal__cursor"></span>';
            out.appendChild(line);
            out.scrollTop = out.scrollHeight;
        }

        function execute(raw) {
            var command = raw.trim();
            if (!command) {
                return;
            }
            printCommand(command);
            history.unshift(command);
            historyIndex = -1;
            var parts = command.split(/\s+/);
            var name = parts[0].toLowerCase();
            var argument = parts.slice(1).join(" ");

            if (name === "help") {
                print("可用命令：");
                print("  help                     显示帮助");
                print("  apps                     列出全部应用");
                print("  open <app-id>            打开应用窗口");
                print("  close                    关闭当前窗口");
                print("  theme <accent>           切换强调色（cyan/magenta/violet/lime/amber）");
                print("  wallpaper <id>           切换壁纸（aurora/mesh/grid/void）");
                print("  date                     显示当前时间");
                print("  whoami                   当前登录账号");
                print("  neofetch                 系统信息");
                print("  echo <text>              回显文本");
                print("  clear                    清屏");
                print("  exit                     关闭终端");
                return;
            }
            if (name === "apps") {
                DpzOS.listApps().forEach(function (app) {
                    print(
                        "  " + app.id.padEnd(20) + app.name + "  ·  " + app.group +
                            (app.status === "planned" ? "  [蓝图]" : "")
                    );
                });
                return;
            }
            if (name === "open") {
                var app = DpzOS.getApp(argument);
                if (!app) {
                    print("找不到应用：" + argument, "app-term__line--err");
                    return;
                }
                DpzOS.wm.open(argument);
                print("已打开 " + app.name, "app-term__line--ok");
                return;
            }
            if (name === "close") {
                ctx.close();
                return;
            }
            if (name === "theme") {
                var exists = settings.accents.some(function (accent) {
                    return accent.id === argument;
                });
                if (!exists) {
                    print("未知强调色，可选：" + settings.accents.map(function (a) { return a.id; }).join(", "), "app-term__line--err");
                    return;
                }
                settings.set("accent", argument);
                print("强调色已切换为 " + argument, "app-term__line--ok");
                return;
            }
            if (name === "wallpaper") {
                var known = settings.wallpapers.some(function (item) {
                    return item.id === argument;
                });
                if (!known) {
                    print("未知壁纸，可选：" + settings.wallpapers.map(function (w) { return w.id; }).join(", "), "app-term__line--err");
                    return;
                }
                settings.set("wallpaper", argument);
                print("壁纸已切换为 " + argument, "app-term__line--ok");
                return;
            }
            if (name === "date") {
                var now = new Date();
                print(util.formatDate(now) + " " + util.formatTime(now));
                return;
            }
            if (name === "whoami") {
                print(DpzOS.data.profile.account + " · " + DpzOS.data.profile.role);
                return;
            }
            if (name === "neofetch") {
                print(BANNER, "app-term__line--ok");
                print("");
                print("  OS        DPZ OS 2.7.0-preview (NEXUS)");
                print("  Shell     nexshell 1.0");
                print("  Runtime   Blazor WebAssembly · net11.0");
                print("  UI        Cyber Desktop · Dark");
                print("  Theme     " + settings.state.accent + " / " + settings.state.wallpaper);
                print("  Windows   " + DpzOS.wm.list().length + " 个已打开");
                print("  Data      静态演示数据");
                return;
            }
            if (name === "echo") {
                print(argument || "");
                return;
            }
            if (name === "clear") {
                out.innerHTML = "";
                return;
            }
            if (name === "exit") {
                ctx.close();
                return;
            }
            print("未知命令：" + name + "，输入 help 查看帮助", "app-term__line--err");
        }

        input.addEventListener("keydown", function (event) {
            if (event.key === "Enter") {
                execute(input.value);
                input.value = "";
            }
            if (event.key === "ArrowUp") {
                event.preventDefault();
                if (history.length) {
                    historyIndex = Math.min(history.length - 1, historyIndex + 1);
                    input.value = history[historyIndex];
                }
            }
            if (event.key === "ArrowDown") {
                event.preventDefault();
                if (historyIndex > 0) {
                    historyIndex -= 1;
                    input.value = history[historyIndex];
                } else {
                    historyIndex = -1;
                    input.value = "";
                }
            }
        });

        input.focus();
        ctx.onClose(function () {
            void input;
        });
    }

    DpzOS.registerApp({
        id: "terminal",
        name: "终端",
        en: "Terminal",
        icon: "terminal",
        tone: "success",
        group: "系统",
        size: { w: 860, h: 540 },
        singleton: true,
        desc: "命令驱动外壳：打开应用、切换主题、查看状态",
        render: render,
        mount: mount
    });
})(window.DpzOS);
