using ClothesStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ClothesStore.Controllers
{
    public class ClothesDetailController : Controller
    {
        // GET: ClothesDetail
        private TESTEntities db = new TESTEntities();
        // GET: ClothesDetail
        public ActionResult Index(string id)
        {

            ViewBag.Categories = db.Categories.ToList();
            ViewBag.ClothingTypes = db.ClothingTypes.ToList();
            ViewBag.Clothes = db.Clothes.ToList();
            ViewBag.Images = db.Images.ToList();
            ViewBag.Colors = db.Colors.ToList();
            ViewBag.Sizes = db.Sizes.ToList();
            ViewBag.Quantity = db.Clothes_Color_Size.ToList();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cloth cloth = db.Clothes.Find(id);
            if (cloth == null)
            {
                return HttpNotFound();
            }
            return View(cloth);
        }
    }
}