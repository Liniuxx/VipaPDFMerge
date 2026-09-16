using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace VipaPDFMerge.Services;

public sealed class PdfMergeService
{
    public MemoryStream Merge(IEnumerable<(string Name, Stream Stream)> files)
    {
        var output = new MemoryStream();

        using (var outputDocument = new PdfDocument())
        {
            foreach (var file in files)
            {
                file.Stream.Position = 0;

                using var inputDocument =
                    PdfReader.Open(file.Stream, PdfDocumentOpenMode.Import);

                for (var i = 0; i < inputDocument.PageCount; i++)
                    outputDocument.AddPage(inputDocument.Pages[i]);
            }

            outputDocument.Save(output, false);
        }

        output.Position = 0;
        return output;
    }
}
