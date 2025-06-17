using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniPhotoshop.Backend.Filters
{
    public class GaussianBlurFilter : IImageFilter
    {
        public string Name => "GaussianBlur";
        
        public string Description => "Làm mờ Gaussian với độ mịn cao";
        
        public Task ApplyAsync(Image image, Dictionary<string, object> parameters)
        {
            if (parameters.TryGetValue("sigma", out var sigmaObj) && 
                float.TryParse(sigmaObj.ToString(), out float sigma))
            {
                // Giới hạn sigma trong khoảng 0.5 đến 30
                sigma = Math.Clamp(sigma, 0.5f, 30f);
                
                image.Mutate(x => x.GaussianBlur(sigma));
            }
            else
            {
                // Mặc định làm mờ Gaussian với sigma = 3.0
                image.Mutate(x => x.GaussianBlur(3.0f));
            }
            
            return Task.CompletedTask;
        }
    }
} 