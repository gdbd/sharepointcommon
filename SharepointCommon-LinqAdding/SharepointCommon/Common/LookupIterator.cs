namespace SharepointCommon.Common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.SharePoint;

    internal sealed class LookupIterator<T> : IEnumerable<T> where T : Item, new()
    {
        private readonly SPField _fieldLookup;

        private readonly SPListItem _listItem;

        public LookupIterator(SPField fieldLookup, SPListItem listItem)
        {
            _fieldLookup = fieldLookup;
            _listItem = listItem;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var spItemsIter = GetLookupItems();
            return spItemsIter.Select(Convert).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private T Convert(SPListItem item)
        {
            return (T)EntityMapper.ToEntity(typeof(T), item);
        }

        private IEnumerable<SPListItem> GetLookupItems()
        {
            if (_fieldLookup.Type == SPFieldType.Lookup)
            {
                var spfl = (SPFieldLookup)_fieldLookup;

                // Reload item, because it may been changed before lazy load requested

                using (var wf = WebFactory.Open(_listItem.Web.Url))
                {
                    var list = wf.Web.Lists[_listItem.ParentList.ID];
                    var item = list.GetItemById(_listItem.ID);

                    var lkplist = wf.Web.Lists[new Guid(spfl.LookupList)];
                    var lkpValues =
                        new SPFieldLookupValueCollection(
                            item[spfl.InternalName] != null
                                ? item[spfl.InternalName].ToString()
                                : string.Empty);

                    foreach (var lkpValue in lkpValues)
                    {
                        if (lkpValue.LookupId == 0) yield return null;

                        yield return lkplist.GetItemById(lkpValue.LookupId);
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
