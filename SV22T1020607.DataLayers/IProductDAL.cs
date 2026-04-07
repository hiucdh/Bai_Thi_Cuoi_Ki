using SV_22T1020607.Models.Catalog;
using System.Collections.Generic;

namespace SV22T1020607.DataLayers
{
    /// <summary>
    /// Định nghĩa các nghiệp vụ liên quan đến dữ liệu mặt hàng
    /// </summary>
    public interface IProductDAL
    {
        /// <summary>
        /// Tìm kiếm và lấy danh sách mặt hàng dưới dạng phân trang
        /// </summary>
        IList<Product> List(int page = 1, int pageSize = 0, string searchValue = "", int categoryID = 0, int supplierID = 0, decimal minPrice = 0, decimal maxPrice = 0);
        
        /// <summary>
        /// Đếm số lượng mặt hàng tìm được
        /// </summary>
        int Count(string searchValue = "", int categoryID = 0, int supplierID = 0, decimal minPrice = 0, decimal maxPrice = 0);
        
        /// <summary>
        /// Lấy thông tin mặt hàng theo mã
        /// </summary>
        Product? Get(int productID);
        
        /// <summary>
        /// Bổ sung mặt hàng mới (hàm trả về mã của mặt hàng được bổ sung)
        /// </summary>
        int Add(Product data);
        
        /// <summary>
        /// Cập nhật thông tin mặt hàng
        /// </summary>
        bool Update(Product data);
        
        /// <summary>
        /// Xóa mặt hàng
        /// </summary>
        bool Delete(int productID);
        
        /// <summary>
        /// Kiểm tra xem mặt hàng hiện có dữ liệu liên quan hay không?
        /// </summary>
        bool InUsed(int productID);
        
        /// <summary>
        /// Lấy danh sách ảnh của mặt hàng (sắp xếp theo thứ tự hiển thị)
        /// </summary>
        IList<ProductPhoto> ListPhotos(int productID);
        
        /// <summary>
        /// Lấy thông tin 1 ảnh dựa vào mã ảnh
        /// </summary>
        ProductPhoto? GetPhoto(long photoID);
        
        /// <summary>
        /// Bổ sung ảnh cho mặt hàng
        /// </summary>
        long AddPhoto(ProductPhoto data);
        
        /// <summary>
        /// Cập nhật ảnh của mặt hàng
        /// </summary>
        bool UpdatePhoto(ProductPhoto data);
        
        /// <summary>
        /// Xóa ảnh của mặt hàng
        /// </summary>
        bool DeletePhoto(long photoID);
        
        /// <summary>
        /// Lấy danh sách các thuộc tính của mặt hàng (sắp xếp theo thứ tự hiển thị)
        /// </summary>
        IList<ProductAttribute> ListAttributes(int productID);
        
        /// <summary>
        /// Lấy thông tin của 1 thuộc tính dựa vào mã thuộc tính
        /// </summary>
        ProductAttribute? GetAttribute(long attributeID);
        
        /// <summary>
        /// Bổ sung thuộc tính cho mặt hàng
        /// </summary>
        long AddAttribute(ProductAttribute data);
        
        /// <summary>
        /// Cập nhật thuộc tính của mặt hàng
        /// </summary>
        bool UpdateAttribute(ProductAttribute data);
        
        /// <summary>
        /// Xóa thuộc tính
        /// </summary>
        bool DeleteAttribute(long attributeID);
    }
}
