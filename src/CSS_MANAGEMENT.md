# CSS 管理与维护指南

## 概览 (Overview)

本项目采用自定义的 CSS 架构，旨在 Blazor WebAssembly 环境中实现无需预处理器（如 SASS/SCSS）的可维护性和可扩展性。构建系统（`build.ps1`）会将特定的 CSS 文件合并为一个单一的 `global.min.css` 包。

## 文件结构 (File Structure)

```
wwwroot/css/
 _variables.css       # 全局设计变量（颜色、间距等）
 _layout.css          # 主布局结构（侧边栏、顶部导航）
 _form.css            # 共享的表单控件与按钮样式
 _tag-selector.css    # 共享的标签/徽章组件样式
 _pagination.css      # 共享的分页样式
 _skeleton.css        # 加载骨架屏样式
 index.css            # 仪表盘/首页专用样式
 article-list.css     # 文章管理列表页样式
 article-form.css     # 文章发布/编辑页样式
 ... (其他页面专用文件)
```

## 关键原则 (Key Principles)

### 1. "下划线" 约定 (The "Underscore" Convention)
以 `_` 开头的文件（例如 `_form.css`）是 **抽象/共享组件**。
- 它们包含在多个页面中使用的样式。
- 它们通常应优先加载或具有全局通用性。
- **不要** 将特定页面的逻辑放在这里。

### 2. BEM 命名约定 (BEM Naming Convention)
我们严格使用 [BEM](http://getbem.com/) (Block Element Modifier) 来防止样式冲突。

- **Block (块)**: `.article-card`
- **Element (元素)**: `.article-card__title` (双下划线)
- **Modifier (修饰符)**: `.article-card--featured` (双连字符)

**错误示例 (Bad):**
```css
.card { ... }
.title { ... } /* 太通用，容易产生冲突 */
```

**正确示例 (Good):**
```css
.article-card { ... }
.article-card__title { ... }
```

### 3. 关注点分离 (Separation of Concerns)
- **Layout (布局)**: 定义在 `_layout.css` 中。处理外壳（侧边栏、页头）。
- **Components (组件)**: 按钮、输入框、标签等位于各自的 `_*.css` 文件中。
- **Pages (页面)**: 特定功能的样式（例如 `.music-player`）应放在 `music-list.css` 中。

### 4. 避免重复 (Avoiding Duplication)
在编写新的 CSS 之前:
1. 检查 `_variables.css` 获取标准颜色/间距。
2. 检查 `_form.css` 获取输入框/按钮样式。
3. 检查 `_tag-selector.css` 获取标签/Chips样式。

**禁止** 将 `.btn` 或 `.form-control` 的样式复制粘贴到页面特定的 CSS 文件中。

## 工作流 (Workflows)

### 添加新页面
1. 创建 `mypage.css`。
2. 尽可能将所有样式包裹在一个唯一的块级类中，或使用 BEM 前缀（例如 `.mypage-container`）。
3. 使用全局变量：
   ```css
   .mypage-header {
       padding: var(--spacing-md);
       color: var(--text-primary);
   }
   ```

### 修改共享组件
1. 编辑 `_form.css`（举例）。
2. **测试** 所有使用该组件的页面（例如文章编辑页、设置页）。
3. 不要在共享文件中添加特定页面的覆盖样式。

### 构建与部署
运行构建脚本以合并和压缩样式文件：
```powershell
.\build.ps1
```
此操作需要安装 `cleancss`。

## 常用变量参考 (Variable Reference)
(完整列表请参阅 `_variables.css`)
- `--primary`: 主品牌色
- `--bg-surface`: 卡片/容器背景色
- `--text-primary`: 主要文本颜色
- `--spacing-md`: 标准内边距 (1rem)
- `--border-radius`: 标准圆角
