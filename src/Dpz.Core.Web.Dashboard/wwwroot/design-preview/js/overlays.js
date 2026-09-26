(function (DpzOS) {
    "use strict";

    var util = DpzOS.util;
    var icon = DpzOS.icon;
    var ui = DpzOS.ui;

    function root(id) {
        return document.getElementById(id);
    }

    function closeOnBackdrop(node, close) {
        node.addEventListener("click", function (event) {
            if (event.target === node) {
                close();
            }
        });
    }

    function animateOut(node, done) {
        node.classList.add("is-leaving");
        window.setTimeout(done, 160);
    }

    function openDialog(options) {
        var opts = options || {};
        var tone = opts.tone || "accent";
        var modal = document.createElement("div");
        modal.className = "os-modal";
        modal.innerHTML =
            '<div class="os-modal__box" data-tone="' +
            util.esc(tone) +
            '" role="dialog" aria-modal="true">' +
            '<div class="os-modal__head">' +
            '<span class="os-modal__icon">' +
            icon(opts.icon || (tone === "danger" ? "alert" : "info")) +
            "</span>" +
            "<div>" +
            '<div class="os-modal__title">' +
            util.esc(opts.title || "确认操作") +
            "</div>" +
            (opts.subtitle
                ? '<div class="os-modal__subtitle">' + util.esc(opts.subtitle) + "</div>"
                : "") +
            "</div>" +
            "</div>" +
            '<div class="os-modal__body">' +
            (opts.html || "<p>" + util.esc(opts.text || "") + "</p>") +
            "</div>" +
            '<div class="os-modal__foot">' +
            (opts.cancelText === null
                ? ""
                : '<button type="button" class="ui-btn" data-act="cancel">' +
                  util.esc(opts.cancelText || "取消") +
                  "</button>") +
            '<button type="button" class="ui-btn ' +
            (tone === "danger" ? "ui-btn--danger" : "ui-btn--primary") +
            '" data-act="ok">' +
            util.esc(opts.confirmText || "确定") +
            "</button>" +
            "</div>" +
            "</div>";

        root("modal-root").appendChild(modal);

        return new Promise(function (resolve) {
            var settled = false;

            function finish(value) {
                if (settled) {
                    return;
                }
                settled = true;
                animateOut(modal, function () {
                    modal.remove();
                });
                resolve(value);
            }

            modal.addEventListener("click", function (event) {
                if (event.target.closest('[data-act="cancel"]')) {
                    finish(false);
                }
                if (event.target.closest('[data-act="ok"]')) {
                    finish(true);
                }
            });
            closeOnBackdrop(modal, function () {
                finish(false);
            });

            function onKey(event) {
                if (event.key === "Escape") {
                    event.stopPropagation();
                    window.removeEventListener("keydown", onKey, true);
                    finish(false);
                }
                if (event.key === "Enter") {
                    event.stopPropagation();
                    window.removeEventListener("keydown", onKey, true);
                    finish(true);
                }
            }

            window.addEventListener("keydown", onKey, true);
            var focusTarget = modal.querySelector('[data-act="ok"]');
            if (focusTarget) {
                focusTarget.focus();
            }
        });
    }

    function prompt(options) {
        var opts = options || {};
        var modal = document.createElement("div");
        modal.className = "os-modal";
        modal.innerHTML =
            '<div class="os-modal__box">' +
            '<div class="os-modal__head">' +
            '<span class="os-modal__icon">' +
            icon(opts.icon || "edit") +
            "</span>" +
            "<div>" +
            '<div class="os-modal__title">' +
            util.esc(opts.title || "输入内容") +
            "</div>" +
            "</div>" +
            "</div>" +
            '<div class="os-modal__body">' +
            '<label class="ui-field">' +
            (opts.label
                ? '<span class="ui-field__label">' + util.esc(opts.label) + "</span>"
                : "") +
            '<input class="ui-input" data-role="value" value="' +
            util.esc(opts.value || "") +
            '" placeholder="' +
            util.esc(opts.placeholder || "") +
            '">' +
            "</label>" +
            "</div>" +
            '<div class="os-modal__foot">' +
            '<button type="button" class="ui-btn" data-act="cancel">取消</button>' +
            '<button type="button" class="ui-btn ui-btn--primary" data-act="ok">确定</button>' +
            "</div>" +
            "</div>";
        root("modal-root").appendChild(modal);
        var input = modal.querySelector('[data-role="value"]');

        return new Promise(function (resolve) {
            function finish(value) {
                animateOut(modal, function () {
                    modal.remove();
                });
                resolve(value);
            }

            modal.addEventListener("click", function (event) {
                if (event.target.closest('[data-act="cancel"]')) {
                    finish(null);
                }
                if (event.target.closest('[data-act="ok"]')) {
                    finish(input.value);
                }
            });
            closeOnBackdrop(modal, function () {
                finish(null);
            });
            input.addEventListener("keydown", function (event) {
                if (event.key === "Enter") {
                    finish(input.value);
                }
            });
            input.focus();
            input.select();
        });
    }

    var lightboxState = null;

    function openLightbox(options) {
        var opts = options || {};
        var node = root("lightbox");
        lightboxState = opts;
        node.innerHTML =
            '<div class="os-lightbox__stage">' +
            '<div class="os-lightbox__image" style="' +
            util.coverStyle(opts.seed || opts.title || "image") +
            ';background-size:cover;background-position:center"></div>' +
            "</div>" +
            '<aside class="os-lightbox__side">' +
            '<button type="button" class="ui-btn ui-btn--sm os-lightbox__close" data-act="close">' +
            icon("x") +
            "<span>关闭</span></button>" +
            '<div>' +
            '<div class="ui-title">' +
            util.esc(opts.title || "图片预览") +
            "</div>" +
            (opts.desc ? '<div class="ui-subtitle">' + util.esc(opts.desc) + "</div>" : "") +
            "</div>" +
            '<div class="ui-callout"><div>' +
            (opts.metaHtml || "") +
            "</div></div>" +
            "</aside>";
        node.hidden = false;
        node.addEventListener("click", function (event) {
            if (event.target === node || event.target.closest('[data-act="close"]')) {
                node.hidden = true;
                node.innerHTML = "";
                lightboxState = null;
            }
        });
    }

    function closeLightbox() {
        var node = root("lightbox");
        if (node) {
            node.hidden = true;
            node.innerHTML = "";
            lightboxState = null;
        }
    }

    var contextNode = null;

    function openContextMenu(event, items) {
        var node = root("context-menu");
        contextNode = node;
        var html = items
            .map(function (item) {
                if (item.sep) {
                    return '<div class="os-context__sep"></div>';
                }
                return (
                    '<button type="button" class="os-context__item" data-idx="' +
                    items.indexOf(item) +
                    '">' +
                    (item.icon ? icon(item.icon) : '<span style="width:16px"></span>') +
                    "<span>" +
                    util.esc(item.label) +
                    "</span>" +
                    (item.shortcut
                        ? "<small>" + util.esc(item.shortcut) + "</small>"
                        : "") +
                    "</button>"
                );
            })
            .join("");
        node.innerHTML = html;
        node.hidden = false;
        var x = event.clientX;
        var y = event.clientY;
        var rect = node.getBoundingClientRect();
        if (x + rect.width > window.innerWidth - 8) {
            x = window.innerWidth - rect.width - 8;
        }
        if (y + rect.height > window.innerHeight - 8) {
            y = window.innerHeight - rect.height - 8;
        }
        node.style.left = Math.max(8, x) + "px";
        node.style.top = Math.max(8, y) + "px";

        node.onclick = function (clickEvent) {
            var button = clickEvent.target.closest("[data-idx]");
            if (!button) {
                return;
            }
            var item = items[Number(button.getAttribute("data-idx"))];
            closeContextMenu();
            if (item && item.action) {
                item.action();
            }
        };
    }

    function closeContextMenu() {
        if (contextNode) {
            contextNode.hidden = true;
            contextNode.innerHTML = "";
            contextNode = null;
        }
    }

    DpzOS.dialog = {
        confirm: function (options) {
            return openDialog(options);
        },
        alert: function (options) {
            var opts = Object.assign({}, options, { cancelText: null, confirmText: "知道了" });
            return openDialog(opts);
        },
        open: openDialog,
        prompt: prompt
    };

    DpzOS.lightbox = {
        open: openLightbox,
        close: closeLightbox,
        isOpen: function () {
            return lightboxState !== null;
        }
    };

    DpzOS.contextMenu = {
        open: openContextMenu,
        close: closeContextMenu
    };

    DpzOS.overlays = {
        closeAll: function () {
            closeContextMenu();
        }
    };
})(window.DpzOS || (window.DpzOS = {}));
