using System.Collections.Generic;

namespace SV22T1020607.DataLayers
{
    /// <summary>
    /// Định nghĩa các phép dữ liệu chung cho các bảng đơn giản
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICommonDAL<T> where T : class
    {
        /// <summary>
        /// Tìm kiếm và lấy danh sách dữ liệu dưới dạng phân trang
        /// </summary>
        /// <param name="page">Trang cần hiển thị</param>
        /// <param name="pageSize">Số dòng hiển thị trên mỗi trang (bằng 0 nếu không phân trang)</param>
        /// <param name="searchValue">Giá trị cần tìm kiếm (chuỗi rỗng nếu lấy toàn bộ dữ liệu)</param>
        /// <returns></returns>
        IList<T> List(int page = 1, int pageSize = 0, string searchValue = "");
        /// <summary>
        /// Đếm số lượng dòng dữ liệu tìm được
        /// </summary>
        /// <param name="searchValue">Giá trị cần tìm kiếm (chuỗi rỗng nếu lấy toàn bộ dữ liệu)</param>
        /// <returns></returns>
        int Count(string searchValue = "");
        /// <summary>
        /// Lấy một dòng dữ liệu dựa vào mã (ID)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        T? Get(int id);
        /// <summary>
        /// Bổ sung dữ liệu vào bảng. Hàm trả về mã của dữ liệu vừa được bổ sung
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        int Add(T data);
        /// <summary>
        /// Cập nhật dữ liệu
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        bool Update(T data);
        /// <summary>
        /// Xóa dữ liệu dựa vào mã
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool Delete(int id);
        /// <summary>
        /// Kiểm tra xem một dòng dữ liệu hiện có đang được sử dụng bởi các bảng khác hay không?
        /// (Có dữ liệu liên quan hay không?)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool InUsed(int id);
    }
}
