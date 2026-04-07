using SV_22T1020607.Models.Catalog;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using SV22T1020607.DataLayers.Interfaces;
using SV_22T1020607.Models.Common;
namespace SV22T1020607.DataLayers.SQLServerDAL
{
    public class ProductDAL : BaseDAL, IProductDAL, IProductRepository
    {
        public ProductDAL(string connectionString) : base(connectionString)
        {
        }

        public int Add(Product data)
        {
            int id = 0;
            using (var connection = GetConnection())
            {
                var sql = @"insert into Products(ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsSelling)
                            values(@ProductName, @ProductDescription, @SupplierID, @CategoryID, @Unit, @Price, @Photo, @IsSelling);
                            select @@identity;";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@ProductName", data.ProductName ?? "");
                    command.Parameters.AddWithValue("@ProductDescription", data.ProductDescription ?? "");
                    command.Parameters.AddWithValue("@SupplierID", data.SupplierID > 0 ? data.SupplierID : DBNull.Value);
                    command.Parameters.AddWithValue("@CategoryID", data.CategoryID > 0 ? data.CategoryID : DBNull.Value);
                    command.Parameters.AddWithValue("@Unit", data.Unit ?? "");
                    command.Parameters.AddWithValue("@Price", data.Price);
                    command.Parameters.AddWithValue("@Photo", data.Photo ?? "");
                    command.Parameters.AddWithValue("@IsSelling", data.IsSelling);

                    id = Convert.ToInt32(command.ExecuteScalar());
                }
                connection.Close();
            }
            return id;
        }

        public long AddAttribute(ProductAttribute data)
        {
            long id = 0;
            using (var connection = GetConnection())
            {
                var sql = @"insert into ProductAttributes(ProductID, AttributeName, AttributeValue, DisplayOrder)
                            values(@ProductID, @AttributeName, @AttributeValue, @DisplayOrder);
                            select @@identity;";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@ProductID", data.ProductID);
                    command.Parameters.AddWithValue("@AttributeName", data.AttributeName ?? "");
                    command.Parameters.AddWithValue("@AttributeValue", data.AttributeValue ?? "");
                    command.Parameters.AddWithValue("@DisplayOrder", data.DisplayOrder);

                    id = Convert.ToInt64(command.ExecuteScalar());
                }
                connection.Close();
            }
            return id;
        }

        public long AddPhoto(ProductPhoto data)
        {
            long id = 0;
            using (var connection = GetConnection())
            {
                var sql = @"insert into ProductPhotos(ProductID, Photo, Description, DisplayOrder, IsHidden)
                            values(@ProductID, @Photo, @Description, @DisplayOrder, @IsHidden);
                            select @@identity;";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@ProductID", data.ProductID);
                    command.Parameters.AddWithValue("@Photo", data.Photo ?? "");
                    command.Parameters.AddWithValue("@Description", data.Description ?? "");
                    command.Parameters.AddWithValue("@DisplayOrder", data.DisplayOrder);
                    command.Parameters.AddWithValue("@IsHidden", data.IsHidden);

                    id = Convert.ToInt64(command.ExecuteScalar());
                }
                connection.Close();
            }
            return id;
        }

        public int Count(string searchValue = "", int categoryID = 0, int supplierID = 0, decimal minPrice = 0, decimal maxPrice = 0)
        {
            int count = 0;
            searchValue = $"%{searchValue}%";
            using (var connection = GetConnection())
            {
                var sql = @"select count(*) from Products 
                            where (@searchValue = N'' or ProductName like @searchValue)
                                and (@categoryID = 0 or CategoryID = @categoryID)
                                and (@supplierID = 0 or SupplierID = @supplierID)
                                and (Price >= @minPrice)
                                and (@maxPrice <= 0 or Price <= @maxPrice)";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@searchValue", searchValue);
                    command.Parameters.AddWithValue("@categoryID", categoryID);
                    command.Parameters.AddWithValue("@supplierID", supplierID);
                    command.Parameters.AddWithValue("@minPrice", minPrice);
                    command.Parameters.AddWithValue("@maxPrice", maxPrice);
                    
                    count = Convert.ToInt32(command.ExecuteScalar());
                }
                connection.Close();
            }
            return count;
        }

        public bool Delete(int productID)
        {
            bool result = false;
            using (var connection = GetConnection())
            {
                var sql = @"delete from Products where ProductID = @ProductID";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@ProductID", productID);
                    result = command.ExecuteNonQuery() > 0;
                }
                connection.Close();
            }
            return result;
        }

        public bool DeleteAttribute(long attributeID)
        {
            bool result = false;
            using (var connection = GetConnection())
            {
                var sql = @"delete from ProductAttributes where AttributeID = @AttributeID";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@AttributeID", attributeID);
                    result = command.ExecuteNonQuery() > 0;
                }
                connection.Close();
            }
            return result;
        }

        public bool DeletePhoto(long photoID)
        {
            bool result = false;
            using (var connection = GetConnection())
            {
                var sql = @"delete from ProductPhotos where PhotoID = @PhotoID";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@PhotoID", photoID);
                    result = command.ExecuteNonQuery() > 0;
                }
                connection.Close();
            }
            return result;
        }

        public Product? Get(int productID)
        {
            Product? data = null;
            using (var connection = GetConnection())
            {
                var sql = @"select * from Products where ProductID = @ProductID";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@ProductID", productID);
                    using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        if (reader.Read())
                        {
                            data = new Product()
                            {
                                ProductID = Convert.ToInt32(reader["ProductID"]),
                                ProductName = Convert.ToString(reader["ProductName"]) ?? "",
                                ProductDescription = Convert.ToString(reader["ProductDescription"]) ?? "",
                                SupplierID = reader["SupplierID"] != DBNull.Value ? Convert.ToInt32(reader["SupplierID"]) : null,
                                CategoryID = reader["CategoryID"] != DBNull.Value ? Convert.ToInt32(reader["CategoryID"]) : null,
                                Unit = Convert.ToString(reader["Unit"]) ?? "",
                                Price = Convert.ToDecimal(reader["Price"]),
                                Photo = Convert.ToString(reader["Photo"]) ?? "",
                                IsSelling = Convert.ToBoolean(reader["IsSelling"])
                            };
                        }
                    }
                }
                connection.Close();
            }
            return data;
        }

        public ProductAttribute? GetAttribute(long attributeID)
        {
            ProductAttribute? data = null;
            using (var connection = GetConnection())
            {
                var sql = @"select * from ProductAttributes where AttributeID = @AttributeID";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@AttributeID", attributeID);
                    using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        if (reader.Read())
                        {
                            data = new ProductAttribute()
                            {
                                AttributeID = Convert.ToInt64(reader["AttributeID"]),
                                ProductID = Convert.ToInt32(reader["ProductID"]),
                                AttributeName = Convert.ToString(reader["AttributeName"]) ?? "",
                                AttributeValue = Convert.ToString(reader["AttributeValue"]) ?? "",
                                DisplayOrder = Convert.ToInt32(reader["DisplayOrder"])
                            };
                        }
                    }
                }
                connection.Close();
            }
            return data;
        }

        public ProductPhoto? GetPhoto(long photoID)
        {
            ProductPhoto? data = null;
            using (var connection = GetConnection())
            {
                var sql = @"select * from ProductPhotos where PhotoID = @PhotoID";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@PhotoID", photoID);
                    using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        if (reader.Read())
                        {
                            data = new ProductPhoto()
                            {
                                PhotoID = Convert.ToInt64(reader["PhotoID"]),
                                ProductID = Convert.ToInt32(reader["ProductID"]),
                                Photo = Convert.ToString(reader["Photo"]) ?? "",
                                Description = Convert.ToString(reader["Description"]) ?? "",
                                DisplayOrder = Convert.ToInt32(reader["DisplayOrder"]),
                                IsHidden = Convert.ToBoolean(reader["IsHidden"])
                            };
                        }
                    }
                }
                connection.Close();
            }
            return data;
        }

        public bool InUsed(int productID)
        {
            bool result = false;
            using (var connection = GetConnection())
            {
                var sql = @"if exists(select * from OrderDetails where ProductID = @ProductID)
                                select 1
                            else 
                                select 0";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@ProductID", productID);
                    result = Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
                connection.Close();
            }
            return result;
        }

        public IList<Product> List(int page = 1, int pageSize = 0, string searchValue = "", int categoryID = 0, int supplierID = 0, decimal minPrice = 0, decimal maxPrice = 0)
        {
            List<Product> data = new List<Product>();
            searchValue = $"%{searchValue}%";
            using (var connection = GetConnection())
            {
                var sql = @"with cte as
                            (
                                select	*, row_number() over (order by ProductName) as RowNumber
                                from	Products
                                where	(@searchValue = N'' or ProductName like @searchValue)
                                    and (@categoryID = 0 or CategoryID = @categoryID)
                                    and (@supplierID = 0 or SupplierID = @supplierID)
                                    and (Price >= @minPrice)
                                    and (@maxPrice <= 0 or Price <= @maxPrice)
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
                    command.Parameters.AddWithValue("@categoryID", categoryID);
                    command.Parameters.AddWithValue("@supplierID", supplierID);
                    command.Parameters.AddWithValue("@minPrice", minPrice);
                    command.Parameters.AddWithValue("@maxPrice", maxPrice);

                    using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            data.Add(new Product()
                            {
                                ProductID = Convert.ToInt32(reader["ProductID"]),
                                ProductName = Convert.ToString(reader["ProductName"]) ?? "",
                                ProductDescription = Convert.ToString(reader["ProductDescription"]) ?? "",
                                SupplierID = reader["SupplierID"] != DBNull.Value ? Convert.ToInt32(reader["SupplierID"]) : null,
                                CategoryID = reader["CategoryID"] != DBNull.Value ? Convert.ToInt32(reader["CategoryID"]) : null,
                                Unit = Convert.ToString(reader["Unit"]) ?? "",
                                Price = Convert.ToDecimal(reader["Price"]),
                                Photo = Convert.ToString(reader["Photo"]) ?? "",
                                IsSelling = Convert.ToBoolean(reader["IsSelling"])
                            });
                        }
                    }
                }
                connection.Close();
            }
            return data;
        }

        public IList<ProductAttribute> ListAttributes(int productID)
        {
            List<ProductAttribute> data = new List<ProductAttribute>();
            using (var connection = GetConnection())
            {
                var sql = @"select * from ProductAttributes where ProductID = @ProductID order by DisplayOrder";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@ProductID", productID);
                    using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            data.Add(new ProductAttribute()
                            {
                                AttributeID = Convert.ToInt64(reader["AttributeID"]),
                                ProductID = Convert.ToInt32(reader["ProductID"]),
                                AttributeName = Convert.ToString(reader["AttributeName"]) ?? "",
                                AttributeValue = Convert.ToString(reader["AttributeValue"]) ?? "",
                                DisplayOrder = Convert.ToInt32(reader["DisplayOrder"])
                            });
                        }
                    }
                }
                connection.Close();
            }
            return data;
        }

        public IList<ProductPhoto> ListPhotos(int productID)
        {
            List<ProductPhoto> data = new List<ProductPhoto>();
            using (var connection = GetConnection())
            {
                var sql = @"select * from ProductPhotos where ProductID = @ProductID order by DisplayOrder";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@ProductID", productID);
                    using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            data.Add(new ProductPhoto()
                            {
                                PhotoID = Convert.ToInt64(reader["PhotoID"]),
                                ProductID = Convert.ToInt32(reader["ProductID"]),
                                Photo = Convert.ToString(reader["Photo"]) ?? "",
                                Description = Convert.ToString(reader["Description"]) ?? "",
                                DisplayOrder = Convert.ToInt32(reader["DisplayOrder"]),
                                IsHidden = Convert.ToBoolean(reader["IsHidden"])
                            });
                        }
                    }
                }
                connection.Close();
            }
            return data;
        }

        public bool Update(Product data)
        {
            bool result = false;
            using (var connection = GetConnection())
            {
                var sql = @"update Products 
                            set ProductName = @ProductName,
                                ProductDescription = @ProductDescription,
                                SupplierID = @SupplierID,
                                CategoryID = @CategoryID,
                                Unit = @Unit,
                                Price = @Price,
                                Photo = @Photo,
                                IsSelling = @IsSelling
                            where ProductID = @ProductID";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@ProductName", data.ProductName ?? "");
                    command.Parameters.AddWithValue("@ProductDescription", data.ProductDescription ?? "");
                    command.Parameters.AddWithValue("@SupplierID", data.SupplierID > 0 ? data.SupplierID : DBNull.Value);
                    command.Parameters.AddWithValue("@CategoryID", data.CategoryID > 0 ? data.CategoryID : DBNull.Value);
                    command.Parameters.AddWithValue("@Unit", data.Unit ?? "");
                    command.Parameters.AddWithValue("@Price", data.Price);
                    command.Parameters.AddWithValue("@Photo", data.Photo ?? "");
                    command.Parameters.AddWithValue("@IsSelling", data.IsSelling);
                    command.Parameters.AddWithValue("@ProductID", data.ProductID);
                    
                    result = command.ExecuteNonQuery() > 0;
                }
                connection.Close();
            }
            return result;
        }

        public bool UpdateAttribute(ProductAttribute data)
        {
            bool result = false;
            using (var connection = GetConnection())
            {
                var sql = @"update ProductAttributes 
                            set AttributeName = @AttributeName,
                                AttributeValue = @AttributeValue,
                                DisplayOrder = @DisplayOrder
                            where AttributeID = @AttributeID";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@AttributeName", data.AttributeName ?? "");
                    command.Parameters.AddWithValue("@AttributeValue", data.AttributeValue ?? "");
                    command.Parameters.AddWithValue("@DisplayOrder", data.DisplayOrder);
                    command.Parameters.AddWithValue("@AttributeID", data.AttributeID);
                    
                    result = command.ExecuteNonQuery() > 0;
                }
                connection.Close();
            }
            return result;
        }

        public bool UpdatePhoto(ProductPhoto data)
        {
            bool result = false;
            using (var connection = GetConnection())
            {
                var sql = @"update ProductPhotos 
                            set Photo = @Photo,
                                Description = @Description,
                                DisplayOrder = @DisplayOrder,
                                IsHidden = @IsHidden
                            where PhotoID = @PhotoID";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@Photo", data.Photo ?? "");
                    command.Parameters.AddWithValue("@Description", data.Description ?? "");
                    command.Parameters.AddWithValue("@DisplayOrder", data.DisplayOrder);
                    command.Parameters.AddWithValue("@IsHidden", data.IsHidden);
                    command.Parameters.AddWithValue("@PhotoID", data.PhotoID);
                    
                    result = command.ExecuteNonQuery() > 0;
                }
                connection.Close();
            }
            return result;
        }

        // IProductRepository async implementations
        public Task<PagedResult<Product>> ListAsync(ProductSearchInput input)
        {
            int rowCount = Count(input.SearchValue, input.CategoryID, input.SupplierID, input.MinPrice, input.MaxPrice);
            var items = List(input.Page, input.PageSize, input.SearchValue, input.CategoryID, input.SupplierID, input.MinPrice, input.MaxPrice) as List<Product> ?? new List<Product>();
            return Task.FromResult(new PagedResult<Product>
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = items
            });
        }

        public Task<Product?> GetAsync(int productID)
        {
            return Task.FromResult(Get(productID));
        }

        public Task<int> AddAsync(Product data)
        {
            return Task.FromResult(Add(data));
        }

        public Task<bool> UpdateAsync(Product data)
        {
            return Task.FromResult(Update(data));
        }

        public Task<bool> DeleteAsync(int productID)
        {
            return Task.FromResult(Delete(productID));
        }

        public Task<bool> IsUsedAsync(int productID)
        {
            return Task.FromResult(InUsed(productID));
        }

        public Task<List<ProductAttribute>> ListAttributesAsync(int productID)
        {
            return Task.FromResult(ListAttributes(productID) as List<ProductAttribute> ?? new List<ProductAttribute>());
        }

        public Task<ProductAttribute?> GetAttributeAsync(long attributeID)
        {
            return Task.FromResult(GetAttribute(attributeID));
        }

        public Task<long> AddAttributeAsync(ProductAttribute data)
        {
            return Task.FromResult(AddAttribute(data));
        }

        public Task<bool> UpdateAttributeAsync(ProductAttribute data)
        {
            return Task.FromResult(UpdateAttribute(data));
        }

        public Task<bool> DeleteAttributeAsync(long attributeID)
        {
            return Task.FromResult(DeleteAttribute(attributeID));
        }

        public Task<List<ProductPhoto>> ListPhotosAsync(int productID)
        {
            return Task.FromResult(ListPhotos(productID) as List<ProductPhoto> ?? new List<ProductPhoto>());
        }

        public Task<ProductPhoto?> GetPhotoAsync(long photoID)
        {
            return Task.FromResult(GetPhoto(photoID));
        }

        public Task<long> AddPhotoAsync(ProductPhoto data)
        {
            return Task.FromResult(AddPhoto(data));
        }

        public Task<bool> UpdatePhotoAsync(ProductPhoto data)
        {
            return Task.FromResult(UpdatePhoto(data));
        }

        public Task<bool> DeletePhotoAsync(long photoID)
        {
            return Task.FromResult(DeletePhoto(photoID));
        }
    }
}
