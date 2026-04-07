namespace SV22T1020607.DataLayers.SQLServerDAL
{
    /// <summary>
    /// Lớp cơ sở cho các lớp cài đặt các phép dữ liệu trên SQL Server
    /// </summary>
    public abstract class BaseDAL
    {
        /// <summary>
        /// Chuỗi tham số kết nối đến CSDL SQL Server
        /// </summary>
        protected string _connectionString = "";
        /// <summary>
        /// Cấu trúc khởi tạo
        /// </summary>
        /// <param name="connectionString"></param>
        public BaseDAL(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Tạo và mở kết nối đến CSDL SQL Server
        /// </summary>
        /// <returns></returns>
        protected Microsoft.Data.SqlClient.SqlConnection GetConnection()
        {
            var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }
}
