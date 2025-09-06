using CDN.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace CDN.Controllers
{
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IFtpFileService _ftpFileService;

        public FileController(IFtpFileService ftpFileService)
        {
            _ftpFileService = ftpFileService;
        }

        // Serve files from CDN_Files
        [EnableCors]
        [HttpGet("cdn/{*filePath}")]
        public async Task<IActionResult> GetCdnFile(string filePath)
        {
            // Create the full path to the file
            var fullPath = $"/CDN_Files/{filePath}";

            // Get the folder path by removing the last segment from the filePath
            var folderPath = "/CDN_Files" + (filePath.Contains("/") ? $"/{string.Join("/", filePath.Split('/').SkipLast(1))}" : "");

            var contents = await _ftpFileService.GetFolderContentsAsync(folderPath);

            var file = contents.FirstOrDefault(x => x.FullName == fullPath);
            if (file == null)
            {
                return NotFound();
            }

            // Download or serve the file here
            var stream = await _ftpFileService.DownloadFileAsync(file.FullName);
            return File(stream, "application/octet-stream", file.Name); // Adjust the content type as needed
        }

        // Serve files from Data_files
        [HttpGet("files/{*filePath}")]
        public async Task<IActionResult> GetDataFile(string filePath)
        {
            // Create the full path to the file
            var fullPath = $"/Data_Files/{filePath}";

            // Get the folder path by removing the last segment from the filePath
            var folderPath = "/Data_Files" + (filePath.Contains("/") ? $"/{string.Join("/", filePath.Split('/').SkipLast(1))}" : "");

            var contents = await _ftpFileService.GetFolderContentsAsync(folderPath);

            var file = contents.FirstOrDefault(x => x.FullName == fullPath);
            if (file == null)
            {
                return NotFound();
            }

            // Download or serve the file here
            var stream = await _ftpFileService.DownloadFileAsync(file.FullName);

            // Use a provider to determine the content type based on the file extension.
            // This is a more robust approach than manual if/else checks.
            var provider = new FileExtensionContentTypeProvider();
            string contentType;
            if (!provider.TryGetContentType(filePath, out contentType))
            {
                // Fallback to a generic binary stream if the content type is unknown.
                contentType = "application/octet-stream";
            }
            else if (contentType == "application/pdf")
            {
                return File(stream, contentType);
            }

            // Return the file with the correct content type
            return File(stream, contentType, file.Name);
        }

    }
}
