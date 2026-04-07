using SV22T1020607.DataLayers.Interfaces;
using SV_22T1020607.Models.Common;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System;

namespace SV22T1020607.DataLayers.SQLServerDAL
{
    public class ProvinceDAL : BaseDAL, ISimpleSelectDAL<Province>, IDataDictionaryRepository<Province>
    {
        public ProvinceDAL(string connectionString) : base(connectionString)
        {
        }

        public IList<Province> List()
        {
            return ListAsync().GetAwaiter().GetResult();
        }

        public async Task<List<Province>> ListAsync()
        {
            List<Province> data = new List<Province>();
            using (var connection = GetConnection())
            {
                var sql = "select * from Provinces";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        while (await reader.ReadAsync())
                        {
                            data.Add(new Province()
                            {
                                ProvinceName = Convert.ToString(reader["ProvinceName"]) ?? ""
                            });
                        }
                    }
                }
                connection.Close();
            }
            return data;
        }
    }
}
