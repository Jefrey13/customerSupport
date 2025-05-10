using System.Collections.Generic;

namespace CustomerService.API.Utils
{
    public class PagedResponse<T>
    {
        public IEnumerable<T> Items { get; }
        public MetaData Meta { get; }

        public PagedResponse(IEnumerable<T> items, MetaData meta)
        {
            Items = items;
            Meta = meta;
        }
    }

    public class MetaData
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}