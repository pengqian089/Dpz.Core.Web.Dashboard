using System;
using System.Collections.Generic;
using System.Linq;

namespace Dpz.Core.Web.Dashboard.Helper
{
    /// <summary>
    /// 表示包含分页数据对象的强类型列表。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedList<T> : List<T>, IPagedList<T>
    {
        /// <summary>
        /// 使用要分页的所有数据项、当前页索引和每页显示的记录数初始化PagedList对象
        /// </summary>
        /// <param name="allItems">要分页的所有数据项</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">每页显示的记录数</param>
        public PagedList(IEnumerable<T> allItems, int pageIndex, int pageSize)
        {
            PageSize = pageSize;
            var items = allItems as IList<T> ?? allItems.ToList();
            TotalItemCount = items.Count();
            CurrentPageIndex = pageIndex;
            AddRange(items.Skip(StartItemIndex - 1).Take(pageSize));
        }

        /// <summary>
        /// 使用当前页数据项、当前页索引、每页显示的记录数和要分页的总记录数初始化PagedList对象
        /// </summary>
        /// <param name="currentPageItems">当前页数据项</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">每页显示的记录数</param>
        /// <param name="totalItemCount">要分页数据的总记录数</param>
        public PagedList(IEnumerable<T> currentPageItems, int pageIndex, int pageSize, int totalItemCount)
        {
            AddRange(currentPageItems);
            TotalItemCount = totalItemCount;
            CurrentPageIndex = pageIndex;
            PageSize = pageSize;
        }

        /// <summary>
        /// 当前页索引
        /// </summary>
        public int CurrentPageIndex { get; set; }

        /// <summary>
        /// 每页显示的记录数
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 要分页的数据总数
        /// </summary>
        public int TotalItemCount { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPageCount => (int) Math.Ceiling(TotalItemCount / (double) PageSize);

        /// <summary>
        /// 开始记录索引
        /// </summary>
        public int StartItemIndex => (CurrentPageIndex - 1) * PageSize + 1;

        /// <summary>
        /// 结束记录索引
        /// </summary>
        public int EndItemIndex =>
            TotalItemCount > CurrentPageIndex * PageSize ? CurrentPageIndex * PageSize : TotalItemCount;
    }
}