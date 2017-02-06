using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SQLite
{
	public static class SqliteExtensions
	{
		public static CreateTablesResult CreateTables(this SQLiteAsyncConnection connection, params Type[] types)
		{
			//Result has internal constructor
			CreateTablesResult result = (CreateTablesResult)Activator.CreateInstance(typeof(CreateTablesResult));
			var conn = connection.GetWriteConnection();
			using (conn.Lock())
			{
				foreach (Type type in types)
				{
					try
					{
						int aResult = conn.CreateTable(type);
						result.Results[type] = aResult;
					}
					catch (Exception)
					{
						Debug.WriteLine("Error creating table for {0}", type);
						throw;
					}
				}
			}
			return result;
		}
		public static CreateTablesResult CreateTables(this SQLiteConnection connection, params Type[] types)
		{
			//Result has internal constructor
			CreateTablesResult result = (CreateTablesResult)Activator.CreateInstance(typeof(CreateTablesResult));
			foreach (Type type in types)
			{
				try
				{
					int aResult = connection.CreateTable(type);
					result.Results[type] = aResult;
				}
				catch (Exception)
				{
					Debug.WriteLine("Error creating table for {0}", type);
					throw;
				}
			}

			return result;
		}
		public static int Execute( this SQLiteAsyncConnection connection,string query, params object[] args)
		{
			var conn = connection.GetWriteConnection();
			using (conn.Lock())
			{
				return conn.Execute(query, args);
			}
		}

		public static List<T> Query<T>(this SQLiteAsyncConnection connection,string sql, params object[] args)
			where T : new()
		{
			var conn = connection.GetReadConnection();
			return conn.Query<T>(sql, args);
		}

		public static T ExecuteScalar<T>(this SQLiteAsyncConnection connection,string sql, params object[] args)
		{
			var conn = connection.GetWriteConnection();
			using (conn.Lock())
			{
				var command = conn.CreateCommand(sql, args);
				return command.ExecuteScalar<T>();
			}
		}


		public static int InsertAll(this SQLiteAsyncConnection connection,IEnumerable items)
		{
			var conn = connection.GetWriteConnection();
			using (conn.Lock())
			{
				return conn.InsertAll(items);
			}
		}

		public static int InsertAll(this SQLiteAsyncConnection connection,IEnumerable items, string extra)
		{
			var conn = connection.GetWriteConnection();
			using (conn.Lock())
			{
				return conn.InsertAll(items, extra);
			}
		}
		public static Task<int> InsertAllAsync(this SQLiteAsyncConnection connection,IEnumerable items, string extra)
		{
			return Task.Factory.StartNew(() =>
			{
				return connection.InsertAll(items, extra);
			});
		}

		public static int Insert(this SQLiteAsyncConnection connection, object item)
		{
			var conn = connection.GetWriteConnection();
			using (conn.Lock())
			{
				return conn.Insert(item);
			}
		}

		public static int Insert(this SQLiteAsyncConnection connection, object item, string extra)
		{
			var conn = connection.GetWriteConnection();
			using (conn.Lock())
			{
				return conn.Insert(item, extra);
			}
		}

		public static int Insert(this SQLiteAsyncConnection connection, object item, Type extra)
		{
			var conn = connection.GetWriteConnection();
			using (conn.Lock())
			{
				return conn.Insert(item, extra);
			}
		}


		public static Task<int> InsertAsync(this SQLiteAsyncConnection connection, object item, string extra)
		{
			return Task.Run(() => connection.Insert(item, extra));
		}

		public static int InsertAll(this SQLiteAsyncConnection connection, IEnumerable items, Type objtype)
		{
			var conn = connection.GetWriteConnection();
			using (conn.Lock())
			{
				return conn.InsertAll(items, objtype);
			}
		}

		public static int InsertOrReplace(this SQLiteAsyncConnection connection, object item)
		{
			var conn = connection.GetWriteConnection();
			using (conn.Lock())
			{
				return conn.InsertOrReplace(item);
			}
		}

		public static int InsertOrReplace(this SQLiteAsyncConnection connection, object item, Type objtype)
		{
			var conn = connection.GetWriteConnection();
			using (conn.Lock())
			{
				return conn.InsertOrReplace(item,objtype);
			}
		}



		public static int InsertOrReplaceAll(this SQLiteAsyncConnection connection, IEnumerable items)
		{
			var conn = connection.GetWriteConnection();
			using (conn.Lock())
			{
				return conn.InsertAll(items,"OR REPLACE");
			}
		}

		public static int InsertOrReplaceAll(this SQLiteAsyncConnection connection, IEnumerable items, Type objType)
		{
			var conn = connection.GetWriteConnection();
			using (conn.Lock())
			{
				var c = 0;

				conn.RunInTransaction(() =>
				{
					foreach (var r in items)
					{
						c += conn.InsertOrReplace(r, objType);
					}
				});

				return c;
			}
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
		public static int Delete(this SQLiteAsyncConnection connection, object item)
		{
			var conn = connection.GetWriteConnection();
			using (conn.Lock())
			{
				return conn.Delete(item);
			}
		}

		public static int DeleteAll(this SQLiteAsyncConnection connection, IEnumerable items)
		{
			var conn = connection.GetWriteConnection();
			using (conn.Lock())
			{
				var c = 0;

				conn.RunInTransaction(() =>
				{
					foreach (var r in items)
					{
						c += conn.Delete(r);
					}
				});

				return c;
			}
		}

		public static int DeleteAll(this SQLiteAsyncConnection connection, IEnumerable items, Type objType)
		{
			var conn = connection.GetWriteConnection();
			using (conn.Lock())
			{
				var c = 0;

				conn.RunInTransaction(() =>
				{
					foreach (var r in items)
					{
						c += conn.Delete(r,objType);
					}
				});

				return c;
			}
		}

		public static int Update(this SQLiteAsyncConnection connection, object item)
		{
			var conn = connection.GetWriteConnection();
			using (conn.Lock())
			{
				return conn.Update(item);
			}
		}

		public static int UpdateAll(this SQLiteAsyncConnection connection, IEnumerable items)
		{
			var conn = connection.GetWriteConnection();
			using (conn.Lock())
			{
				return conn.UpdateAll(items);
			}
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
