import js from "@eslint/js";
import tseslint from "typescript-eslint";

export default tseslint.config(js.configs.recommended, ...tseslint.configs.recommended, {
    files: ["src/**/*.ts", "vite.config.ts"],
    rules: {
        curly: ["error", "all"],
        "line-comment-position": ["error", { position: "above" }],
        "max-len": ["error", { code: 100, ignoreStrings: true, ignoreTemplateLiterals: true }]
    }
});
