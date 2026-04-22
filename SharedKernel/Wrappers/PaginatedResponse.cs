using System;
using System.Collections.Generic;
using System.Text;

namespace SharedKernel.Wrappers
{
    public class PaginatedResponse<T>
    {
        public IList<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
