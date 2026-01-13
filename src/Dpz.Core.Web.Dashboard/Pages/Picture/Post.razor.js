import PhotoSwipeLightbox from 'https://dpangzi.com/library/photoswipe/photoswipe.esm.min.js';
import PhotoSwipe from 'https://dpangzi.com/library/photoswipe/photoswipe.esm.min.js';

export async function setImagePreview(imageElementId, imageStream) {
    const arrayBuffer = await imageStream.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const img = document.getElementById(imageElementId);
    if (img) {
        img.src = url;
        img.style.display = "inline";
    }
}

export async function uploadWithProgress(formData, uploadUrl, progressCallback) {
    return new Promise((resolve, reject) => {
        const xhr = new XMLHttpRequest();

        xhr.upload.addEventListener('progress', (e) => {
            if (e.lengthComputable) {
                const percentComplete = (e.loaded / e.total) * 100;
                progressCallback(percentComplete);
            }
        });

        xhr.addEventListener('load', () => {
            if (xhr.status >= 200 && xhr.status < 300) {
                resolve(xhr.response);
            } else {
                reject(new Error(`上传失败: ${xhr.status} ${xhr.statusText}`));
            }
        });

        xhr.addEventListener('error', () => {
            reject(new Error('网络错误'));
        });

        xhr.addEventListener('abort', () => {
            reject(new Error('上传已取消'));
        });

        xhr.open('POST', uploadUrl);
        xhr.send(formData);
    });
}
