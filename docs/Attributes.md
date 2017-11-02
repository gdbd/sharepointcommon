## Attributes
# ContentTypeAttribute
Used to mark entity class as Sharepoint ContentType. 
It means:
* **IQueryWeb.Create** will create list and add content type to it
* **IQueryList.Items** will filter returned items by content type
# FieldAttribute
Used to mark entity properties to provide additional information about mapping:
* **Name**  of field on which property maps
* **LookupList** name of list on which references lookup field
# NotFieldAttribute
Used on entity properties to inform that property not mapped to sharepoint field