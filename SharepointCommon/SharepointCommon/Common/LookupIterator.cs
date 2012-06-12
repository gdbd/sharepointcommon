namespace SharepointCommon.Common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.SharePoint;

    internal sealed class LookupIterator<T> : IEnumerable<T>
        where T : Item, new()
    {
        private readonly SPFieldLookup _fieldLookup;

        private readonly SPListItem _listItem;

        public LookupIterator(SPFieldLookup fieldLookup, SPListItem listItem)
        {
            _fieldLookup = fieldLookup;
            _listItem = listItem;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var spItemsIter = this.GetLookupItems();
            return spItemsIter.Select(this.Convert).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private T Convert(SPListItem item)
        {
            return (T)EntityMapper.ToEntity(typeof(T), item);
        }

        private IEnumerable<SPListItem> GetLookupItems()
        {
            // Reload item, because it may been changed before lazy load requested

            using (var wf = WebFactory.Open(_listItem.Web.Url))
            {
                var list = wf.Web.Lists[_listItem.ParentList.ID];
                var item = list.GetItemById(_listItem.ID);

                var lkplist = wf.Web.Lists[new Guid(_fieldLookup.LookupList)];
                var lkpValues =
                    new SPFieldLookupValueCollection(
                        item[_fieldLookup.InternalName] != null
                            ? item[_fieldLookup.InternalName].ToString()
                            : string.Empty);

                foreach (var lkpValue in lkpValues)
                {
                    if (lkpValue.LookupId == 0) yield return null;

                    yield return lkplist.GetItemById(lkpValue.LookupId);
                }
            }
        }
    }
}
