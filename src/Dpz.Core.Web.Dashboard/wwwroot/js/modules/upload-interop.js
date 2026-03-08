export async function uploadFormWithProgress(
    files,
    fields,
    uploadUrl,
    accessToken,
    dotNetHelper
) {
    const formData = new FormData();

    if (Array.isArray(files)) {
        for (const file of files) {
            const arrayBuffer = await file.stream.arrayBuffer();
            const blob = new Blob([arrayBuffer], { type: file.contentType });
            formData.append(file.name, blob, file.fileName);
        }
    }

    if (Array.isArray(fields)) {
        fields.forEach((field) => {
            if (field && field.name) {
                formData.append(field.name, field.value ?? '');
            }
        });
    }

    return new Promise((resolve, reject) => {
        const xhr = new XMLHttpRequest();

        xhr.upload.addEventListener('progress', (e) => {
            if (e.lengthComputable && dotNetHelper) {
                const percentComplete = (e.loaded / e.total) * 100;
                dotNetHelper.invokeMethodAsync('ReportProgress', percentComplete);
            }
        });

        xhr.addEventListener('load', () => {
            if (xhr.status >= 200 && xhr.status < 300) {
                resolve(xhr.responseText ?? '');
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
        if (accessToken) {
            xhr.setRequestHeader('Authorization', `Bearer ${accessToken}`);
        }
        xhr.send(formData);
    });
}
