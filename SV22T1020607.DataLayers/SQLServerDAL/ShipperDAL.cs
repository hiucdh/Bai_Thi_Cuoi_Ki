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
    public class ShipperDAL : BaseDAL, ICommonDAL<Shipper>, IGenericRepository<Shipper>
    {
        public ShipperDAL(string connectionString) : base(connectionString)
        {
        }

        public int Add(Shipper data)
        {
            return AddAsync(data).GetAwaiter().GetResult();
        }

        public async Task<int> AddAsync(Shipper data)
        {
            int id = 0;
            using (var connection = GetConnection())
            {
                var sql = @"insert into Shippers(ShipperName,Phone)
                            values(@ShipperName,@Phone);
                            select @@identity;";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@ShipperName", data.ShipperName ?? "");
                    command.Parameters.AddWithValue("@Phone", data.Phone ?? "");

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
                var sql = @"select count(*) from Shippers 
                            where (ShipperName like @searchValue)";
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
                var sql = @"delete from Shippers where ShipperID = @ShipperID";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@ShipperID", id);
                    result = await command.ExecuteNonQueryAsync() > 0;
                }
                connection.Close();
            }
            return result;
        }

        public Shipper? Get(int id)
        {
            return GetAsync(id).GetAwaiter().GetResult();
        }

        public async Task<Shipper?> GetAsync(int id)
        {
            Shipper? data = null;
            using (var connection = GetConnection())
            {
                var sql = @"select * from Shippers where ShipperID = @ShipperID";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@ShipperID", id);
                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (await reader.ReadAsync())
                        {
                            data = new Shipper()
                            {
                                ShipperID = Convert.ToInt32(reader["ShipperID"]),
                                ShipperName = Convert.ToString(reader["ShipperName"]) ?? "",
                                Phone = Convert.ToString(reader["Phone"]) ?? ""
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
                var sql = @"if exists(select * from Orders where ShipperID = @ShipperID)
                                select 1
                            else 
                                select 0";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@ShipperID", id);
                    result = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
                }
                connection.Close();
            }
            return result;
        }

        public IList<Shipper> List(int page = 1, int pageSize = 0, string searchValue = "")
        {
            List<Shipper> data = new List<Shipper>();
            searchValue = $"%{searchValue}%";
            using (var connection = GetConnection())
            {
                var sql = @"with cte as
                            (
                                select	*, row_number() over (order by ShipperName) as RowNumber
                                from	Shippers
                                where	(ShipperName like @searchValue)
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
                            data.Add(new Shipper()
                            {
                                ShipperID = Convert.ToInt32(reader["ShipperID"]),
                                ShipperName = Convert.ToString(reader["ShipperName"]) ?? "",
                                Phone = Convert.ToString(reader["Phone"]) ?? ""
                            });
                        }
                    }
                }
                connection.Close();
            }
            return data;
        }

        public async Task<PagedResult<Shipper>> ListAsync(PaginationSearchInput input)
        {
            int rowCount = Count(input.SearchValue);
            var data = List(input.Page, input.PageSize, input.SearchValue) as List<Shipper> ?? new List<Shipper>();
            return await Task.FromResult(new PagedResult<Shipper>
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data
            });
        }

        public bool Update(Shipper data)
        {
            return UpdateAsync(data).GetAwaiter().GetResult();
        }

        public async Task<bool> UpdateAsync(Shipper data)
        {
            bool result = false;
            using (var connection = GetConnection())
            {
                var sql = @"update Shippers 
                            set ShipperName = @ShipperName,
                                Phone = @Phone
                            where ShipperID = @ShipperID";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@ShipperID", data.ShipperID);
                    command.Parameters.AddWithValue("@ShipperName", data.ShipperName ?? "");
                    command.Parameters.AddWithValue("@Phone", data.Phone ?? "");

                    result = await command.ExecuteNonQueryAsync() > 0;
                }
                connection.Close();
            }
            return result;
        }
    }
}
