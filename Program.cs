using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MiniPhotoshop.Backend.Filters;
using MiniPhotoshop.Backend.Filters.BasicFilters;
using MiniPhotoshop.Backend.Filters.TransformFilters;
using MiniPhotoshop.Backend.Services;
using SixLabors.ImageSharp.Web.DependencyInjection;
using System;
using System.IO;

namespace MiniPhotoshop
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Thêm cấu hình để đọc biến môi trường API key
            builder.Configuration.AddEnvironmentVariables();
            Environment.SetEnvironmentVariable("IMAGE_API_KEY", builder.Configuration["IMAGE_API_KEY"]);
            Environment.SetEnvironmentVariable("IMGBB_API_KEY", builder.Configuration["IMGBB_API_KEY"]);

            // Thêm services vào container.
            builder.Services.AddControllersWithViews()
                .AddApplicationPart(typeof(MiniPhotoshop.Backend.Controllers.HomeController).Assembly)
                .AddControllersAsServices();
            builder.Services.AddRazorPages();

            // Add session services
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Add SixLabors.ImageSharp.Web services
            builder.Services.AddImageSharp();

            // Đăng ký các dịch vụ
            builder.Services.AddSingleton<FileStorageService>();
            builder.Services.AddScoped<ImageProcessingService>();

            // Đăng ký các bộ lọc
            // Bộ lọc màu sắc và độ sáng
            builder.Services.AddTransient<IImageFilter, MiniPhotoshop.Backend.Filters.GrayscaleFilter>();
            builder.Services.AddTransient<IImageFilter, MiniPhotoshop.Backend.Filters.SepiaFilter>();
            builder.Services.AddTransient<IImageFilter, MiniPhotoshop.Backend.Filters.BrightnessFilter>();
            builder.Services.AddTransient<IImageFilter, MiniPhotoshop.Backend.Filters.ContrastFilter>();
            builder.Services.AddTransient<IImageFilter, MiniPhotoshop.Backend.Filters.SaturationFilter>();
            builder.Services.AddTransient<IImageFilter, MiniPhotoshop.Backend.Filters.HueFilter>();
            builder.Services.AddTransient<IImageFilter, MiniPhotoshop.Backend.Filters.GammaFilter>();
            builder.Services.AddTransient<IImageFilter, MiniPhotoshop.Backend.Filters.InvertFilter>();

            // Bộ lọc biến đổi
            builder.Services.AddTransient<IImageFilter, MiniPhotoshop.Backend.Filters.FlipHorizontalFilter>();
            builder.Services.AddTransient<IImageFilter, MiniPhotoshop.Backend.Filters.FlipVerticalFilter>();
            builder.Services.AddTransient<IImageFilter, MiniPhotoshop.Backend.Filters.RotateFilter>();
            builder.Services.AddTransient<IImageFilter, MiniPhotoshop.Backend.Filters.ResizeFilter>();
            builder.Services.AddTransient<IImageFilter, MiniPhotoshop.Backend.Filters.CropFilter>();

            // Bộ lọc làm mờ và sắc nét
            builder.Services.AddTransient<IImageFilter, MiniPhotoshop.Backend.Filters.BlurFilter>();
            builder.Services.AddTransient<IImageFilter, MiniPhotoshop.Backend.Filters.GaussianBlurFilter>();
            builder.Services.AddTransient<IImageFilter, MiniPhotoshop.Backend.Filters.BoxBlurFilter>();
            builder.Services.AddTransient<IImageFilter, MiniPhotoshop.Backend.Filters.SharpenFilter>();

            // Bộ lọc hiệu ứng đặc biệt
            builder.Services.AddTransient<IImageFilter, MiniPhotoshop.Backend.Filters.PixelateFilter>();
            builder.Services.AddTransient<IImageFilter, MiniPhotoshop.Backend.Filters.OilPaintFilter>();
            builder.Services.AddTransient<IImageFilter, MiniPhotoshop.Backend.Filters.LomographFilter>();
            builder.Services.AddTransient<IImageFilter, MiniPhotoshop.Backend.Filters.PolaroidFilter>();
            builder.Services.AddTransient<IImageFilter, MiniPhotoshop.Backend.Filters.KodachromeFilter>();
            builder.Services.AddTransient<IImageFilter, MiniPhotoshop.Backend.Filters.GlowFilter>();
            builder.Services.AddTransient<IImageFilter, MiniPhotoshop.Backend.Filters.VignetteFilter>();

            // Thêm CORS policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });

            var app = builder.Build();

            // Configure HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // Disable HTTPS redirection for Render.com
                // app.UseHsts();
            }

            // Disable HTTPS redirection for Render.com
            // app.UseHttpsRedirection();
            
            app.UseStaticFiles();
            app.UseImageSharp();
            app.UseRouting();

            // Add session middleware
            app.UseSession();

            app.UseCors("AllowAll");
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}",
                defaults: new { area = "", controller = "Home", action = "Index" },
                constraints: null,
                dataTokens: new { Namespace = "MiniPhotoshop.Backend.Controllers" });

            // Đảm bảo thư mục uploads tồn tại
            var uploadsDir = Path.Combine(app.Environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            app.Run();
        }
    }
} 