using SV22T1020607.DataLayers.Interfaces;
using SV22T1020607.DataLayers.SQLServerDAL;
using SV_22T1020607.Models.Common;
using System.Threading.Tasks;

namespace SV22T1020607.BusinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến từ điển dữ liệu
    /// </summary>
    public static class DictionaryDataService
    {
        private static IDataDictionaryRepository<Province> provinceDB = null!;

        /// <summary>
        /// Khởi tạo
        /// </summary>
        public static void Initialize()
        {
            provinceDB = new ProvinceDAL(Configuration.ConnectionString);
        }
        /// <summary>
        /// Lấy danh sách tỉnh thành
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Province>> ListProvincesAsync()
        {
            return await provinceDB.ListAsync();
        }

        public static List<Province> ListOfProvinces()
        {
            return ListProvincesAsync().Result;
        }
    }
}
