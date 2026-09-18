using Microsoft.AspNetCore.Mvc;
using VipaPDFMerge.Models;
using VipaPDFMerge.Services;

namespace VipaPDFMerge.Controllers;

[ApiController]
[Route("api")]
public sealed class PdfMergeController : ControllerBase
{
    private readonly GraphOneDriveService _oneDrive;
    private readonly PdfMergeService _pdfMerge;
    private readonly ILogger<PdfMergeController> _logger;
    private readonly IConfiguration _configuration;

    public PdfMergeController(
        GraphOneDriveService oneDrive,
        PdfMergeService pdfMerge,
        ILogger<PdfMergeController> logger,
        IConfiguration configuration)
    {
        _oneDrive = oneDrive;
        _pdfMerge = pdfMerge;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost("merge-pdf")]
    public async Task<IActionResult> MergePdf(
        [FromBody] MergePdfRequest input,
        CancellationToken cancellationToken)
    {
        var response = new MergePdfResponse();

        try
        {
            // Optional API key. If ApiKey is empty/not configured, this check is disabled.
            var configuredApiKey = _configuration["ApiKey"];
            if (!string.IsNullOrWhiteSpace(configuredApiKey))
            {
                var suppliedApiKey = Request.Headers["x-api-key"].FirstOrDefault();

                if (!string.Equals(configuredApiKey, suppliedApiKey, StringComparison.Ordinal))
                {
                    response.Result.ErrorMessage = "Unauthorized.";
                    return Unauthorized(response);
                }
            }

            if (string.IsNullOrWhiteSpace(input.OneDriveUrl) ||
                string.IsNullOrWhiteSpace(input.FolderPath))
            {
                response.Result.ErrorMessage = "OneDriveUrl and FolderPath are required.";
                return BadRequest(response);
            }

            if (!Uri.TryCreate(input.OneDriveUrl, UriKind.Absolute, out var oneDriveUri) ||
                !Uri.TryCreate(input.FolderPath, UriKind.Absolute, out var folderUri))
            {
                response.Result.ErrorMessage = "OneDriveUrl and FolderPath must be valid absolute URLs.";
                return BadRequest(response);
            }

            if (!string.Equals(oneDriveUri.Scheme, "https", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(folderUri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
            {
                response.Result.ErrorMessage = "Only HTTPS URLs are allowed.";
                return BadRequest(response);
            }

            if (!string.Equals(oneDriveUri.Host, folderUri.Host, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("OneDriveUrl and FolderPath must use the same host.");

            var sitePath = Uri.UnescapeDataString(oneDriveUri.AbsolutePath).TrimEnd('/');
            var folderAbsolutePath = Uri.UnescapeDataString(folderUri.AbsolutePath).TrimEnd('/');

            if (!folderAbsolutePath.StartsWith(sitePath + "/", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("FolderPath must be inside OneDriveUrl.");

            // FolderPath is a SharePoint web URL and normally contains the
            // document library segment "Documents". The Graph /drive endpoint
            // already points at that library, so root:/ paths must be relative
            // to the drive root and must NOT contain "Documents" again.
            var drivePath = folderAbsolutePath[(sitePath.Length + 1)..].Trim('/');

            if (drivePath.Equals("Documents", StringComparison.OrdinalIgnoreCase))
            {
                drivePath = string.Empty;
            }
            else if (drivePath.StartsWith("Documents/", StringComparison.OrdinalIgnoreCase))
            {
                drivePath = drivePath["Documents/".Length..];
            }

            var location = await _oneDrive.ResolveLocationAsync(
                oneDriveUri.Host,
                sitePath,
                drivePath,
                cancellationToken);

            var pdfFiles = await _oneDrive.GetPdfFilesAsync(
                location.DriveId,
                location.FolderItemId,
                cancellationToken);

            if (pdfFiles.Count == 0)
                throw new InvalidOperationException("No PDF files found in the specified folder.");

            // Stable deterministic merge order.
            pdfFiles = pdfFiles
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var pdfStreams = new List<(string Name, Stream Stream)>();

            try
            {
                foreach (var file in pdfFiles)
                {
                    var stream = await _oneDrive.DownloadFileAsync(
                        location.DriveId,
                        file.Id,
                        cancellationToken);

                    pdfStreams.Add((file.Name, stream));
                }

                await using var mergedStream = _pdfMerge.Merge(pdfStreams);

                var mergedFolderPath = $"{drivePath}/Merged";

                var mergedFolder = await _oneDrive.EnsureFolderPathAsync(
                    location.DriveId,
                    mergedFolderPath,
                    cancellationToken);

                var fileName =
                    $"merged_pdf_{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss-fff}.pdf";

                var uploaded = await _oneDrive.UploadFileAsync(
                    location.DriveId,
                    mergedFolder.Id,
                    fileName,
                    mergedStream,
                    cancellationToken);

                response.Result.IsSuccessful = true;
                response.Result.MergedPDFFilePath =
                    uploaded.WebUrl ??
                    BuildWebUrl(input.OneDriveUrl, mergedFolderPath, fileName);

                return Ok(response);
            }
            finally
            {
                foreach (var item in pdfStreams)
                    await item.Stream.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF merge failed.");

            response.Result.IsSuccessful = false;
            response.Result.ErrorMessage = ex.Message;
            response.Result.InnerErrorMessage = ex.InnerException?.Message ?? string.Empty;

            // Kept as HTTP 200 intentionally to match the requested existing API contract.
            return Ok(response);
        }
    }

    private static string BuildWebUrl(
        string oneDriveUrl,
        string folderPath,
        string fileName)
    {
        var baseUrl = oneDriveUrl.TrimEnd('/');

        var escaped = string.Join("/",
            (folderPath + "/" + fileName)
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.EscapeDataString));

        // oneDriveUrl points at the personal site root; folderPath is drive-relative.
        return $"{baseUrl}/Documents/{escaped}";
    }
}
