using System.Text.Json.Serialization;

namespace VipaPDFMerge.Models;

public sealed class MergePdfResponse
{
    [JsonPropertyName("result")]
    public MergePdfResult Result { get; set; } = new();
}

public sealed class MergePdfResult
{
    [JsonPropertyName("Message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("ErrorMessage")]
    public string ErrorMessage { get; set; } = string.Empty;

    [JsonPropertyName("InnerErrorMessage")]
    public string InnerErrorMessage { get; set; } = string.Empty;

    [JsonPropertyName("IsSuccessful")]
    public bool IsSuccessful { get; set; }

    [JsonPropertyName("MergedPDFFilePath")]
    public string MergedPDFFilePath { get; set; } = string.Empty;
}
