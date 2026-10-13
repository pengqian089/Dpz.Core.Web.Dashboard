(function (DpzOS) {
    "use strict";

    var apps = {};
    var groupOrder = [
        "工作台",
        "内容创作",
        "互动管理",
        "站点配置",
        "安全防护",
        "开发工具",
        "系统"
    ];

    DpzOS.registerApp = function (definition) {
        apps[definition.id] = definition;
    };

    DpzOS.getApp = function (id) {
        return apps[id] || null;
    };

    DpzOS.listApps = function () {
        return Object.keys(apps).map(function (id) {
            return apps[id];
        });
    };

    DpzOS.appGroups = groupOrder;

    DpzOS.appsByGroup = function () {
        return groupOrder
            .map(function (group) {
                return {
                    name: group,
                    apps: DpzOS.listApps().filter(function (app) {
                        return app.group === group;
                    })
                };
            })
            .filter(function (entry) {
                return entry.apps.length > 0;
            });
    };
})(window.DpzOS || (window.DpzOS = {}));
