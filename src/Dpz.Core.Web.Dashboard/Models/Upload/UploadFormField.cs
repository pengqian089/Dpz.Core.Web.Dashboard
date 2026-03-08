namespace Dpz.Core.Web.Dashboard.Models.Upload;

public sealed class UploadFormField
{
    public UploadFormField(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }

    public string Value { get; }
}
