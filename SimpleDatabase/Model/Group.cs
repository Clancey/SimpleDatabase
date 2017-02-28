using System;
using SQLite;
using System.Collections.Generic;
using System.Linq;

namespace SimpleDatabase
{
	class SimpleDatabaseGroup : GroupInfo
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
		public GroupInfo()
		{
			Filter = "";
			From = "";
		}
		[Indexed]
		public string GroupBy {get;set;}
		public Dictionary<string, object> Params { get; set;}= new Dictionary<string, object>();
		public bool OrderByDesc { get; set;}
		public string GroupString {get;set;}
		public bool GroupOrderByDesc { get; set;}
		[Indexed]
		public string OrderBy { get; set; }
		[Indexed]
		public string Filter { get; set;}

		public string From { get; set;}

		public int Limit {get;set;}

		public GroupInfo Clone ()
		{
			return new GroupInfo
			{
				GroupBy = this.GroupBy,
				OrderByDesc = this.OrderByDesc,
				GroupString = this.GroupString,
				GroupOrderByDesc = this.GroupOrderByDesc,
				OrderBy = this.OrderBy,
				Filter = this.Filter,
				From = this.From,
				Limit = this.Limit,
				Params = Params?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, object>(),
			};
		}

		public string FromString(string table)
		{
			return $" {table} {From} ";
		}

		public string OrderByString(bool includeOrerBy = true)
		{
			
			if (string.IsNullOrEmpty (OrderBy))
				return "";
			string orderby = includeOrerBy ? " order by " : " , ";
			orderby += (string.IsNullOrEmpty(GroupBy) ? "" : GroupBy + (GroupOrderByDesc ? " desc " : "") + " , ") + OrderBy + ( OrderByDesc ? " desc " : "");
			return orderby;
		}

		public string FilterString(bool includeWhere)
		{
			if (string.IsNullOrEmpty (Filter))
				return "";
			string filter = includeWhere ? " where " : " and ";
			filter += Filter;
			return filter;
		}

		public void AddFilter(string filter)
		{
			if (string.IsNullOrEmpty (Filter))
				Filter = filter;
			else
				Filter += " and " + filter;
		}

		public string LimitString()
		{
			return (Limit > 0 ? " Limit " + Limit : " ");
		}

		public override bool Equals (object obj)
		{
			return obj.ToString() == this.ToString();
		}

		public override int GetHashCode()
		{
			return this.ToString().GetHashCode();
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
			return $"[GroupInfo: GroupBy={GroupBy}, OrderBy={OrderBy}, Filter={Filter}, From={From} ,Params{string.Join(",", Params)}]";
		}

		public Tuple<string, object[]> ConvertSqlFromNamed(string sql, Dictionary<string, object> injectedParams = null)
		{
			return ConvertSqlFromNamed(sql,Params,injectedParams);
		}

		public static Tuple<string, object[]> ConvertSqlFromNamed(string sql, Dictionary<string, object> namedParameters, Dictionary<string,object> injectedParams = null)
		{
			var foundParamters = sql.Split(' ').Where(x => x.StartsWith("@")).Select(x => x.Trim().TrimEnd(')')).ToList();
			var hasQuestion = sql.Contains("?");
			if (hasQuestion)
			{
				throw new Exception("Please covert to named parameters");
			}

			string returnSql = sql;
			List<object> parameterValues = new List<object>();
			foreach (var param in foundParamters)
			{
				object value;
				returnSql = returnSql.Replace(param, "?");
				if (!namedParameters.TryGetValue(param, out value) && !(injectedParams?.TryGetValue(param, out value) ?? false))
					throw new Exception($"\"{param}\" was not found in the Named Parameters");
				parameterValues.Add(value);
			}
			return new Tuple<string, object[]>(returnSql, parameterValues.ToArray());
		}

	}
}

