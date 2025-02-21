namespace WakeCommerce.ApiService.Controllers.Base
{
    public class SuccessResponse<T>
    {
        public SuccessResponse(T data, bool sucess, int statusCode)
        {
            Data = data;
            Sucess = sucess;
            StatusCode = statusCode;
        }
        public T Data { get; set; }
        public bool Sucess { get; set; }
        public int StatusCode { get; set; }
    }
}
