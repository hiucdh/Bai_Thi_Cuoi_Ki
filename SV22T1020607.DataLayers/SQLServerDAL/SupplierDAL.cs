using SV22T1020607.DataLayers.Interfaces;
using SV_22T1020607.Models.Partner;
using SV_22T1020607.Models.Common;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace SV22T1020607.DataLayers.SQLServerDAL
{
    public class SupplierDAL : BaseDAL, ICommonDAL<Supplier>, IGenericRepository<Supplier>
    {
        public SupplierDAL(string connectionString) : base(connectionString)
        {
        }

        public int Add(Supplier data)
        {
            return AddAsync(data).GetAwaiter().GetResult();
        }

        public async Task<int> AddAsync(Supplier data)
        {
            int id = 0;
            using (var connection = GetConnection())
            {
                var sql = @"if exists(select * from Suppliers where Email = @Email)
                                select -1
                            else
                                begin
                                    insert into Suppliers(SupplierName,ContactName,Province,Address,Phone,Email)
                                    values(@SupplierName,@ContactName,@Province,@Address,@Phone,@Email);
                                    select @@identity;
                                end";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@SupplierName", data.SupplierName ?? "");
                    command.Parameters.AddWithValue("@ContactName", data.ContactName ?? "");
                    command.Parameters.AddWithValue("@Province", data.Province ?? "");
                    command.Parameters.AddWithValue("@Address", data.Address ?? "");
                    command.Parameters.AddWithValue("@Phone", data.Phone ?? "");
                    command.Parameters.AddWithValue("@Email", data.Email ?? "");

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
                var sql = @"select count(*) from Suppliers 
                            where (SupplierName like @searchValue) or (ContactName like @searchValue)";
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
                var sql = @"delete from Suppliers where SupplierID = @SupplierID";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@SupplierID", id);
                    result = await command.ExecuteNonQueryAsync() > 0;
                }
                connection.Close();
            }
            return result;
        }

        public Supplier? Get(int id)
        {
            return GetAsync(id).GetAwaiter().GetResult();
        }

        public async Task<Supplier?> GetAsync(int id)
        {
            Supplier? data = null;
            using (var connection = GetConnection())
            {
                var sql = @"select * from Suppliers where SupplierID = @SupplierID";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@SupplierID", id);
                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (await reader.ReadAsync())
                        {
                            data = new Supplier()
                            {
                                SupplierID = Convert.ToInt32(reader["SupplierID"]),
                                SupplierName = Convert.ToString(reader["SupplierName"]) ?? "",
                                ContactName = Convert.ToString(reader["ContactName"]) ?? "",
                                Province = Convert.ToString(reader["Province"]) ?? "",
                                Address = Convert.ToString(reader["Address"]) ?? "",
                                Phone = Convert.ToString(reader["Phone"]) ?? "",
                                Email = Convert.ToString(reader["Email"]) ?? ""
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
                var sql = @"if exists(select * from Products where SupplierID = @SupplierID)
                                select 1
                            else 
                                select 0";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@SupplierID", id);
                    result = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
                }
                connection.Close();
            }
            return result;
        }

        public IList<Supplier> List(int page = 1, int pageSize = 0, string searchValue = "")
        {
            List<Supplier> data = new List<Supplier>();
            searchValue = $"%{searchValue}%";
            using (var connection = GetConnection())
            {
                var sql = @"with cte as
                            (
                                select	*, row_number() over (order by SupplierName) as RowNumber
                                from	Suppliers
                                where	(SupplierName like @searchValue) or (ContactName like @searchValue)
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
                            data.Add(new Supplier()
                            {
                                SupplierID = Convert.ToInt32(reader["SupplierID"]),
                                SupplierName = Convert.ToString(reader["SupplierName"]) ?? "",
                                ContactName = Convert.ToString(reader["ContactName"]) ?? "",
                                Province = Convert.ToString(reader["Province"]) ?? "",
                                Address = Convert.ToString(reader["Address"]) ?? "",
                                Phone = Convert.ToString(reader["Phone"]) ?? "",
                                Email = Convert.ToString(reader["Email"]) ?? ""
                            });
                        }
                    }
                }
                connection.Close();
            }
            return data;
        }

        public async Task<PagedResult<Supplier>> ListAsync(PaginationSearchInput input)
        {
            int rowCount = Count(input.SearchValue);
            var data = List(input.Page, input.PageSize, input.SearchValue) as List<Supplier> ?? new List<Supplier>();
            return await Task.FromResult(new PagedResult<Supplier>
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data
            });
        }

        public bool Update(Supplier data)
        {
            return UpdateAsync(data).GetAwaiter().GetResult();
        }

        public async Task<bool> UpdateAsync(Supplier data)
        {
            bool result = false;
            using (var connection = GetConnection())
            {
                var sql = @"if exists(select * from Suppliers where SupplierID <> @SupplierID and Email = @Email)
                                select 0
                            else
                                begin
                                    update Suppliers 
                                    set SupplierName = @SupplierName,
                                        ContactName = @ContactName,
                                        Province = @Province,
                                        Address = @Address,
                                        Phone = @Phone,
                                        Email = @Email
                                    where SupplierID = @SupplierID;
                                    select 1;
                                end";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@SupplierID", data.SupplierID);
                    command.Parameters.AddWithValue("@SupplierName", data.SupplierName ?? "");
                    command.Parameters.AddWithValue("@ContactName", data.ContactName ?? "");
                    command.Parameters.AddWithValue("@Province", data.Province ?? "");
                    command.Parameters.AddWithValue("@Address", data.Address ?? "");
                    command.Parameters.AddWithValue("@Phone", data.Phone ?? "");
                    command.Parameters.AddWithValue("@Email", data.Email ?? "");

                    result = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
                }
                connection.Close();
            }
            return result;
        }
    }
}
