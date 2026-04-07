using System.Collections.Generic;

namespace SV_22T1020607.Models.Common
{
    /// <summary>
    /// Lớp cơ sở dùng để biểu diễn kết quả của một truy vấn/tìm kiếm dữ liệu dưới dạng phân trang
    /// </summary>
    public abstract class PaginationSearchResult
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 0;
        public string SearchValue { get; set; } = "";
        public int RowCount { get; set; } = 0;
        public int PageCount
        {
            get
            {
                if (PageSize <= 0) return 1;
                int n = RowCount / PageSize;
                if (RowCount % PageSize > 0) n++;
                return n;
            }
        }
    }

    /// <summary>
    /// Kết quả tìm kiếm dữ liệu dưới dạng phân trang (có kèm theo danh sách dữ liệu)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PaginationSearchResult<T> : PaginationSearchResult where T : class
    {
        public IList<T> Data { get; set; } = new List<T>();
    }
}
