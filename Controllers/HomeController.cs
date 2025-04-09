using System.Diagnostics;
using DocumentWebsite.Models;
using DocumentWebsite.Repositories;
using DocumentWebsite.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace DocumentWebsite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDocumentRepo _documentRepo;
        private readonly ICategoryRepo _categoryRepo;

        public HomeController(ILogger<HomeController> logger, IDocumentRepo documentRepo, ICategoryRepo categoryRepo)
        {
            _logger = logger;
            _documentRepo = documentRepo;
            _categoryRepo = categoryRepo;
        }

        public async Task<IActionResult> Index(int currentGroupTopFavorites = 1, int currentGroupTopViewed = 1)
        {
            // Lấy danh sách sách mới nhất (giới hạn 12 sách)
            var documents = await _documentRepo.GetAllAsync();
            documents = documents.OrderByDescending(b => b.CreatedDate).Take(12);

            // Lấy danh sách danh mục
            var categories = await _categoryRepo.GetAllAsync();

            // Lấy Top sách được yêu thích
            var topFavorites = await _documentRepo.GetTopFavoritedDocumentsAsync(10);
            int groupSize = 4;
            int totalFavorites = topFavorites.Count();
            int totalGroupsFavorites = (int)Math.Ceiling((double)totalFavorites / groupSize);
            int startIndexFavorites = (currentGroupTopFavorites - 1) * groupSize;
            var currentFavorites = topFavorites.Skip(startIndexFavorites).Take(groupSize).ToList();

            // Lấy Top sách được xem nhiều nhất
            var topViewed = await _documentRepo.GetTopViewedDocumentsAsync(10);
            int totalViewed = topViewed.Count();
            int totalGroupsViewed = (int)Math.Ceiling((double)totalViewed / groupSize);
            int startIndexViewed = (currentGroupTopViewed - 1) * groupSize;
            var currentViewed = topViewed.Skip(startIndexViewed).Take(groupSize).ToList();

            // Tạo ViewModel để truyền dữ liệu
            HomeViewModel home = new HomeViewModel()
            {
                Categories = categories,
                Documents = documents,
                // Thêm sách thích nhiều
                TopFavorites = currentFavorites,
                TotalGroupsFavorites = totalGroupsFavorites,
                CurrentGroupFavorites = currentGroupTopFavorites,
                StartIndexFavorites = startIndexFavorites,
                // Thêm sách xem nhiều
                TopViewed = currentViewed,
                TotalGroupsViewed = totalGroupsViewed,
                CurrentGroupViewed = currentGroupTopViewed,
                StartIndexViewed = startIndexViewed
            };

            return View(home);
        }

        public async Task<IActionResult> LoadFavorites(int currentGroupTopFavorites)
        {
            int groupSize = 4;

            var topFavorites = await _documentRepo.GetTopFavoritedDocumentsAsync(10);
            int totalFavorites = topFavorites.Count();
            int totalGroupsFavorites = (int)Math.Ceiling((double)totalFavorites / groupSize);

            int startIndexFavorites = (currentGroupTopFavorites - 1) * groupSize;
            var currentFavorites = topFavorites.Skip(startIndexFavorites).Take(groupSize).ToList();

            var viewModel = new HomeViewModel
            {
                TopFavorites = currentFavorites,
                CurrentGroupFavorites = currentGroupTopFavorites,
                TotalGroupsFavorites = totalGroupsFavorites,
                StartIndexFavorites = startIndexFavorites
            };

            return PartialView("_TopPartial", viewModel);
        }

        public async Task<IActionResult> LoadViewed(int currentGroupTopViewed)
        {
            int groupSize = 4;

            var topViewed = await _documentRepo.GetTopViewedDocumentsAsync(10);
            int totalViewed = topViewed.Count();
            int totalGroupsViewed = (int)Math.Ceiling((double)totalViewed / groupSize);

            int startIndexViewed = (currentGroupTopViewed - 1) * groupSize;
            var currentViewed = topViewed.Skip(startIndexViewed).Take(groupSize).ToList();

            var viewModel = new HomeViewModel
            {
                TopViewed = currentViewed,
                CurrentGroupViewed = currentGroupTopViewed,
                TotalGroupsViewed = totalGroupsViewed,
                StartIndexViewed = startIndexViewed
            };

            return PartialView("_PartialView", viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult TermOfUse()
        {
            return View();
        }
        public IActionResult ContactUs()
        {
            return View();
        }
        public IActionResult AboutUs()
        {
            return View();
        }
        public IActionResult Service()
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
