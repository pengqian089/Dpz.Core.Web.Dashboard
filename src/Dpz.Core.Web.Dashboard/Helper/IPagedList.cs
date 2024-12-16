using System.Collections;
using System.Collections.Generic;

namespace Dpz.Core.Web.Dashboard.Helper
{
    /// <summary>
    /// 表示包含分页数据对象的非泛型集合。
    /// </summary>
    public interface IPagedList:IEnumerable
    {
        /// <summary>
        /// 当前页索引
        /// </summary>
        int CurrentPageIndex { get; set; }

        /// <summary>
        /// 每页显示的记录数
        /// </summary>
        int PageSize { get; set; }

        /// <summary>
        /// 要分页的数据总数
        /// </summary>
        int TotalItemCount { get; set; }
        
        /// <summary>
        /// 总页数
        /// </summary>
        int TotalPageCount { get;  }
        
        /// <summary>
        /// 开始记录索引
        /// </summary>
        int StartItemIndex { get;  }

        /// <summary>
        /// 结束记录索引
        /// </summary>
        int EndItemIndex { get;  }
    }

    /// <summary>
    /// 表示包含分页数据对象的泛型集合。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPagedList<T>:IList<T>,IPagedList{}
}