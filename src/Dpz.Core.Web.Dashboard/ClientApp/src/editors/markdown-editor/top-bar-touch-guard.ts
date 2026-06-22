const dragThreshold = 8;
const syntheticPointerDownFlag = "markdownEditorSyntheticPointerDown";

type PointerDownEvent = PointerEvent & {
    [syntheticPointerDownFlag]?: boolean;
};

type GestureState = {
    pointerId: number;
    target: HTMLElement;
    startX: number;
    startY: number;
    dragged: boolean;
};

/**
 * 移动端顶部工具栏触摸守卫。
 *
 * Milkdown 顶部工具栏按钮在 pointerdown 阶段就执行命令。触摸屏横向拖动工具栏时，
 * 原始 pointerdown 会先触发按钮命令，再发生滚动。这里在 capture 阶段拦截触摸型
 * pointerdown：如果后续移动超过阈值，就把它视为拖动；如果没有移动，则补发一次
 * 带内部标记的 pointerdown，让 Milkdown 按原逻辑执行点击。
 */
export class TopBarTouchGuard {
    private gesture: GestureState | null = null;

    private readonly onPointerDown = (event: PointerEvent): void => {
        const guardedEvent = event as PointerDownEvent;
        if (guardedEvent[syntheticPointerDownFlag] || !this.shouldGuard(event)) {
            return;
        }

        const target = this.getActionTarget(event.target);
        if (!target) {
            return;
        }

        event.preventDefault();
        event.stopPropagation();
        event.stopImmediatePropagation();

        this.gesture = {
            pointerId: event.pointerId,
            target,
            startX: event.clientX,
            startY: event.clientY,
            dragged: false
        };
    };

    private readonly onPointerMove = (event: PointerEvent): void => {
        const gesture = this.gesture;
        if (!gesture || gesture.pointerId !== event.pointerId) {
            return;
        }

        const deltaX = Math.abs(event.clientX - gesture.startX);
        const deltaY = Math.abs(event.clientY - gesture.startY);
        if (deltaX >= dragThreshold || deltaY >= dragThreshold) {
            gesture.dragged = true;
        }
    };

    private readonly onPointerUp = (event: PointerEvent): void => {
        const gesture = this.gesture;
        if (!gesture || gesture.pointerId !== event.pointerId) {
            return;
        }

        this.gesture = null;
        if (gesture.dragged) {
            return;
        }

        this.dispatchSyntheticPointerDown(gesture.target, event);
    };

    private readonly onPointerCancel = (event: PointerEvent): void => {
        if (this.gesture?.pointerId === event.pointerId) {
            this.gesture = null;
        }
    };

    public constructor(private readonly root: HTMLElement) {}

    /** 开始监听工具栏触摸输入。 */
    public start(): void {
        this.root.addEventListener("pointerdown", this.onPointerDown, true);
        this.root.addEventListener("pointermove", this.onPointerMove, true);
        this.root.addEventListener("pointerup", this.onPointerUp, true);
        this.root.addEventListener("pointercancel", this.onPointerCancel, true);
    }

    /** 停止监听，避免编辑器销毁后留下 capture 阶段事件处理器。 */
    public stop(): void {
        this.root.removeEventListener("pointerdown", this.onPointerDown, true);
        this.root.removeEventListener("pointermove", this.onPointerMove, true);
        this.root.removeEventListener("pointerup", this.onPointerUp, true);
        this.root.removeEventListener("pointercancel", this.onPointerCancel, true);
        this.gesture = null;
    }

    private shouldGuard(event: PointerEvent): boolean {
        return (
            event.pointerType === "touch" &&
            this.root.classList.contains("markdown-editor-container--mobile")
        );
    }

    private getActionTarget(target: EventTarget | null): HTMLElement | null {
        if (!(target instanceof Element)) {
            return null;
        }

        return target.closest<HTMLElement>(".top-bar-item, .top-bar-heading-button");
    }

    private dispatchSyntheticPointerDown(target: HTMLElement, source: PointerEvent): void {
        const event = new PointerEvent("pointerdown", {
            bubbles: true,
            cancelable: true,
            composed: true,
            pointerId: source.pointerId,
            pointerType: source.pointerType,
            clientX: source.clientX,
            clientY: source.clientY,
            button: source.button,
            buttons: source.buttons,
            ctrlKey: source.ctrlKey,
            altKey: source.altKey,
            shiftKey: source.shiftKey,
            metaKey: source.metaKey
        }) as PointerDownEvent;

        event[syntheticPointerDownFlag] = true;
        target.dispatchEvent(event);
    }
}
