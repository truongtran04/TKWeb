using ClothesStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothesStore.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        private readonly TESTEntities db = new TESTEntities();

        private void UpdateTotalAmount(int cartId)
        {
            var cart = db.Carts.Find(cartId);
            if (cart != null)
            {
                cart.TotalAmount = db.CartDetails
                    .Where(cd => cd.CartID == cartId)
                    .Sum(cd => (decimal?)(cd.Quantity * cd.Price)) ?? 0;
                db.SaveChanges();
            }
        }

        // Thêm sản phẩm vào giỏ hàng
        [HttpPost]
        public ActionResult AddToCart(string clothesId, string clothesName, string color, string size, decimal price, string mainImage)
        {
            // Kiểm tra người dùng đã đăng nhập chưa
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account"); // Chuyển hướng đến trang đăng nhập nếu chưa đăng nhập
            }

            // Lấy userId từ Session
            int userId = (int)Session["UserId"];

            // Tìm giỏ hàng chưa hoàn tất của người dùng
            var cart = db.Carts.SingleOrDefault(c => c.UserID == userId && !(c.IsCompleted ?? false));

            if (cart == null)
            {
                // Nếu không tìm thấy giỏ hàng, tạo mới
                cart = new Cart { UserID = userId, TotalAmount = 0 }; // Khởi tạo TotalAmount là 0 khi tạo giỏ mới
                db.Carts.Add(cart);
                db.SaveChanges();
            }

            // Kiểm tra sản phẩm đã tồn tại trong CartDetail chưa
            var cartItem = db.CartDetails.SingleOrDefault(item =>
                item.CartID == cart.CartID && item.ClothesID == clothesId && item.ColorID == color && item.SizeName == size);

            if (cartItem == null)
            {
                // Thêm sản phẩm mới vào CartDetail
                cartItem = new CartDetail
                {
                    CartID = cart.CartID,
                    ClothesID = clothesId,
                    ClothesName = clothesName,
                    MainImage = mainImage,
                    SizeName = size,
                    ColorID = color,
                    Quantity = 1,
                    Price = price
                };
                db.CartDetails.Add(cartItem);
            }
            else
            {
                // Cập nhật số lượng nếu sản phẩm đã có trong giỏ
                cartItem.Quantity += 1;
            }

            db.SaveChanges();

            // Cập nhật lại TotalAmount của giỏ hàng
            UpdateTotalAmount(cart.CartID);

            return RedirectToAction("Index", "Home"); // Chuyển hướng về trang chính
        }

        // Xóa sản phẩm khỏi giỏ hàng
        [HttpPost]
        public ActionResult RemoveFromCart(string uniqueId)
        {
            var parts = uniqueId.Split('_'); // Tách ID sản phẩm
            var clothesId = parts[0]; // ID sản phẩm
            var colorId = parts[1]; // ID màu
            var sizeName = parts[2]; // Kích thước

            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account"); // Chuyển hướng đến trang đăng nhập
            }

            int userId = (int)Session["UserId"];
            var cart = db.Carts.SingleOrDefault(c => c.UserID == userId && !(c.IsCompleted ?? false));

            if (cart != null)
            {
                var itemToRemove = db.CartDetails.SingleOrDefault(item =>
                    item.CartID == cart.CartID &&
                    item.ClothesID == clothesId &&
                    item.ColorID == colorId &&
                    item.SizeName == sizeName);

                if (itemToRemove != null)
                {
                    db.CartDetails.Remove(itemToRemove);
                    db.SaveChanges();

                    UpdateTotalAmount(cart.CartID);
                    return Json(new { success = true, message = "Sản phẩm đã được xóa thành công." });
                }
                return Json(new { success = false, message = "Sản phẩm không tìm thấy trong giỏ hàng." });
            }
            return RedirectToAction("Index", "Home");
        }

        // Cập nhật số lượng sản phẩm trong giỏ hàng
        [HttpPost]
        public JsonResult UpdateCart(string clothesID, string colorID, string sizeName, int quantity)
        {
            if (quantity < 0)
                return Json(new { success = false, message = "Số lượng không được âm." });

            if (Session["UserId"] == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập trước khi cập nhật giỏ hàng." });
            }

            int userId = (int)Session["UserId"];
            var cart = db.Carts.SingleOrDefault(c => c.UserID == userId && !(c.IsCompleted ?? false));

            if (cart != null)
            {
                var cartItem = db.CartDetails.FirstOrDefault(cd => cd.CartID == cart.CartID && cd.ClothesID == clothesID && cd.ColorID == colorID && cd.SizeName == sizeName);
                if (cartItem != null)
                {
                    cartItem.Quantity = quantity; // Cập nhật số lượng
                    cartItem.TotalPrice = cartItem.Price * quantity; // Cập nhật tổng giá
                    db.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                    UpdateTotalAmount(cart.CartID); // Cập nhật tổng số tiền giỏ hàng
                    return Json(new { success = true });
                }
            }
            return Json(new { success = false, message = "Sản phẩm không tìm thấy." });
        }
    }
}