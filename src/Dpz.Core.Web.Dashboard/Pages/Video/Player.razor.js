import Hls from 'https://dpangzi.com/library/hls.js/hls.mjs';

export function initVideoPlayer(videoId, url) {
    const video = document.getElementById(videoId);
    if (!video) {
        console.error(`Video element with id "${videoId}" not found`);
        return;
    }

    video.controls = true;
    video.style.width = '100%';
    video.style.height = '100%';

    if (Hls.isSupported()) {
        const hls = new Hls();
        hls.attachMedia(video);
        hls.on(Hls.Events.MEDIA_ATTACHED, function () {
            hls.loadSource(url);
        });
    } else if (video.canPlayType('application/vnd.apple.mpegurl')) {
        video.src = url;
    } else {
        console.error('HLS is not supported in this browser');
    }
}
