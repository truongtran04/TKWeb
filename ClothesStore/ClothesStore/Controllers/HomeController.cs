using ClothesStore.Models;
using ClothesStore.Models.ViewModels;
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
            // Lọc CartDetails theo UserID
            var cartDetails = db.Carts
                .Where(c => c.UserID == userId) // Lọc theo UserID
                .SelectMany(c => c.CartDetails) // Lấy CartDetails của các Cart
                .ToList();

            ViewBag.CartDetails = cartDetails; // Gán kết quả vào ViewBag

            return View(clothesViewModel);
            

        }

        public ActionResult _Category()
        {
            var cate = db.Categories.Where(cat => cat.IsHidden == false).ToList();
            return View(cate);
        }
    }
}