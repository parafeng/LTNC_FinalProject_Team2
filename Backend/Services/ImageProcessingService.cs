using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using MiniPhotoshop.Backend.Filters;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Services
{
    public class ImageProcessingService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ImageProcessingService> _logger;
        private readonly FileStorageService _fileStorageService;
        private readonly Dictionary<string, IImageFilter> _filters;
        
        // Dictionary để lưu trữ các bộ lọc đã áp dụng cho mỗi ảnh
        private readonly Dictionary<string, List<AppliedFilter>> _appliedFilters = new Dictionary<string, List<AppliedFilter>>();
        
        // Dictionary để lưu trữ ảnh gốc cho mỗi ảnh đã xử lý
        private readonly Dictionary<string, string> _originalImages = new Dictionary<string, string>();
        
        // Đường dẫn đến file JSON lưu trữ thông tin bộ lọc
        private readonly string _filtersJsonPath;
        
        /// <summary>
        /// Lớp lưu trữ dữ liệu bộ lọc để serialize/deserialize
        /// </summary>
        public class FiltersData
        {
            public Dictionary<string, List<AppliedFilter>> AppliedFilters { get; set; } = new Dictionary<string, List<AppliedFilter>>();
            public Dictionary<string, string> OriginalImages { get; set; } = new Dictionary<string, string>();
        }
        
        public ImageProcessingService(
            IWebHostEnvironment environment,
            ILogger<ImageProcessingService> logger,
            FileStorageService fileStorageService,
            IEnumerable<IImageFilter> filters)
        {
            _environment = environment;
            _logger = logger;
            _fileStorageService = fileStorageService;
            _filters = filters.ToDictionary(f => f.Name);
            
            _filtersJsonPath = Path.Combine(environment.ContentRootPath, "filters_data.json");
            
            // Tải dữ liệu từ file JSON nếu có
            LoadFiltersData();
            
            _logger.LogInformation("Đã đăng ký {0} bộ lọc", _filters.Count);
        }
        
        /// <summary>
        /// Tải dữ liệu bộ lọc từ file JSON
        /// </summary>
        private void LoadFiltersData()
        {
            try
            {
                if (File.Exists(_filtersJsonPath))
                {
                    var json = File.ReadAllText(_filtersJsonPath);
                    var data = System.Text.Json.JsonSerializer.Deserialize<FiltersData>(json);
                    
                    if (data != null)
                    {
                        // Chuyển đổi từ FiltersData sang Dictionary
                        foreach (var entry in data.AppliedFilters)
                        {
                            _appliedFilters[entry.Key] = entry.Value;
                        }
                        
                        foreach (var entry in data.OriginalImages)
                        {
                            _originalImages[entry.Key] = entry.Value;
                        }
                        
                        _logger.LogInformation($"Đã tải {_appliedFilters.Count} bộ lọc và {_originalImages.Count} ảnh gốc từ file JSON");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải dữ liệu bộ lọc từ file JSON");
            }
        }
        
        /// <summary>
        /// Lưu dữ liệu bộ lọc vào file JSON
        /// </summary>
        private void SaveFiltersData()
        {
            try
            {
                var data = new FiltersData
                {
                    AppliedFilters = _appliedFilters,
                    OriginalImages = _originalImages
                };
                
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                };
                
                var json = System.Text.Json.JsonSerializer.Serialize(data, options);
                File.WriteAllText(_filtersJsonPath, json);
                
                _logger.LogInformation($"Đã lưu {_appliedFilters.Count} bộ lọc và {_originalImages.Count} ảnh gốc vào file JSON");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu dữ liệu bộ lọc vào file JSON");
            }
        }
        
        /// <summary>
        /// Áp dụng bộ lọc lên ảnh
        /// </summary>
        /// <param name="imagePath">Đường dẫn tương đối của ảnh gốc</param>
        /// <param name="filterName">Tên bộ lọc</param>
        /// <param name="parameters">Tham số cho bộ lọc</param>
        /// <returns>Đường dẫn tương đối của ảnh đã xử lý</returns>
        public async Task<string> ApplyFilterAsync(string imagePath, string filterName, Dictionary<string, object> parameters)
        {
            _logger.LogInformation("Áp dụng bộ lọc {0} cho ảnh {1}", filterName, imagePath);

            if (!_filters.TryGetValue(filterName, out var filter))
            {
                throw new ArgumentException($"Bộ lọc '{filterName}' không tồn tại");
            }
            
            // Chuyển đổi tham số từ chuỗi sang kiểu dữ liệu phù hợp
            var convertedParameters = ConvertParameters(parameters);

            // Đường dẫn đầy đủ đến file ảnh
            var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));

            // Tạo tên file mới - sử dụng cùng tên file nếu đã có _filtered
            string newFileName;
            if (Path.GetFileNameWithoutExtension(fullPath).Contains("_filtered"))
            {
                newFileName = Path.GetFileName(fullPath);
            }
            else
            {
                newFileName = $"{Path.GetFileNameWithoutExtension(fullPath)}_filtered{Path.GetExtension(fullPath)}";
            }
            
            var newFilePath = Path.Combine(_fileStorageService.UploadDirectory, newFileName);
            var newFullPath = Path.Combine(_environment.WebRootPath, newFilePath);

            // Tạo thư mục nếu chưa tồn tại
            var directory = Path.GetDirectoryName(newFullPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Đường dẫn tương đối của ảnh mới
            var relativePath = $"/{newFilePath.Replace('\\', '/')}";
            
            // Xác định ảnh gốc
            string originalImagePath = imagePath;
            
            // Nếu ảnh hiện tại đã là ảnh đã xử lý, sử dụng ảnh gốc của nó
            if (_originalImages.ContainsKey(imagePath))
            {
                originalImagePath = _originalImages[imagePath];
                _logger.LogInformation($"Sử dụng ảnh gốc: {originalImagePath} cho ảnh đã xử lý: {imagePath}");
            }
            
            // Lưu thông tin về ảnh gốc cho ảnh mới
            _originalImages[relativePath] = originalImagePath;
            _logger.LogInformation($"Đã lưu thông tin ảnh gốc: {originalImagePath} cho ảnh mới: {relativePath}");

            // Nếu đường dẫn mới trùng với đường dẫn cũ, giữ lại bộ lọc cũ
            if (relativePath == imagePath)
            {
                _logger.LogInformation($"Đường dẫn mới trùng với đường dẫn cũ, giữ lại bộ lọc cũ cho {imagePath}");
            }
            
            // Tạo danh sách bộ lọc mới cho ảnh mới nếu chưa có
            if (!_appliedFilters.ContainsKey(relativePath))
            {
                _appliedFilters[relativePath] = new List<AppliedFilter>();
                _logger.LogInformation($"Tạo danh sách bộ lọc mới cho ảnh {relativePath}");
            }
            
            // Sao chép các bộ lọc từ ảnh trước nếu có và đường dẫn khác nhau
            if (imagePath != relativePath && _appliedFilters.ContainsKey(imagePath))
            {
                // Không xóa bộ lọc cũ, chỉ thêm vào nếu chưa có
                foreach (var existingFilter in _appliedFilters[imagePath])
                {
                    if (!_appliedFilters[relativePath].Any(f => f.FilterName == existingFilter.FilterName))
                    {
                        _appliedFilters[relativePath].Add(existingFilter);
                    }
                }
                _logger.LogInformation($"Đã sao chép {_appliedFilters[imagePath].Count} bộ lọc từ ảnh {imagePath}");
            }

            // Thêm bộ lọc mới
            _appliedFilters[relativePath].Add(new AppliedFilter
            {
                FilterName = filterName,
                Parameters = convertedParameters
            });
            _logger.LogInformation($"Đã thêm bộ lọc {filterName} vào danh sách bộ lọc của ảnh {relativePath}");

            // Áp dụng tất cả các bộ lọc từ ảnh gốc
            using (var image = await Image.LoadAsync(Path.Combine(_environment.WebRootPath, originalImagePath.TrimStart('/'))))
            {
                // Áp dụng tất cả các bộ lọc theo thứ tự
                foreach (var appliedFilter in _appliedFilters[relativePath])
                {
                    if (_filters.TryGetValue(appliedFilter.FilterName, out var appliedFilterObj))
                    {
                        _logger.LogInformation($"Áp dụng bộ lọc {appliedFilter.FilterName} lên ảnh");
                        await appliedFilterObj.ApplyAsync(image, appliedFilter.Parameters);
                    }
                }
                
                await image.SaveAsync(newFullPath);
                _logger.LogInformation($"Đã lưu ảnh đã xử lý tại: {newFullPath}");
            }

            // Ghi log toàn bộ danh sách bộ lọc đã áp dụng để debug
            _logger.LogInformation($"Tổng số ảnh có bộ lọc: {_appliedFilters.Count}");
            foreach (var entry in _appliedFilters)
            {
                _logger.LogInformation($"Ảnh {entry.Key} có {entry.Value.Count} bộ lọc:");
                foreach (var appliedFilter in entry.Value)
                {
                    _logger.LogInformation($"  - {appliedFilter.FilterName}");
                }
            }

            // Lưu dữ liệu bộ lọc vào file JSON
            SaveFiltersData();

            return relativePath;
        }
        
        /// <summary>
        /// Xem trước bộ lọc mà không lưu ảnh
        /// </summary>
        /// <param name="imagePath">Đường dẫn tương đối của ảnh gốc</param>
        /// <param name="filterName">Tên bộ lọc</param>
        /// <param name="parameters">Tham số cho bộ lọc</param>
        /// <returns>Ảnh đã xử lý</returns>
        public async Task<Image> PreviewFilterAsync(string imagePath, string filterName, Dictionary<string, object> parameters)
        {
            _logger.LogInformation("Xem trước bộ lọc {0} cho ảnh {1}", filterName, imagePath);

            if (!_filters.TryGetValue(filterName, out var filter))
            {
                throw new ArgumentException($"Bộ lọc '{filterName}' không tồn tại");
            }
            
            // Chuyển đổi tham số từ chuỗi sang kiểu dữ liệu phù hợp
            var convertedParameters = ConvertParameters(parameters);

            // Xác định ảnh gốc
            string originalImagePath = imagePath;
            if (_originalImages.ContainsKey(imagePath))
            {
                originalImagePath = _originalImages[imagePath];
            }

            // Đường dẫn đầy đủ đến file ảnh gốc
            var fullPath = Path.Combine(_environment.WebRootPath, originalImagePath.TrimStart('/'));

            // Tạo danh sách các bộ lọc cần áp dụng
            var filtersToApply = new List<AppliedFilter>();
            
            // Thêm các bộ lọc đã áp dụng trước đó
            if (_appliedFilters.ContainsKey(imagePath))
            {
                filtersToApply.AddRange(_appliedFilters[imagePath]);
            }
            
            // Thêm bộ lọc mới đang xem trước
            filtersToApply.Add(new AppliedFilter
            {
                FilterName = filterName,
                Parameters = convertedParameters
            });

            // Áp dụng tất cả các bộ lọc
            var image = await Image.LoadAsync(fullPath);
            foreach (var appliedFilter in filtersToApply)
            {
                if (_filters.TryGetValue(appliedFilter.FilterName, out var appliedFilterObj))
                {
                    await appliedFilterObj.ApplyAsync(image, appliedFilter.Parameters);
                }
            }
            
            return image;
        }
        
        /// <summary>
        /// Chuyển đổi tham số từ chuỗi sang kiểu dữ liệu phù hợp
        /// </summary>
        private Dictionary<string, object> ConvertParameters(Dictionary<string, object> parameters)
        {
            var result = new Dictionary<string, object>();
            
            foreach (var param in parameters)
            {
                if (param.Value is string stringValue)
                {
                    // Thử chuyển đổi sang số thực
                    if (float.TryParse(stringValue, out float floatValue))
                    {
                        result[param.Key] = floatValue;
                    }
                    // Thử chuyển đổi sang số nguyên
                    else if (int.TryParse(stringValue, out int intValue))
                    {
                        result[param.Key] = intValue;
                    }
                    // Giữ nguyên giá trị chuỗi
                    else
                    {
                        result[param.Key] = stringValue;
                    }
                }
                else
                {
                    // Giữ nguyên giá trị không phải chuỗi
                    result[param.Key] = param.Value;
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Đặt lại ảnh về trạng thái gốc
        /// </summary>
        /// <param name="imagePath">Đường dẫn tương đối của ảnh</param>
        /// <returns>Đường dẫn tương đối của ảnh gốc</returns>
        public string ResetImage(string imagePath)
        {
            _logger.LogInformation($"Đặt lại ảnh {imagePath} về trạng thái gốc");
            
            // Chuẩn hóa đường dẫn
            var normalizedPath = imagePath.Replace('\\', '/').TrimStart('/');
            
            // Lấy tên file để so sánh
            var fileName = Path.GetFileName(imagePath);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(imagePath);
            var fileId = fileNameWithoutExt.Split('_').FirstOrDefault();
            
            // Ghi log thông tin debug
            _logger.LogInformation($"Thông tin ảnh: Path={imagePath}, FileName={fileName}, ID={fileId}");
            
            // Trường hợp 1: Ảnh hiện tại là ảnh đã xử lý, có ảnh gốc
            if (_originalImages.ContainsKey(imagePath))
            {
                var originalPath = _originalImages[imagePath];
                _logger.LogInformation($"Tìm thấy ảnh gốc: {originalPath} cho ảnh: {imagePath}");
                
                // Xóa bộ lọc của ảnh hiện tại
                if (_appliedFilters.ContainsKey(imagePath))
                {
                    _appliedFilters.Remove(imagePath);
                    _logger.LogInformation($"Đã xóa bộ lọc cho ảnh: {imagePath}");
                }
                
                // Xóa khỏi danh sách ảnh gốc
                _originalImages.Remove(imagePath);
                _logger.LogInformation($"Đã xóa khỏi danh sách ảnh gốc: {imagePath}");
                
                _logger.LogInformation($"Đã đặt lại ảnh về trạng thái gốc: {originalPath}");
                
                // Lưu dữ liệu bộ lọc vào file JSON
                SaveFiltersData();
                
                return originalPath;
            }
            
            // Trường hợp 2: Kiểm tra sau khi chuẩn hóa đường dẫn
            foreach (var key in _originalImages.Keys.ToList())
            {
                var normalizedKey = key.Replace('\\', '/').TrimStart('/');
                if (normalizedKey == normalizedPath)
                {
                    var originalPath = _originalImages[key];
                    _logger.LogInformation($"Tìm thấy ảnh gốc: {originalPath} cho ảnh: {key} (sau khi chuẩn hóa đường dẫn)");
                    
                    if (_appliedFilters.ContainsKey(key))
                    {
                        _appliedFilters.Remove(key);
                        _logger.LogInformation($"Đã xóa bộ lọc cho ảnh: {key}");
                    }
                    
                    _originalImages.Remove(key);
                    _logger.LogInformation($"Đã xóa khỏi danh sách ảnh gốc: {key}");
                    
                    // Lưu dữ liệu bộ lọc vào file JSON
                    SaveFiltersData();
                    
                    return originalPath;
                }
            }
            
            // Trường hợp 3: Kiểm tra dựa trên tên file
            var matchingKeys = _originalImages.Keys.Where(k => Path.GetFileName(k) == fileName).ToList();
            if (matchingKeys.Any())
            {
                foreach (var key in matchingKeys)
                {
                    var originalPath = _originalImages[key];
                    _logger.LogInformation($"Tìm thấy ảnh gốc: {originalPath} cho ảnh: {key} (dựa trên tên file)");
                    
                    if (_appliedFilters.ContainsKey(key))
                    {
                        _appliedFilters.Remove(key);
                        _logger.LogInformation($"Đã xóa bộ lọc cho ảnh: {key}");
                    }
                    
                    _originalImages.Remove(key);
                    _logger.LogInformation($"Đã xóa khỏi danh sách ảnh gốc: {key}");
                }
                
                // Tìm ảnh gốc dựa trên ID trong tên file
                if (!string.IsNullOrEmpty(fileId))
                {
                    var originalFileName = fileId + Path.GetExtension(imagePath);
                    var possibleOriginalPath = $"/uploads/{originalFileName}";
                    
                    if (System.IO.File.Exists(Path.Combine(_environment.WebRootPath, possibleOriginalPath.TrimStart('/'))))
                    {
                        _logger.LogInformation($"Đã tìm thấy ảnh gốc dựa trên ID: {possibleOriginalPath}");
                        
                        // Lưu dữ liệu bộ lọc vào file JSON
                        SaveFiltersData();
                        
                        return possibleOriginalPath;
                    }
                    
                    // Tìm file bắt đầu bằng ID trong thư mục uploads
                    var uploadDir = Path.Combine(_environment.WebRootPath, "uploads");
                    var matchingFiles = Directory.GetFiles(uploadDir, $"{fileId}*")
                        .Where(f => !Path.GetFileName(f).Contains("_filtered"))
                        .ToList();
                        
                    if (matchingFiles.Any())
                    {
                        var shortestMatch = matchingFiles
                            .OrderBy(f => Path.GetFileName(f).Length)
                            .First();
                            
                        var relativePath = $"/uploads/{Path.GetFileName(shortestMatch)}";
                        _logger.LogInformation($"Đã tìm thấy ảnh gốc có cùng ID: {relativePath}");
                        
                        // Lưu dữ liệu bộ lọc vào file JSON
                        SaveFiltersData();
                        
                        return relativePath;
                    }
                }
            }
            
            // Trường hợp 4: Kiểm tra xem ảnh có phải là ảnh gốc của ảnh nào khác không
            var relatedImages = _originalImages.Where(pair => pair.Value == imagePath).ToList();
            if (relatedImages.Any())
            {
                _logger.LogInformation($"Ảnh {imagePath} là ảnh gốc của {relatedImages.Count} ảnh khác");
                
                foreach (var pair in relatedImages)
                {
                    if (_appliedFilters.ContainsKey(pair.Key))
                    {
                        _appliedFilters.Remove(pair.Key);
                        _logger.LogInformation($"Đã xóa bộ lọc cho ảnh liên quan: {pair.Key}");
                    }
                    _originalImages.Remove(pair.Key);
                    _logger.LogInformation($"Đã xóa khỏi danh sách ảnh gốc: {pair.Key}");
                }
            }
            
            // Trường hợp 5: Kiểm tra dựa trên ID
            if (!string.IsNullOrEmpty(fileId))
            {
                var relatedByFileId = _originalImages
                    .Where(pair => Path.GetFileNameWithoutExtension(pair.Key).Split('_').FirstOrDefault() == fileId)
                    .ToList();
                    
                if (relatedByFileId.Any())
                {
                    _logger.LogInformation($"Ảnh {imagePath} liên quan đến {relatedByFileId.Count} ảnh khác dựa trên ID {fileId}");
                    
                    foreach (var pair in relatedByFileId)
                    {
                        if (_appliedFilters.ContainsKey(pair.Key))
                        {
                            _appliedFilters.Remove(pair.Key);
                            _logger.LogInformation($"Đã xóa bộ lọc cho ảnh liên quan: {pair.Key}");
                        }
                        _originalImages.Remove(pair.Key);
                        _logger.LogInformation($"Đã xóa khỏi danh sách ảnh gốc: {pair.Key}");
                    }
                    
                    // Tìm file gốc có cùng ID
                    var uploadDir = Path.Combine(_environment.WebRootPath, "uploads");
                    var matchingFiles = Directory.GetFiles(uploadDir, $"{fileId}*")
                        .Where(f => !Path.GetFileName(f).Contains("_filtered"))
                        .ToList();
                        
                    if (matchingFiles.Any())
                    {
                        var shortestMatch = matchingFiles
                            .OrderBy(f => Path.GetFileName(f).Length)
                            .First();
                            
                        var relativePath = $"/uploads/{Path.GetFileName(shortestMatch)}";
                        _logger.LogInformation($"Đã tìm thấy ảnh gốc có cùng ID: {relativePath}");
                        
                        // Lưu dữ liệu bộ lọc vào file JSON
                        SaveFiltersData();
                        
                        return relativePath;
                    }
                }
            }
            
            // Trường hợp 6: Nếu tên file có _filtered, thử tìm phiên bản không có _filtered
            if (fileNameWithoutExt.Contains("_filtered"))
            {
                var baseFileName = fileNameWithoutExt.Substring(0, fileNameWithoutExt.IndexOf("_filtered"));
                var originalFileName = baseFileName + Path.GetExtension(imagePath);
                var possibleOriginalPath = $"/uploads/{originalFileName}";
                
                if (System.IO.File.Exists(Path.Combine(_environment.WebRootPath, possibleOriginalPath.TrimStart('/'))))
                {
                    _logger.LogInformation($"Đã tìm thấy ảnh gốc bằng cách loại bỏ _filtered: {possibleOriginalPath}");
                    
                    // Xóa bộ lọc của ảnh hiện tại
                    if (_appliedFilters.ContainsKey(imagePath))
                    {
                        _appliedFilters.Remove(imagePath);
                        _logger.LogInformation($"Đã xóa bộ lọc cho ảnh: {imagePath}");
                    }
                    
                    // Lưu dữ liệu bộ lọc vào file JSON
                    SaveFiltersData();
                    
                    return possibleOriginalPath;
                }
            }
            
            // Trường hợp 7: Nếu ảnh không có trong danh sách ảnh gốc và không phải là ảnh gốc của ảnh nào khác
            // Xóa bộ lọc của ảnh hiện tại nếu có
            if (_appliedFilters.ContainsKey(imagePath))
            {
                _appliedFilters.Remove(imagePath);
                _logger.LogInformation($"Đã xóa bộ lọc cho ảnh: {imagePath}");
            }
            
            // Xóa bộ lọc dựa trên tên file
            var filtersToRemove = _appliedFilters.Keys
                .Where(k => Path.GetFileName(k) == fileName)
                .ToList();
                
            foreach (var key in filtersToRemove)
            {
                _appliedFilters.Remove(key);
                _logger.LogInformation($"Đã xóa bộ lọc cho ảnh: {key} (dựa trên tên file)");
            }
            
            // Ghi log danh sách bộ lọc còn lại
            _logger.LogInformation($"Sau khi reset, còn {_appliedFilters.Count} ảnh có bộ lọc:");
            foreach (var entry in _appliedFilters)
            {
                _logger.LogInformation($"  - {entry.Key}: {entry.Value.Count} bộ lọc");
            }
            
            // Lưu dữ liệu bộ lọc vào file JSON
            SaveFiltersData();
            
            _logger.LogInformation($"Trả về đường dẫn ảnh gốc: {imagePath}");
            return imagePath;
        }
        
        /// <summary>
        /// Lấy danh sách các bộ lọc đã áp dụng cho ảnh
        /// </summary>
        /// <param name="imagePath">Đường dẫn tương đối của ảnh</param>
        /// <returns>Danh sách các bộ lọc đã áp dụng</returns>
        public List<AppliedFilter> GetAppliedFilters(string imagePath)
        {
            _logger.LogInformation($"Lấy danh sách bộ lọc cho ảnh: {imagePath}");
            
            // Kiểm tra trực tiếp
            if (_appliedFilters.ContainsKey(imagePath))
            {
                _logger.LogInformation($"Tìm thấy {_appliedFilters[imagePath].Count} bộ lọc cho ảnh {imagePath}");
                return _appliedFilters[imagePath];
            }
            
            // Chuẩn hóa đường dẫn để so sánh
            var normalizedPath = imagePath.Replace('\\', '/').TrimStart('/');
            foreach (var key in _appliedFilters.Keys)
            {
                var normalizedKey = key.Replace('\\', '/').TrimStart('/');
                if (normalizedKey == normalizedPath)
                {
                    _logger.LogInformation($"Tìm thấy {_appliedFilters[key].Count} bộ lọc cho ảnh {key} (sau khi chuẩn hóa đường dẫn)");
                    return _appliedFilters[key];
                }
            }
            
            // Kiểm tra dựa trên tên file đầy đủ (không phân biệt đường dẫn)
            var fileName = Path.GetFileName(imagePath);
            foreach (var key in _appliedFilters.Keys)
            {
                if (Path.GetFileName(key) == fileName)
                {
                    _logger.LogInformation($"Tìm thấy {_appliedFilters[key].Count} bộ lọc cho ảnh {key} (dựa trên tên file {fileName})");
                    return _appliedFilters[key];
                }
            }
            
            // Kiểm tra xem ảnh có phải là ảnh gốc của ảnh nào khác không
            foreach (var entry in _originalImages)
            {
                if (entry.Value == imagePath && _appliedFilters.ContainsKey(entry.Key))
                {
                    _logger.LogInformation($"Ảnh {imagePath} là ảnh gốc của {entry.Key}, trả về {_appliedFilters[entry.Key].Count} bộ lọc");
                    return _appliedFilters[entry.Key];
                }
            }
            
            // Kiểm tra dựa trên ID của ảnh (phần đầu của tên file)
            var fileId = Path.GetFileNameWithoutExtension(imagePath).Split('_').FirstOrDefault();
            if (!string.IsNullOrEmpty(fileId))
            {
                // Tìm tất cả các ảnh có cùng ID và kết hợp tất cả các bộ lọc
                var combinedFilters = new List<AppliedFilter>();
                var foundMatches = false;
                
                foreach (var key in _appliedFilters.Keys)
                {
                    var keyId = Path.GetFileNameWithoutExtension(key).Split('_').FirstOrDefault();
                    if (fileId == keyId)
                    {
                        foundMatches = true;
                        
                        // Thêm các bộ lọc chưa có vào danh sách kết hợp
                        foreach (var filter in _appliedFilters[key])
                        {
                            if (!combinedFilters.Any(f => f.FilterName == filter.FilterName))
                            {
                                combinedFilters.Add(filter);
                            }
                        }
                        
                        _logger.LogInformation($"Tìm thấy {_appliedFilters[key].Count} bộ lọc cho ảnh {key} (dựa trên ID {fileId})");
                    }
                }
                
                if (foundMatches)
                {
                    _logger.LogInformation($"Kết hợp tất cả, tìm thấy {combinedFilters.Count} bộ lọc dựa trên ID {fileId}");
                    return combinedFilters;
                }
            }
            
            // Kiểm tra dựa trên tên file không có phần _filtered
            var baseFileName = Path.GetFileNameWithoutExtension(imagePath);
            if (baseFileName.Contains("_filtered"))
            {
                baseFileName = baseFileName.Substring(0, baseFileName.IndexOf("_filtered"));
                var extension = Path.GetExtension(imagePath);
                
                // Tìm tất cả các ảnh có cùng tên cơ sở và kết hợp tất cả các bộ lọc
                var combinedFilters = new List<AppliedFilter>();
                var foundMatches = false;
                
                foreach (var key in _appliedFilters.Keys)
                {
                    var keyBaseName = Path.GetFileNameWithoutExtension(key);
                    if (keyBaseName.StartsWith(baseFileName) && Path.GetExtension(key) == extension)
                    {
                        foundMatches = true;
                        
                        // Thêm các bộ lọc chưa có vào danh sách kết hợp
                        foreach (var filter in _appliedFilters[key])
                        {
                            if (!combinedFilters.Any(f => f.FilterName == filter.FilterName))
                            {
                                combinedFilters.Add(filter);
                            }
                        }
                        
                        _logger.LogInformation($"Tìm thấy {_appliedFilters[key].Count} bộ lọc cho ảnh {key} (dựa trên tên cơ sở {baseFileName})");
                    }
                }
                
                if (foundMatches)
                {
                    _logger.LogInformation($"Kết hợp tất cả, tìm thấy {combinedFilters.Count} bộ lọc dựa trên tên cơ sở {baseFileName}");
                    return combinedFilters;
                }
            }
            
            // Ghi log toàn bộ danh sách bộ lọc để debug
            _logger.LogInformation($"Không tìm thấy bộ lọc nào cho ảnh {imagePath}. Danh sách bộ lọc hiện có:");
            foreach (var key in _appliedFilters.Keys)
            {
                _logger.LogInformation($"  - {key}: {_appliedFilters[key].Count} bộ lọc");
            }
            
            // Nếu không tìm thấy, trả về danh sách rỗng
            return new List<AppliedFilter>();
        }
        
        /// <summary>
        /// Lấy danh sách các bộ lọc có sẵn
        /// </summary>
        public IEnumerable<FilterInfo> GetAvailableFilters()
        {
            return _filters.Values.Select(f => new FilterInfo
            {
                Name = f.Name,
                Description = f.Description
            });
        }

        public async Task<string> ApplyAIEditAsync(string imagePath, string command)
        {
            try
            {
                // Đảm bảo đường dẫn bắt đầu bằng /
                if (!imagePath.StartsWith("/"))
                {
                    imagePath = "/" + imagePath;
                }
                
                _logger.LogInformation($"Đường dẫn ảnh đã chuẩn hóa: {imagePath}");
                
                // Load the image
                var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));
                
                // Kiểm tra tệp tồn tại
                if (!File.Exists(fullPath))
                {
                    _logger.LogError($"Không tìm thấy tệp ảnh tại đường dẫn: {fullPath}");
                    
                    // Thử kiểm tra trong thư mục uploads với tên tệp đã được URL decode
                    var fileName = Path.GetFileName(imagePath);
                    var decodedFileName = System.Net.WebUtility.UrlDecode(fileName);
                    var alternativePath = Path.Combine(_environment.WebRootPath, "uploads", decodedFileName);
                    _logger.LogInformation($"Thử tìm tệp ảnh tại đường dẫn thay thế (decoded): {alternativePath}");
                    
                    if (File.Exists(alternativePath))
                    {
                        _logger.LogInformation($"Đã tìm thấy tệp ảnh tại đường dẫn thay thế: {alternativePath}");
                        fullPath = alternativePath;
                    }
                    else
                    {
                        // Thử tìm tệp với tên tương tự trong thư mục uploads
                        var fileId = fileName.Split('_').FirstOrDefault();
                        if (!string.IsNullOrEmpty(fileId))
                        {
                            var uploadDir = Path.Combine(_environment.WebRootPath, "uploads");
                            var matchingFiles = Directory.GetFiles(uploadDir, $"{fileId}*");
                            
                            if (matchingFiles.Length > 0)
                            {
                                fullPath = matchingFiles[0];
                                _logger.LogInformation($"Đã tìm thấy tệp ảnh tương tự: {fullPath}");
                            }
                            else
                            {
                                throw new FileNotFoundException($"Không tìm thấy tệp ảnh tại đường dẫn: {imagePath}", fullPath);
                            }
                        }
                        else
                        {
                            throw new FileNotFoundException($"Không tìm thấy tệp ảnh tại đường dẫn: {imagePath}", fullPath);
                        }
                    }
                }
                
                _logger.LogInformation($"Đang xử lý ảnh tại đường dẫn: {fullPath}");
                using var image = await Image.LoadAsync(fullPath);

                // Chuyển đổi ảnh sang base64
                using var memoryStream = new MemoryStream();
                await image.SaveAsJpegAsync(memoryStream);
                var base64Image = Convert.ToBase64String(memoryStream.ToArray());

                // Gọi API 4oimageapi.io để chỉnh sửa ảnh
                var editedImageBase64 = await CallGeminiImageEditAPI(base64Image, command);
                
                if (string.IsNullOrEmpty(editedImageBase64))
                {
                    throw new Exception("Không thể chỉnh sửa ảnh với API 4oimageapi.io");
                }

                // Chuyển đổi base64 trở lại thành ảnh
                var editedImageBytes = Convert.FromBase64String(editedImageBase64);
                var editedImage = await Image.LoadAsync(new MemoryStream(editedImageBytes));

                // Save the processed image
                var newPath = GetNewImagePath(imagePath);
                var newFullPath = Path.Combine(_environment.WebRootPath, newPath.TrimStart('/'));
                
                // Đảm bảo thư mục tồn tại
                var directory = Path.GetDirectoryName(newFullPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                _logger.LogInformation($"Lưu ảnh đã xử lý tại: {newFullPath}");
                await editedImage.SaveAsJpegAsync(newFullPath);

                // Add to applied filters
                AddAppliedFilter(imagePath, "AI Edit", new Dictionary<string, object>
                {
                    { "command", command }
                });

                return newPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying AI edit: {0}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Gọi API 4oimageapi.io để chỉnh sửa ảnh
        /// </summary>
        private async Task<string> CallGeminiImageEditAPI(string base64Image, string command)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromMinutes(3); // Tăng timeout lên 3 phút
                
                // Lấy API key từ cấu hình
                var apiKey = Environment.GetEnvironmentVariable("IMAGE_API_KEY");
                
                // Nếu không tìm thấy trong biến môi trường, thử đọc từ appsettings.json
                if (string.IsNullOrEmpty(apiKey))
                {
                    // Đọc trực tiếp từ file appsettings.json
                    try
                    {
                        var appSettingsPath = Path.Combine(_environment.ContentRootPath, "appsettings.json");
                        if (File.Exists(appSettingsPath))
                        {
                            var jsonString = File.ReadAllText(appSettingsPath);
                            var jsonDoc = System.Text.Json.JsonDocument.Parse(jsonString);
                            
                            if (jsonDoc.RootElement.TryGetProperty("IMAGE_API_KEY", out var keyElement))
                            {
                                apiKey = keyElement.GetString();
                                _logger.LogInformation("Đã lấy API key từ appsettings.json");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Lỗi khi đọc API key từ appsettings.json");
                    }
                }
                
                _logger.LogInformation($"Sử dụng API key: {(string.IsNullOrEmpty(apiKey) ? "không tìm thấy" : apiKey.Substring(0, Math.Min(5, apiKey.Length)) + "...")}");
                
                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogError("IMAGE_API_KEY không được cấu hình");
                    throw new Exception("IMAGE_API_KEY không được cấu hình");
                }

                // Thiết lập Authorization header
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                
                // Tải ảnh lên server tạm thời để có URL
                var imageUrl = await UploadBase64ImageToTemporaryServer(base64Image);
                
                // Tạo ID duy nhất cho file và adapter
                var fileId = Guid.NewGuid().ToString();
                var adapterId = Guid.NewGuid().ToString();
                
                // Chuẩn bị nội dung yêu cầu cho 4oimageapi.io
                var requestContent = new
                {
                    prompt = command,
                    negative_prompt = "low quality, blurry, distorted",
                    filesUrl = new[] { imageUrl }, // Sử dụng URL ảnh thay vì base64
                    file_id = fileId, // Thêm file_id để xác định tệp
                    width = 512,
                    height = 512,
                    aspect_ratio = "1:1", // Tỷ lệ khung hình 1:1
                    samples = 1,
                    steps = 20,
                    safety_checker = true,
                    enhance_prompt = true,
                    seed = 0,
                    guidance_scale = 7.5,
                    webhook_url = "",
                    track_id = Guid.NewGuid().ToString(),
                    model_type = "realistic", // Thêm model_type để sử dụng mô hình phù hợp
                    adapter_type = "control", // Sử dụng adapter_type "control" cho image-to-image generation
                    adapter_id = adapterId, // Thêm adapter_id cho image-to-image generation
                    controlnet_conditioning_scale = 0.8 // Thêm mức độ ảnh hưởng của ảnh gốc (0-1)
                };

                _logger.LogInformation($"Gửi yêu cầu đến 4oimageapi.io với prompt: {command}");
                _logger.LogInformation($"Sử dụng URL ảnh: {imageUrl}");
                _logger.LogInformation($"File ID: {fileId}, Adapter ID: {adapterId}");
                
                var jsonContent = System.Text.Json.JsonSerializer.Serialize(requestContent);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                // Gửi yêu cầu đến API 4oimageapi.io theo tài liệu chính thức
                _logger.LogInformation("Bắt đầu gửi yêu cầu đến API...");
                var response = await httpClient.PostAsync(
                    "https://4oimageapiio.erweima.ai/api/v1/gpt4o-image/generate",
                    content);

                _logger.LogInformation($"Nhận phản hồi từ API với mã trạng thái: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Lỗi từ 4oimageapi.io: {errorContent}");
                    throw new Exception($"Lỗi từ 4oimageapi.io: {response.StatusCode}, Chi tiết: {errorContent}");
                }

                // Xử lý phản hồi
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Nhận được phản hồi từ API, độ dài nội dung: {responseContent.Length} ký tự");
                
                var responseJson = System.Text.Json.JsonDocument.Parse(responseContent);

                // Kiểm tra xem có task_id không
                if (responseJson.RootElement.TryGetProperty("data", out var dataElement) &&
                    dataElement.TryGetProperty("taskId", out var taskIdElement))
                {
                    var taskId = taskIdElement.GetString();
                    _logger.LogInformation($"Nhận được taskId: {taskId}, đang đợi kết quả...");
                    
                    // Đợi và lấy kết quả
                    return await WaitForImageGenerationResult(httpClient, taskId);
                }
                else
                {
                    _logger.LogError("Không tìm thấy taskId trong phản hồi");
                    _logger.LogError($"Cấu trúc phản hồi: {responseContent.Substring(0, Math.Min(500, responseContent.Length))}...");
                    
                    // Thử phân tích JSON một cách thủ công để tìm taskId
                    try {
                        if (responseContent.Contains("taskId"))
                        {
                            // Tìm vị trí của "taskId"
                            int taskIdIndex = responseContent.IndexOf("taskId");
                            // Tìm vị trí của dấu nháy kép sau "taskId"
                            int firstQuoteIndex = responseContent.IndexOf("\"", taskIdIndex + 8);
                            if (firstQuoteIndex > 0)
                            {
                                // Tìm vị trí của dấu nháy kép thứ hai
                                int secondQuoteIndex = responseContent.IndexOf("\"", firstQuoteIndex + 1);
                                if (secondQuoteIndex > firstQuoteIndex)
                                {
                                    // Trích xuất taskId
                                    string taskId = responseContent.Substring(firstQuoteIndex + 1, secondQuoteIndex - firstQuoteIndex - 1);
                                    _logger.LogInformation($"Đã tìm thấy taskId bằng phương pháp thủ công: {taskId}");
                                    return await WaitForImageGenerationResult(httpClient, taskId);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Lỗi khi cố gắng phân tích JSON thủ công: {0}", ex.Message);
                    }
                    
                    throw new Exception("Không tìm thấy taskId trong phản hồi từ API");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi API 4oimageapi.io: {0}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Tải ảnh base64 lên server tạm thời và trả về URL
        /// </summary>
        private async Task<string> UploadBase64ImageToTemporaryServer(string base64Image)
        {
            try
            {
                // Chuyển đổi base64 thành bytes
                byte[] imageBytes = Convert.FromBase64String(base64Image);
                
                // Tạo tên tệp tạm thời
                string tempFileName = $"{Guid.NewGuid()}.jpg";
                string tempFilePath = Path.Combine(_environment.WebRootPath, "uploads", tempFileName);
                
                // Đảm bảo thư mục tồn tại
                Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath));
                
                // Lưu tệp
                await File.WriteAllBytesAsync(tempFilePath, imageBytes);
                
                // Tải lên dịch vụ lưu trữ ảnh trực tuyến để có URL công khai
                // API bên ngoài cần URL công khai có thể truy cập từ internet
                try
                {
                    // Sử dụng imgbb.com để tạo URL công khai
                    using (var httpClient = new HttpClient())
                    using (var formContent = new MultipartFormDataContent())
                    {
                        // Tạo nội dung multipart để tải lên
                        var imageContent = new ByteArrayContent(imageBytes);
                        imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                        formContent.Add(imageContent, "image", tempFileName);
                        
                        // API key cho imgbb.com
                        var imgbbApiKey = Environment.GetEnvironmentVariable("IMGBB_API_KEY");
                        if (string.IsNullOrEmpty(imgbbApiKey))
                        {
                            _logger.LogWarning("IMGBB_API_KEY không được cấu hình, sử dụng API key mặc định");
                            imgbbApiKey = "b8e7d00b9d5a95e0be1a3c5f99ad3997"; // API key mặc định cho mục đích demo
                        }
                        
                        _logger.LogInformation($"Đang tải ảnh lên imgbb.com với API key: {imgbbApiKey.Substring(0, 5)}...");
                        var response = await httpClient.PostAsync($"https://api.imgbb.com/1/upload?key={imgbbApiKey}", formContent);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            var responseContent = await response.Content.ReadAsStringAsync();
                            _logger.LogInformation($"Phản hồi từ imgbb.com: {responseContent.Substring(0, Math.Min(200, responseContent.Length))}...");
                            
                            var responseJson = System.Text.Json.JsonDocument.Parse(responseContent);
                            
                            if (responseJson.RootElement.TryGetProperty("data", out var dataElement))
                            {
                                // Ưu tiên sử dụng URL trực tiếp
                                if (dataElement.TryGetProperty("url", out var urlElement))
                                {
                                    var publicUrl = urlElement.GetString();
                                    _logger.LogInformation($"Đã tải ảnh lên imgbb.com thành công: {publicUrl}");
                                    return publicUrl;
                                }
                                // Nếu không có URL trực tiếp, thử lấy URL hiển thị
                                else if (dataElement.TryGetProperty("display_url", out var displayUrlElement))
                                {
                                    var publicUrl = displayUrlElement.GetString();
                                    _logger.LogInformation($"Đã tải ảnh lên imgbb.com thành công (display_url): {publicUrl}");
                                    return publicUrl;
                                }
                            }
                            
                            _logger.LogWarning("Không tìm thấy URL trong phản hồi từ imgbb.com");
                        }
                        else
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            _logger.LogError($"Lỗi khi tải ảnh lên imgbb.com: {response.StatusCode}, Chi tiết: {errorContent}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tải ảnh lên dịch vụ lưu trữ trực tuyến: {0}", ex.Message);
                }
                
                // Nếu không thể tải lên dịch vụ lưu trữ, thử phương án khác
                // Tạo URL công khai sử dụng base64 data URI
                _logger.LogWarning("Không thể tải ảnh lên dịch vụ lưu trữ trực tuyến, sử dụng base64 data URI");
                return $"data:image/jpeg;base64,{base64Image}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý ảnh base64: {0}", ex.Message);
                throw new Exception("Không thể xử lý ảnh base64", ex);
            }
        }

        /// <summary>
        /// Đợi và lấy kết quả từ API 4oimageapi.io
        /// </summary>
        private async Task<string> WaitForImageGenerationResult(HttpClient httpClient, string taskId)
        {
            int maxRetries = 30; // Tối đa 30 lần thử (khoảng 5 phút)
            int retryCount = 0;
            int delaySeconds = 10; // Đợi 10 giây giữa các lần thử
            
            while (retryCount < maxRetries)
            {
                _logger.LogInformation($"Kiểm tra trạng thái của task {taskId}, lần thử {retryCount + 1}/{maxRetries}");
                
                // Gửi yêu cầu kiểm tra trạng thái
                var response = await httpClient.GetAsync($"https://4oimageapiio.erweima.ai/api/v1/gpt4o-image/record-info?taskId={taskId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Nhận được phản hồi kiểm tra trạng thái: {responseContent.Substring(0, Math.Min(200, responseContent.Length))}...");
                    
                    var responseJson = System.Text.Json.JsonDocument.Parse(responseContent);
                    
                    if (responseJson.RootElement.TryGetProperty("data", out var dataElement))
                    {
                        // Kiểm tra trạng thái
                        if (dataElement.TryGetProperty("status", out var statusElement))
                        {
                            var status = statusElement.GetString();
                            _logger.LogInformation($"Trạng thái hiện tại: {status}");
                            
                            if (status == "SUCCESS")
                            {
                                // Tìm URL ảnh trong cấu trúc response.resultUrls
                                if (dataElement.TryGetProperty("response", out var responseElement) &&
                                    responseElement.TryGetProperty("resultUrls", out var resultUrlsElement) &&
                                    resultUrlsElement.GetArrayLength() > 0)
                                {
                                    var imageUrl = resultUrlsElement[0].GetString();
                                    _logger.LogInformation($"Đã nhận được URL ảnh từ resultUrls: {imageUrl}");
                                    
                                    // Tải ảnh từ URL
                                    var imageResponse = await httpClient.GetAsync(imageUrl);
                                    if (imageResponse.IsSuccessStatusCode)
                                    {
                                        var imageBytes = await imageResponse.Content.ReadAsByteArrayAsync();
                                        var base64Image = Convert.ToBase64String(imageBytes);
                                        _logger.LogInformation($"Đã chuyển đổi ảnh thành base64, độ dài: {base64Image.Length} ký tự");
                                        return base64Image;
                                    }
                                    else
                                    {
                                        _logger.LogError($"Không thể tải ảnh từ URL: {imageUrl}, Mã trạng thái: {imageResponse.StatusCode}");
                                    }
                                }
                                // Kiểm tra cấu trúc cũ (images) nếu không tìm thấy trong response.resultUrls
                                else if (dataElement.TryGetProperty("images", out var imagesElement) && 
                                    imagesElement.GetArrayLength() > 0)
                                {
                                    var imageUrl = imagesElement[0].GetString();
                                    _logger.LogInformation($"Đã nhận được URL ảnh từ images: {imageUrl}");
                                    
                                    // Tải ảnh từ URL
                                    var imageResponse = await httpClient.GetAsync(imageUrl);
                                    if (imageResponse.IsSuccessStatusCode)
                                    {
                                        var imageBytes = await imageResponse.Content.ReadAsByteArrayAsync();
                                        var base64Image = Convert.ToBase64String(imageBytes);
                                        _logger.LogInformation($"Đã chuyển đổi ảnh thành base64, độ dài: {base64Image.Length} ký tự");
                                        return base64Image;
                                    }
                                    else
                                    {
                                        _logger.LogError($"Không thể tải ảnh từ URL: {imageUrl}, Mã trạng thái: {imageResponse.StatusCode}");
                                    }
                                }
                                else
                                {
                                    _logger.LogError("Không tìm thấy URL ảnh trong phản hồi thành công");
                                    _logger.LogError($"Nội dung phản hồi: {responseContent}");
                                    
                                    // Thử tìm URL ảnh theo cách thủ công
                                    try {
                                        if (responseContent.Contains("resultUrls") && responseContent.Contains("http"))
                                        {
                                            int urlStartIndex = responseContent.IndexOf("http");
                                            if (urlStartIndex > 0)
                                            {
                                                // Tìm dấu nháy kép đóng URL
                                                int urlEndIndex = responseContent.IndexOf("\"", urlStartIndex);
                                                if (urlEndIndex > urlStartIndex)
                                                {
                                                    string imageUrl = responseContent.Substring(urlStartIndex, urlEndIndex - urlStartIndex);
                                                    _logger.LogInformation($"Đã tìm thấy URL ảnh bằng phương pháp thủ công: {imageUrl}");
                                                    
                                                    // Tải ảnh từ URL
                                                    var imageResponse = await httpClient.GetAsync(imageUrl);
                                                    if (imageResponse.IsSuccessStatusCode)
                                                    {
                                                        var imageBytes = await imageResponse.Content.ReadAsByteArrayAsync();
                                                        var base64Image = Convert.ToBase64String(imageBytes);
                                                        _logger.LogInformation($"Đã chuyển đổi ảnh thành base64, độ dài: {base64Image.Length} ký tự");
                                                        return base64Image;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "Lỗi khi tìm URL ảnh thủ công: {0}", ex.Message);
                                    }
                                }
                            }
                            else if (status == "GENERATING")
                            {
                                // Tiếp tục đợi
                                _logger.LogInformation($"Ảnh đang được tạo, đợi {delaySeconds} giây...");
                            }
                            else if (status == "CREATE_TASK_FAILED" || status == "GENERATE_FAILED")
                            {
                                // Lỗi khi tạo ảnh
                                _logger.LogError($"Tạo ảnh thất bại với trạng thái: {status}");
                                throw new Exception($"Tạo ảnh thất bại: {status}");
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogWarning($"Không thể kiểm tra trạng thái, mã trạng thái: {response.StatusCode}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Nội dung lỗi: {errorContent}");
                }
                
                // Đợi trước khi thử lại
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                retryCount++;
            }
            
            _logger.LogError($"Đã hết thời gian chờ sau {maxRetries} lần thử");
            throw new TimeoutException($"Đã hết thời gian chờ kết quả từ API sau {maxRetries * delaySeconds} giây");
        }

        /// <summary>
        /// Generates a new image path for processed images
        /// </summary>
        private string GetNewImagePath(string originalPath)
        {
            var fileName = Path.GetFileNameWithoutExtension(originalPath);
            var extension = Path.GetExtension(originalPath);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var newFileName = $"{fileName}_ai_{timestamp}{extension}";
            return Path.Combine(_fileStorageService.UploadDirectory, newFileName).Replace('\\', '/');
        }

        /// <summary>
        /// Adds an applied filter to the tracking dictionary
        /// </summary>
        private void AddAppliedFilter(string imagePath, string filterName, Dictionary<string, object> parameters)
        {
            if (!_appliedFilters.ContainsKey(imagePath))
            {
                _appliedFilters[imagePath] = new List<AppliedFilter>();
            }

            _appliedFilters[imagePath].Add(new AppliedFilter
            {
                FilterName = filterName,
                Parameters = parameters
            });
        }

        /// <summary>
        /// Tạo ảnh mới bằng AI từ mô tả văn bản
        /// </summary>
        /// <param name="prompt">Mô tả văn bản để tạo ảnh</param>
        /// <returns>Đường dẫn tương đối của ảnh đã tạo</returns>
        public async Task<string> GenerateImageAsync(string prompt)
        {
            try
            {
                _logger.LogInformation($"Bắt đầu tạo ảnh từ mô tả: {prompt}");
                
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromMinutes(3); // Tăng timeout lên 3 phút
                
                // Lấy API key từ cấu hình
                var apiKey = Environment.GetEnvironmentVariable("IMAGE_API_KEY");
                
                // Nếu không tìm thấy trong biến môi trường, thử đọc từ appsettings.json
                if (string.IsNullOrEmpty(apiKey))
                {
                    // Đọc trực tiếp từ file appsettings.json
                    try
                    {
                        var appSettingsPath = Path.Combine(_environment.ContentRootPath, "appsettings.json");
                        if (File.Exists(appSettingsPath))
                        {
                            var jsonString = File.ReadAllText(appSettingsPath);
                            var jsonDoc = System.Text.Json.JsonDocument.Parse(jsonString);
                            
                            if (jsonDoc.RootElement.TryGetProperty("IMAGE_API_KEY", out var keyElement))
                            {
                                apiKey = keyElement.GetString();
                                _logger.LogInformation("Đã lấy API key từ appsettings.json");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Lỗi khi đọc API key từ appsettings.json");
                    }
                }
                
                _logger.LogInformation($"Sử dụng API key: {(string.IsNullOrEmpty(apiKey) ? "không tìm thấy" : apiKey.Substring(0, Math.Min(5, apiKey.Length)) + "...")}");
                
                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogError("IMAGE_API_KEY không được cấu hình");
                    throw new Exception("IMAGE_API_KEY không được cấu hình");
                }

                // Thiết lập Authorization header
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                
                // Tạo ID duy nhất cho file và adapter
                var fileId = Guid.NewGuid().ToString();
                
                // Chuẩn bị nội dung yêu cầu cho 4oimageapi.io
                var requestContent = new
                {
                    prompt = prompt,
                    negative_prompt = "low quality, blurry, distorted",
                    width = 512,
                    height = 512,
                    aspect_ratio = "1:1", // Tỷ lệ khung hình 1:1
                    samples = 1,
                    steps = 20,
                    safety_checker = true,
                    enhance_prompt = true,
                    seed = 0,
                    guidance_scale = 7.5,
                    webhook_url = "",
                    track_id = Guid.NewGuid().ToString(),
                    model_type = "realistic" // Sử dụng mô hình "realistic"
                };

                _logger.LogInformation($"Gửi yêu cầu đến 4oimageapi.io với prompt: {prompt}");
                
                var jsonContent = System.Text.Json.JsonSerializer.Serialize(requestContent);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                // Gửi yêu cầu đến API 4oimageapi.io
                _logger.LogInformation("Bắt đầu gửi yêu cầu đến API...");
                var response = await httpClient.PostAsync(
                    "https://4oimageapiio.erweima.ai/api/v1/gpt4o-image/generate",
                    content);

                _logger.LogInformation($"Nhận phản hồi từ API với mã trạng thái: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Lỗi từ 4oimageapi.io: {errorContent}");
                    throw new Exception($"Lỗi từ 4oimageapi.io: {response.StatusCode}, Chi tiết: {errorContent}");
                }

                // Xử lý phản hồi
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Nhận được phản hồi từ API, độ dài nội dung: {responseContent.Length} ký tự");
                
                var responseJson = System.Text.Json.JsonDocument.Parse(responseContent);

                // Kiểm tra xem có task_id không
                if (responseJson.RootElement.TryGetProperty("data", out var dataElement) &&
                    dataElement.TryGetProperty("taskId", out var taskIdElement))
                {
                    var taskId = taskIdElement.GetString();
                    _logger.LogInformation($"Nhận được taskId: {taskId}, đang đợi kết quả...");
                    
                    // Đợi và lấy kết quả
                    var imageBase64 = await WaitForImageGenerationResult(httpClient, taskId);
                    
                    if (string.IsNullOrEmpty(imageBase64))
                    {
                        throw new Exception("Không thể tạo ảnh với API 4oimageapi.io");
                    }

                    // Chuyển đổi base64 thành ảnh
                    var imageBytes = Convert.FromBase64String(imageBase64);
                    var generatedImage = await Image.LoadAsync(new MemoryStream(imageBytes));

                    // Tạo tên file mới
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var newFileName = $"ai_generated_{timestamp}.jpg";
                    var newFilePath = Path.Combine(_fileStorageService.UploadDirectory, newFileName);
                    var newFullPath = Path.Combine(_environment.WebRootPath, newFilePath);
                    
                    // Đảm bảo thư mục tồn tại
                    var directory = Path.GetDirectoryName(newFullPath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    
                    _logger.LogInformation($"Lưu ảnh đã tạo tại: {newFullPath}");
                    await generatedImage.SaveAsJpegAsync(newFullPath);

                    return "/" + newFilePath.Replace('\\', '/');
                }
                else
                {
                    _logger.LogError("Không tìm thấy taskId trong phản hồi");
                    throw new Exception("Không tìm thấy taskId trong phản hồi từ API");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo ảnh bằng AI: {0}", ex.Message);
                throw;
            }
        }
    }
    
    public class FilterInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
    
    public class AppliedFilter
    {
        public string FilterName { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }
} 