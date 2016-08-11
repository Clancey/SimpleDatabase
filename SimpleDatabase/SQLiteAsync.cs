//
// Copyright (c) 2012 Krueger Systems, Inc.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SQLite
{
	public partial class SQLiteAsyncConnection 
	{
		static SQLiteAsyncConnection ()
		{
			//This will globally set the state to Serialized
			SQLite3.Shutdown ();
			SQLite3.Config (SQLite3.ConfigOption.Serialized);
			SQLite3.Initialize ();
		}

		SQLiteConnection _connection;
		SQLiteConnectionWithLock _lockedConnection;
		public SQLiteAsyncConnection (string databasePath, bool storeDateTimeAsTicks = true)
			: this (databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create, storeDateTimeAsTicks)
		{
		}

		public SQLiteAsyncConnection (string databasePath, SQLiteOpenFlags openFlags, bool storeDateTimeAsTicks = true)
		{
			_connection = new SQLiteConnection (databasePath,openFlags, storeDateTimeAsTicks);
			//Put into WAL mode
			_connection.ExecuteScalar<string> ("PRAGMA journal_mode=WAL");
			_lockedConnection = new SQLiteConnectionWithLock (_connection);
		}


		public SQLiteConnectionWithLock GetWriteConnection ()
		{
			return _lockedConnection;
		}
		public SQLiteConnection GetReadConnection ()
		{
			return _connection;
		}
		
		public Task<CreateTablesResult> CreateTableAsync<T> ()
			where T : new ()
		{
			return CreateTablesAsync (typeof(T));
		}
		
		public Task<CreateTablesResult> CreateTablesAsync<T, T2> ()
			where T : new ()
				where T2 : new ()
		{
			return CreateTablesAsync (typeof(T), typeof(T2));
		}
		
		public Task<CreateTablesResult> CreateTablesAsync<T, T2, T3> ()
			where T : new ()
				where T2 : new ()
				where T3 : new ()
		{
			return CreateTablesAsync (typeof(T), typeof(T2), typeof(T3));
		}
		
		public Task<CreateTablesResult> CreateTablesAsync<T, T2, T3, T4> ()
			where T : new ()
				where T2 : new ()
				where T3 : new ()
				where T4 : new ()
		{
			return CreateTablesAsync (typeof(T), typeof(T2), typeof(T3), typeof(T4));
		}
		
		public Task<CreateTablesResult> CreateTablesAsync<T, T2, T3, T4, T5> ()
			where T : new ()
				where T2 : new ()
				where T3 : new ()
				where T4 : new ()
				where T5 : new ()
		{
			return CreateTablesAsync (typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
		}
		
		public Task<CreateTablesResult> CreateTablesAsync (params Type[] types)
		{
			return Task.Factory.StartNew (() => {
				return CreateTables(types);
			});
		}

		public CreateTablesResult CreateTables(params Type[] types)
		{
			CreateTablesResult result = new CreateTablesResult ();
			var conn = GetWriteConnection ();
			using (conn.Lock ()) {
				foreach (Type type in types) {
					try
					{
						int aResult = conn.Connection.CreateTable(type);
						result.Results[type] = aResult;
					}
					catch (Exception)
					{
						Console.WriteLine("Error creating table for {0}",type);
						throw;
					}
				}
			}
			return result;
		}
		
		public Task<int> DropTableAsync<T> ()
			where T : new ()
		{
			return Task.Factory.StartNew (() => {
				var conn = GetWriteConnection ();
				using (conn.Lock ()) {
					return conn.Connection.DropTable<T> ();
				}
			});
		}
		
		public Task<int> InsertAsync (object item)
		{
			return Task.Factory.StartNew (() => {
				return Insert (item);
			});
		}
		
		public int Insert (object item)
		{
			var conn = GetWriteConnection ();
			using (conn.Lock ()) {
				return conn.Connection.Insert (item);
			}
		}
		
		public Task<int> InsertAsync (object item,string extra)
		{
			return Task.Factory.StartNew (() => {
				return Insert (item,extra);
			});
		}
		
		public int Insert (object item,string extra)
		{
			var conn = GetWriteConnection ();
			using (conn.Lock ()) {
				return conn.Connection.Insert (item,extra);
			}
		}
		
		public Task<int> InsertAsync (object item, Type type)
		{
			return Task.Factory.StartNew (() => {
				return Insert (item,type);
			});
		}
		
		public int Insert (object item, Type type)
		{
			var conn = GetWriteConnection ();
			using (conn.Lock ()) {
				return conn.Connection.Insert (item,type);
			}
		}
		
		
		public Task<int> InsertAsync (object item, string extra, Type type)
		{
			return Task.Factory.StartNew (() => {
				return Insert (item,extra,type);
			});
		}
		
		public int Insert (object item, string extra, Type type)
		{
			var conn = GetWriteConnection ();
			using (conn.Lock ()) {
				return conn.Connection.Insert (item,extra,type);
			}
		}

		public Task<int> UpdateAsync (object item)
		{
			return Task.Factory.StartNew (() => {
				return Update (item);
			});
		}
		
		public int Update (object item)
		{
			var conn = GetWriteConnection ();
			using (conn.Lock ()) {
				return conn.Connection.Update (item);
			}
		}

		public Task<int> UpdateAllAsync (IEnumerable items)
		{
			return Task.Factory.StartNew (() => {
				return UpdateAll (items);
			});
		}
		
		public int UpdateAll (IEnumerable items)
		{
			var conn = GetWriteConnection ();
			using (conn.Lock ()) {
				return conn.Connection.UpdateAll (items);
			}
		}
		
		public Task<int> DeleteAsync (object item)
		{
			return Task.Factory.StartNew (() => {
				return Delete (item);
			});
		}
		public int Delete (object item)
		{
			var conn = GetWriteConnection ();
			using (conn.Lock ()) {
				return conn.Connection.Delete (item);
			}
		}

		public int DeleteAll (IEnumerable items)
		{
			var conn = GetWriteConnection ();
			using (conn.Lock ()) {
				return conn.Connection.DeleteAll (items);
			}
		}

		public int DeleteAll (IEnumerable items, Type type)
		{
			var conn = GetWriteConnection ();
			using (conn.Lock ()) {
				return conn.Connection.DeleteAll (items,type);
			}
		}
		
		public Task<T> GetAsync<T> (object pk)
			where T : new()
		{
			return Task.Factory.StartNew (() =>
			{
				var conn = GetReadConnection ();
				return conn.Get<T> (pk);
			});
		}
		
		public Task<T> FindAsync<T> (object pk)
			where T : new ()
		{
			return Task.Factory.StartNew (() => {
				var conn = GetReadConnection ();
				return conn.Find<T> (pk);
			});
		}
		
		public Task<T> GetAsync<T> (Expression<Func<T, bool>> predicate)
			where T : new()
		{
			return Task.Factory.StartNew (() =>
			{
				var conn = GetReadConnection ();
				return conn.Get<T> (predicate);
			});
		}
		
		public Task<T> FindAsync<T> (Expression<Func<T, bool>> predicate)
			where T : new ()
		{
			return Task.Factory.StartNew (() => {
				var conn = GetReadConnection ();
				return conn.Find<T> (predicate);
			});
		}
		
		public Task<int> ExecuteAsync (string query, params object[] args)
		{
			return Task<int>.Factory.StartNew (() => {
				return Execute (query, args);
			});
		}
		
		
		public int Execute (string query, params object[] args)
		{
			var conn = GetWriteConnection ();
			using (conn.Lock ()) {
				return conn.Connection.Execute (query, args);
			}
		}
		
		public Task<int> InsertAllAsync (IEnumerable items)
		{
			return Task.Factory.StartNew (() => {
				return InsertAll (items);
			});
		}
		
		public int InsertAll (IEnumerable items)
		{
			var conn = GetWriteConnection ();
			using (conn.Lock ()) {
				return conn.Connection.InsertAll (items);
			}
		}
		public Task<int> InsertAllAsync (IEnumerable items, string extra)
		{
			return Task.Factory.StartNew (() => {
				return InsertAll (items,extra);
			});
		}
		public int InsertAll (IEnumerable items, string extra)
		{
			var conn = GetWriteConnection ();
			using (conn.Lock ()) {
				return conn.Connection.InsertAll (items,extra);
			}
		}
		public Task<int> InsertAllAsync (IEnumerable items, Type type)
		{
			return Task.Factory.StartNew (() => {
				return InsertAll (items,type);
			});
		}
		public int InsertAll (IEnumerable items, Type type)
		{
			var conn = GetWriteConnection ();
			using (conn.Lock ()) {
				return conn.Connection.InsertAll (items,type);
			}
		}
		
		public Task<int> InsertOrReplaceAsync (object item)
		{
			return Task.Factory.StartNew (() => {
				return InsertOrReplace (item);
			});
		}
		
		public int InsertOrReplace (object item)
		{
			var conn = GetWriteConnection ();
			using (conn.Lock ()) {
				return conn.Connection.InsertOrReplace (item);
			}
		}


		public int InsertOrReplaceAll(IEnumerable items)
		{
			var conn = GetWriteConnection ();
			using (conn.Lock())
			{
				return conn.Connection.InsertOrReplaceAll(items);
			}
		}


		public int InsertOrReplaceAll(IEnumerable items, Type objType)
		{
			var conn = GetWriteConnection ();
			using (conn.Lock())
			{
				return conn.Connection.InsertOrReplaceAll(items,objType);
			}
		}

		
		
		public Task<int> InsertOrReplaceAsync (object item, Type type)
		{
			return Task.Factory.StartNew (() => {
				return InsertOrReplace (item,type);
			});
		}
		
		public int InsertOrReplace (object item, Type type)
		{
			var conn = GetWriteConnection ();
			using (conn.Lock ()) {
				return conn.Connection.InsertOrReplace (item,type);
			}
		}

		public Task RunInTransactionAsync (Action<SQLiteConnection> action)
		{
			return Task.Factory.StartNew (() =>
			{
				var conn = this.GetWriteConnection ();
				using (conn.Lock()) {
					conn.Connection.BeginTransaction ();
					try {
						action (conn.Connection);
						conn.Connection.Commit ();
					} catch (Exception) {
						conn.Connection.Rollback ();
						throw;
					}
				}
			});
		}
		
		public AsyncTableQuery<T> TableAsync<T> ()
			where T : new ()
		{
			//
			// This isn't async as the underlying connection doesn't go out to the database
			// until the query is performed. The Async methods are on the query iteself.
			//
			var conn = GetReadConnection ();
			return new AsyncTableQuery<T> (conn.Table<T> ());
		}
		
		public Task<T> ExecuteScalarAsync<T> (string sql, params object[] args)
		{
			return Task<T>.Factory.StartNew (() => {
				return ExecuteScalar<T> (sql,args);
			});
		}

		public T ExecuteScalar<T> (string sql, params object[] args)
		{
			var conn = GetWriteConnection ();
			using (conn.Lock ()) {
				var command = conn.Connection.CreateCommand (sql, args);
				return command.ExecuteScalar<T> ();
			}
		}
		
		public Task<List<T>> QueryAsync<T> (string sql, params object[] args)
			where T : new ()
		{
			return Task<List<T>>.Factory.StartNew (() => {
					return Query<T> (sql, args);
			});
		}
		public List<T> Query<T> (string sql, params object[] args)
			where T : new ()
		{
				var conn = GetReadConnection ();
				return conn.Query<T> (sql, args);
		}
	}
	
	//
	// TODO: Bind to AsyncConnection.GetConnection instead so that delayed
	// execution can still work after a Pool.Reset.
	//
	public class AsyncTableQuery<T>
		where T : new ()
	{
		TableQuery<T> _innerQuery;
		
		public AsyncTableQuery (TableQuery<T> innerQuery)
		{
			_innerQuery = innerQuery;
		}
		
		public AsyncTableQuery<T> Where (Expression<Func<T, bool>> predExpr)
		{
			return new AsyncTableQuery<T> (_innerQuery.Where (predExpr));
		}
		
		public AsyncTableQuery<T> Skip (int n)
		{
			return new AsyncTableQuery<T> (_innerQuery.Skip (n));
		}
		
		public AsyncTableQuery<T> Take (int n)
		{
			return new AsyncTableQuery<T> (_innerQuery.Take (n));
		}
		
		public AsyncTableQuery<T> OrderBy<U> (Expression<Func<T, U>> orderExpr)
		{
			return new AsyncTableQuery<T> (_innerQuery.OrderBy<U> (orderExpr));
		}
		
		public AsyncTableQuery<T> OrderByDescending<U> (Expression<Func<T, U>> orderExpr)
		{
			return new AsyncTableQuery<T> (_innerQuery.OrderByDescending<U> (orderExpr));
		}
		
		public Task<List<T>> ToListAsync ()
		{
			return Task.Factory.StartNew (() => {
				return _innerQuery.ToList ();
			});
		}
		
		public Task<int> CountAsync ()
		{
			return Task.Factory.StartNew (() => {
				return _innerQuery.Count ();
			});
		}
		
		public Task<T> ElementAtAsync (int index)
		{
			return Task.Factory.StartNew (() => {
				return _innerQuery.ElementAt (index);
			});
		}
		
		public Task<T> FirstAsync ()
		{
			return Task<T>.Factory.StartNew (() => {
				return _innerQuery.First ();
			});
		}
		
		public Task<T> FirstOrDefaultAsync ()
		{
			return Task<T>.Factory.StartNew (() => {
				return _innerQuery.FirstOrDefault ();
			});
		}
	}
	
	public class CreateTablesResult
	{
		public Dictionary<Type, int> Results { get; private set; }
		
		internal CreateTablesResult ()
		{
			this.Results = new Dictionary<Type, int> ();
		}
	}

	public class SQLiteConnectionWithLock
	{
		readonly object _lockPoint = new object ();
		public SQLiteConnection Connection { get; private set; }
		public SQLiteConnectionWithLock (SQLiteConnection connection)
		{
			Connection = connection;
		}

		public IDisposable Lock ()
		{
			return new LockWrapper (_lockPoint);
		}

		private class LockWrapper : IDisposable
		{
			object _lockPoint;

			public LockWrapper (object lockPoint)
			{
				_lockPoint = lockPoint;
				Monitor.Enter (_lockPoint);
			}

			public void Dispose ()
			{
				Monitor.Exit (_lockPoint);
			}
		}
	}
}
