using System.Collections.Generic;

namespace Blueprint
{
    public interface IPagedApiResource
    {
        IEnumerable<object> GetEnumerable();
    }
}
