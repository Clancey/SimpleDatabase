using System;
using SQLite;

namespace Xamarin.Data
{
	class InstantDatabaseGroup : GroupInfo
	{
		[Indexed]
		public string ClassName {get;set;}
		[Indexed]
		public int RowCount {get;set;}
		public int Order {get;set;}
		[Ignore]
		public bool Loaded {get;set;}
	}
	public class GroupInfo
	{
		[Indexed]
		public string GroupBy {get;set;}
		public string GroupString {get;set;}
		[Indexed]
		public string OrderBy { get; set; }
		[Indexed]
		public string Filter { get; set;}

		public string FilterString(bool includeWhere)
		{
			if (string.IsNullOrEmpty (Filter))
				return "";
			string filter = includeWhere ? " where " : " and ";
			filter += Filter;
			return filter;
		}

		public override bool Equals (object obj)
		{
			if (obj is GroupInfo) {
				var obj2 = ((GroupInfo)obj);
				var isTrue = this.GroupBy == obj2.GroupBy && this.Filter == obj2.Filter && this.OrderBy == obj2.OrderBy;
				return isTrue;
			}
			return false;
		}
		
		public static bool operator == (GroupInfo x, GroupInfo y)
		{
			if (object.ReferenceEquals (x, y)) {
				// handles if both are null as well as object identity
				return true;
			}
			
			if ((object)x == null || (object)y == null) {
				return false;
			}
			return x.Equals (y);
		}
		
		public static bool operator != (GroupInfo x, GroupInfo y)
		{
			if (object.ReferenceEquals (x, y)) {
				// handles if both are null as well as object identity
				return false;
			}
			
			if ((object)x == null || (object)y == null) {
				return true;
			}
			return !x.Equals (y);
		}
		public override string ToString ()
		{
			return string.Format ("[GroupInfo: GroupBy={0}, OrderBy={1}, Filter={2}]", GroupBy, OrderBy, Filter);
		}

	}
}

