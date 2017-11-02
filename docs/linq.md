## Filter by text value:
{{
var items = list.Items().Where(i => i.Title == "test");
}}
Converted to CAML:
{{
<Where>
  <Eq>     
     <FieldRef  Name=”Title” /> 
     <Value Type=”Text”>test</Value>
  </Eq>
</Where>
}}

## Filter value not null
{{
var items = list.Items().Where(i => i.Title != null)
}}
Converted to CAML:
{{
<Where>
  <IsNotNull>
     <FieldRef Name="Title" />
  </IsNotNull>
</Where>
}}

## Limit count of result:
{{
var items = list.Items().Take(3);
}}
Converted to CAML:
{{
<Query/>
<RowLimit>3<RowLimit>
}}

## Select to limit ViewFields:
{{
var items = list.Items().Select(new { i.Title} );
}}

Converted to CAML:
{{
<Query/>
<ViewFields>
  <FieldRef Name="Title" />
</ViewFields>
}}

# In-Memory calculations
## Sum
{{
var items = list.Items().Sum(i => i.Id)
}}
## Skip
{{
var items = list.Items().Skip(5)
}}

## Warning: Other operations are still not tested and may work or not while 3.0 in preview

For more details see source of unit tests