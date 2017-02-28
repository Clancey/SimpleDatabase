Simple Database
================
Creating fast, responsive and grouped Tableviews is hard. Espcially with very large data sets.  Simple Database takes care of this for you.

Available on Nuget
================

https://www.nuget.org/packages/Clancey.SimpleDatabase/


API
================

Simple tables gives you the interface you need to populate a ListView

```cs
Database.Main.RowsInSection<T>(section);

Database.Main.NumberOfSections<T>();

Database.Main.ObjectForRow<T>(section, row);

Database.Main.SectionHeader<T>(section);

Database.Main.QuickJump<T>();

```

Model Attributes
===
Add attributes for OrderBy and Grouping

```cs

class MyClass
{
	[Indexed, PrimaryKey]
	public string Id {get;set;}

	//Typically this is just one letter, and the first letter of the displayed text
	[GroupBy]
	public virtual string IndexCharacter {get;set;}

	[OrderBy]
	public string Name {get;set;}
}
```


GroupInfo
================

Sometimes you need to filter or Add dynamically compose a query. Simple Auth uses named parameters

```cs
var group = Database.Main.GetGroupInfo<Song>().Clone();
group.Filter = "ArtistId = @ArtistId";
group.Params["@ArtistId"] = value.Id;

Database.Main.RowsInSection<Song>(group , section);

```


