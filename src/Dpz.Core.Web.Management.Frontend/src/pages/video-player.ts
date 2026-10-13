class VideoPlayerPage {
    private readonly players = new Map<string, import("hls.js").default>();
    private hlsModule: Promise<typeof import("hls.js").default> | null = null;

    public async initVideoPlayer(videoId: string, url: string): Promise<void> {
        const video = document.getElementById(videoId);
        if (!(video instanceof HTMLVideoElement)) {
            console.error(`Video element with id "${videoId}" not found`);
            return;
        }

        this.disposeVideoPlayer(videoId);

        video.controls = true;
        video.style.width = "100%";
        video.style.height = "100%";

        if (video.canPlayType("application/vnd.apple.mpegurl")) {
            video.src = url;
            return;
        }

        const Hls = await this.getHls();
        if (Hls.isSupported()) {
            const hls = new Hls();
            hls.attachMedia(video);
            hls.on(Hls.Events.MEDIA_ATTACHED, () => {
                hls.loadSource(url);
            });
            this.players.set(videoId, hls);
            return;
        }

        console.error("HLS is not supported in this browser");
    }

    public disposeVideoPlayer(videoId: string): void {
        const player = this.players.get(videoId);
        if (!player) {
            return;
        }

        player.destroy();
        this.players.delete(videoId);
    }

    public dispose(): void {
        this.players.forEach((player) => player.destroy());
        this.players.clear();
    }

    private getHls(): Promise<typeof import("hls.js").default> {
        this.hlsModule ??= import("hls.js").then((module) => module.default);
        return this.hlsModule;
    }
}

const videoPlayerPage = new VideoPlayerPage();

export async function initVideoPlayer(videoId: string, url: string): Promise<void> {
    await videoPlayerPage.initVideoPlayer(videoId, url);
}

export function disposeVideoPlayer(videoId: string): void {
    videoPlayerPage.disposeVideoPlayer(videoId);
}

export function dispose(): void {
    videoPlayerPage.dispose();
}
