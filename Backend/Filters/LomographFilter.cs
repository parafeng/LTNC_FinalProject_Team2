using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters
{
    public class LomographFilter : IImageFilter
    {
        public string Name => "Lomograph";
        
        public string Description => "Áp dụng hiệu ứng lomography vintage";
        
        public Task ApplyAsync(Image image, Dictionary<string, object> parameters)
        {
            // Tạo hiệu ứng lomography bằng cách kết hợp nhiều bộ lọc
            image.Mutate(x => x
                .Saturate(1.3f)          // Tăng độ bão hòa màu
                .Contrast(0.15f)         // Tăng độ tương phản
                .Brightness(0.05f)       // Tăng độ sáng nhẹ
                .Vignette()              // Thêm viền tối
            );
            
            return Task.CompletedTask;
        }
    }
} 