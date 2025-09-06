using CDN.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

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

            // Get the file extension
            var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
            string contentType;
        
            // Determine the content type based on the file extension
            if (fileExtension == ".pdf")
            {
                contentType = "application/pdf";
            }
            else
            {
                // Default to a generic binary stream for other file types
                contentType = "application/octet-stream";
            }
        
            return File(stream, contentType, file.Name);
        }

    }
}
