using System.Collections.Generic;

namespace Blueprint.Http
{
    /// <summary>
    /// A specialisation of <see cref="ListApiResource{T}" /> that represents a list of resources
    /// that can be paged on the server.
    /// </summary>
    /// <typeparam name="T">The type of the API resource represented.</typeparam>
    public class PagedApiResource<T> : ListApiResource<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedApiResource{T}"/> class.
        /// </summary>
        /// <param name="values">The values of the current page.</param>
        /// <param name="total">The total number of Api resources (i.e. how many resources exist if they were not paged).</param>
        /// <param name="pageSize">The size of the page requested.</param>
        /// <param name="currentPage">The 1-based index of the page requested.</param>
        public PagedApiResource(IEnumerable<T> values, long total, int pageSize, int currentPage)
            : base(values)
        {
            this.Total = total;
            this.PageSize = pageSize;
            this.CurrentPage = currentPage;
        }

        /// <summary>
        /// The total number of Api resources (i.e. how many resources exist if they were not paged).
        /// </summary>
        public long Total { get; }

        /// <summary>
        /// The size of the page requested.
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// The 1-based index of the page requested.
        /// </summary>
        public int CurrentPage { get; }
    }
}
