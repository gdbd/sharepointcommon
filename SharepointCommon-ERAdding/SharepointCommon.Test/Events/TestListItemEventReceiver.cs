using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using SharepointCommon.Attributes;
using SharepointCommon.Test.Entity;

namespace SharepointCommon.Test.Events
{
    public class TestListItemEventReceiver : ListEventReceiver<CustomItem>
    {
        
        protected override void ItemAdding(CustomItem addingItem)
        {
            base.ItemAdding(addingItem);
            //addingItem.CustomBoolean = false;
            var list = addingItem.ParentList.ParentWeb.GetByName<Item>("ListForLookup");
            var items = list.Items(
                new CamlQuery().Query(Q.Where(Q.Eq(Q.FieldRef<CustomItem>(f => f.Title), Q.Value("Add_Adds_CustomItem_Test_Lookup_4")))));
            addingItem.CustomMultiLookup = items;
            addingItem.CustomUser = addingItem.CustomUsers.LastOrDefault();
        }

        [Async(false)]
        protected override void ItemAdded(CustomItem addedItem)
        {
            base.ItemAdded(addedItem);
            addedItem.CustomBoolean = false;
            addedItem.CustomFieldNumber = 321.8;
            addedItem.ParentList.Update(addedItem, false);
        }

        protected override void ItemUpdating(CustomItem updatingItem, CustomItem changedItem)
        {
            base.ItemUpdating(updatingItem, changedItem);
        }

        [Async(false)]
        protected override void ItemUpdated(CustomItem updatedItem)
        {
            base.ItemUpdated(updatedItem);
        }
    }
}
