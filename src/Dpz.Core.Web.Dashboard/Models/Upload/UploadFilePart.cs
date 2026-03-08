using System.IO;

namespace Dpz.Core.Web.Dashboard.Models.Upload;

public sealed class UploadFilePart
{
    public UploadFilePart(string name, string fileName, string contentType, Stream content)
    {
        Name = name;
        FileName = fileName;
        ContentType = contentType;
        Content = content;
    }

    public string Name { get; }

    public string FileName { get; }

    public string ContentType { get; }

    public Stream Content { get; }
}
