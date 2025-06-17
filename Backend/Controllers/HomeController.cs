using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using MiniPhotoshop.Backend.Filters;
using MiniPhotoshop.Backend.Models;
using MiniPhotoshop.Backend.Services;
using System.Collections.Generic;

namespace MiniPhotoshop.Backend.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ImageProcessingService _imageProcessingService;

        public HomeController(
            ILogger<HomeController> logger,
            ImageProcessingService imageProcessingService)
        {
            _logger = logger;
            _imageProcessingService = imageProcessingService;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Trang chủ được truy cập");
            
            // Lấy danh sách các bộ lọc có sẵn
            ViewBag.Filters = _imageProcessingService.GetAvailableFilters();
            
            // Lấy đường dẫn ảnh từ session (nếu có)
            ViewBag.CurrentImagePath = TempData["CurrentImagePath"] as string;
            ViewBag.ImageWidth = TempData["ImageWidth"];
            ViewBag.ImageHeight = TempData["ImageHeight"];
            
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
} 