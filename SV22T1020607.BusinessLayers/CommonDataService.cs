using SV22T1020607.DataLayers;
using SV22T1020607.DataLayers.SQLServerDAL;
using SV_22T1020607.Models.Partner;
using SV_22T1020607.Models.Common;
using SV_22T1020607.Models.Catalog;
using SV_22T1020607.Models.HR;
using System.Collections.Generic;

namespace SV22T1020607.BusinessLayers
{
    /// <summary>
    /// Các chức năng nghiệp vụ liên quan đến dữ liệu chung
    /// (loại hàng, nhân viên)
    /// </summary>
    public static class CommonDataService
    {
        private static ICommonDAL<Category> categoryDAL = null!;
        private static ICommonDAL<Employee> employeeDAL = null!;

        /// <summary>
        /// Khởi tạo dịch vụ
        /// </summary>
        public static void Initialize()
        {
            categoryDAL = new CategoryDAL(Configuration.ConnectionString);
            employeeDAL = new EmployeeDAL(Configuration.ConnectionString);
        }

        #region Các nghiệp vụ liên quan đến loại hàng
        public static IList<Category> ListOfCategories(int page = 1, int pageSize = 0, string searchValue = "")
        {
            return categoryDAL.List(page, pageSize, searchValue);
        }
        public static int CountCategories(string searchValue = "")
        {
            return categoryDAL.Count(searchValue);
        }
        public static Category? GetCategory(int id)
        {
            return categoryDAL.Get(id);
        }
        public static int AddCategory(Category data)
        {
            return categoryDAL.Add(data);
        }
        public static bool UpdateCategory(Category data)
        {
            return categoryDAL.Update(data);
        }
        public static bool DeleteCategory(int id)
        {
            return categoryDAL.Delete(id);
        }
        public static bool InUsedCategory(int id)
        {
            return categoryDAL.InUsed(id);
        }
        #endregion

        #region Các nghiệp vụ liên quan đến nhân viên
        public static IList<Employee> ListOfEmployees(int page = 1, int pageSize = 0, string searchValue = "")
        {
            return employeeDAL.List(page, pageSize, searchValue);
        }
        public static int CountEmployees(string searchValue = "")
        {
            return employeeDAL.Count(searchValue);
        }
        public static Employee? GetEmployee(int id)
        {
            return employeeDAL.Get(id);
        }
        public static int AddEmployee(Employee data)
        {
            return employeeDAL.Add(data);
        }
        public static bool UpdateEmployee(Employee data)
        {
            return employeeDAL.Update(data);
        }
        public static bool DeleteEmployee(int id)
        {
            return employeeDAL.Delete(id);
        }
        public static bool InUsedEmployee(int id)
        {
            return employeeDAL.InUsed(id);
        }
        #endregion
    }
}
