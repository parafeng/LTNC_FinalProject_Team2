using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MiniPhotoshop.Backend.Services;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Controllers
{
    [Route("api/[controller]")]
    public class ImageController : Controller
    {
        private readonly FileStorageService _fileStorageService;
        private readonly ImageProcessingService _imageProcessingService;
        private readonly ILogger<ImageController> _logger;
        
        public ImageController(
            FileStorageService fileStorageService,
            ImageProcessingService imageProcessingService,
            ILogger<ImageController> logger)
        {
            _fileStorageService = fileStorageService;
            _imageProcessingService = imageProcessingService;
            _logger = logger;
        }
        
        /// <summary>
        /// Tải lên một ảnh mới
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {
                _logger.LogInformation("Đang tải lên file {0} có kích thước {1} byte", file?.FileName, file?.Length);
                
                // Lưu file
                var filePath = await _fileStorageService.SaveFileAsync(file);
                
                // Lấy kích thước ảnh
                using var image = await Image.LoadAsync(System.IO.Path.Combine(_fileStorageService.WebRootPath, filePath.TrimStart('/')));
                
                // Lưu thông tin ảnh vào TempData để hiển thị trên trang chủ
                TempData["CurrentImagePath"] = filePath;
                TempData["ImageWidth"] = image.Width;
                TempData["ImageHeight"] = image.Height;
                
                // Chuyển hướng về trang chủ
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải lên file: {0}", ex.Message);
                TempData["ErrorMessage"] = $"Lỗi khi tải lên file: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }
        
        /// <summary>
        /// Áp dụng bộ lọc lên ảnh
        /// </summary>
        [HttpPost("apply")]
        public async Task<IActionResult> ApplyFilter([FromForm] FilterRequest request)
        {
            try
            {
                _logger.LogInformation("Đang áp dụng bộ lọc {0} cho ảnh {1}", request.FilterName, request.ImagePath);
                
                // Chuyển đổi các tham số từ form sang Dictionary
                var parameters = new Dictionary<string, object>();
                foreach (var key in Request.Form.Keys)
                {
                    if (key != "imagePath" && key != "filterName")
                    {
                        var value = Request.Form[key].ToString();
                        parameters[key] = value;
                        _logger.LogInformation("Tham số: {0} = {1}", key, value);
                    }
                }
                
                // Áp dụng bộ lọc
                var newImagePath = await _imageProcessingService.ApplyFilterAsync(
                    request.ImagePath, 
                    request.FilterName, 
                    parameters);
                
                // Lưu thông tin ảnh mới vào TempData
                TempData["CurrentImagePath"] = newImagePath;
                
                // Chuyển hướng về trang chủ
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi áp dụng bộ lọc: {0}", ex.Message);
                TempData["ErrorMessage"] = $"Lỗi khi áp dụng bộ lọc: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }
        
        /// <summary>
        /// Xem trước bộ lọc mà không lưu ảnh
        /// </summary>
        [HttpPost("preview")]
        public async Task<IActionResult> PreviewFilter([FromForm] FilterRequest request)
        {
            try
            {
                _logger.LogInformation("Đang xem trước bộ lọc {0} cho ảnh {1}", request.FilterName, request.ImagePath);
                
                // Chuyển đổi các tham số từ form sang Dictionary
                var parameters = new Dictionary<string, object>();
                foreach (var key in Request.Form.Keys)
                {
                    if (key != "imagePath" && key != "filterName")
                    {
                        var value = Request.Form[key].ToString();
                        parameters[key] = value;
                        _logger.LogInformation("Tham số xem trước: {0} = {1}", key, value);
                    }
                }
                
                // Áp dụng bộ lọc và nhận về ảnh đã xử lý
                var processedImage = await _imageProcessingService.PreviewFilterAsync(
                    request.ImagePath, 
                    request.FilterName, 
                    parameters);
                
                // Chuyển đổi ảnh thành base64 để trả về cho client
                using var memoryStream = new MemoryStream();
                await processedImage.SaveAsJpegAsync(memoryStream);
                var base64String = Convert.ToBase64String(memoryStream.ToArray());
                
                return Ok(new { imageData = $"data:image/jpeg;base64,{base64String}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xem trước bộ lọc: {0}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
        }
        
        /// <summary>
        /// Đặt lại ảnh về trạng thái gốc
        /// </summary>
        [HttpPost("reset")]
        public IActionResult ResetImage([FromForm] string imagePath)
        {
            try
            {
                _logger.LogInformation("Đang đặt lại ảnh {0} về trạng thái gốc", imagePath);
                
                // Đặt lại ảnh về trạng thái gốc
                var originalPath = _imageProcessingService.ResetImage(imagePath);
                
                // Lưu thông tin ảnh gốc vào TempData
                TempData["CurrentImagePath"] = originalPath;
                
                // Chuyển hướng về trang chủ
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đặt lại ảnh: {0}", ex.Message);
                TempData["ErrorMessage"] = $"Lỗi khi đặt lại ảnh: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }
        
        /// <summary>
        /// Lấy danh sách các bộ lọc đã áp dụng cho ảnh
        /// </summary>
        [HttpGet("applied-filters")]
        public IActionResult GetAppliedFilters(string imagePath)
        {
            try
            {
                var appliedFilters = _imageProcessingService.GetAppliedFilters(imagePath);
                return Ok(appliedFilters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách bộ lọc đã áp dụng: {0}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
        }
        
        /// <summary>
        /// Lấy danh sách các bộ lọc có sẵn
        /// </summary>
        [HttpGet("filters")]
        public IActionResult GetFilters()
        {
            try
            {
                var filters = _imageProcessingService.GetAvailableFilters();
                return Ok(filters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách bộ lọc: {0}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
        }
        
        /// <summary>
        /// Apply AI-based image editing based on a text command
        /// </summary>
        [HttpPost("ai-edit")]
        public async Task<IActionResult> AIEdit([FromForm] string imagePath, [FromForm] string command)
        {
            try
            {
                _logger.LogInformation("Applying AI edit with command: {0} for image {1}", command, imagePath);
                
                if (string.IsNullOrEmpty(imagePath))
                {
                    _logger.LogError("Image path is empty");
                    return BadRequest(new { error = "Không tìm thấy ảnh để chỉnh sửa. Vui lòng tải lên ảnh trước." });
                }

                // Apply AI edit
                var newImagePath = await _imageProcessingService.ApplyAIEditAsync(imagePath, command);
                
                // Save the new image path to TempData
                TempData["CurrentImagePath"] = newImagePath;
                
                // Return success with the new image path
                return Ok(new { success = true, imagePath = newImagePath });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying AI edit: {0}", ex.Message);
                return BadRequest(new { error = $"Lỗi khi áp dụng chỉnh sửa AI: {ex.Message}" });
            }
        }
    }
    
    public class FilterRequest
    {
        public string ImagePath { get; set; }
        public string FilterName { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }
} 