import PhotoSwipe from 'https://dpangzi.com/library/photoswipe/photoswipe.esm.min.js';

let photoViewer = null;
let pswpRoot = null;

function ensureRoot() {
    if (pswpRoot) return pswpRoot;
    pswpRoot = document.createElement('div');
    pswpRoot.className = 'pswp';
    document.body.appendChild(pswpRoot);
    return pswpRoot;
}

export async function setImagePreview(imageElementId, imageStream) {
    const arrayBuffer = await imageStream.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const img = document.getElementById(imageElementId);
    if (img) {
        img.src = url;
        img.style.display = "inline";
        img.style.cursor = "zoom-in";
        
        img.onload = function() {
            bindPreviewClick(img);
        };
    }
}

function bindPreviewClick(img) {
    img.removeEventListener('click', handlePreviewClick);
    img.addEventListener('click', handlePreviewClick);
}

function handlePreviewClick(event) {
    event.preventDefault();
    const img = event.currentTarget;
    openPhotoSwipe(img);
}

function openPhotoSwipe(imgElement) {
    const src = imgElement?.src;
    if (!src) return;

    const width = imgElement.naturalWidth || 1920;
    const height = imgElement.naturalHeight || 1080;

    const pswp = new PhotoSwipe({
        dataSource: [{ src, width, height }],
        index: 0,
        showHideAnimationType: 'fade',
        bgOpacity: 0.88,
        escKey: true,
        returnFocus: false,
        zoom: true,
        pswpModule: ensureRoot()
    });

    pswp.init();
    photoViewer = pswp;
    return pswp;
}

export function initPhotoSwipe(selector) {
    const img = document.querySelector(selector + ' img');
    if (img && img.complete && img.src) {
        bindPreviewClick(img);
    }
}

export function destroyPhotoViewer() {
    if (photoViewer && typeof photoViewer.destroy === 'function') {
        photoViewer.destroy();
    }
    photoViewer = null;
}

