using ClothesStore.Models;
using ClothesStore.Models.ViewModels;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothesStore.Controllers
{
    public class HomeController : Controller
    {
        private TESTEntities db = new TESTEntities();
        // GET: Home

        public ActionResult Index()
        {
            var userId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            // Lọc CartDetails theo UserID
            var cartDetails = db.Carts
                .Where(c => c.UserID == userId) // Lọc theo UserID
                .SelectMany(c => c.CartDetails) // Lấy CartDetails của các Cart
                .ToList();

            ViewBag.CartDetails = cartDetails; // Gán kết quả vào ViewBag

            var clothesViewModel = db.Clothes
              .Select(c => new ClothesViewModel
              {
                  ClothesItem = c,
                  Images = db.Images.Where(img => img.ClothesID == c.ClothesID).ToList(),
                  Clothes_Color_Sizes = db.Clothes_Color_Size.Where(clo => clo.ClothesID == c.ClothesID).ToList(),

                  Colors = db.Colors
                      .Where(col => db.Images.Any(img => img.ColorID == col.ColorID && img.ClothesID == c.ClothesID))
                      .ToList(),

                  // Get sizes associated with each color for the clothing item
                  Sizes = db.Sizes
                      .Where(size => db.Clothes_Color_Size
                          .Any(clo => clo.SizeID == size.SizeID && clo.ClothesID == c.ClothesID
                                       && clo.ColorID == db.Colors
                                          .Where(col => db.Images.Any(img => img.ColorID == col.ColorID && img.ClothesID == c.ClothesID))
                                          .Select(col => col.ColorID).FirstOrDefault()))
                      .ToList()
              })
              .ToList();

            // Tạo danh sách kết quả cuối cùng
            var finalClothes = new List<ClothesViewModel>();

            // Khởi tạo đối tượng Random để sinh số ngẫu nhiên
            var random = new Random();

            // Tổng số sản phẩm cần xử lý
            int totalProducts = 8;

            // Số lượng ngẫu nhiên cho từng loại sản phẩm (Nam, Nữ, Bé gái, Bé trai)
            int maleCount = random.Next(1, Math.Min(4, totalProducts));
            int femaleCount = totalProducts - maleCount;
            // Group by ClothingTypeID
            var groupedClothes = clothesViewModel.GroupBy(clo => clo.ClothesItem.ClothingTypeID).ToList();

            foreach (var group in groupedClothes)
            {
                // Lấy sản phẩm nam và nữ trong từng nhóm
                var male = group.Where(clo => clo.ClothesItem.Category.CategoryName == "Nam").Take(maleCount).ToList();
                var female = group.Where(clo => clo.ClothesItem.Category.CategoryName == "Nữ").Take(femaleCount).ToList();

                // Nếu không đủ nam hoặc nữ, lấy bù từ danh sách còn lại
                int maleShortage = maleCount - male.Count;
                int femaleShortage = femaleCount - female.Count;

                if (maleShortage > 0)
                {
                    var extraMale = group.Where(clo => clo.ClothesItem.Category.CategoryName == "Nữ")
                                         .Skip(femaleCount)
                                         .Take(maleShortage)
                                         .ToList();
                    male.AddRange(extraMale);
                }

                if (femaleShortage > 0)
                {
                    var extraFemale = group.Where(clo => clo.ClothesItem.Category.CategoryName == "Nam")
                                           .Skip(maleCount)
                                           .Take(femaleShortage)
                                           .ToList();
                    female.AddRange(extraFemale);
                }

                // Gộp nam và nữ vào danh sách cuối cùng
                finalClothes.AddRange(male.Concat(female));
            }

            return View(finalClothes);
        }

        public ActionResult _Category()
        {
            var cate = db.Categories.Where(cat => cat.IsHidden == false).ToList();
            return View(cate);
        }

        public ActionResult Search(string searchTerm, int? page, int? minPrice, int? maxPrice, List<int> selectedSizes)
        {
            int pageSize = 12;  // Số sản phẩm hiển thị mỗi trang
            int pageNumber = (page ?? 1);  // Mặc định là trang 1

            if (string.IsNullOrEmpty(searchTerm))
            {
                return RedirectToAction("Index"); // Quay lại trang chủ nếu không có từ khóa tìm kiếm
            }

            // Lọc sản phẩm theo từ khóa tìm kiếm
            var query = db.Clothes.AsQueryable();

            query = query.Where(c =>
                c.ClothesName.Contains(searchTerm) || // Tìm trong tên sản phẩm
                db.Categories.Any(cat => cat.CategoryID == c.CategoryID && cat.CategoryName.Contains(searchTerm)) // Tìm trong tên danh mục
            );

            // Lọc theo giá nếu có
            if (minPrice.HasValue)
            {
                query = query.Where(c => c.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(c => c.Price <= maxPrice.Value);
            }

            //// Lọc theo kích thước
            //if (selectedSizes != null && selectedSizes.Any())
            //{
            //    query = query.Where(c => c.Clothes_Color_Size.Any(ccs => selectedSizes.Contains(ccs.SizeID) && ccs.ClothesID == c.ClothesID));
            //}

            var searchResults = query
                .Select(c => new ClothesViewModel
                {
                    ClothesItem = c,
                    Images = db.Images.Where(img => img.ClothesID == c.ClothesID).ToList(),
                    Clothes_Color_Sizes = db.Clothes_Color_Size.Where(clo => clo.ClothesID == c.ClothesID).ToList(),
                    Colors = db.Colors
                        .Where(col => db.Images.Any(img => img.ColorID == col.ColorID && img.ClothesID == c.ClothesID))
                        .ToList(),
                    Sizes = db.Sizes
                        .Where(size => db.Clothes_Color_Size
                            .Any(clo => clo.SizeID == size.SizeID && clo.ClothesID == c.ClothesID
                                         && clo.ColorID == db.Colors
                                            .Where(col => db.Images.Any(img => img.ColorID == col.ColorID && img.ClothesID == c.ClothesID))
                                            .Select(col => col.ColorID).FirstOrDefault()))
                        .ToList()
                })
                .ToList();  // Get the filtered results

            var pagedResults = searchResults.ToPagedList(pageNumber, pageSize);

            ViewBag.AllSizes = db.Sizes.ToList();
            ViewBag.SearchTerm = searchTerm;
            ViewBag.MinPrice = minPrice ?? 0;
            ViewBag.MaxPrice = maxPrice ?? 1000000;
            ViewBag.SelectedSizes = selectedSizes;

            return View("Search", pagedResults);
        }

    }
}