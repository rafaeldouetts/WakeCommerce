namespace WakeCommerce.ApiService.Controllers.Base
{
    public class ProblemDatails<T>
    {
        public int Status { get; set; }
        public string Title { get; set; }
        public T Detail { get; set; }
        public string Type { get; set; }
    }
}
