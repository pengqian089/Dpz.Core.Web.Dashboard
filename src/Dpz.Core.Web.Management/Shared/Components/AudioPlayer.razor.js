export class AudioPlayer {
    constructor(dotNetHelper, audioElement) {
        this.dotNetHelper = dotNetHelper;
        this.audio = audioElement;
        this.isDragging = false;
        this.isPlaying = false;
        this.onTimeUpdate = this.handleTimeUpdate.bind(this);
        this.onLoadedMetadata = this.handleLoadedMetadata.bind(this);
        this.onEnded = this.handleEnded.bind(this);
        this.onError = this.handleError.bind(this);
        
        this.setupEvents();
    }

    setupEvents() {
        this.audio.addEventListener("timeupdate", this.onTimeUpdate);
        this.audio.addEventListener("loadedmetadata", this.onLoadedMetadata);
        this.audio.addEventListener("ended", this.onEnded);
        this.audio.addEventListener("error", this.onError);
    }

    handleTimeUpdate() {
        if (!this.audio || !this.dotNetHelper || this.isDragging) {
            return;
        }

        this.dotNetHelper.invokeMethodAsync("OnTimeUpdate", this.audio.currentTime);
    }

    handleLoadedMetadata() {
        if (!this.audio || !this.dotNetHelper) {
            return;
        }

        this.dotNetHelper.invokeMethodAsync("OnDurationChange", this.audio.duration);
    }

    handleEnded() {
        if (!this.dotNetHelper) {
            return;
        }

        this.isPlaying = false;
        this.dotNetHelper.invokeMethodAsync("OnEnded");
    }

    handleError() {
        if (!this.audio || !this.isPlaying) {
            return;
        }
        
        const error = this.audio.error;
        if (!error) {
            return;
        }

        console.warn("Audio playback failed.", {
            code: error.code,
            message: error.message,
            src: this.audio.currentSrc || this.audio.src
        });
    }

    play() {
        this.isPlaying = true;
        return this.audio.play().catch((error) => {
            this.isPlaying = false;
            throw error;
        });
    }

    pause() {
        this.isPlaying = false;
        this.audio.pause();
    }

    setCurrentTime(time) {
        if (isFinite(time)) {
            this.audio.currentTime = time;
        }
    }
    
    setVolume(value) {
        this.audio.volume = value;
    }

    dispose() {
        if (this.audio) {
            this.audio.removeEventListener("timeupdate", this.onTimeUpdate);
            this.audio.removeEventListener("loadedmetadata", this.onLoadedMetadata);
            this.audio.removeEventListener("ended", this.onEnded);
            this.audio.removeEventListener("error", this.onError);
        }

        this.audio = null;
        this.dotNetHelper = null;
    }
}

export function init(dotNetHelper, audioElement) {
    return new AudioPlayer(dotNetHelper, audioElement);
}
