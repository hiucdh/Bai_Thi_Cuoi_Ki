namespace SV22T1020607.Shop.Models
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = "";
        public string Photo { get; set; } = "";
        public int Quantity { get; set; }
        public decimal SalePrice { get; set; }

        public decimal TotalPrice => Quantity * SalePrice;
    }
}
