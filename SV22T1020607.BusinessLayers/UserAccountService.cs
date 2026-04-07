using SV22T1020607.DataLayers.Interfaces;
using SV22T1020607.DataLayers.SQLServerDAL;
using SV_22T1020607.Models.Security;
using System.Threading.Tasks;

namespace SV22T1020607.BusinessLayers
{
    public static class UserAccountService
    {
        private static IUserAccountRepository employeeAccountDB;
        private static CustomerAccountDAL customerAccountDB;

        public static void Initialize(string connectionString)
        {
            employeeAccountDB = new EmployeeAccountDAL(connectionString);
            customerAccountDB = new CustomerAccountDAL(connectionString);
        }

        public static async Task<UserAccount?> AuthorizeAsync(string userName, string password)
        {
            return await employeeAccountDB.AuthorizeAsync(userName, password);
        }

        public static async Task<bool> ChangePasswordAsync(string userName, string password)
        {
            return await employeeAccountDB.ChangePassword(userName, password);
        }

        // DÀNH CHO CUSTOMER (SHOP)
        public static async Task<UserAccount?> AuthorizeCustomerAsync(string userName, string password)
        {
            return await customerAccountDB.AuthorizeAsync(userName, password);
        }

        public static async Task<bool> ChangeCustomerPasswordAsync(string userName, string password)
        {
            return await customerAccountDB.ChangePassword(userName, password);
        }

        public static async Task<bool> RegisterCustomerAsync(string customerName, string contactName, string email, string phone, string address, string province, string password)
        {
            return await customerAccountDB.RegisterAsync(customerName, contactName, email, phone, address, province, password);
        }
    }
}
