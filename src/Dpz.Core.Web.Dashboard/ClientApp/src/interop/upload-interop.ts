type UploadStream = {
    arrayBuffer(): Promise<ArrayBuffer>;
};

type UploadFilePart = {
    stream: UploadStream;
    contentType: string;
    name: string;
    fileName: string;
};

type UploadFormField = {
    name: string;
    value?: string;
};

type UploadProgressHelper = {
    invokeMethodAsync(methodName: string, value: number): Promise<void>;
};

class UploadInterop {
    public async uploadFormWithProgress(
        files: UploadFilePart[] | null,
        fields: UploadFormField[] | null,
        uploadUrl: string,
        accessToken: string | null,
        dotNetHelper: UploadProgressHelper | null
    ): Promise<string> {
        const formData = await this.createFormData(files, fields);

        return await new Promise<string>((resolve, reject) => {
            const xhr = new XMLHttpRequest();

            xhr.upload.addEventListener("progress", (event) => {
                if (event.lengthComputable && dotNetHelper) {
                    const percentComplete = (event.loaded / event.total) * 100;
                    void dotNetHelper.invokeMethodAsync("ReportProgress", percentComplete);
                }
            });

            xhr.addEventListener("load", () => {
                if (xhr.status >= 200 && xhr.status < 300) {
                    resolve(xhr.responseText ?? "");
                    return;
                }

                reject(new Error(`上传失败: ${xhr.status} ${xhr.statusText}`));
            });

            xhr.addEventListener("error", () => reject(new Error("网络错误")));
            xhr.addEventListener("abort", () => reject(new Error("上传已取消")));

            xhr.open("POST", uploadUrl);
            if (accessToken) {
                xhr.setRequestHeader("Authorization", `Bearer ${accessToken}`);
            }
            xhr.send(formData);
        });
    }

    private async createFormData(
        files: UploadFilePart[] | null,
        fields: UploadFormField[] | null
    ): Promise<FormData> {
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
                if (field?.name) {
                    formData.append(field.name, field.value ?? "");
                }
            });
        }

        return formData;
    }
}

const uploadInterop = new UploadInterop();

export async function uploadFormWithProgress(
    files: UploadFilePart[] | null,
    fields: UploadFormField[] | null,
    uploadUrl: string,
    accessToken: string | null,
    dotNetHelper: UploadProgressHelper | null
): Promise<string> {
    return await uploadInterop.uploadFormWithProgress(
        files,
        fields,
        uploadUrl,
        accessToken,
        dotNetHelper
    );
}
