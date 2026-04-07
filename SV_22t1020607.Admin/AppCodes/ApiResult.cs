namespace SV22T1020607.Admin.AppCodes
{
    public class ApiResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public object? Data { get; set; }
    }
}
