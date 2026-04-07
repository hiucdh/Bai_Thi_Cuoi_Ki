using SV22T1020607.Admin.Models;

namespace SV22T1020607.Admin.AppCodes
{
    public static class ShoppingCartService
    {
        private const string SHOPPING_CART = "ShoppingCart";

        public static List<CartItem> GetShoppingCart()
        {
            var cart = ApplicationContext.GetSessionData<List<CartItem>>(SHOPPING_CART);
            if (cart == null)
            {
                cart = new List<CartItem>();
                ApplicationContext.SetSessionData(SHOPPING_CART, cart);
            }
            return cart;
        }

        public static void AddToCart(CartItem item)
        {
            var cart = GetShoppingCart();
            var exists = cart.FirstOrDefault(x => x.ProductID == item.ProductID);
            if (exists != null)
            {
                exists.Quantity += item.Quantity;
                exists.SalePrice = item.SalePrice;
            }
            else
            {
                cart.Add(item);
            }
            ApplicationContext.SetSessionData(SHOPPING_CART, cart);
        }

        public static void RemoveFromCart(int productID)
        {
            var cart = GetShoppingCart();
            var exists = cart.FirstOrDefault(x => x.ProductID == productID);
            if (exists != null)
            {
                cart.Remove(exists);
                ApplicationContext.SetSessionData(SHOPPING_CART, cart);
            }
        }

        public static void ClearCart()
        {
            var cart = GetShoppingCart();
            cart.Clear();
            ApplicationContext.SetSessionData(SHOPPING_CART, cart);
        }
    }
}
