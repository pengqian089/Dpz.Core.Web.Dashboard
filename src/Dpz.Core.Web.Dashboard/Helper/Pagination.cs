namespace Dpz.Core.Web.Dashboard.Helper
{
    public class Pagination
    {
        public int CurrentPage { get; set; }

        public int TotalCount { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }

        public int StartItemIndex { get; set; }

        public int EndItemIndex { get; set; }
    }
}