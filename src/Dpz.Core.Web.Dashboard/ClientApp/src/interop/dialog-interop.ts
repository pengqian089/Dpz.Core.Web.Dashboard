type DotNetHelper = {
    invokeMethodAsync(methodName: string): Promise<void>;
};

type DomChangeManager = {
    pause(): void;
    resume(): void;
};

declare global {
    interface Window {
        appDOMManager?: DomChangeManager;
    }
}

class DialogInterop {
    private isDialogOpen = false;
    private viewportChangeHandler: (() => void) | null = null;

    public isOpen(): boolean {
        return this.isDialogOpen;
    }

    public disableBodyScroll(disableScroll = true): void {
        this.isDialogOpen = true;
        this.bindViewportChange();

        if (disableScroll) {
            window.appDOMManager?.pause();
            document.body.style.overflow = "hidden";
        }
    }

    public enableBodyScroll(): void {
        this.isDialogOpen = false;
        this.unbindViewportChange();
        document.body.style.overflow = "";
        window.appDOMManager?.resume();
    }

    public initKeyboardListener(dotNetHelper: DotNetHelper): void {
        document.addEventListener("keydown", (event) => {
            if (event.key === "Escape") {
                void dotNetHelper.invokeMethodAsync("HandleGlobalEsc");
            }
        });
    }

    public updateLazyLoad(): void {
        window.dispatchEvent(new CustomEvent("dpz:dialog-content-ready"));
    }

    private updateDialogViewportHeight(): void {
        const viewportHeight = window.visualViewport?.height ?? window.innerHeight;
        document.documentElement.style.setProperty(
            "--dialog-viewport-height",
            `${viewportHeight}px`
        );
    }

    private bindViewportChange(): void {
        if (this.viewportChangeHandler) {
            return;
        }

        this.viewportChangeHandler = () => this.updateDialogViewportHeight();
        this.updateDialogViewportHeight();

        window.addEventListener("resize", this.viewportChangeHandler);
        window.visualViewport?.addEventListener("resize", this.viewportChangeHandler);
        window.visualViewport?.addEventListener("scroll", this.viewportChangeHandler);
    }

    private unbindViewportChange(): void {
        if (!this.viewportChangeHandler) {
            return;
        }

        window.removeEventListener("resize", this.viewportChangeHandler);
        window.visualViewport?.removeEventListener("resize", this.viewportChangeHandler);
        window.visualViewport?.removeEventListener("scroll", this.viewportChangeHandler);
        this.viewportChangeHandler = null;
        document.documentElement.style.removeProperty("--dialog-viewport-height");
    }
}

const dialogInterop = new DialogInterop();

export function isOpen(): boolean {
    return dialogInterop.isOpen();
}

export function disableBodyScroll(disableScroll = true): void {
    dialogInterop.disableBodyScroll(disableScroll);
}

export function enableBodyScroll(): void {
    dialogInterop.enableBodyScroll();
}

export function initKeyboardListener(dotNetHelper: DotNetHelper): void {
    dialogInterop.initKeyboardListener(dotNetHelper);
}

export function updateLazyLoad(): void {
    dialogInterop.updateLazyLoad();
}
