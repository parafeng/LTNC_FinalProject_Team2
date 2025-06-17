using System.Collections.Generic;

namespace MiniPhotoshop.Backend.Data.DTOs
{
    /// <summary>
    /// Data Transfer Object cho yêu cầu áp dụng bộ lọc
    /// </summary>
    public class FilterRequest
    {
        /// <summary>
        /// Đường dẫn tương đối của ảnh cần xử lý
        /// </summary>
        public string ImagePath { get; set; }
        
        /// <summary>
        /// Tên bộ lọc cần áp dụng
        /// </summary>
        public string FilterName { get; set; }
        
        /// <summary>
        /// Tham số cho bộ lọc
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; }
    }
    
    /// <summary>
    /// Data Transfer Object cho kết quả xử lý ảnh
    /// </summary>
    public class FilterResponse
    {
        /// <summary>
        /// Đường dẫn tương đối của ảnh đã xử lý
        /// </summary>
        public string FilePath { get; set; }
    }
    
    /// <summary>
    /// Data Transfer Object cho thông tin bộ lọc
    /// </summary>
    public class FilterInfo
    {
        /// <summary>
        /// Tên bộ lọc
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Mô tả bộ lọc
        /// </summary>
        public string Description { get; set; }
    }
} 