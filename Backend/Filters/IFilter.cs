using SixLabors.ImageSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters
{
    public interface IFilter
    {
        /// <summary>
        /// Tên của bộ lọc
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Mô tả bộ lọc
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// Áp dụng bộ lọc lên ảnh
        /// </summary>
        /// <param name="image">Ảnh gốc</param>
        /// <param name="parameters">Tham số cho bộ lọc</param>
        /// <returns>Ảnh sau khi đã áp dụng bộ lọc</returns>
        Task<Image> Apply(Image image, Dictionary<string, object> parameters);
        
        /// <summary>
        /// Kiểm tra tính hợp lệ của tham số
        /// </summary>
        /// <param name="parameters">Tham số cần kiểm tra</param>
        /// <param name="errorMessage">Thông báo lỗi nếu không hợp lệ</param>
        /// <returns>True nếu tham số hợp lệ, ngược lại False</returns>
        bool ValidateParameters(Dictionary<string, object> parameters, out string errorMessage);
    }
} 