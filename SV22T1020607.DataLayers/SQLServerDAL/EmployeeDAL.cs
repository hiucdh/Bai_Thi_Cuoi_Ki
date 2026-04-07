using SV22T1020607.DataLayers.Interfaces;
using SV_22T1020607.Models.HR;
using SV_22T1020607.Models.Common;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace SV22T1020607.DataLayers.SQLServerDAL
{
    public class EmployeeDAL : BaseDAL, ICommonDAL<Employee>, IEmployeeRepository
    {
        public EmployeeDAL(string connectionString) : base(connectionString)
        {
        }

        public int Add(Employee data)
        {
            return AddAsync(data).GetAwaiter().GetResult();
        }

        public async Task<int> AddAsync(Employee data)
        {
            int id = 0;
            using (var connection = GetConnection())
            {
                var sql = @"if exists(select * from Employees where Email = @Email)
                                select -1
                            else
                                begin
                                    insert into Employees(FullName,BirthDate,Address,Phone,Email,Photo,IsWorking)
                                    values(@FullName,@BirthDate,@Address,@Phone,@Email,@Photo,@IsWorking);
                                    select @@identity;
                                end";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@FullName", data.FullName ?? "");
                    command.Parameters.AddWithValue("@BirthDate", (object?)data.BirthDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Address", data.Address ?? "");
                    command.Parameters.AddWithValue("@Phone", data.Phone ?? "");
                    command.Parameters.AddWithValue("@Email", data.Email ?? "");
                    command.Parameters.AddWithValue("@Photo", data.Photo ?? "");
                    command.Parameters.AddWithValue("@IsWorking", data.IsWorking ?? true);

                    id = Convert.ToInt32(await command.ExecuteScalarAsync());
                }
                connection.Close();
            }
            return id;
        }

        public int Count(string searchValue = "")
        {
            int count = 0;
            searchValue = $"%{searchValue}%";
            using (var connection = GetConnection())
            {
                var sql = @"select count(*) from Employees 
                            where (FullName like @searchValue)";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@searchValue", searchValue);
                    count = Convert.ToInt32(command.ExecuteScalar());
                }
                connection.Close();
            }
            return count;
        }

        public bool Delete(int id)
        {
            return DeleteAsync(id).GetAwaiter().GetResult();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            bool result = false;
            using (var connection = GetConnection())
            {
                var sql = @"delete from Employees where EmployeeID = @EmployeeID";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@EmployeeID", id);
                    result = await command.ExecuteNonQueryAsync() > 0;
                }
                connection.Close();
            }
            return result;
        }

        public Employee? Get(int id)
        {
            return GetAsync(id).GetAwaiter().GetResult();
        }

        public async Task<Employee?> GetAsync(int id)
        {
            Employee? data = null;
            using (var connection = GetConnection())
            {
                var sql = @"select * from Employees where EmployeeID = @EmployeeID";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@EmployeeID", id);
                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (await reader.ReadAsync())
                        {
                            data = new Employee()
                            {
                                EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                                FullName = Convert.ToString(reader["FullName"]) ?? "",
                                BirthDate = reader["BirthDate"] != DBNull.Value ? Convert.ToDateTime(reader["BirthDate"]) : (DateTime?)null,
                                Address = Convert.ToString(reader["Address"]) ?? "",
                                Phone = Convert.ToString(reader["Phone"]) ?? "",
                                Email = Convert.ToString(reader["Email"]) ?? "",
                                Photo = Convert.ToString(reader["Photo"]) ?? "",
                                IsWorking = Convert.ToBoolean(reader["IsWorking"])
                            };
                        }
                    }
                }
                connection.Close();
            }
            return data;
        }

        public bool InUsed(int id)
        {
            return IsUsedAsync(id).GetAwaiter().GetResult();
        }

        public async Task<bool> IsUsedAsync(int id)
        {
            bool result = false;
            using (var connection = GetConnection())
            {
                var sql = @"if exists(select * from Orders where EmployeeID = @EmployeeID)
                                select 1
                            else 
                                select 0";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@EmployeeID", id);
                    result = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
                }
                connection.Close();
            }
            return result;
        }

        public IList<Employee> List(int page = 1, int pageSize = 0, string searchValue = "")
        {
            List<Employee> data = new List<Employee>();
            searchValue = $"%{searchValue}%";
            using (var connection = GetConnection())
            {
                var sql = @"with cte as
                            (
                                select	*, row_number() over (order by FullName) as RowNumber
                                from	Employees
                                where	(FullName like @searchValue)
                            )
                            select * from cte
                            where (@pageSize = 0) 
                                or (RowNumber between (@page - 1) * @pageSize + 1 and @page * @pageSize)
                            order by RowNumber";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@page", page);
                    command.Parameters.AddWithValue("@pageSize", pageSize);
                    command.Parameters.AddWithValue("@searchValue", searchValue);

                    using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            data.Add(new Employee()
                            {
                                EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                                FullName = Convert.ToString(reader["FullName"]) ?? "",
                                BirthDate = reader["BirthDate"] != DBNull.Value ? Convert.ToDateTime(reader["BirthDate"]) : (DateTime?)null,
                                Address = Convert.ToString(reader["Address"]) ?? "",
                                Phone = Convert.ToString(reader["Phone"]) ?? "",
                                Email = Convert.ToString(reader["Email"]) ?? "",
                                Photo = Convert.ToString(reader["Photo"]) ?? "",
                                IsWorking = Convert.ToBoolean(reader["IsWorking"])
                            });
                        }
                    }
                }
                connection.Close();
            }
            return data;
        }

        public async Task<PagedResult<Employee>> ListAsync(PaginationSearchInput input)
        {
            int rowCount = Count(input.SearchValue);
            var data = List(input.Page, input.PageSize, input.SearchValue) as List<Employee> ?? new List<Employee>();
            return await Task.FromResult(new PagedResult<Employee>
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data
            });
        }

        public bool Update(Employee data)
        {
            return UpdateAsync(data).GetAwaiter().GetResult();
        }

        public async Task<bool> UpdateAsync(Employee data)
        {
            bool result = false;
            using (var connection = GetConnection())
            {
                var sql = @"if exists(select * from Employees where EmployeeID <> @EmployeeID and Email = @Email)
                                select 0
                            else
                                begin
                                    update Employees 
                                    set FullName = @FullName,
                                        BirthDate = @BirthDate,
                                        Address = @Address,
                                        Phone = @Phone,
                                        Email = @Email,
                                        Photo = @Photo,
                                        IsWorking = @IsWorking
                                    where EmployeeID = @EmployeeID;
                                    select 1;
                                end";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@EmployeeID", data.EmployeeID);
                    command.Parameters.AddWithValue("@FullName", data.FullName ?? "");
                    command.Parameters.AddWithValue("@BirthDate", (object?)data.BirthDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Address", data.Address ?? "");
                    command.Parameters.AddWithValue("@Phone", data.Phone ?? "");
                    command.Parameters.AddWithValue("@Email", data.Email ?? "");
                    command.Parameters.AddWithValue("@Photo", data.Photo ?? "");
                    command.Parameters.AddWithValue("@IsWorking", data.IsWorking ?? true);

                    result = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
                }
                connection.Close();
            }
            return result;
        }

        public async Task<bool> ValidateEmailAsync(string email, int id = 0)
        {
            bool result = false;
            using (var connection = GetConnection())
            {
                var sql = @"if exists(select * from Employees where EmployeeID <> @EmployeeID and Email = @Email)
                                select 0
                            else
                                select 1";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@EmployeeID", id);
                    command.Parameters.AddWithValue("@Email", email);
                    result = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
                }
                connection.Close();
            }
            return result;
        }
    }
}
