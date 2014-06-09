namespace SharepointCommon.Common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.SharePoint;

    internal sealed class LookupIterator<T> : IEnumerable<T> where T : Item, new()
    {
        private readonly SPFieldLookup _fieldLookup;

        private readonly SPListItem _listItem;
        private readonly bool _reloadLookupItem;

        private readonly object _lookupValue;

        private readonly SPList _list;
        public LookupIterator(SPFieldLookup fieldLookup, SPListItem listItem, bool reloadLookupItem = true)
        {
            _fieldLookup = fieldLookup;
            _listItem = listItem;
            _reloadLookupItem = reloadLookupItem;
        }

        public LookupIterator(SPList list, SPFieldLookup fieldLookup, object value, bool reloadLookupItem = true)
        {
            _fieldLookup = fieldLookup;
            _list = list;
            _lookupValue = value;
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
            if (_listItem != null)
            {
                var wf = _listItem.ParentList;
                var item = _listItem;
                if (_reloadLookupItem)
                {
                    // Reload item, because it may been changed before lazy load requested
                    var list = wf.Lists[_listItem.ParentList.ID];
                    item = list.GetItemById(_listItem.ID);
                }

                var lkplist = wf.Lists[new Guid(_fieldLookup.LookupList)];
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
            else
            {
                using (var wf = WebFactory.Open(_list.ParentWeb.Url))
                {
                    var lkpValues = (SPFieldLookupValueCollection)_lookupValue;
                    var lkplist = wf.Web.Lists[new Guid(_fieldLookup.LookupList)];
                foreach (var lkpValue in lkpValues)
                {
                    if (lkpValue.LookupId == 0) yield return null;

                    yield return lkplist.GetItemById(lkpValue.LookupId);
                }
                }
                
            }
         
        }
    }
}
