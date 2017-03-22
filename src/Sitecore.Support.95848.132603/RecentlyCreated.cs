using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.ContentSearch.Security;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Resources;
using Sitecore.Web.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Buckets.Search;

namespace Sitecore.Support.Buckets.Search.SearchDropdowns
{
    internal class RecentlyCreated : ISearchDropDown
    {
        private readonly IContentSearchConfigurationSettings settings;

        public RecentlyCreated()
        {
            this.settings = ContentSearchManager.Locator.GetInstance<IContentSearchConfigurationSettings>();
        }

        public RecentlyCreated(IContentSearchConfigurationSettings settings)
        {
            this.settings = settings;
        }

        public List<string> Process()
        {
            List<string> result;
            using (IProviderSearchContext providerSearchContext = ContentSearchManager.GetIndex((SitecoreIndexableItem)Context.ContentDatabase.GetItem(ItemIDs.RootID)).CreateSearchContext(SearchSecurityOptions.Default))
            {
                string format = FieldFormat.GetFieldFormat(providerSearchContext.Index, "__smallcreateddate", typeof(DateTime));
                IQueryable<SitecoreUISearchResultItem> queryable = (from i in providerSearchContext.GetQueryable<SitecoreUISearchResultItem>()
                                                                    where i["__smallcreateddate"].Between(DateTime.UtcNow.AddDays(-1.0).ToString(format, Context.ContentLanguage.CultureInfo), DateTime.UtcNow.AddDays(1.0).ToString(format, Context.ContentLanguage.CultureInfo), Inclusion.Both)
                                                                    select i into date
                                                                    orderby date.CreatedDate descending
                                                                    select date).Page(0, 20);
                List<string> list = new List<string>();
                foreach (SitecoreUISearchResultItem current in queryable)
                {
                    if (current != null)
                    {
                        Item item = current.GetItem();
                        if (item != null)
                        {
                            list.Add(string.Concat(new object[]
                            {
                                item.Name,
                                "|",
                                item.ID,
                                "|la=",
                                item.Language.Name,
                                "|",
                                Images.GetThemedImageSource(item.Appearance.Icon, ImageDimension.id16x16)
                            }));
                        }
                    }
                }
                list.Reverse();
                result = (list.Any<string>() ? list : new List<string>
                {
                    Translate.Text("<span>There are no items to display</span>")
                });
            }
            return result;
        }
    }
}