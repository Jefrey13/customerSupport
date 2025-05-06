namespace CustomerService.API.Utils
{
    public class PagedResponse<T> : ApiResponse<IEnumerable<T>>
    {
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        public PagedResponse(IEnumerable<T> data, int pageNumber, int pageSize, int totalCount)
            : base(data, "", true)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalCount = totalCount;
        }
    }
}
