using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using SV22T1020607.DataLayers.Interfaces;
using SV_22T1020607.Models.Security;

namespace SV22T1020607.DataLayers.SQLServerDAL
{
    public class CustomerAccountDAL : BaseDAL, IUserAccountRepository
    {
        public CustomerAccountDAL(string connectionString) : base(connectionString)
        {
        }

        public async Task<UserAccount?> AuthorizeAsync(string userName, string password)
        {
            UserAccount? data = null;
            using (var connection = GetConnection())
            {
                var sql = @"
                    select  CustomerID as UserId,
                            Email as UserName,
                            CustomerName as DisplayName,
                            Email,
                            '' as Photo,
                            'customer' as RoleNames
                    from    Customers
                    where   Email = @Email and Password = @Password and (IsLocked = 0 or IsLocked is null)
                ";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = System.Data.CommandType.Text;
                    command.Parameters.AddWithValue("@Email", userName);
                    command.Parameters.AddWithValue("@Password", password);

                    using (var reader = await command.ExecuteReaderAsync(System.Data.CommandBehavior.CloseConnection))
                    {
                        if (await reader.ReadAsync())
                        {
                            data = new UserAccount()
                            {
                                UserId = Convert.ToString(reader["UserId"]) ?? "",
                                UserName = Convert.ToString(reader["UserName"]) ?? "",
                                DisplayName = Convert.ToString(reader["DisplayName"]) ?? "",
                                Email = Convert.ToString(reader["Email"]) ?? "",
                                Photo = Convert.ToString(reader["Photo"]) ?? "",
                                RoleNames = Convert.ToString(reader["RoleNames"]) ?? ""
                            };
                        }
                    }
                }
            }
            return data;
        }

        public async Task<bool> ChangePassword(string userName, string password)
        {
            bool result = false;
            using (var connection = GetConnection())
            {
                var sql = @"
                    update  Customers
                    set     Password = @Password
                    where   Email = @Email and (IsLocked = 0 or IsLocked is null)
                ";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = System.Data.CommandType.Text;
                    command.Parameters.AddWithValue("@Email", userName);
                    command.Parameters.AddWithValue("@Password", password);
                    result = await command.ExecuteNonQueryAsync() > 0;
                }
                connection.Close();
            }
            return result;
        }

        /// <summary>
        /// Xử lý đăng ký cho Customer. (Đây là hàm phụ, không có trong Interface)
        /// </summary>
        public async Task<bool> RegisterAsync(string customerName, string contactName, string email, string phone, string address, string province, string password)
        {
            bool result = false;
            using (var connection = GetConnection())
            {
                var sql = @"if not exists(select * from Customers where Email = @Email)
                            begin
                                insert into Customers(CustomerName, ContactName, Province, Address, Phone, Email, Password, IsLocked)
                                values(@CustomerName, @ContactName, @Province, @Address, @Phone, @Email, @Password, 0);
                                select 1;
                            end
                            else select 0;";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = System.Data.CommandType.Text;
                    command.Parameters.AddWithValue("@CustomerName", customerName ?? "");
                    command.Parameters.AddWithValue("@ContactName", contactName ?? "");
                    command.Parameters.AddWithValue("@Province", province ?? "");
                    command.Parameters.AddWithValue("@Address", address ?? "");
                    command.Parameters.AddWithValue("@Phone", phone ?? "");
                    command.Parameters.AddWithValue("@Email", email ?? "");
                    command.Parameters.AddWithValue("@Password", password ?? "");

                    result = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
                }
                connection.Close();
            }
            return result;
        }
    }
}
