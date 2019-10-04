using System.Collections.Generic;

namespace Blueprint.Api
{
    public interface IPagedApiResource
    {
        IEnumerable<object> GetEnumerable();
    }
}
