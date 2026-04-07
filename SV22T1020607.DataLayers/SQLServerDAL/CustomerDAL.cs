using SV22T1020607.DataLayers.Interfaces;
using SV_22T1020607.Models.Partner;
using SV_22T1020607.Models.Common;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace SV22T1020607.DataLayers.SQLServerDAL
{
    public class CustomerDAL : BaseDAL, ICommonDAL<Customer>, ICustomerRepository
    {
        public CustomerDAL(string connectionString) : base(connectionString)
        {
        }

        public int Add(Customer data)
        {
            return AddAsync(data).GetAwaiter().GetResult();
        }

        public async Task<int> AddAsync(Customer data)
        {
            int id = 0;
            using (var connection = GetConnection())
            {
                var sql = @"if exists(select * from Customers where Email = @Email)
                                select -1
                            else
                                begin
                                    insert into Customers(CustomerName,ContactName,Province,Address,Phone,Email,IsLocked)
                                    values(@CustomerName,@ContactName,@Province,@Address,@Phone,@Email,@IsLocked);
                                    select @@identity;
                                end";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@CustomerName", data.CustomerName ?? "");
                    command.Parameters.AddWithValue("@ContactName", data.ContactName ?? "");
                    command.Parameters.AddWithValue("@Province", data.Province ?? "");
                    command.Parameters.AddWithValue("@Address", data.Address ?? "");
                    command.Parameters.AddWithValue("@Phone", data.Phone ?? "");
                    command.Parameters.AddWithValue("@Email", data.Email ?? "");
                    command.Parameters.AddWithValue("@IsLocked", data.IsLocked ?? false);

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
                var sql = @"select count(*) from Customers 
                            where (CustomerName like @searchValue) or (ContactName like @searchValue)";
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
                var sql = @"delete from Customers where CustomerID = @CustomerID";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@CustomerID", id);
                    result = await command.ExecuteNonQueryAsync() > 0;
                }
                connection.Close();
            }
            return result;
        }

        public Customer? Get(int id)
        {
            return GetAsync(id).GetAwaiter().GetResult();
        }

        public async Task<Customer?> GetAsync(int id)
        {
            Customer? data = null;
            using (var connection = GetConnection())
            {
                var sql = @"select * from Customers where CustomerID = @CustomerID";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@CustomerID", id);
                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (await reader.ReadAsync())
                        {
                            data = new Customer()
                            {
                                CustomerID = Convert.ToInt32(reader["CustomerID"]),
                                CustomerName = Convert.ToString(reader["CustomerName"]) ?? "",
                                ContactName = Convert.ToString(reader["ContactName"]) ?? "",
                                Province = Convert.ToString(reader["Province"]) ?? "",
                                Address = Convert.ToString(reader["Address"]) ?? "",
                                Phone = Convert.ToString(reader["Phone"]) ?? "",
                                Email = Convert.ToString(reader["Email"]) ?? "",
                                IsLocked = Convert.ToBoolean(reader["IsLocked"])
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
                var sql = @"if exists(select * from Orders where CustomerID = @CustomerID)
                                select 1
                            else 
                                select 0";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@CustomerID", id);
                    result = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
                }
                connection.Close();
            }
            return result;
        }

        public IList<Customer> List(int page = 1, int pageSize = 0, string searchValue = "")
        {
            List<Customer> data = new List<Customer>();
            searchValue = $"%{searchValue}%";
            using (var connection = GetConnection())
            {
                var sql = @"with cte as
                            (
                                select	*, row_number() over (order by CustomerName) as RowNumber
                                from	Customers
                                where	(CustomerName like @searchValue) or (ContactName like @searchValue)
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
                            data.Add(new Customer()
                            {
                                CustomerID = Convert.ToInt32(reader["CustomerID"]),
                                CustomerName = Convert.ToString(reader["CustomerName"]) ?? "",
                                ContactName = Convert.ToString(reader["ContactName"]) ?? "",
                                Province = Convert.ToString(reader["Province"]) ?? "",
                                Address = Convert.ToString(reader["Address"]) ?? "",
                                Phone = Convert.ToString(reader["Phone"]) ?? "",
                                Email = Convert.ToString(reader["Email"]) ?? "",
                                IsLocked = Convert.ToBoolean(reader["IsLocked"])
                            });
                        }
                    }
                }
                connection.Close();
            }
            return data;
        }

        public async Task<PagedResult<Customer>> ListAsync(PaginationSearchInput input)
        {
            int rowCount = Count(input.SearchValue);
            var data = List(input.Page, input.PageSize, input.SearchValue) as List<Customer> ?? new List<Customer>();
            return await Task.FromResult(new PagedResult<Customer>
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data
            });
        }

        public bool Update(Customer data)
        {
            return UpdateAsync(data).GetAwaiter().GetResult();
        }

        public async Task<bool> UpdateAsync(Customer data)
        {
            bool result = false;
            using (var connection = GetConnection())
            {
                var sql = @"if exists(select * from Customers where CustomerID <> @CustomerID and Email = @Email)
                                select 0
                            else
                                begin
                                    update Customers 
                                    set CustomerName = @CustomerName,
                                        ContactName = @ContactName,
                                        Province = @Province,
                                        Address = @Address,
                                        Phone = @Phone,
                                        Email = @Email,
                                        IsLocked = @IsLocked
                                    where CustomerID = @CustomerID;
                                    select 1;
                                end";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@CustomerID", data.CustomerID);
                    command.Parameters.AddWithValue("@CustomerName", data.CustomerName ?? "");
                    command.Parameters.AddWithValue("@ContactName", data.ContactName ?? "");
                    command.Parameters.AddWithValue("@Province", data.Province ?? "");
                    command.Parameters.AddWithValue("@Address", data.Address ?? "");
                    command.Parameters.AddWithValue("@Phone", data.Phone ?? "");
                    command.Parameters.AddWithValue("@Email", data.Email ?? "");
                    command.Parameters.AddWithValue("@IsLocked", data.IsLocked ?? false);

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
                var sql = @"if exists(select * from Customers where CustomerID <> @CustomerID and Email = @Email)
                                select 0
                            else
                                select 1";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@CustomerID", id);
                    command.Parameters.AddWithValue("@Email", email);
                    result = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
                }
                connection.Close();
            }
            return result;
        }
    }
}
