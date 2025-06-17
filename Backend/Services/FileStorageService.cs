using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Services
{
    public class FileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileStorageService> _logger;
        
        public string WebRootPath => _environment.WebRootPath;
        public string UploadDirectory => "uploads";
        
        public FileStorageService(IWebHostEnvironment environment, ILogger<FileStorageService> logger)
        {
            _environment = environment;
            _logger = logger;
            
            // Đảm bảo thư mục uploads tồn tại
            var uploadsPath = Path.Combine(_environment.WebRootPath, UploadDirectory);
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }
        }
        
        /// <summary>
        /// Lưu file được tải lên
        /// </summary>
        /// <param name="file">File từ form</param>
        /// <returns>Đường dẫn tương đối của file</returns>
        public async Task<string> SaveFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File không hợp lệ");
            }
            
            // Tạo tên file ngẫu nhiên để tránh trùng lặp
            var fileName = $"{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";
            
            // Đường dẫn lưu file
            var filePath = Path.Combine(UploadDirectory, fileName);
            var fullPath = Path.Combine(_environment.WebRootPath, filePath);
            
            // Đảm bảo thư mục tồn tại
            var directory = Path.GetDirectoryName(fullPath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Lưu file
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            
            // Kiểm tra loại tệp
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !IsValidImageExtension(extension))
            {
                throw new ArgumentException($"Loại tệp {extension} không được hỗ trợ. Chỉ chấp nhận .jpg, .jpeg, .png, .gif");
            }
            
            // Kiểm tra tệp có phải hình ảnh hợp lệ không
            using (var image = await Image.LoadAsync(fullPath))
            {
                // Nếu là hình ảnh hợp lệ, không cần làm gì thêm
            }
            
            // Trả về đường dẫn tương đối
            return $"/{filePath.Replace('\\', '/')}";
        }
        
        /// <summary>
        /// Lưu đối tượng Image
        /// </summary>
        /// <param name="image">Đối tượng Image</param>
        /// <param name="fileName">Tên file (nếu null sẽ tạo tên ngẫu nhiên)</param>
        /// <returns>Đường dẫn tương đối của file</returns>
        public async Task<string> SaveImageAsync(Image image, string fileName = null)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image), "Ảnh không được phép là null");
            }
            
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = $"{Guid.NewGuid()}.jpg";
            }
            
            // Đường dẫn lưu file
            var filePath = Path.Combine(UploadDirectory, fileName);
            var fullPath = Path.Combine(_environment.WebRootPath, filePath);
            
            // Đảm bảo thư mục tồn tại
            var directory = Path.GetDirectoryName(fullPath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Lưu ảnh
            await image.SaveAsJpegAsync(fullPath, new JpegEncoder { Quality = 90 });
            
            // Kiểm tra tệp có phải hình ảnh hợp lệ không
            using (var loadedImage = await Image.LoadAsync(fullPath))
            {
                // Nếu là hình ảnh hợp lệ, không cần làm gì thêm
            }
            
            // Trả về đường dẫn tương đối
            return $"/{filePath.Replace('\\', '/')}";
        }
        
        /// <summary>
        /// Xóa file
        /// </summary>
        /// <param name="relativePath">Đường dẫn tương đối của file</param>
        public void DeleteFile(string relativePath)
        {
            try
            {
                var fullPath = Path.Combine(WebRootPath, relativePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi xóa file: {ex.Message}", ex);
            }
        }
        
        // Kiểm tra loại tệp
        private bool IsValidImageExtension(string extension)
        {
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            foreach (var validExt in validExtensions)
            {
                if (extension.Equals(validExt, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
} 