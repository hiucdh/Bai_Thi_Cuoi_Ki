using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Collections.Generic;
using SV22T1020607.Shop.Models;
using System.Linq;

namespace SV22T1020607.Shop.AppCodes
{
    public static class WebExtensions
    {
        public static void SetUserData<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T? GetUserData<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonSerializer.Deserialize<T>(value);
        }
    }

    public static class CartHelper
    {
        private const string GUEST_CART = "ShoppingCart";

        public static string GetCartKey(this HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                return $"ShoppingCart_{userId}";
            }
            return GUEST_CART;
        }

        public static List<CartItem> GetCart(this HttpContext context)
        {
            var key = context.GetCartKey();
            var cart = context.Session.GetUserData<List<CartItem>>(key);
            return cart ?? new List<CartItem>();
        }

        public static void SaveCart(this HttpContext context, List<CartItem> cart)
        {
            var key = context.GetCartKey();
            context.Session.SetUserData(key, cart);
        }

        public static void ClearCart(this HttpContext context)
        {
            var key = context.GetCartKey();
            context.Session.Remove(key);
        }

        public static void MergeCartAfterLogin(this HttpContext context)
        {
            var guestCart = context.Session.GetUserData<List<CartItem>>(GUEST_CART) ?? new List<CartItem>();
            if (guestCart.Count == 0) return;

            var userCart = context.GetCart(); 
            foreach (var item in guestCart)
            {
                var exist = userCart.FirstOrDefault(x => x.ProductID == item.ProductID);
                if (exist != null) exist.Quantity += item.Quantity;
                else userCart.Add(item);
            }
            context.SaveCart(userCart);
            context.Session.Remove(GUEST_CART);
        }
    }
}
