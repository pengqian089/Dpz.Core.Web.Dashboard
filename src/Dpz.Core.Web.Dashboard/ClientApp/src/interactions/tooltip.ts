import "@awesome.me/webawesome/dist/components/tooltip/tooltip.js";
import type WaTooltip from "@awesome.me/webawesome/dist/components/tooltip/tooltip.js";

type TooltipPlacement = WaTooltip["placement"];

type TooltipOptions = {
    placement?: TooltipPlacement;
    trigger?: string;
    showDelay?: number;
    hideDelay?: number;
    distance?: number;
};

const defaultOptions: Required<TooltipOptions> = {
    placement: "top",
    trigger: "hover focus",
    showDelay: 180,
    hideDelay: 80,
    distance: 8
};

class TooltipIdFactory {
    private nextId = 0;

    public createId(): string {
        this.nextId += 1;
        return `dpz-tooltip-target-${this.nextId}`;
    }
}

export class TooltipController {
    private readonly tooltips = new Map<HTMLElement, WaTooltip>();
    private readonly idFactory = new TooltipIdFactory();

    public register(target: HTMLElement, label: string, options: TooltipOptions = {}): WaTooltip {
        const tooltip = this.ensureTooltip(target);
        const nextOptions = { ...defaultOptions, ...options };

        target.removeAttribute("title");
        target.setAttribute("aria-label", label);

        tooltip.textContent = label;
        tooltip.placement = nextOptions.placement;
        tooltip.trigger = nextOptions.trigger;
        tooltip.showDelay = nextOptions.showDelay;
        tooltip.hideDelay = nextOptions.hideDelay;
        tooltip.distance = nextOptions.distance;
        tooltip.setAttribute("for", this.ensureTargetId(target));

        return tooltip;
    }

    public unregister(target: HTMLElement): void {
        this.tooltips.get(target)?.remove();
        this.tooltips.delete(target);
    }

    public destroy(): void {
        this.tooltips.forEach((tooltip) => tooltip.remove());
        this.tooltips.clear();
    }

    private ensureTooltip(target: HTMLElement): WaTooltip {
        const existing = this.tooltips.get(target);
        if (existing?.isConnected) {
            return existing;
        }

        const tooltip = document.createElement("wa-tooltip") as WaTooltip;
        tooltip.classList.add("dpz-tooltip");
        document.body.appendChild(tooltip);
        this.tooltips.set(target, tooltip);
        return tooltip;
    }

    private ensureTargetId(target: HTMLElement): string {
        if (target.id) {
            return target.id;
        }

        target.id = this.idFactory.createId();
        return target.id;
    }
}
