using FluentFTP;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using CDN.Data;
using System.Runtime.Intrinsics.X86;
using System.Security.Authentication;
using FluentFTP.Exceptions;


namespace CDN.Services
{
    public interface IFtpFileService
    {
        Task<List<FtpListItem>> GetFolderContentsAsync(string folderPath);
        Task UploadFileAsync(Stream fileStream, string fileName, string folderPath);
        Task CreateFolderAsync(string folderPath);
        Task UploadDirectoryFromStreamsAsync(List<(Stream fileStream, string fileName)> files, string remoteDirectoryPath);
        Task<Stream> DownloadFileAsync(string remoteFilePath);
    }

    public class FtpFileService : IFtpFileService, IHostedService
    {
        private string _ftpHost = "ftp://localhost";
        private string _ftpUser = "ftpuser";
        private string _ftpPass = "ftppassword";
        private readonly CDNContext _context;
        private AsyncFtpClient client;
        public FtpFileService(CDNContext cDNContext)
        {
            _context = cDNContext;

            _ftpHost = _context.PowerSettings.Find("FTPHost")?.Value ?? _ftpHost;
            _ftpUser = _context.PowerSettings.Find("FTPUser")?.Value ?? _ftpUser;
            _ftpPass = _context.PowerSettings.Find("FTPPass")?.Value ?? _ftpPass;

            client = new AsyncFtpClient(_ftpHost, _ftpUser, _ftpPass);
            // Enable FTPS(FTP over SSL / TLS)
            client.Config.EncryptionMode = FtpEncryptionMode.Explicit; // Enable TLS
            client.Config.ValidateAnyCertificate = true; 
            client.Config.DataConnectionType = FtpDataConnectionType.PASV;

            client.Config.DataConnectionEncryption = true;
            client.Config.SslProtocols = SslProtocols.Tls12;
            client.Config.LogToConsole = true;

        }

        public async Task<List<FtpListItem>> GetFolderContentsAsync(string folderPath)
        {
            try
            {
                await client.AutoConnect();
                var items = await client.GetListing(folderPath);
                return new List<FtpListItem>(items);
            }
            catch (FtpCommandException ex)
            {
                // Log detailed error message
                Console.WriteLine($"FTP command failed: {ex.Message}");
                throw; // Re-throw or handle as needed
            }
            finally
            {
                await client.Disconnect();
            }

        }

        public async Task UploadFileAsync(Stream fileStream, string fileName, string folderPath)
        {
            await client.AutoConnect();

            // Create folder if it doesn't exist
            if (!await client.DirectoryExists(folderPath))
            {
                await client.CreateDirectory(folderPath);
            }

            // Save stream to a temporary file
            var tempFilePath = Path.GetTempFileName();
            try
            {
                using (var file = File.Create(tempFilePath))
                {
                    fileStream.Seek(0, SeekOrigin.Begin); // Make sure we're at the beginning of the stream
                    await fileStream.CopyToAsync(file);
                }

                // Upload the temp file
                await client.UploadFile(tempFilePath, Path.Combine(folderPath, fileName));
            }
            finally
            {
                // Clean up the temp file
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
                await client.Disconnect();
            } 
        }


        public async Task CreateFolderAsync(string folderPath)
        {
            
            await client.AutoConnect();

            if (!await client.DirectoryExists(folderPath))
            {
                await client.CreateDirectory(folderPath);
            }

            await client.Disconnect();

        }

        public async Task UploadDirectoryFromStreamsAsync(List<(Stream fileStream, string fileName)> files, string remoteDirectoryPath)
        {
            await client.AutoConnect();

            // Create remote folder if it doesn't exist
            if (!await client.DirectoryExists(remoteDirectoryPath))
            {
                await client.CreateDirectory(remoteDirectoryPath);
            }

            // Create a temporary local directory
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);

            try
            {
                // Save each stream to a temporary file in the temp directory
                foreach (var (fileStream, fileName) in files)
                {
                    var tempFilePath = Path.Combine(tempDirectory, fileName);

                    using (var file = File.Create(tempFilePath))
                    {
                        fileStream.Seek(0, SeekOrigin.Begin); // Make sure we're at the beginning of the stream
                        await fileStream.CopyToAsync(file);
                    }
                }

                // Upload the temp directory with synchronization
                await client.UploadDirectory(tempDirectory, remoteDirectoryPath, FtpFolderSyncMode.Update);
            }
            finally
            {
                // Clean up the temp directory
                if (Directory.Exists(tempDirectory))
                {
                    Directory.Delete(tempDirectory, true); // Delete directory and its contents
                }
                await client.Disconnect();
            }
            
        }

        public async Task<Stream> DownloadFileAsync(string remoteFilePath)
        {
            await client.AutoConnect();

            var memoryStream = new MemoryStream();
            await client.DownloadStream(memoryStream, remoteFilePath);
            memoryStream.Position = 0; // Reset the stream position to the beginning

            await client.Disconnect();
            return memoryStream;
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Your startup logic here
            Console.WriteLine("Service is starting.");
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // Your shutdown logic here
            await client.Disconnect();
            Console.WriteLine("Service is stopping.");
        }

    }
}
