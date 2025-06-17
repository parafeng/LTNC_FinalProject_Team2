using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters
{
    public class PixelateFilter : IImageFilter
    {
        public string Name => "Pixelate";
        
        public string Description => "Tạo hiệu ứng pixel hóa cho ảnh";
        
        public Task ApplyAsync(Image image, Dictionary<string, object> parameters)
        {
            if (parameters.TryGetValue("size", out var sizeObj) && 
                int.TryParse(sizeObj.ToString(), out int size))
            {
                // Giới hạn kích thước pixel trong khoảng 2 đến 100
                size = Math.Clamp(size, 2, 100);
                
                image.Mutate(x => x.Pixelate(size));
            }
            else
            {
                // Mặc định pixel hóa với kích thước 8
                image.Mutate(x => x.Pixelate(8));
            }
            
            return Task.CompletedTask;
        }
    }
} 