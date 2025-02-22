namespace WakeCommerce.ApiService.Controllers.Base
{
    public class SuccessResponse
    {
        public SuccessResponse(object? data, bool sucess, int statusCode)
        {
            Data = data;
            Sucess = sucess;
            StatusCode = statusCode;
        }
        public object? Data { get; set; }
        public bool Sucess { get; set; }
        public int StatusCode { get; set; }
    }
}
