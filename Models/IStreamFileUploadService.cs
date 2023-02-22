using Microsoft.AspNetCore.WebUtilities;

public interface IStreamFileUploadService
{
    Task<bool> UploadFile(MultipartReader reader, MultipartSection section);
}