import PhotoSwipe from "photoswipe";

type StreamReference = {
    arrayBuffer(): Promise<ArrayBuffer>;
};

class PhotoSwipeGallery {
    private photoViewer: PhotoSwipe | null = null;

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
            this.openPhotoSwipe(image);
        }
    };

    private openPhotoSwipe(image: HTMLImageElement): PhotoSwipe | null {
        if (!image.src) {
            return null;
        }

        const images = this.getGalleryImages(image);
        const index = Math.max(images.indexOf(image), 0);
        this.destroyPhotoViewer();
        this.photoViewer = new PhotoSwipe({
            dataSource: images.map((item) => ({
                src: item.src,
                width: item.naturalWidth || 1920,
                height: item.naturalHeight || 1080
            })),
            index,
            showHideAnimationType: "fade",
            bgOpacity: 0.88,
            escKey: true,
            returnFocus: false,
            zoom: true
        });

        this.photoViewer.init();
        return this.photoViewer;
    }

    private getGalleryImages(image: HTMLImageElement): HTMLImageElement[] {
        const gallery = image.closest(".pswp-gallery");
        if (!gallery) {
            return [image];
        }

        const images = Array.from(gallery.querySelectorAll("img")).filter(
            (item): item is HTMLImageElement => item instanceof HTMLImageElement && !!item.src
        );
        return images.length > 0 ? images : [image];
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
