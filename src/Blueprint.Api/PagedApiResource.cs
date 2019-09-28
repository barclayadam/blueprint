using System.Collections.Generic;
using System.Linq;

namespace Blueprint.Api
{
    public class PagedApiResource<T> : ApiResource, IPagedApiResource
    {
        public PagedApiResource(IEnumerable<T> values)
        {
            Object = $"list.{GetTypeName(typeof(T))}";

            // NB: It's important to consume values which could be a LINQ query as otherwise modifications
            // in middleware could be lost if the values are enumerated multiple times
            Values = values.ToList();

            Total = Values.Count();
            CurrentPage = 1;
        }

        public PagedApiResource(IEnumerable<T> values, long total, int pageSize, int currentPage)
        {
            Object = $"list.{GetTypeName(typeof(T))}";

            // NB: It's important to consume values which could be a LINQ query as otherwise modifications
            // in middleware could be lost if the values are enumerated multiple times
            Values = values.ToList();

            Total = total;
            PageSize = pageSize;
            CurrentPage = currentPage;
        }

        public IEnumerable<T> Values { get; }

        public long? Total { get; }

        public int? PageSize { get; }

        public int? CurrentPage { get; }

        public IEnumerable<object> GetEnumerable()
        {
            return Values as IEnumerable<object>;
        }
    }

    public interface IPagedApiResource
    {
        IEnumerable<object> GetEnumerable();
    }
}