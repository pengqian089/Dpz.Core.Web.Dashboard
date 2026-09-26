(function (DpzOS) {
    "use strict";

    var STORAGE_KEY = "dpz-os:settings";

    var defaults = {
        accent: "cyan",
        wallpaper: "aurora",
        density: "cozy",
        motion: "on",
        restoreSession: true
    };

    var listeners = [];

    var settings = {
        state: Object.assign({}, defaults, DpzOS.util.storageGet(STORAGE_KEY, {})),

        accents: [
            { id: "cyan", label: "青蓝", color: "#22d3ee", color2: "#8b5cf6" },
            { id: "magenta", label: "品红", color: "#f472b6", color2: "#22d3ee" },
            { id: "violet", label: "紫罗兰", color: "#a78bfa", color2: "#22d3ee" },
            { id: "lime", label: "酸性绿", color: "#a3e635", color2: "#22d3ee" },
            { id: "amber", label: "琥珀", color: "#fbbf24", color2: "#fb7185" }
        ],

        wallpapers: [
            { id: "aurora", label: "极光" },
            { id: "mesh", label: "星云" },
            { id: "grid", label: "网格" },
            { id: "void", label: "深空" }
        ],

        apply: function () {
            var root = document.documentElement;
            root.setAttribute("data-accent", settings.state.accent);
            root.setAttribute("data-wallpaper", settings.state.wallpaper);
            root.setAttribute("data-density", settings.state.density);
            root.setAttribute("data-motion", settings.state.motion);
        },

        set: function (key, value) {
            settings.state[key] = value;
            DpzOS.util.storageSet(STORAGE_KEY, settings.state);
            settings.apply();
            listeners.forEach(function (listener) {
                listener(key, value);
            });
        },

        onChange: function (listener) {
            listeners.push(listener);
        }
    };

    DpzOS.settings = settings;
})(window.DpzOS || (window.DpzOS = {}));
