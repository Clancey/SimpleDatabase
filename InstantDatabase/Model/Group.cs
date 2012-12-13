using System;
using SQLite;

namespace Xamarin.Data
{
	public class InstantDatabaseGroup
	{
		[Indexed]
		public string ClassName {get;set;}
		[Indexed]
		public string Grouping {get;set;}
		[Indexed]
		public int RowCount {get;set;}
		public int Order {get;set;}
		[Ignore]
		public bool Loaded {get;set;}
	}
}

