export class AudioPlayer {
    constructor(dotNetHelper, audioElement) {
        this.dotNetHelper = dotNetHelper;
        this.audio = audioElement;
        this.isDragging = false;
        
        this.setupEvents();
    }

    setupEvents() {
        // Time Update
        this.audio.addEventListener('timeupdate', () => {
            if (!this.isDragging) {
                this.dotNetHelper.invokeMethodAsync('OnTimeUpdate', this.audio.currentTime);
            }
        });

        // Metadata Loaded (Duration)
        this.audio.addEventListener('loadedmetadata', () => {
            this.dotNetHelper.invokeMethodAsync('OnDurationChange', this.audio.duration);
        });

        // Ended
        this.audio.addEventListener('ended', () => {
            this.dotNetHelper.invokeMethodAsync('OnEnded');
        });
        
        // Error
        this.audio.addEventListener('error', (e) => {
            console.error("Audio Error:", e);
        });
    }

    play() {
        return this.audio.play();
    }

    pause() {
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
        this.audio = null;
        this.dotNetHelper = null;
    }
}

export function init(dotNetHelper, audioElement) {
    return new AudioPlayer(dotNetHelper, audioElement);
}
