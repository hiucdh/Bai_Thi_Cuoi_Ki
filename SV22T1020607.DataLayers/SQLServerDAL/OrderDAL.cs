using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using SV22T1020607.DataLayers.Interfaces;
using SV_22T1020607.Models.Common;
using SV_22T1020607.Models.Sales;

namespace SV22T1020607.DataLayers.SQLServerDAL
{
    public class OrderDAL : BaseDAL, IOrderRepository
    {
        public OrderDAL(string connectionString) : base(connectionString)
        {
        }

        public async Task<int> AddAsync(Order data)
        {
            int id = 0;
            using (var connection = GetConnection())
            {
                var sql = @"insert into Orders(CustomerID, OrderTime, DeliveryProvince, DeliveryAddress, EmployeeID, Status)
                            values(@CustomerID, getdate(), @DeliveryProvince, @DeliveryAddress, @EmployeeID, @Status);
                            select @@identity;";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@CustomerID", data.CustomerID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@DeliveryProvince", data.DeliveryProvince ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@DeliveryAddress", data.DeliveryAddress ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@EmployeeID", data.EmployeeID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Status", (int)OrderStatusEnum.New);

                    id = Convert.ToInt32(await command.ExecuteScalarAsync());
                }
                connection.Close();
            }
            return id;
        }

        public async Task<bool> AddDetailAsync(OrderDetail data)
        {
            bool result = false;
            using (var connection = GetConnection())
            {
                var sql = @"insert into OrderDetails(OrderID, ProductID, Quantity, SalePrice)
                            values(@OrderID, @ProductID, @Quantity, @SalePrice)";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@OrderID", data.OrderID);
                    command.Parameters.AddWithValue("@ProductID", data.ProductID);
                    command.Parameters.AddWithValue("@Quantity", data.Quantity);
                    command.Parameters.AddWithValue("@SalePrice", data.SalePrice);

                    result = await command.ExecuteNonQueryAsync() > 0;
                }
                connection.Close();
            }
            return result;
        }

        public Task<bool> DeleteAsync(int orderID)
        {
            return Task.FromResult(false);
        }

        public Task<bool> DeleteDetailAsync(int orderID, int productID)
        {
            return Task.FromResult(false);
        }

        public async Task<OrderViewInfo?> GetAsync(int orderID)
        {
            OrderViewInfo? data = null;
            using (var connection = GetConnection())
            {
                var sql = @"
                    select  o.*,
                            c.CustomerName, c.ContactName as CustomerContactName, c.Phone as CustomerPhone, c.Email as CustomerEmail, c.Address as CustomerAddress,
                            e.FullName as EmployeeName,
                            s.ShipperName, s.Phone as ShipperPhone
                    from    Orders o
                            left join Customers c on o.CustomerID = c.CustomerID
                            left join Employees e on o.EmployeeID = e.EmployeeID
                            left join Shippers s on o.ShipperID = s.ShipperID
                    where   o.OrderID = @OrderID
                ";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@OrderID", orderID);
                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (await reader.ReadAsync())
                        {
                            data = new OrderViewInfo()
                            {
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                OrderTime = Convert.ToDateTime(reader["OrderTime"]),
                                AcceptTime = reader["AcceptTime"] == DBNull.Value ? null : Convert.ToDateTime(reader["AcceptTime"]),
                                ShippedTime = reader["ShippedTime"] == DBNull.Value ? null : Convert.ToDateTime(reader["ShippedTime"]),
                                FinishedTime = reader["FinishedTime"] == DBNull.Value ? null : Convert.ToDateTime(reader["FinishedTime"]),
                                Status = (OrderStatusEnum)Convert.ToInt32(reader["Status"]),
                                CustomerID = reader["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CustomerID"]),
                                CustomerName = Convert.ToString(reader["CustomerName"]) ?? "",
                                CustomerContactName = Convert.ToString(reader["CustomerContactName"]) ?? "",
                                CustomerAddress = Convert.ToString(reader["CustomerAddress"]) ?? "",
                                CustomerPhone = Convert.ToString(reader["CustomerPhone"]) ?? "",
                                CustomerEmail = Convert.ToString(reader["CustomerEmail"]) ?? "",
                                EmployeeID = reader["EmployeeID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["EmployeeID"]),
                                EmployeeName = Convert.ToString(reader["EmployeeName"]) ?? "",
                                ShipperID = reader["ShipperID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ShipperID"]),
                                ShipperName = Convert.ToString(reader["ShipperName"]) ?? "",
                                ShipperPhone = Convert.ToString(reader["ShipperPhone"]) ?? "",
                                DeliveryProvince = Convert.ToString(reader["DeliveryProvince"]) ?? "",
                                DeliveryAddress = Convert.ToString(reader["DeliveryAddress"]) ?? ""
                            };
                        }
                    }
                }
            }
            return data;
        }

        public Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID)
        {
            return Task.FromResult<OrderDetailViewInfo?>(null);
        }

        public async Task<PagedResult<OrderViewInfo>> ListAsync(OrderSearchInput input)
        {
            var data = new List<OrderViewInfo>();
            int rowCount = 0;

            using (var connection = GetConnection())
            {
                var sqlCount = @"
                    select count(*) 
                    from Orders o
                    left join Customers c on o.CustomerID = c.CustomerID
                    where (@Status = 0 or o.Status = @Status)
                      and (@FromTime is null or o.OrderTime >= @FromTime)
                      and (@ToTime is null or o.OrderTime <= @ToTime)
                      and (@SearchValue = N'' or c.CustomerName like @SearchValue)
                ";
                using (var command = new SqlCommand(sqlCount, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@Status", (int)input.Status);
                    command.Parameters.AddWithValue("@FromTime", input.DateFrom ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ToTime", input.DateTo?.AddDays(1).AddTicks(-1) ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@SearchValue", $"%{input.SearchValue}%");
                    rowCount = Convert.ToInt32(await command.ExecuteScalarAsync());
                }

                if (rowCount > 0 && input.PageSize > 0) 
                {
                    var sqlData = @"
                        with cte as
                        (
                            select  o.*,
                                    c.CustomerName, c.ContactName as CustomerContactName, c.Phone as CustomerPhone, c.Email as CustomerEmail, c.Address as CustomerAddress,
                                    e.FullName as EmployeeName,
                                    s.ShipperName, s.Phone as ShipperPhone,
                                    row_number() over(order by o.OrderTime desc) as RowNumber
                            from    Orders o
                                    left join Customers c on o.CustomerID = c.CustomerID
                                    left join Employees e on o.EmployeeID = e.EmployeeID
                                    left join Shippers s on o.ShipperID = s.ShipperID
                            where   (@Status = 0 or o.Status = @Status)
                                and (@FromTime is null or o.OrderTime >= @FromTime)
                                and (@ToTime is null or o.OrderTime <= @ToTime)
                                and (@SearchValue = N'' or c.CustomerName like @SearchValue)
                        )
                        select * from cte
                        where  (@PageSize = 0)
                            or (RowNumber between (@Page - 1) * @PageSize + 1 and @Page * @PageSize)
                        order by RowNumber;
                    ";
                    using (var command = new SqlCommand(sqlData, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@Status", (int)input.Status);
                        command.Parameters.AddWithValue("@FromTime", input.DateFrom ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ToTime", input.DateTo?.AddDays(1).AddTicks(-1) ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@SearchValue", $"%{input.SearchValue}%");
                        command.Parameters.AddWithValue("@Page", input.Page);
                        command.Parameters.AddWithValue("@PageSize", input.PageSize);
                        
                        using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                        {
                            while (await reader.ReadAsync())
                            {
                                data.Add(new OrderViewInfo()
                                {
                                    OrderID = Convert.ToInt32(reader["OrderID"]),
                                    OrderTime = Convert.ToDateTime(reader["OrderTime"]),
                                    AcceptTime = reader["AcceptTime"] == DBNull.Value ? null : Convert.ToDateTime(reader["AcceptTime"]),
                                    ShippedTime = reader["ShippedTime"] == DBNull.Value ? null : Convert.ToDateTime(reader["ShippedTime"]),
                                    FinishedTime = reader["FinishedTime"] == DBNull.Value ? null : Convert.ToDateTime(reader["FinishedTime"]),
                                    Status = (OrderStatusEnum)Convert.ToInt32(reader["Status"]),
                                    CustomerID = reader["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CustomerID"]),
                                    CustomerName = Convert.ToString(reader["CustomerName"]) ?? "",
                                    CustomerContactName = Convert.ToString(reader["CustomerContactName"]) ?? "",
                                    CustomerAddress = Convert.ToString(reader["CustomerAddress"]) ?? "",
                                    CustomerPhone = Convert.ToString(reader["CustomerPhone"]) ?? "",
                                    CustomerEmail = Convert.ToString(reader["CustomerEmail"]) ?? "",
                                    EmployeeID = reader["EmployeeID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["EmployeeID"]),
                                    EmployeeName = Convert.ToString(reader["EmployeeName"]) ?? "",
                                    ShipperID = reader["ShipperID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ShipperID"]),
                                    ShipperName = Convert.ToString(reader["ShipperName"]) ?? "",
                                    ShipperPhone = Convert.ToString(reader["ShipperPhone"]) ?? "",
                                    DeliveryProvince = Convert.ToString(reader["DeliveryProvince"]) ?? "",
                                    DeliveryAddress = Convert.ToString(reader["DeliveryAddress"]) ?? ""
                                });
                            }
                        }
                    }
                }
                else
                {
                    connection.Close();
                }
            }

            return new PagedResult<OrderViewInfo>
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data
            };
        }

        public async Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID)
        {
            var data = new List<OrderDetailViewInfo>();
            using (var connection = GetConnection())
            {
                var sql = @"
                    select  od.*,
                            p.ProductName, p.Photo, p.Unit
                    from    OrderDetails od
                            join Products p on od.ProductID = p.ProductID
                    where   od.OrderID = @OrderID
                ";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@OrderID", orderID);
                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        while (await reader.ReadAsync())
                        {
                            data.Add(new OrderDetailViewInfo()
                            {
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                ProductID = Convert.ToInt32(reader["ProductID"]),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                SalePrice = Convert.ToDecimal(reader["SalePrice"]),
                                ProductName = Convert.ToString(reader["ProductName"]) ?? "",
                                Photo = Convert.ToString(reader["Photo"]) ?? "",
                                Unit = Convert.ToString(reader["Unit"]) ?? ""
                            });
                        }
                    }
                }
            }
            return data;
        }

        public async Task<bool> UpdateAsync(Order data)
        {
            using (var connection = GetConnection())
            {
                var sql = @"
                    UPDATE Orders
                    SET CustomerID = @CustomerID,
                        OrderTime = @OrderTime,
                        DeliveryProvince = @DeliveryProvince,
                        DeliveryAddress = @DeliveryAddress,
                        EmployeeID = @EmployeeID,
                        AcceptTime = @AcceptTime,
                        ShipperID = @ShipperID,
                        ShippedTime = @ShippedTime,
                        FinishedTime = @FinishedTime,
                        Status = @Status
                    WHERE OrderID = @OrderID
                ";
                using (var command = new Microsoft.Data.SqlClient.SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@CustomerID", (data.CustomerID == null || data.CustomerID <= 0) ? (object)DBNull.Value : data.CustomerID);
                    command.Parameters.AddWithValue("@OrderTime", data.OrderTime);
                    command.Parameters.AddWithValue("@DeliveryProvince", data.DeliveryProvince ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@DeliveryAddress", data.DeliveryAddress ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@EmployeeID", (data.EmployeeID == null || data.EmployeeID <= 0) ? (object)DBNull.Value : data.EmployeeID);
                    command.Parameters.AddWithValue("@AcceptTime", data.AcceptTime ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ShipperID", (data.ShipperID == null || data.ShipperID <= 0) ? (object)DBNull.Value : data.ShipperID);
                    command.Parameters.AddWithValue("@ShippedTime", data.ShippedTime ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@FinishedTime", data.FinishedTime ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Status", (int)data.Status);
                    command.Parameters.AddWithValue("@OrderID", data.OrderID);

                    int result = await command.ExecuteNonQueryAsync();
                    return result > 0;
                }
            }
        }

        public Task<bool> UpdateDetailAsync(OrderDetail data)
        {
            return Task.FromResult(false);
        }
    }
}
