import photoSwipeStylesUrl from "./styles/feature-photoswipe.css?url";
import { ensureStylesheet } from "./interop/stylesheet";

type StreamReference = {
    arrayBuffer(): Promise<ArrayBuffer>;
};

type PhotoSwipeModule = typeof import("photoswipe").default;
type PhotoSwipeInstance = InstanceType<PhotoSwipeModule>;

class PhotoSwipeGallery {
    private photoViewer: PhotoSwipeInstance | null = null;
    private photoSwipeModule: Promise<PhotoSwipeModule> | null = null;

    public async setImagePreview(
        imageElementId: string,
        imageStream: StreamReference
    ): Promise<void> {
        const arrayBuffer = await imageStream.arrayBuffer();
        const blob = new Blob([arrayBuffer]);
        const url = URL.createObjectURL(blob);
        const image = document.getElementById(imageElementId);
        if (!(image instanceof HTMLImageElement)) {
            return;
        }

        image.src = url;
        image.style.display = "inline";
        image.style.cursor = "zoom-in";
        image.onload = () => this.bindPreviewClick(image);
    }

    public initPhotoSwipe(selector?: string): void {
        ensureStylesheet(photoSwipeStylesUrl);
        const galleries = document.querySelectorAll(selector || ".pswp-gallery");
        galleries.forEach((gallery) => {
            gallery.querySelectorAll("img").forEach((image) => {
                if (!(image instanceof HTMLImageElement) || !image.src) {
                    return;
                }

                if (image.complete) {
                    this.bindPreviewClick(image);
                    return;
                }

                image.onload = () => this.bindPreviewClick(image);
            });
        });
    }

    public destroyPhotoViewer(): void {
        this.photoViewer?.destroy();
        this.photoViewer = null;
    }

    private bindPreviewClick(image: HTMLImageElement): void {
        image.style.cursor = "zoom-in";
        image.removeEventListener("click", this.handlePreviewClick);
        image.addEventListener("click", this.handlePreviewClick);
    }

    private readonly handlePreviewClick = (event: Event): void => {
        event.preventDefault();
        const image = event.currentTarget;
        if (image instanceof HTMLImageElement) {
            void this.openPhotoSwipe(image);
        }
    };

    private async openPhotoSwipe(image: HTMLImageElement): Promise<PhotoSwipeInstance | null> {
        if (!image.src) {
            return null;
        }

        const PhotoSwipe = await this.getPhotoSwipe();
        this.destroyPhotoViewer();
        this.photoViewer = new PhotoSwipe({
            dataSource: [
                {
                    src: image.src,
                    width: image.naturalWidth || 1920,
                    height: image.naturalHeight || 1080
                }
            ],
            index: 0,
            showHideAnimationType: "fade",
            bgOpacity: 0.88,
            escKey: true,
            returnFocus: false,
            zoom: true
        });

        this.photoViewer.init();
        return this.photoViewer;
    }

    private getPhotoSwipe(): Promise<PhotoSwipeModule> {
        this.photoSwipeModule ??= import("photoswipe").then((module) => module.default);
        return this.photoSwipeModule;
    }
}

const gallery = new PhotoSwipeGallery();

export async function setImagePreview(
    imageElementId: string,
    imageStream: StreamReference
): Promise<void> {
    await gallery.setImagePreview(imageElementId, imageStream);
}

export function initPhotoSwipe(selector?: string): void {
    gallery.initPhotoSwipe(selector);
}

export function destroyPhotoViewer(): void {
    gallery.destroyPhotoViewer();
}
