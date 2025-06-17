using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters
{
    public class OilPaintFilter : IImageFilter
    {
        public string Name => "OilPaint";
        
        public string Description => "Tạo hiệu ứng sơn dầu cho ảnh";
        
        public Task ApplyAsync(Image image, Dictionary<string, object> parameters)
        {
            int levels = 10;
            int brushSize = 5;
            
            if (parameters.TryGetValue("levels", out var levelsObj) && 
                int.TryParse(levelsObj.ToString(), out int levelsValue))
            {
                levels = Math.Clamp(levelsValue, 2, 30);
            }
            
            if (parameters.TryGetValue("brushSize", out var brushObj) && 
                int.TryParse(brushObj.ToString(), out int brushValue))
            {
                brushSize = Math.Clamp(brushValue, 1, 30);
            }
            
            // Tạo hiệu ứng sơn dầu bằng cách kết hợp các kỹ thuật
            image.Mutate(x => x
                .Saturate(1.2f)          // Tăng độ bão hòa màu
                .GaussianBlur(0.5f)      // Làm mờ nhẹ
                .Contrast(0.2f)          // Tăng độ tương phản
                .Pixelate(brushSize)     // Tạo hiệu ứng nét cọ
                .Contrast(0.5f)          // Tăng độ tương phản thay cho Sharpen
            );
            
            return Task.CompletedTask;
        }
    }
} 