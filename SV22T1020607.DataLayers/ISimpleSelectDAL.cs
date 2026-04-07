using System.Collections.Generic;

namespace SV22T1020607.DataLayers
{
    /// <summary>
    /// Định nghĩa phép lấy dữ liệu đơn giản (chỉ lấy danh sách)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISimpleSelectDAL<T> where T : class
    {
        /// <summary>
        /// Lấy toàn bộ danh sách dữ liệu
        /// </summary>
        /// <returns></returns>
        IList<T> List();
    }
}
