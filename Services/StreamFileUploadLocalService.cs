using System.Net.Http.Headers;
using Microsoft.AspNetCore.WebUtilities;

public class StreamFileUploadLocalService : IStreamFileUploadService
{
    public async Task<bool> UploadFile(MultipartReader reader,MultipartSection? section)
    {
        while (section != null)
        {
            var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(
                section.ContentDisposition, out var contentDisposition
            );

            if (hasContentDispositionHeader)
            {
                if (contentDisposition.DispositionType.Equals("form-data") &&
                (!string.IsNullOrEmpty(contentDisposition.FileName) ||
                !string.IsNullOrEmpty(contentDisposition.FileNameStar)))
                {
                    string filePath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "UploadedFiles"));
                    byte[] fileArray;
                    using (var memoryStream = new MemoryStream())
                    {
                        await section.Body.CopyToAsync(memoryStream);
                        fileArray = memoryStream.ToArray();
                    }
                    using (var fileStream = System.IO.File.Create(Path.Combine(filePath, contentDisposition.FileName)))
                    {
                        await fileStream.WriteAsync(fileArray);
                    }
                }
            }
            section = await reader.ReadNextSectionAsync();
        }
        return true;
    }
}