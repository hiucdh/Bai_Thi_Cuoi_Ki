using SV22T1020607.DataLayers;
using SV22T1020607.DataLayers.SQLServerDAL;
using SV_22T1020607.Models.Catalog;
using System.Collections.Generic;

namespace SV22T1020607.BusinessLayers
{
    public static class ProductDataService
    {
        private static IProductDAL productDAL = null!;

        /// <summary>
        /// Khởi tạo dịch vụ (phải được gọi trước khi sử dụng các chức năng khác)
        /// </summary>
        public static void Initialize()
        {
            productDAL = new ProductDAL(Configuration.ConnectionString);
        }

        #region Các nghiệp vụ liên quan đến Mặt hàng
        public static IList<Product> ListProducts(string searchValue = "")
        {
            return productDAL.List(1, 0, searchValue, 0, 0, 0, 0);
        }

        public static IList<Product> ListProducts(int page, int pageSize, string searchValue, int categoryID, int supplierID, decimal minPrice, decimal maxPrice)
        {
            return productDAL.List(page, pageSize, searchValue, categoryID, supplierID, minPrice, maxPrice);
        }

        public static int CountProducts(string searchValue, int categoryID, int supplierID, decimal minPrice, decimal maxPrice)
        {
            return productDAL.Count(searchValue, categoryID, supplierID, minPrice, maxPrice);
        }

        public static Product? GetProduct(int productID)
        {
            return productDAL.Get(productID);
        }

        public static int AddProduct(Product data)
        {
            return productDAL.Add(data);
        }

        public static bool UpdateProduct(Product data)
        {
            return productDAL.Update(data);
        }

        public static bool DeleteProduct(int productID)
        {
            return productDAL.Delete(productID);
        }

        public static bool InUsedProduct(int productID)
        {
            return productDAL.InUsed(productID);
        }
        #endregion

        #region Các nghiệp vụ liên quan đến Ảnh của mặt hàng
        public static IList<ProductPhoto> ListPhotos(int productID)
        {
            return productDAL.ListPhotos(productID);
        }

        public static ProductPhoto? GetPhoto(long photoID)
        {
            return productDAL.GetPhoto(photoID);
        }

        public static long AddPhoto(ProductPhoto data)
        {
            return productDAL.AddPhoto(data);
        }

        public static bool UpdatePhoto(ProductPhoto data)
        {
            return productDAL.UpdatePhoto(data);
        }

        public static bool DeletePhoto(long photoID)
        {
            return productDAL.DeletePhoto(photoID);
        }
        #endregion

        #region Các nghiệp vụ liên quan đến Thuộc tính của mặt hàng
        public static IList<ProductAttribute> ListAttributes(int productID)
        {
            return productDAL.ListAttributes(productID);
        }

        public static ProductAttribute? GetAttribute(long attributeID)
        {
            return productDAL.GetAttribute(attributeID);
        }

        public static long AddAttribute(ProductAttribute data)
        {
            return productDAL.AddAttribute(data);
        }

        public static bool UpdateAttribute(ProductAttribute data)
        {
            return productDAL.UpdateAttribute(data);
        }

        public static bool DeleteAttribute(long attributeID)
        {
            return productDAL.DeleteAttribute(attributeID);
        }
        #endregion
    }
}
