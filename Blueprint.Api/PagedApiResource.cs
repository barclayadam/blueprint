using System.Collections.Generic;
using System.Linq;

namespace Blueprint.Api
{
    public class PagedApiResource<T> : ApiResource, IPagedApiResource
    {
        public PagedApiResource(IEnumerable<T> values)
        {
            this.Object = $"list.{GetTypeName(typeof(T))}";

            // NB: It's important to consume values which could be a LINQ query as otherwise modifications
            // in middleware could be lost if the values are enumerated multiple times
            this.Values = values.ToList();

            this.Total = this.Values.Count();
            this.CurrentPage = 1;
        }

        public PagedApiResource(IEnumerable<T> values, long total, int pageSize, int currentPage)
        {
            this.Object = $"list.{GetTypeName(typeof(T))}";

            // NB: It's important to consume values which could be a LINQ query as otherwise modifications
            // in middleware could be lost if the values are enumerated multiple times
            this.Values = values.ToList();

            this.Total = total;
            this.PageSize = pageSize;
            this.CurrentPage = currentPage;
        }

        public IEnumerable<T> Values { get; }

        public long? Total { get; }

        public int? PageSize { get; }

        public int? CurrentPage { get; }

        public IEnumerable<object> GetEnumerable()
        {
            return this.Values as IEnumerable<object>;
        }
    }

    public interface IPagedApiResource
    {
        IEnumerable<object> GetEnumerable();
    }
}