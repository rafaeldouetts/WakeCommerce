﻿namespace WakeCommerce.ApiService.Controllers.Base
{
    public class ProblemDatails
    {
        public int Status { get; set; }
        public string Title { get; set; }
        public object? Detail { get; set; }
        public string Type { get; set; }
    }
}
