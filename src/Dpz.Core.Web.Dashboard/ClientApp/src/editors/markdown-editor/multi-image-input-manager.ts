/**
 * 多图片上传增强管理器。
 *
 * Milkdown 的图片上传弹窗默认只处理单文件。本管理器监听动态创建的
 * image file input，将它们改为 multiple，并在用户一次选择多张图片时
 * 抢先处理事件：第一张图片交给当前弹窗，其余图片由上层追加为 Markdown。
 */
export class MultiImageInputManager {
    private readonly observer: MutationObserver;
    private readonly onChange = (event: Event) => {
        const input = event.target;
        if (!(input instanceof HTMLInputElement) || input.type !== "file") {
            return;
        }

        const files = input.files;
        if (!this.isImageInput(input) || !files || !this.shouldHandle(input, files)) {
            return;
        }

        event.preventDefault();
        event.stopPropagation();
        event.stopImmediatePropagation();

        const images = Array.from(files).filter((file) => file.type.includes("image"));
        if (images.length === 0) {
            input.value = "";
            return;
        }

        this.onUpload(input, images)
            .catch((error: unknown) => {
                console.error("An error occurred while uploading images");
                console.error(error);
            })
            .finally(() => {
                input.value = "";
            });
    };

    public constructor(
        private readonly root: HTMLElement,
        private readonly onUpload: (input: HTMLInputElement, files: File[]) => Promise<void>,
        private readonly shouldHandle: (input: HTMLInputElement, files: FileList) => boolean = (
            _,
            files
        ) => files.length > 1
    ) {
        this.observer = new MutationObserver(() => this.apply());
    }

    /** 开始监听 file input 变化，capture 阶段保证先于 Milkdown 处理多选。 */
    public start(): void {
        this.apply();
        this.root.addEventListener("change", this.onChange, true);
        this.observer.observe(this.root, {
            childList: true,
            subtree: true
        });
    }

    /** 停止监听 DOM 与 change 事件，防止编辑器销毁后保留回调。 */
    public stop(): void {
        this.root.removeEventListener("change", this.onChange, true);
        this.observer.disconnect();
    }

    private apply(): void {
        this.root.querySelectorAll<HTMLInputElement>('input[type="file"]').forEach((input) => {
            if (this.isImageInput(input)) {
                input.multiple = true;
            }
        });
    }

    private isImageInput(input: HTMLInputElement): boolean {
        return input.accept
            .split(",")
            .map((value) => value.trim().toLowerCase())
            .some((value) => value === "image/*" || value.startsWith("image/"));
    }
}
