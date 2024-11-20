using ClothesStore.Models;
using ClothesStore.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothesStore.Controllers
{
    public class ClothesController : Controller
    {
        private TESTEntities db = new TESTEntities();
        // GET: Clothes
        public ActionResult Index()
        {
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

            return View(clothesViewModel);
        }

        public ActionResult GetClothesByCate(string categoryId)
        {
            CategoryModels clothesModel = new CategoryModels();
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

            clothesModel.clothes = clothesViewModel.Where(clo => clo.ClothesItem.CategoryID == categoryId).ToList();

            clothesModel.clothingTypes = db.Category_ClothingType.Where(cate => cate.CategoryID == categoryId).ToList();

            return View(clothesModel);
        }

        public ActionResult _ClothingType()
        {
            var cloType = db.Category_ClothingType.Where(clo => clo.IsHidden ==  false).ToList(); 
            return View(cloType);
        }

        public ActionResult _ClothingStyle()
        {
            var cloStyle = db.ClothingStyles.Where(clo => clo.IsHidden == false).ToList();
            return View(cloStyle);
        }

        public ActionResult GetClothesByType(string idCate, string idType)
        {
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
            var clothes = clothesViewModel.Where(clo => clo.ClothesItem.CategoryID == idCate && clo.ClothesItem.ClothingTypeID == idType).ToList();

            var cloType = db.Category_ClothingType.Where(clo => clo.IsHidden == false && clo.CategoryID == idCate).ToList();
            ViewBag.ClothingType = cloType;

            var cloStyle = db.ClothingStyles.Where(clo => clo.ClothingTypeID == idType).ToList();
            ViewBag.ClothesStyle = cloStyle;

            return View(clothes);
        }

        public ActionResult GetClothesByStyle(string idCate, string idStyle)
        {
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

            var clothes = clothesViewModel.Where(clo => clo.ClothesItem.CategoryID == idCate && clo.ClothesItem.ClothingStyleID == idStyle).ToList();

            return PartialView("_GetClothes", clothes);
        }
    }
}