using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters
{
    public class KodachromeFilter : IImageFilter
    {
        public string Name => "Kodachrome";
        
        public string Description => "Áp dụng hiệu ứng phim Kodachrome cổ điển";
        
        public Task ApplyAsync(Image image, Dictionary<string, object> parameters)
        {
            // Tạo hiệu ứng Kodachrome bằng cách kết hợp nhiều bộ lọc
            image.Mutate(x => x
                .Saturate(0.8f)          // Giảm độ bão hòa màu một chút
                .Contrast(0.3f)          // Tăng độ tương phản
                .Brightness(0.05f)       // Tăng độ sáng nhẹ thay vì giảm
                .Hue(0.05f)              // Chỉnh sắc độ nhẹ
            );
            
            return Task.CompletedTask;
        }
    }
} 