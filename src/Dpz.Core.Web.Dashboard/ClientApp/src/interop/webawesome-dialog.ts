type DotNetDialog = {
    invokeMethodAsync(methodName: "HandleAfterHideFromDialog"): Promise<void>;
};

type DialogBindings = {
    handleHide: (event: Event) => void;
    handleAfterHide: () => void;
};

class WebAwesomeDialogInterop {
    private readonly bindings = new WeakMap<HTMLElement, DialogBindings>();

    public bindDialog(element: HTMLElement, dotNet: DotNetDialog): void {
        this.unbindDialog(element);

        const handleHide = (event: Event) => {
            if (element.dataset.closing === "true") {
                return;
            }

            if (element.dataset.escToClose !== "true") {
                event.preventDefault();
            }
        };

        const handleAfterHide = () => {
            void dotNet.invokeMethodAsync("HandleAfterHideFromDialog");
        };

        element.addEventListener("wa-hide", handleHide);
        element.addEventListener("wa-after-hide", handleAfterHide);
        this.bindings.set(element, { handleHide, handleAfterHide });
    }

    public hideDialog(element: HTMLElement): void {
        element.dataset.closing = "true";
        element.removeAttribute("open");
    }

    public unbindDialog(element: HTMLElement): void {
        const binding = this.bindings.get(element);

        if (!binding) {
            return;
        }

        element.removeEventListener("wa-hide", binding.handleHide);
        element.removeEventListener("wa-after-hide", binding.handleAfterHide);
        this.bindings.delete(element);
    }

    public notifyContentReady(): void {
        window.dispatchEvent(new CustomEvent("dpz:dialog-content-ready"));
    }
}

const dialogInterop = new WebAwesomeDialogInterop();

export function bindDialog(element: HTMLElement, dotNet: DotNetDialog): void {
    dialogInterop.bindDialog(element, dotNet);
}

export function hideDialog(element: HTMLElement): void {
    dialogInterop.hideDialog(element);
}

export function unbindDialog(element: HTMLElement): void {
    dialogInterop.unbindDialog(element);
}

export function notifyContentReady(): void {
    dialogInterop.notifyContentReady();
}
