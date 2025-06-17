using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters
{
    public class PolaroidFilter : IImageFilter
    {
        public string Name => "Polaroid";
        
        public string Description => "Áp dụng hiệu ứng ảnh Polaroid";
        
        public Task ApplyAsync(Image image, Dictionary<string, object> parameters)
        {
            // Tạo hiệu ứng Polaroid bằng cách kết hợp nhiều hiệu ứng khác
            image.Mutate(x => x
                .Saturate(1.2f)                  // Tăng độ bão hòa màu
                .Contrast(0.1f)                  // Tăng nhẹ độ tương phản
                .Brightness(0.05f)               // Tăng nhẹ độ sáng
                .Sepia(0.2f)                     // Thêm chút hiệu ứng sepia
                .Vignette()                      // Thêm viền tối
            );
            
            return Task.CompletedTask;
        }
    }
} 