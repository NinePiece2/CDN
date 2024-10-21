using CDN.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CDN.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        public readonly IFtpFileService _ftpFileService;
        public AdminController(IFtpFileService ftpFileService)
        {
            _ftpFileService = ftpFileService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> BrowseFolder(string folderPath = "/")
        {
            var items = await _ftpFileService.GetFolderContentsAsync(folderPath);
            return Json(items); // Returning JSON to be consumed by the frontend
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var fileName = Path.GetFileName(file.FileName);
            var folderPath = folder ?? "uploads"; // Default folder

            using (var stream = file.OpenReadStream())
            {
                await _ftpFileService.UploadFileAsync(stream, fileName, folderPath);
            }

            return Ok(new { message = "File uploaded successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> CreateFolder(string folderName, string parentFolder = "/")
        {
            if (string.IsNullOrEmpty(folderName))
                return BadRequest("Folder name cannot be empty");

            var folderPath = Path.Combine(parentFolder, folderName);
            await _ftpFileService.CreateFolderAsync(folderPath);

            return Ok(new { message = "Folder created successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> UploadFolder(List<IFormFile> files, string folder)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded");

            var folderPath = folder ?? "uploads"; // Default folder

            var fileStreams = new List<(Stream fileStream, string fileName)>();

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    fileStreams.Add((file.OpenReadStream(), fileName));
                }
            }

            await _ftpFileService.UploadDirectoryFromStreamsAsync(fileStreams, folderPath);

            return Ok(new { message = "Folder uploaded successfully" });
        }
    }
}
