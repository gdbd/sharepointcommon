## Create event receiver class:
{{
public class AddedReceiver : ListEventReceiver<AddedItem>
{
      [Async](Async)
      public override void ItemAdded(AddedItem addedItem)
      {          
      }
      
      [Sequence(10500)](Sequence(10500))
      public override void ItemAdding(AddingItem addedItem)
      {        
      }

      public override void ItemUpdating(UpdatingDoc orig, UpdatingDoc changed)
      {    
      }

      public override void ItemDeleted(int id)
      {        
      }
}
}}

# full list of supported events:

{{
public abstract class ListEventReceiver<T> where T : Item, new()
{
      public virtual void ItemAdding(T addingItem) { }
      public virtual void ItemAdded(T addedItem) { }
      public virtual void ItemUpdating(T originalItem, T changedItem) { }
      public virtual void ItemUpdated(T updatedItem) { }
      public virtual void ItemDeleting(T deletingItem) { }
      public virtual void ItemDeleted(int deletedItemId) { }
}
}}

# Attributes to mark event handlers:
**AsyncAttribute** - Mark methods to set event receiver is synchronous or asynchronous
**SequenceAttribute** - Mark methods to set event receiver sequence.

# Add and remove subscription

{{
var list = WebFactory.CurrentContext().GetByUrl<Contract>("lists/contact"); 
list.AddEventReceiver<ContractsReceiver>();
list.RemoveEventReceiver<ContractsReceiver>();
}}

# Notes
	* receivers called by framework through reflection. Info about registered type stored in Data of sharepoint event receiver

	* some combinations of methods and sync/async attribute is invalid and will throw exception(platform limitation)

	* use can use same Item or Document in type argument of receiver to handle list or document library events
