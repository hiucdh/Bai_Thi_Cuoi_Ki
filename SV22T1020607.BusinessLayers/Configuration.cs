namespace SV22T1020607.BusinessLayers
{
    /// <summary>
    /// Khởi tạo và cấu hình các dịch vụ của Business Layer
    /// </summary>
    public static class Configuration
    {
        /// <summary>
        /// Chuỗi tham số kết nối đến cơ sở dữ liệu
        /// </summary>
        public static string ConnectionString { get; private set; } = "";

        /// <summary>
        /// Khởi tạo toàn bộ các dịch vụ trong Business Layer
        /// </summary>
        /// <param name="connectionString"></param>
        public static void Initialize(string connectionString)
        {
            ConnectionString = connectionString;

            CommonDataService.Initialize();
            ProductDataService.Initialize();
            DictionaryDataService.Initialize();
            PartnerDataService.Initialize();
            CatalogDataService.Initialize();
            HRDataService.Initialize();
            SalesDataService.Initialize();
            UserAccountService.Initialize(connectionString);
        }
    }
}
