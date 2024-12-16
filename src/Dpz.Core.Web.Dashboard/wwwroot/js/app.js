const imageExtensionNames = [".jpg", ".jpeg", ".gif", ".png", ".bmp", ".webp", ".jiff"];
async function setImageUsingStreaming(imageElementId, imageStream) {
    const arrayBuffer = await imageStream.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    let img = document.getElementById(imageElementId);
    img.src = url;
    img.style.display = "inline";    
}

function initVideoPlayer(url) {
    let video = document.getElementById("videoPlayer");
    if (Hls.isSupported() && video != null) {
        video.controls = true;
        video.style.width = "100%";
        video.style.height = "100%";
        let hls = new Hls();
        hls.attachMedia(video);
        hls.on(Hls.Events.MEDIA_ATTACHED, function () {
            hls.loadSource(url);
        });
    }
}