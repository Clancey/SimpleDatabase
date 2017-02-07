using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SQLite
{
	public static class SqliteExtensions
	{
		public static Dictionary<Type,int> CreateTables(this SQLiteConnection connection, params Type[] types)
		{
			//CreateTablesResult Result has internal constructor
			var results = new Dictionary<Type, int>();
			foreach (Type type in types)
			{
				try
				{
					int aResult = connection.CreateTable(type);
					results[type] = aResult;
				}
				catch (Exception)
				{
					Debug.WriteLine("Error creating table for {0}", type);
					throw;
				}
			}

			return results;
		}

		public static int Delete(this SQLiteConnection connection, object objectToDelete, Type type)
		{
			var map = connection.GetMapping(type);
			var pk = map.PK;
			if (pk == null)
			{
				throw new NotSupportedException("Cannot delete " + map.TableName + ": it has no PK");
			}
			var q = string.Format("delete from \"{0}\" where \"{1}\" = ?", map.TableName, pk.Name);
			var count = connection.Execute(q, pk.GetValue(objectToDelete));

			return count;
		}
	
		public static int InsertOrReplaceAll( this SQLiteConnection connection, IEnumerable objects, bool runInTransaction = true)
		{
			var c = 0;
			if (runInTransaction)
			{
				connection.RunInTransaction(() =>
				{
					foreach (var r in objects)
					{
						c += connection.InsertOrReplace(r);
					}
				});
			}
			else
			{
				foreach (var r in objects)
				{
					c += connection.InsertOrReplace(r);
				}
			}
			return c;
		}

		public static int InsertOrReplaceAll(this SQLiteConnection connection, IEnumerable objects, Type objType, bool runInTransaction = true)
		{
			var c = 0;
			if (runInTransaction)
			{
				connection.RunInTransaction(() =>
				{
					foreach (var r in objects)
					{
						c += connection.InsertOrReplace(r, objType);
					}
				});
			}
			else
			{
				foreach (var r in objects)
				{
					c += connection.InsertOrReplace(r);
				}
			}
			return c;
		}

		public static int DeleteAll(this SQLiteConnection connection, IEnumerable objects)
		{
			var c = 0;
			connection.RunInTransaction(() =>
			{
				foreach (var r in objects)
				{
					c += connection.Delete(r);
				}
			});
			return c;
		}
		public static int DeleteAll(this SQLiteConnection connection, IEnumerable objects, Type type)
		{
			var c = 0;
			connection.RunInTransaction(() =>
			{
				foreach (var r in objects)
				{
					c += connection.Delete(r, type);
				}
			});
			return c;
		}

	}
}
