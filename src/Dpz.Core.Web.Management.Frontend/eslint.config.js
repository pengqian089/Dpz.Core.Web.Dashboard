import js from "@eslint/js";
import tseslint from "typescript-eslint";

export default tseslint.config(
    js.configs.recommended,
    ...tseslint.configs.recommended,
    {
        files: ["src/**/*.ts", "vite.config.ts"],
        rules: {
            curly: ["error", "all"],
            "line-comment-position": ["error", { position: "above" }],
            "max-len": ["error", { code: 100, ignoreStrings: true, ignoreTemplateLiterals: true }]
        }
    },
    {
        // sync-index.mjs 是纯 JS 的 Node 模块，不适用 TS 的规则集，只套用风格约束。
        // 只声明真正用到的 Node 全局变量，避免为此引入 globals 依赖。
        files: ["sync-index.mjs"],
        languageOptions: {
            globals: {
                console: "readonly",
                process: "readonly",
                URL: "readonly"
            }
        },
        rules: {
            curly: ["error", "all"],
            "line-comment-position": ["error", { position: "above" }],
            "max-len": ["error", { code: 100, ignoreStrings: true, ignoreTemplateLiterals: true }]
        }
    }
);
