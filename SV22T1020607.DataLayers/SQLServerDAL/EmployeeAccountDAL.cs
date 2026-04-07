using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using SV22T1020607.DataLayers.Interfaces;
using SV_22T1020607.Models.Security;

namespace SV22T1020607.DataLayers.SQLServerDAL
{
    public class EmployeeAccountDAL : BaseDAL, IUserAccountRepository
    {
        public EmployeeAccountDAL(string connectionString) : base(connectionString)
        {
        }

        public async Task<UserAccount?> AuthorizeAsync(string userName, string password)
        {
            UserAccount? data = null;
            using (var connection = GetConnection())
            {
                var sql = @"
                    select  EmployeeID as UserId,
                            Email as UserName,
                            FullName as DisplayName,
                            Email,
                            Photo,
                            RoleNames
                    from    Employees
                    where   Email = @Email and Password = @Password and IsWorking = 1
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
                    update  Employees
                    set     Password = @Password
                    where   Email = @Email and IsWorking = 1
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
    }
}
