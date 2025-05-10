using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Utils
{
    public class PagedList<T> : List<T>
    {
        public MetaData MetaData { get; private set; }

        private PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
        {
            MetaData = new MetaData
            {
                TotalCount = count,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(count / (double)pageSize)
            };

            AddRange(items);
        }

        /// <summary>
        /// Crea una página a partir de una consulta IQueryable.
        /// </summary>
        public static async Task<PagedList<T>> CreateAsync(
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

            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}
