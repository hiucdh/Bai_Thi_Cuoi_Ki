using SV22T1020607.DataLayers.Interfaces;
using SV_22T1020607.Models.Catalog;
using SV_22T1020607.Models.Common;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace SV22T1020607.DataLayers.SQLServerDAL
{
    public class CategoryDAL : BaseDAL, ICommonDAL<Category>, IGenericRepository<Category>
    {
        public CategoryDAL(string connectionString) : base(connectionString)
        {
        }

        public int Add(Category data)
        {
            return AddAsync(data).GetAwaiter().GetResult();
        }

        public async Task<int> AddAsync(Category data)
        {
            int id = 0;
            using (var connection = GetConnection())
            {
                var sql = @"if exists(select * from Categories where CategoryName = @CategoryName)
                                select -1
                            else
                                begin
                                    insert into Categories(CategoryName, Description)
                                    values(@CategoryName, @Description);
                                    select @@identity;
                                end";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@CategoryName", data.CategoryName ?? "");
                    command.Parameters.AddWithValue("@Description", data.Description ?? "");

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
                var sql = @"select count(*) from Categories 
                            where (CategoryName like @searchValue) or (Description like @searchValue)";
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
                var sql = @"delete from Categories where CategoryID = @CategoryID";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@CategoryID", id);
                    result = await command.ExecuteNonQueryAsync() > 0;
                }
                connection.Close();
            }
            return result;
        }

        public Category? Get(int id)
        {
            return GetAsync(id).GetAwaiter().GetResult();
        }

        public async Task<Category?> GetAsync(int id)
        {
            Category? data = null;
            using (var connection = GetConnection())
            {
                var sql = @"select * from Categories where CategoryID = @CategoryID";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@CategoryID", id);
                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (await reader.ReadAsync())
                        {
                            data = new Category()
                            {
                                CategoryID = Convert.ToInt32(reader["CategoryID"]),
                                CategoryName = Convert.ToString(reader["CategoryName"]) ?? "",
                                Description = Convert.ToString(reader["Description"]) ?? ""
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
                var sql = @"if exists(select * from Products where CategoryID = @CategoryID)
                                select 1
                            else 
                                select 0";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@CategoryID", id);
                    result = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
                }
                connection.Close();
            }
            return result;
        }

        public IList<Category> List(int page = 1, int pageSize = 0, string searchValue = "")
        {
            List<Category> data = new List<Category>();
            searchValue = $"%{searchValue}%";
            using (var connection = GetConnection())
            {
                var sql = @"with cte as
                            (
                                select	*, row_number() over (order by CategoryName) as RowNumber
                                from	Categories
                                where	(CategoryName like @searchValue) or (Description like @searchValue)
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
                            data.Add(new Category()
                            {
                                CategoryID = Convert.ToInt32(reader["CategoryID"]),
                                CategoryName = Convert.ToString(reader["CategoryName"]) ?? "",
                                Description = Convert.ToString(reader["Description"]) ?? ""
                            });
                        }
                    }
                }
                connection.Close();
            }
            return data;
        }

        public async Task<PagedResult<Category>> ListAsync(PaginationSearchInput input)
        {
            int rowCount = Count(input.SearchValue);
            var data = List(input.Page, input.PageSize, input.SearchValue) as List<Category> ?? new List<Category>();
            return await Task.FromResult(new PagedResult<Category>
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data
            });
        }

        public bool Update(Category data)
        {
            return UpdateAsync(data).GetAwaiter().GetResult();
        }

        public async Task<bool> UpdateAsync(Category data)
        {
            bool result = false;
            using (var connection = GetConnection())
            {
                var sql = @"if exists(select * from Categories where CategoryID <> @CategoryID and CategoryName = @CategoryName)
                                select 0
                            else
                                begin
                                    update Categories 
                                    set CategoryName = @CategoryName,
                                        Description = @Description
                                    where CategoryID = @CategoryID;
                                    select 1;
                                end";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@CategoryID", data.CategoryID);
                    command.Parameters.AddWithValue("@CategoryName", data.CategoryName ?? "");
                    command.Parameters.AddWithValue("@Description", data.Description ?? "");

                    result = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
                }
                connection.Close();
            }
            return result;
        }
    }
}
