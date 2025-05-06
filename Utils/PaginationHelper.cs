using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Utils
{
    public static class PaginationHelper
    {
        public static async Task<PagedResponse<T>> CreateAsync<T>(
            IQueryable<T> source,
            int pageNumber,
            int pageSize,
            CancellationToken cancellation = default)
        {
            var count = await source.CountAsync(cancellation);
            var items = await source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellation);
            return new PagedResponse<T>(items, pageNumber, pageSize, count);
        }
    }
}
