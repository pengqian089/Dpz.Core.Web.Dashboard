(function (DpzOS) {
    "use strict";

    var util = DpzOS.util;
    var icon = DpzOS.icon;
    var settings = DpzOS.settings;

    var SESSION_KEY = "dpz-os:session";
    var windows = {};
    var order = [];
    var zTop = 20;
    var activeId = null;
    var listeners = [];
    var saveTimer = null;

    function isMobile() {
        return window.matchMedia("(max-width: 767px)").matches;
    }

    function layer() {
        return document.getElementById("windows");
    }

    function emit() {
        listeners.forEach(function (listener) {
            listener();
        });
        scheduleSave();
    }

    function onChange(listener) {
        listeners.push(listener);
    }

    function scheduleSave() {
        window.clearTimeout(saveTimer);
        saveTimer = window.setTimeout(saveSession, 400);
    }

    function saveSession() {
        if (!settings.state.restoreSession || isMobile()) {
            return;
        }
        var payload = order
            .map(function (id) {
                var record = windows[id];
                if (!record) {
                    return null;
                }
                var rect = record.maximized ? record.prevRect : currentRect(record);
                return {
                    appId: record.appId,
                    params: normalizeParams(record.params),
                    x: Math.round(rect.x),
                    y: Math.round(rect.y),
                    w: Math.round(rect.w),
                    h: Math.round(rect.h),
                    maximized: record.maximized,
                    minimized: record.minimized
                };
            })
            .filter(Boolean);
        util.storageSet(SESSION_KEY, payload);
    }

    function normalizeParams(params) {
        if (!params) {
            return null;
        }
        var out = {};
        Object.keys(params).forEach(function (key) {
            if (key.indexOf("__") !== 0) {
                out[key] = params[key];
            }
        });
        return out;
    }

    function currentRect(record) {
        var style = record.root.style;
        return {
            x: parseFloat(style.getPropertyValue("--wx")) || 0,
            y: parseFloat(style.getPropertyValue("--wy")) || 0,
            w: parseFloat(style.getPropertyValue("--ww")) || 0,
            h: parseFloat(style.getPropertyValue("--wh")) || 0
        };
    }

    function setRect(record, rect) {
        var style = record.root.style;
        style.setProperty("--wx", rect.x + "px");
        style.setProperty("--wy", rect.y + "px");
        style.setProperty("--ww", rect.w + "px");
        style.setProperty("--wh", rect.h + "px");
    }

    function defaultGeometry(app, index) {
        var size = app.size || { w: 980, h: 640 };
        var taskbar = 54;
        var width = Math.min(size.w, window.innerWidth - 48);
        var height = Math.min(size.h, window.innerHeight - taskbar - 40);
        var step = (index % 5) * 28;
        var centered = (window.innerWidth - width) / 2;
        var x = window.innerWidth > 1279 ? Math.max(256, centered - 60 + step) : centered - 20 + step;
        x = util.clamp(x, 16, Math.max(16, window.innerWidth - width - 16));
        return {
            w: width,
            h: height,
            x: x,
            y: Math.max(14, (window.innerHeight - taskbar - height) / 2 - 26 + step)
        };
    }

    function findOpenByApp(appId) {
        for (var i = 0; i < order.length; i++) {
            var record = windows[order[i]];
            if (record && record.appId === appId) {
                return record;
            }
        }
        return null;
    }

    function open(appId, options) {
        var app = DpzOS.getApp(appId);
        if (!app) {
            DpzOS.toast({ title: "应用不存在", text: appId, tone: "danger" });
            return null;
        }
        var opts = options || {};

        if (app.singleton !== false) {
            var existing = findOpenByApp(appId);
            if (existing) {
                if (existing.minimized) {
                    restore(existing.id);
                }
                focus(existing.id);
                if (opts.params) {
                    existing.params = Object.assign({}, existing.params, opts.params);
                    render(existing);
                }
                return existing.id;
            }
        }

        var id = util.uid("win");
        var geometry = opts.geometry || defaultGeometry(app, order.length);
        var rootNode = document.createElement("section");
        rootNode.className = "os-window";
        rootNode.setAttribute("data-win-id", id);
        rootNode.setAttribute("data-app-id", app.id);
        rootNode.setAttribute("role", "dialog");
        rootNode.setAttribute("aria-label", app.name);

        rootNode.innerHTML =
            '<header class="os-window__titlebar" data-role="titlebar">' +
            '<button type="button" class="os-window__back" data-role="back" aria-label="返回">' +
            icon("arrow-left") +
            "</button>" +
            '<div class="os-window__identity">' +
            '<span class="os-window__glyph">' +
            icon(app.icon) +
            "</span>" +
            '<div class="os-window__titles">' +
            '<div class="os-window__title">' +
            util.esc(app.name) +
            "</div>" +
            '<div class="os-window__subtitle" data-role="subtitle">' +
            util.esc(app.en || app.group || "") +
            "</div>" +
            "</div>" +
            "</div>" +
            '<span class="os-window__spacer"></span>' +
            '<span class="os-window__badge">' +
            util.esc(app.status === "planned" ? "设计蓝图" : "在线") +
            "</span>" +
            '<div class="os-window__controls">' +
            '<button type="button" class="os-winbtn os-winbtn--min" data-role="min" aria-label="最小化">' +
            icon("minus") +
            "</button>" +
            '<button type="button" class="os-winbtn os-winbtn--max" data-role="max" aria-label="最大化">' +
            icon("maximize") +
            "</button>" +
            '<button type="button" class="os-winbtn os-winbtn--close" data-role="close" aria-label="关闭">' +
            icon("x") +
            "</button>" +
            "</div>" +
            "</header>" +
            '<div class="os-window__loadbar" data-role="loadbar"></div>' +
            '<div class="os-window__body"></div>' +
            ['n', "s", "e", "w", "ne", "nw", "se", "sw"]
                .map(function (dir) {
                    return (
                        '<div class="os-window__resize os-window__resize--' +
                        dir +
                        '" data-dir="' +
                        dir +
                        '"></div>'
                    );
                })
                .join("");

        var record = {
            id: id,
            appId: app.id,
            app: app,
            params: opts.params || {},
            state: {},
            root: rootNode,
            maximized: false,
            minimized: false,
            prevRect: null,
            cleanups: [],
            mountCleanup: null
        };
        windows[id] = record;
        order.push(id);

        if (opts.x !== undefined && opts.y !== undefined) {
            geometry = { x: opts.x, y: opts.y, w: geometry.w, h: geometry.h };
        }
        if (opts.maximized || app.size === "max") {
            record.root.classList.add("is-maximized");
            record.maximized = true;
        }
        setRect(record, geometry);
        layer().appendChild(rootNode);

        bindWindow(record);
        if (opts.minimized) {
            record.minimized = true;
            record.root.classList.add("is-minimized");
        }
        syncMaxButton(record);
        render(record);
        updateFocusClasses();
        if (!record.minimized) {
            focus(id);
        } else {
            emit();
        }
        return id;
    }

    function bindWindow(record) {
        var rootNode = record.root;
        var titlebar = rootNode.querySelector('[data-role="titlebar"]');

        rootNode.addEventListener("pointerdown", function () {
            if (activeId !== record.id && !record.minimized) {
                focus(record.id);
            }
        });

        rootNode.querySelector('[data-role="close"]').addEventListener("click", function () {
            close(record.id);
        });
        rootNode.querySelector('[data-role="min"]').addEventListener("click", function () {
            minimize(record.id);
        });
        rootNode.querySelector('[data-role="max"]').addEventListener("click", function () {
            toggleMaximize(record.id);
        });
        rootNode.querySelector('[data-role="back"]').addEventListener("click", function () {
            close(record.id);
        });

        titlebar.addEventListener("dblclick", function (event) {
            if (event.target.closest("button")) {
                return;
            }
            toggleMaximize(record.id);
        });

        bindDrag(record, titlebar);
        rootNode.querySelectorAll("[data-dir]").forEach(function (handle) {
            bindResize(record, handle);
        });
    }

    function bindDrag(record, titlebar) {
        var dragging = false;
        var start = null;

        titlebar.addEventListener("pointerdown", function (event) {
            if (isMobile() || event.button !== 0 || event.target.closest("button")) {
                return;
            }
            dragging = true;
            start = {
                x: event.clientX,
                y: event.clientY,
                rect: currentRect(record),
                wasMaximized: record.maximized,
                moved: false
            };
            titlebar.setPointerCapture(event.pointerId);
            record.root.classList.add("is-dragging");
        });

        titlebar.addEventListener("pointermove", function (event) {
            if (!dragging) {
                return;
            }
            var dx = event.clientX - start.x;
            var dy = event.clientY - start.y;
            if (Math.abs(dx) + Math.abs(dy) > 4) {
                start.moved = true;
            }
            if (start.wasMaximized) {
                if (!start.moved) {
                    return;
                }
                restoreGeometryFromDrag(record, start, event);
                start.wasMaximized = false;
                return;
            }
            var rect = {
                x: util.clamp(start.rect.x + dx, -start.rect.w + 120, window.innerWidth - 120),
                y: util.clamp(start.rect.y + dy, 0, window.innerHeight - 60),
                w: start.rect.w,
                h: start.rect.h
            };
            setRect(record, rect);
            showSnapPreview(detectSnap(event));
        });

        function finishDrag(event) {
            if (!dragging) {
                return;
            }
            dragging = false;
            record.root.classList.remove("is-dragging");
            titlebar.releasePointerCapture(event.pointerId);
            var snap = start.moved ? detectSnap(event) : null;
            hideSnapPreview();
            if (snap) {
                applySnap(record, snap);
            }
            scheduleSave();
        }

        titlebar.addEventListener("pointerup", finishDrag);
        titlebar.addEventListener("pointercancel", finishDrag);
    }

    function restoreGeometryFromDrag(record, start, event) {
        var ratio = event.clientX / window.innerWidth;
        var geometry = defaultGeometry(record.app, 0);
        record.root.classList.remove("is-maximized");
        record.maximized = false;
        syncMaxButton(record);
        setRect(record, {
            x: util.clamp(event.clientX - geometry.w * ratio, 0, window.innerWidth - 160),
            y: Math.max(0, event.clientY - 20),
            w: geometry.w,
            h: geometry.h
        });
        start.x = event.clientX;
        start.y = event.clientY;
        start.rect = currentRect(record);
    }

    function detectSnap(event) {
        var margin = 8;
        if (isMobile()) {
            return null;
        }
        if (event.clientY <= margin) {
            return "max";
        }
        if (event.clientX <= margin) {
            return "left";
        }
        if (event.clientX >= window.innerWidth - margin) {
            return "right";
        }
        return null;
    }

    function showSnapPreview(target) {
        var preview = document.getElementById("snap-preview");
        if (!target) {
            preview.hidden = true;
            return;
        }
        var taskbar = 54;
        var rect = { x: 8, y: 8, w: window.innerWidth - 16, h: window.innerHeight - taskbar - 16 };
        if (target === "left") {
            rect.w = window.innerWidth / 2 - 12;
        }
        if (target === "right") {
            rect.x = window.innerWidth / 2 + 4;
            rect.w = window.innerWidth / 2 - 12;
        }
        preview.style.left = rect.x + "px";
        preview.style.top = rect.y + "px";
        preview.style.width = rect.w + "px";
        preview.style.height = rect.h + "px";
        preview.hidden = false;
    }

    function hideSnapPreview() {
        var preview = document.getElementById("snap-preview");
        if (preview) {
            preview.hidden = true;
        }
    }

    function applySnap(record, target) {
        var taskbar = 54;
        if (target === "max") {
            if (!record.maximized) {
                toggleMaximize(record.id);
            }
            return;
        }
        if (record.maximized) {
            record.root.classList.remove("is-maximized");
            record.maximized = false;
        }
        var half = window.innerWidth / 2;
        var rect =
            target === "left"
                ? { x: 0, y: 0, w: half, h: window.innerHeight - taskbar }
                : { x: half, y: 0, w: half, h: window.innerHeight - taskbar };
        setRect(record, rect);
    }

    function bindResize(record, handle) {
        var dir = handle.getAttribute("data-dir");
        var resizing = false;
        var start = null;

        handle.addEventListener("pointerdown", function (event) {
            if (isMobile() || record.maximized || event.button !== 0) {
                return;
            }
            resizing = true;
            start = { x: event.clientX, y: event.clientY, rect: currentRect(record) };
            handle.setPointerCapture(event.pointerId);
            record.root.classList.add("is-resizing");
        });

        handle.addEventListener("pointermove", function (event) {
            if (!resizing) {
                return;
            }
            var dx = event.clientX - start.x;
            var dy = event.clientY - start.y;
            var rect = Object.assign({}, start.rect);
            var minW = 380;
            var minH = 240;
            if (dir.indexOf("e") >= 0) {
                rect.w = Math.max(minW, start.rect.w + dx);
            }
            if (dir.indexOf("s") >= 0) {
                rect.h = Math.max(minH, start.rect.h + dy);
            }
            if (dir.indexOf("w") >= 0) {
                var width = Math.max(minW, start.rect.w - dx);
                rect.x = start.rect.x + (start.rect.w - width);
                rect.w = width;
            }
            if (dir.indexOf("n") >= 0) {
                var height = Math.max(minH, start.rect.h - dy);
                rect.y = Math.max(0, start.rect.y + (start.rect.h - height));
                rect.h = height;
            }
            setRect(record, rect);
        });

        function finishResize(event) {
            if (!resizing) {
                return;
            }
            resizing = false;
            handle.releasePointerCapture(event.pointerId);
            record.root.classList.remove("is-resizing");
            scheduleSave();
        }

        handle.addEventListener("pointerup", finishResize);
        handle.addEventListener("pointercancel", finishResize);
    }

    function render(record) {
        if (record.mountCleanup) {
            record.mountCleanup();
            record.mountCleanup = null;
        }
        var body = record.root.querySelector(".os-window__body");
        var ctx = createContext(record);
        record.ctx = ctx;
        var host = document.createElement("div");
        host.className = "os-app-host";
        host.setAttribute("data-app-id", record.appId);
        host.innerHTML = record.app.render(ctx);
        body.innerHTML = "";
        body.appendChild(host);
        if (typeof record.app.mount === "function") {
            record.mountCleanup = record.app.mount(host, ctx) || null;
        }
        if (ctx.__subtitle) {
            record.root.querySelector('[data-role="subtitle"]').textContent = ctx.__subtitle;
        }
        emit();
    }

    function createContext(record) {
        return {
            winId: record.id,
            app: record.app,
            params: record.params,
            state: record.state,
            isMobile: isMobile,
            rerender: function () {
                render(record);
            },
            setSubtitle: function (text) {
                record.root.querySelector('[data-role="subtitle"]').textContent = text;
            },
            toast: DpzOS.toast,
            dialog: function (options) {
                return DpzOS.dialog.open(options);
            },
            confirm: function (options) {
                return DpzOS.dialog.confirm(options);
            },
            notify: function (notice) {
                DpzOS.notifications.push(notice);
            },
            open: function (appId, params) {
                return open(appId, { params: params });
            },
            close: function () {
                close(record.id);
            },
            onClose: function (fn) {
                record.cleanups.push(fn);
            }
        };
    }

    function focus(id) {
        var record = windows[id];
        if (!record) {
            return;
        }
        activeId = id;
        zTop += 1;
        record.root.style.setProperty("--wz", zTop);
        order = order.filter(function (item) {
            return item !== id;
        });
        order.push(id);
        updateFocusClasses();
        emit();
    }

    function updateFocusClasses() {
        order.forEach(function (id) {
            var record = windows[id];
            if (!record) {
                return;
            }
            record.root.classList.toggle("is-active", id === activeId && !record.minimized);
            record.root.classList.toggle("is-inactive", id !== activeId || record.minimized);
        });
    }

    function minimize(id) {
        var record = windows[id];
        if (!record || isMobile()) {
            if (record) {
                close(id);
            }
            return;
        }
        record.minimized = true;
        record.root.classList.add("is-minimizing");
        window.setTimeout(function () {
            if (record.minimized) {
                record.root.classList.add("is-minimized");
                record.root.classList.remove("is-minimizing");
            }
        }, 260);
        if (activeId === id) {
            activeId = previousVisible(id);
            if (activeId) {
                zTop += 1;
                windows[activeId].root.style.setProperty("--wz", zTop);
            }
        }
        updateFocusClasses();
        emit();
    }

    function previousVisible(currentId) {
        for (var i = order.length - 1; i >= 0; i--) {
            var id = order[i];
            if (id !== currentId && windows[id] && !windows[id].minimized) {
                return id;
            }
        }
        return null;
    }

    function restore(id) {
        var record = windows[id];
        if (!record) {
            return;
        }
        record.minimized = false;
        record.root.classList.remove("is-minimized", "is-minimizing");
        focus(id);
    }

    function syncMaxButton(record) {
        var button = record.root.querySelector('[data-role="max"]');
        if (button) {
            button.innerHTML = icon(record.maximized ? "restore" : "maximize");
            button.setAttribute("aria-label", record.maximized ? "还原" : "最大化");
        }
    }

    function toggleMaximize(id) {
        var record = windows[id];
        if (!record || isMobile()) {
            return;
        }
        if (record.maximized) {
            record.root.classList.remove("is-maximized");
            record.maximized = false;
            if (record.prevRect) {
                setRect(record, record.prevRect);
            }
        } else {
            record.prevRect = currentRect(record);
            record.root.classList.add("is-maximized");
            record.maximized = true;
        }
        syncMaxButton(record);
        emit();
    }

    function close(id) {
        var record = windows[id];
        if (!record) {
            return;
        }
        if (record.mountCleanup) {
            record.mountCleanup();
            record.mountCleanup = null;
        }
        record.cleanups.forEach(function (fn) {
            fn();
        });
        record.root.classList.add("is-closing");
        delete windows[id];
        order = order.filter(function (item) {
            return item !== id;
        });
        window.setTimeout(function () {
            record.root.remove();
        }, 190);
        if (activeId === id) {
            activeId = previousVisible(id);
            if (activeId) {
                zTop += 1;
                windows[activeId].root.style.setProperty("--wz", zTop);
            }
        }
        updateFocusClasses();
        emit();
    }

    function closeActive() {
        if (activeId) {
            close(activeId);
        }
    }

    function getActive() {
        return activeId ? windows[activeId] || null : null;
    }

    function list() {
        return order
            .map(function (id) {
                return windows[id];
            })
            .filter(Boolean);
    }

    function restoreSession() {
        if (isMobile()) {
            return false;
        }
        var saved = util.storageGet(SESSION_KEY, null);
        if (!Array.isArray(saved) || saved.length === 0) {
            return false;
        }
        var restored = 0;
        saved.forEach(function (entry) {
            if (!DpzOS.getApp(entry.appId)) {
                return;
            }
            open(entry.appId, {
                params: entry.params || {},
                geometry: { x: entry.x, y: entry.y, w: entry.w, h: entry.h },
                maximized: entry.maximized,
                minimized: entry.minimized
            });
            restored += 1;
        });
        return restored > 0;
    }

    DpzOS.wm = {
        open: open,
        close: close,
        closeActive: closeActive,
        focus: focus,
        minimize: minimize,
        restore: restore,
        toggleMaximize: toggleMaximize,
        render: render,
        list: list,
        getActive: getActive,
        onChange: onChange,
        isMobile: isMobile,
        restoreSession: restoreSession,
        findOpenByApp: findOpenByApp,
        clearSession: function () {
            util.storageSet(SESSION_KEY, []);
        }
    };
})(window.DpzOS || (window.DpzOS = {}));
