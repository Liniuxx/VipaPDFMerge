namespace VipaPDFMerge.Models;

public sealed class MergePdfRequest
{
    public string OneDriveUrl { get; set; } = string.Empty;
    public string FolderPath { get; set; } = string.Empty;
}
