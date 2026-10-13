(function (DpzOS) {
    "use strict";

    function esc(value) {
        if (value === null || value === undefined) {
            return "";
        }
        return String(value)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    function hash(text) {
        var value = 2166136261;
        var input = String(text);
        for (var i = 0; i < input.length; i++) {
            value ^= input.charCodeAt(i);
            value = Math.imul(value, 16777619);
        }
        return Math.abs(value);
    }

    var palettes = [
        ["#22d3ee", "#8b5cf6"],
        ["#f472b6", "#8b5cf6"],
        ["#34d399", "#22d3ee"],
        ["#fbbf24", "#f472b6"],
        ["#60a5fa", "#8b5cf6"],
        ["#fb7185", "#fbbf24"],
        ["#a3e635", "#22d3ee"],
        ["#8b5cf6", "#22d3ee"]
    ];

    function gradient(seed, angle) {
        var pair = palettes[hash(seed) % palettes.length];
        var deg = angle === undefined ? 145 : angle;
        return (
            "linear-gradient(" +
            deg +
            "deg, " +
            pair[0] +
            " 0%, " +
            pair[1] +
            " 100%)"
        );
    }

    function coverStyle(seed, extra) {
        var style =
            "background-image:radial-gradient(120% 120% at 22% 12%, rgba(255,255,255,.16), transparent 44%)," +
            "linear-gradient(160deg, rgba(4,8,16,.3), rgba(3,6,12,.66))," +
            gradient(seed) +
            ";";
        return style + (extra || "");
    }

    function formatNumber(value) {
        var number = Number(value) || 0;
        return number.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    }

    function formatCompact(value) {
        var number = Number(value) || 0;
        if (number >= 100000000) {
            return (number / 100000000).toFixed(1) + "亿";
        }
        if (number >= 10000) {
            return (number / 10000).toFixed(1) + "w";
        }
        if (number >= 1000) {
            return (number / 1000).toFixed(1) + "k";
        }
        return String(number);
    }

    function pad(value) {
        return value < 10 ? "0" + value : String(value);
    }

    function formatTime(date) {
        return pad(date.getHours()) + ":" + pad(date.getMinutes()) + ":" + pad(date.getSeconds());
    }

    function formatClock(date) {
        return pad(date.getHours()) + ":" + pad(date.getMinutes());
    }

    var weekdays = ["周日", "周一", "周二", "周三", "周四", "周五", "周六"];

    function formatDate(date) {
        return (
            date.getFullYear() +
            "-" +
            pad(date.getMonth() + 1) +
            "-" +
            pad(date.getDate()) +
            " " +
            weekdays[date.getDay()]
        );
    }

    function formatBytes(bytes) {
        var value = Number(bytes) || 0;
        var units = ["B", "KB", "MB", "GB"];
        var index = 0;
        while (value >= 1024 && index < units.length - 1) {
            value /= 1024;
            index++;
        }
        return (index === 0 ? value : value.toFixed(1)) + " " + units[index];
    }

    function formatDuration(seconds) {
        var total = Math.max(0, Math.floor(Number(seconds) || 0));
        var hours = Math.floor(total / 3600);
        var minutes = Math.floor((total % 3600) / 60);
        var rest = total % 60;
        if (hours > 0) {
            return hours + ":" + pad(minutes) + ":" + pad(rest);
        }
        return minutes + ":" + pad(rest);
    }

    function relativeTime(date) {
        var diff = Date.now() - date.getTime();
        var minutes = Math.floor(diff / 60000);
        if (minutes < 1) {
            return "刚刚";
        }
        if (minutes < 60) {
            return minutes + " 分钟前";
        }
        var hours = Math.floor(minutes / 60);
        if (hours < 24) {
            return hours + " 小时前";
        }
        var days = Math.floor(hours / 24);
        if (days < 30) {
            return days + " 天前";
        }
        return formatDate(date).slice(0, 10);
    }

    function clamp(value, min, max) {
        return Math.min(max, Math.max(min, value));
    }

    function uid(prefix) {
        return (prefix || "id") + "-" + Math.random().toString(36).slice(2, 9);
    }

    function debounce(fn, wait) {
        var timer = null;
        return function () {
            var args = arguments;
            var self = this;
            clearTimeout(timer);
            timer = setTimeout(function () {
                fn.apply(self, args);
            }, wait);
        };
    }

    function highlight(text, keyword, cls) {
        var safe = esc(text);
        if (!keyword) {
            return safe;
        }
        var needle = esc(keyword);
        var index = safe.toLowerCase().indexOf(needle.toLowerCase());
        if (index < 0) {
            return safe;
        }
        var className = cls || "ui-mark--accent";
        var out = "";
        var rest = safe;
        while (index >= 0) {
            out += rest.slice(0, index) + '<mark class="' + className + '">';
            rest = rest.slice(index);
            out += rest.slice(0, needle.length) + "</mark>";
            rest = rest.slice(needle.length);
            index = rest.toLowerCase().indexOf(needle.toLowerCase());
        }
        return out + rest;
    }

    function initials(name) {
        var text = String(name || "?").trim();
        if (!text) {
            return "?";
        }
        return /[\u4e00-\u9fa5]/.test(text) ? text.slice(0, 1) : text.slice(0, 2).toUpperCase();
    }

    function pinyin(text) {
        var map = {
            概: "gai",
            览: "lan",
            文: "wen",
            章: "zhang",
            相: "xiang",
            册: "ce",
            视: "shi",
            频: "pin",
            音: "yin",
            乐: "yue",
            录: "lu",
            碎: "sui",
            念: "nian",
            时: "shi",
            间: "jian",
            轴: "zhou",
            动: "dong",
            态: "tai",
            页: "ye",
            弹: "dan",
            幕: "mu",
            评: "ping",
            论: "lun",
            网: "wang",
            站: "zhan",
            配: "pei",
            置: "zhi",
            安: "an",
            全: "quan",
            用: "yong",
            户: "hu",
            权: "quan",
            限: "xian",
            源: "yuan",
            码: "ma",
            消: "xiao",
            息: "xi",
            队: "dui",
            列: "lie",
            终: "zhong",
            端: "duan",
            设: "she",
            系: "xi",
            统: "tong",
            管: "guan",
            理: "li",
            友: "you",
            情: "qing",
            链: "lian",
            接: "jie",
            黑: "hei",
            名: "ming",
            单: "dan",
            封: "feng",
            禁: "jin",
            拦: "lan",
            截: "jie",
            规: "gui",
            则: "ze",
            通: "tong",
            知: "zhi",
            工: "gong",
            具: "ju",
            开: "kai",
            发: "fa",
            蓝: "lan",
            图: "tu",
            首: "shou"
        };
        var out = "";
        for (var i = 0; i < text.length; i++) {
            var ch = text.charAt(i);
            out += map[ch] ? map[ch].charAt(0) : ch;
        }
        return out.toLowerCase();
    }

    function matches(query, fields) {
        var keyword = String(query || "").trim().toLowerCase().replace(/\s+/g, "");
        if (!keyword) {
            return true;
        }
        for (var i = 0; i < fields.length; i++) {
            var value = String(fields[i] || "").toLowerCase();
            if (value.indexOf(keyword) >= 0) {
                return true;
            }
            if (pinyin(value).indexOf(keyword) >= 0) {
                return true;
            }
        }
        return false;
    }

    function storageGet(key, fallback) {
        try {
            var raw = window.localStorage.getItem(key);
            return raw ? JSON.parse(raw) : fallback;
        } catch (error) {
            return fallback;
        }
    }

    function storageSet(key, value) {
        try {
            window.localStorage.setItem(key, JSON.stringify(value));
        } catch (error) {
            void error;
        }
    }

    DpzOS.util = {
        esc: esc,
        hash: hash,
        gradient: gradient,
        coverStyle: coverStyle,
        formatNumber: formatNumber,
        formatCompact: formatCompact,
        pad: pad,
        formatTime: formatTime,
        formatClock: formatClock,
        formatDate: formatDate,
        formatBytes: formatBytes,
        formatDuration: formatDuration,
        relativeTime: relativeTime,
        clamp: clamp,
        uid: uid,
        debounce: debounce,
        highlight: highlight,
        initials: initials,
        matches: matches,
        storageGet: storageGet,
        storageSet: storageSet
    };
})(window.DpzOS || (window.DpzOS = {}));
