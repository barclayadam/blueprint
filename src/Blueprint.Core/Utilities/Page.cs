using System;
using System.Collections.Generic;
using System.Linq;

namespace Blueprint.Core.Utilities
{
    /// <summary>
    /// A page of a list, with details about which page it is and details on the original collection.
    /// </summary>
    /// <typeparam name="T">The type of the elements in this page.</typeparam>
    public class Page<T>
    {
        private readonly IEnumerable<T> items;
        private readonly int pageCount;
        private readonly int pageNumber;
        private readonly int pageSize;
        private readonly int totalCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="Page{T}"/> class.
        /// </summary>
        /// <param name="items">The items that represent the selected page.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">Size of each page.</param>
        /// <param name="totalCount">The total count.</param>
        public Page(IEnumerable<T> items, int pageNumber, int pageSize, int totalCount)
        {
            Guard.NotNull(nameof(items), items);
            Guard.GreaterThanOrEqual("pageNumber", pageNumber, 0);
            Guard.GreaterThanOrEqual("pageSize", pageSize, 1);
            Guard.GreaterThanOrEqual("totalCount", totalCount, 0);

            this.pageSize = pageSize;
            this.pageNumber = pageNumber;
            this.totalCount = totalCount;
            this.items = items;

            pageCount = CalculatePageCount(pageSize, totalCount);
        }

        /// <summary>
        /// Gets all the items in the current page.
        /// </summary>
        public IEnumerable<T> Items => items;

        /// <summary>
        /// Gets the number of pages in the collection.
        /// </summary>
        public int PageCount => pageCount;

        /// <summary>
        /// Gets the current page number.
        /// </summary>
        public int PageNumber => items.Any() ? pageNumber : 0;

        /// <summary>
        /// Gets the size of the page.
        /// </summary>
        /// <value>
        /// The size of the page.
        /// </value>
        public int PageSize => pageSize;

        /// <summary>
        /// Gets the total number of objects in the collection.
        /// </summary>
        public int TotalCount => totalCount;

        /// <summary>
        /// Calculates the page count.
        /// </summary>
        /// <param name="pageSize">
        /// Size of the page.
        /// </param>
        /// <param name="totalCount">
        /// The total count.
        /// </param>
        /// <returns>
        /// The number of pages the collection is split into.
        /// </returns>
        internal static int CalculatePageCount(int pageSize, int totalCount)
        {
            return (int)Math.Ceiling((double)totalCount / pageSize);
        }
    }
}
