using SixLabors.ImageSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters
{
    /// <summary>
    /// Interface định nghĩa một bộ lọc ảnh
    /// </summary>
    public interface IImageFilter
    {
        /// <summary>
        /// Tên của bộ lọc
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Mô tả về bộ lọc
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// Áp dụng bộ lọc lên ảnh
        /// </summary>
        /// <param name="image">Ảnh cần xử lý</param>
        /// <param name="parameters">Tham số cho bộ lọc</param>
        /// <returns>Task</returns>
        Task ApplyAsync(Image image, Dictionary<string, object> parameters);
    }
} 