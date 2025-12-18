using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.WebApi.Models.Listing
{
    public class ListingOptions
    {
        public PageOptions PageOptions { get; set; }
        public FilterOptions? FilterOptions { get; set; }
        public SortOptions? SortOptions { get; set; }
    }
}
