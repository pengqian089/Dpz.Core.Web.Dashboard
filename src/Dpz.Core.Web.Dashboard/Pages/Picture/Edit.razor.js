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

function bindPreviewClick(img) {
    img.style.cursor = "zoom-in";
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
    const galleries = document.querySelectorAll(selector || '.pswp-gallery');
    
    galleries.forEach(gallery => {
        const imgs = gallery.querySelectorAll('img');
        imgs.forEach(img => {
            if (img.complete && img.src) {
                bindPreviewClick(img);
            } else if (img.src) {
                img.onload = function() {
                    bindPreviewClick(img);
                };
            }
        });
    });
}

export function destroyPhotoViewer() {
    if (photoViewer && typeof photoViewer.destroy === 'function') {
        photoViewer.destroy();
    }
    photoViewer = null;
}
