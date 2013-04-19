using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using System.Reflection;
using System.Collections;
using System.Threading.Tasks;
//using Java.Lang;
using System.Threading;

namespace Xamarin.Data
{

	public static class Tracer
	{
		public static void Trace(string message)
		{
			return;
			var stackTrace = new System.Diagnostics.StackTrace();
			Console.WriteLine(stackTrace.GetFrame(3).GetMethod().Name + " " + message);
		}
	}
	public class ThreadLock: IDisposable{

		enum Status{
			Acquiring,
			Acquired,
		}
		Status status;
		Object objLock;
		static Thread lockOwner;
		public static ThreadLock Lock(object objLock)
		{         
			return new ThreadLock(objLock);  
			       
		}
		
		public ThreadLock(object objLock)
		{

			this.status = Status.Acquiring; //useful for detecting dead-lock
			this.objLock = objLock; 
			
			//Console.WriteLine("Lock {0}",status);
			//collect useful information about the context such 
			//as stacktrace, time to acquire the lock(T1)
			Monitor.Enter(objLock); 

			//lockOwner = Thread.CurrentThread;
			this.status = Status.Acquired; 
			//Console.WriteLine("Lock {0}",status);
			//lock is acuired, so collect acquired-time(T2)
			//[T2-T1 = time taken to acquire lock]
		}
		
		public void Dispose()
		{

			Monitor.Exit(objLock);
			//Console.WriteLine("Lock Ended");
			//T3: activity in a lock is over
			//Serialize this class for doing analysis of thread-lock activity time 
		}
	}
	public class InstantDatabase 
	{
		Dictionary<Tuple<Type,string>,Dictionary<int,Dictionary<int,Object>>> MemoryStore = new Dictionary<Tuple<Type,string>, Dictionary<int, Dictionary<int, object>>> ();
		Dictionary<Type,Dictionary<object,object>> ObjectsDict = new Dictionary<Type, Dictionary<object, object>> ();
		Dictionary<Type,List<object>> Objects = new Dictionary<Type, List<object>> ();
		Dictionary<Tuple<Type,string>,List<InstantDatabaseGroup>> Groups = new Dictionary<Tuple<Type,string>, List<InstantDatabaseGroup>> ();
		Dictionary<Type,GroupInfo> GroupInfoDict = new Dictionary<Type, GroupInfo> ();
		public static object Locker = new object ();
		object groupLocker = new object ();
		object memStoreLocker = new object ();
		SQLiteConnection connection;
		public InstantDatabase(SQLiteConnection sqliteConnection)
		{
			connection = sqliteConnection;
			init ();
		}
		public InstantDatabase (string databasePath, bool storeDateTimeAsTicks = false)
		{
			connection = new SQLiteConnection (databasePath, true);
			init ();
		}

		void init ()
		{
#if iOS
			MonoTouch.Foundation.NSNotificationCenter.DefaultCenter.AddObserver("UIApplicationDidReceiveMemoryWarningNotification",delegate{
				ClearMemory();
			});
#endif
		}

		public void MakeClassInstant<T> (GroupInfo info)
		{
			MakeClassInstant (typeof(T), info);
		}

		public void MakeClassInstant<T> ()
		{
			var t = typeof(T);
			MakeClassInstant (t);
		}

		public void MakeClassInstant (Type type)
		{
			MakeClassInstant (type, null);
			
		}
		
		public void MakeClassInstant (Type type, GroupInfo info)
		{
			if(info == null)
				info = GetGroupInfo(type);
			SetGroups (type, info);
			FillGroups (type, info);
			if (Groups [new Tuple<Type,string> (type, info.ToString())].Count () == 0)
				SetGroups (type, info);

		}

		public GroupInfo GetGroupInfo<T> ()
		{
			return GetGroupInfo (typeof(T));
		}

		public GroupInfo GetGroupInfo (Type type)
		{
			if (GroupInfoDict.ContainsKey (type))
				return GroupInfoDict [type];
			var groupBy = GetGroupByProperty (type);
			var orderBy = GetOrderByProperty (type);
			var groupInfo = new GroupInfo ();
			if (groupBy != null)
				groupInfo.GroupBy = groupBy.Name;
			if (orderBy != null)
				groupInfo.OrderBy = orderBy.Name;
			GroupInfoDict.Add (type, groupInfo);
			return groupInfo;
		}

		private void SetGroups (Type type, GroupInfo groupInfo)
		{
			List<InstantDatabaseGroup> groups = CreateGroupInfo (type, groupInfo);

			var tuple = new Tuple<Type,string> (type, groupInfo.ToString());
			using(ThreadLock.Lock(groupLocker)) {
				if (Groups.ContainsKey (tuple))
					Groups [tuple] = groups;
				else
					Groups.Add (tuple, groups);
			}
			
		}

		private List<InstantDatabaseGroup> CreateGroupInfo(Type type, GroupInfo groupInfo)
		{
			List<InstantDatabaseGroup> groups;
			lock(Locker){
				if (string.IsNullOrEmpty (groupInfo.GroupBy))
				groups = new List<InstantDatabaseGroup> (){new InstantDatabaseGroup{GroupString = ""}};
				else {
					var query = string.Format ("select distinct {1} as GroupString from {0} {3} order by {2} {4}", type.Name, groupInfo.GroupBy, groupInfo.OrderBy, groupInfo.FilterString(true),groupInfo.LimitString());
					groups = connection.Query<InstantDatabaseGroup> (query).ToList ();
				}
				//var deleteQuery = string.Format ("delete from InstantDatabaseGroup where ClassName = ? and GroupBy = ? and OrderBy = ? and Filter = ?");
				//int deleted = connection.Execute (deleteQuery, type.Name, groupInfo.GroupBy, groupInfo.OrderBy, groupInfo.Filter);
			}
			for (int i = 0; i < groups.Count(); i++) {
				var group = groups [i];
				group.ClassName = type.Name;
				group.Filter = groupInfo.Filter ?? "";
				group.GroupBy = groupInfo.GroupBy ?? "";
				group.OrderBy = groupInfo.OrderBy ?? "";
				group.Order = i;
				string rowQuery;
				if (string.IsNullOrEmpty (groupInfo.GroupBy))
					rowQuery = string.Format ("select count(*) from {0} {1} {2}", type.Name, groupInfo.FilterString (true),groupInfo.LimitString());
				else
					rowQuery = string.Format ("select count(*) from {0} where {1} = ? {2} {3}", type.Name, groupInfo.GroupBy, groupInfo.FilterString (false),groupInfo.LimitString());
				//lock(Locker){
					group.RowCount = connection.ExecuteScalar<int> (rowQuery, group.GroupString);
				//}
			}
			return groups;
		}

		public void UpdateInstant<T> (GroupInfo info)
		{
			UpdateInstant (typeof(T), info);
		}

		public void UpdateInstant<T> ()
		{
			UpdateInstant (typeof(T));	
		}

		public void UpdateInstant (Type type)
		{
			UpdateInstant (type, null);
		}
		
		public void UpdateInstant (Type type, GroupInfo info)
		{
			if(info == null)
				info = GetGroupInfo (type);
			var tuple = new Tuple<Type,string> (type, info.ToString());
			using(ThreadLock.Lock(memStoreLocker)) {
				if (MemoryStore.ContainsKey (tuple)) {
					MemoryStore [tuple] = new Dictionary<int, Dictionary<int, object>> ();
				}
			}
			FillGroups (type, info);
			
		}

		public void ClearMemory ()
		{
			using(ThreadLock.Lock (memStoreLocker)) {
				MemoryStore.Clear ();
				ObjectsDict.Clear ();
				Objects.Clear ();
				//GC.Collect ();
			}
		}
		public void ClearMemoryStore()
		{
			using(ThreadLock.Lock (memStoreLocker)) {
				MemoryStore.Clear ();
				//GC.Collect ();
			}
		}
		public void ClearMemory<T>(GroupInfo groupInfo)
		{
			var t = typeof(T);
			ClearMemory(t,groupInfo);
		}
		public void ClearMemory(Type type, GroupInfo groupInfo)
		{
			var tuple = new Tuple<Type,string> (type, groupInfo.ToString());
			using(ThreadLock.Lock(memStoreLocker)){
				MemoryStore.Remove (tuple);
			}
		}

		public string SectionHeader<T> (int section)
		{
			return SectionHeader<T> (GetGroupInfo (typeof(T)), section);
		}

		public string SectionHeader<T> (GroupInfo info, int section)
		{
			if (info == null)
				info = GetGroupInfo<T> ();

			using(ThreadLock.Lock (groupLocker)) {
				var t = typeof(T);
				var tuple = new Tuple<Type,string> (t, info.ToString());
				if (!Groups.ContainsKey (tuple) || Groups [tuple].Count<= section)
					FillGroups (t, info);
				return Groups [tuple] [section].GroupString;
			}
		}
		
		public string [] QuickJump<T> ()
		{
			return QuickJump<T> (GetGroupInfo<T> ());
		}

		public string [] QuickJump<T> (GroupInfo info)
		{
			if (info == null)
				info = GetGroupInfo<T> ();
			using(ThreadLock.Lock (groupLocker)) {
				var t = typeof(T);
				var tuple = new Tuple<Type,string> (t, info.ToString());
				if (!Groups.ContainsKey (tuple))
					FillGroups (t, info);
				var groups = Groups [tuple];
				var strings = groups.Select (x => string.IsNullOrEmpty (x.GroupString) ? "" : x.GroupString [0].ToString ()).ToArray ();
				return strings;
			}
		}
		
		public int NumberOfSections<T> ()
		{
			return NumberOfSections<T> (GetGroupInfo<T> ());
		}

		public int NumberOfSections<T> (GroupInfo info)
		{
			if (info == null)
				info = GetGroupInfo<T> ();
			using(ThreadLock.Lock (groupLocker)) {
				var t = typeof(T);
				var tuple = new Tuple<Type,string> (t, info.ToString());
				if (!Groups.ContainsKey (tuple))
					FillGroups (t, info);			
				return Groups [tuple].Count;
			}
		}

		public int RowsInSection<T> (int section)
		{
			return RowsInSection<T> (GetGroupInfo<T> (), section);
		}

		public int RowsInSection<T> (GroupInfo info, int section)
		{
			if (info == null)
				info = GetGroupInfo<T> ();
			using(ThreadLock.Lock (groupLocker)) {
				var group = GetGroup<T> (info, section);
				return group.RowCount;
			}
		}
		
		private InstantDatabaseGroup GetGroup<T> (int section)
		{
			return GetGroup<T> (GetGroupInfo<T> (), section);
		}

		private InstantDatabaseGroup GetGroup<T> (GroupInfo info, int section)
		{
			return GetGroup (typeof(T), info, section);

		}

		private InstantDatabaseGroup GetGroup (Type t, GroupInfo info, int section)
		{

				var tuple = new Tuple<Type,string> (t, info.ToString());
				List<InstantDatabaseGroup> group = null;
				int count = 0;
				while(group == null || group.Count <= section || count > 5)
				{
					if(count > 0)
						Console.WriteLine("Trying to fill groups: {0}",count);
					using(ThreadLock.Lock(groupLocker)){
						Groups.TryGetValue(tuple,out group);
					}
					if(group == null)
					{
						count ++;
						FillGroups (t, info);
					}
				}
				if(group == null)
					return null;
				return group [section];

		}

		private void FillGroups (Type t, GroupInfo info)
		{
			List<InstantDatabaseGroup> groups;
				groups = CreateGroupInfo(t,info);
			using(ThreadLock.Lock (groupLocker)) {
				var tuple = new Tuple<Type,string> (t, info.ToString());
				if (!Groups.ContainsKey (tuple))
					Groups.Add (tuple, groups);
				else
					Groups [tuple] = groups;
			}

		}

		public T ObjectForRow<T> (int section, int row) where T : new()
		{
			return ObjectForRow<T> (GetGroupInfo (typeof(T)), section, row);
		}

		public T ObjectForRow<T> (GroupInfo info, int section, int row) where T : new()
		{
			if (info == null)
				info = GetGroupInfo<T> ();
			using(ThreadLock.Lock (memStoreLocker)) {
				var type = typeof(T);
				var tuple = new Tuple<Type,string> (type, info.ToString());
				if (MemoryStore.ContainsKey (tuple)) {
					var groups = MemoryStore [tuple];
					if (groups.ContainsKey (section)) {
						var g = groups [section]; 
						if(g.ContainsKey (row))
							return (T)groups [section] [row];
					}
				}
				
				Precache<T> (info, section);
				return getObject<T> (info, section, row);
			}
		}

		public T GetObject<T> (object primaryKey) where T : new()
		{
			try{
				var type = typeof(T);
				if (!ObjectsDict.ContainsKey (type))
					ObjectsDict.Add (type, new Dictionary<object, object> ());
				if (ObjectsDict [type].ContainsKey (primaryKey)) 
					return (T)ObjectsDict [type] [primaryKey];
				Console.WriteLine("object not in objectsdict");
				var pk = GetPrimaryKeyProperty (type);
				var query = string.Format ("select * from {0} where {1} = ? ", type.Name, pk.Name);

				T item;
				lock(Locker){
					item = connection.Query<T> (query, primaryKey).FirstOrDefault ();
				}
					if(item != null)
						AddObjectToDict(item);
					return item;
			
			}
			catch(Exception ex)
			{
				return default(T);
			}

		}

		private T getObject<T> (GroupInfo info, int section, int row) where T : new()
		{
			T item;
			var t = typeof(T);
			var group = GetGroup<T> (info, section);

			string query;
			if (string.IsNullOrEmpty (info.GroupBy))
				query = string.Format ("select * from {0} {1} {2} LIMIT {3}, 1", t.Name, info.FilterString (true), info.OrderByString(true), row);
			else
				query = string.Format ("select * from {0} where {1} = ? {3} {2} LIMIT ? , 1", t.Name, info.GroupBy, info.OrderByString(true), info.FilterString (false));
			
			
			lock(Locker){
				item = connection.Query<T> (query, group.GroupString, row).FirstOrDefault ();
			}
			if (item == null)
				return new T ();
			var tuple = new Tuple<Type,string> (t, info.ToString());
			using(ThreadLock.Lock (memStoreLocker)) {
				if (!MemoryStore.ContainsKey (tuple))
					MemoryStore.Add (tuple, new Dictionary<int, Dictionary<int, object>> ());
				var groups = MemoryStore [tuple];
				if (!groups.ContainsKey (section))
					groups.Add (section, new Dictionary<int, object> ());
				if (!groups [section].ContainsKey (row))
					groups [section].Add (row, item);
				else
					groups [section] [row] = item;
				AddObjectToDict (item);
				return item;
			}

		}

		private void AddObjectToDict (object item)
		{
			using(ThreadLock.Lock (groupLocker)) {
				var t = item.GetType ();
				var primaryKey = GetPrimaryKeyProperty (t);
				if(primaryKey == null)
					return;
				object pk = primaryKey.GetValue (item, null);
				if (!ObjectsDict.ContainsKey (t))
					ObjectsDict.Add (t, new Dictionary<object, object> ());
				if (ObjectsDict [t].ContainsKey (pk))
					ObjectsDict [t] [pk] = item;
				else
					ObjectsDict [t].Add (pk, item);
				if (!Objects.ContainsKey (t))
					Objects.Add (t, new List<object> ());
				if (!Objects [t].Contains (item))
					Objects [t].Add (item);
			}
		}

		public int GetObjectCount<T> ()
		{
			return GetObjectCount<T> (null);
		}

		public int GetObjectCount<T> (GroupInfo info)
		{
			if (info == null)
				info = GetGroupInfo<T> ();
			var filterString = info.FilterString (true);
			var t = typeof(T);
			string query =  "Select count(*) from " + t.Name + " " + filterString;
			
			lock(Locker){
				return connection.ExecuteScalar<int> (query);
			}
		}
		public int GetDistinctObjectCount<T> (string column)
		{
			return GetDistinctObjectCount<T> (null,column);
		}
		
		public int GetDistinctObjectCount<T> (GroupInfo info,string column)
		{
			if (info == null)
				info = GetGroupInfo<T> ();
			var filterString = info.FilterString (true);
			var t = typeof(T);
			string query =  string.Format("Select distinct count({0}) from ",column) + t.Name + " " + filterString;
			
			lock(Locker){
				return connection.ExecuteScalar<int> (query);
			}
		}


		public T GetObjectByIndex<T> (int index) where T  : new()
		{
			T item;
			var t = typeof(T);
			var info = GetGroupInfo<T>();
			info.OrderByString(true);
			string query = string.Format ("select * from {0} {1} LIMIT {2}, 1", t.Name, info.OrderByString(true), index);
			
			
			lock(Locker){
				item = connection.Query<T> (query).FirstOrDefault ();
			}
			if (item == null)
				return default(T);
			AddObjectToDict(item);
			return item;
		}
		public List<T> GetObjects<T> (GroupInfo info) where T : new()
		{
			if (info == null)
				info = GetGroupInfo<T> ();
			var filterString = info.FilterString (true);
			var t = typeof(T);
			string query =  "Select * from " + t.Name + " " + filterString;
			
			lock(Locker){
				return connection.Query<T> (query).ToList();
			}
		}

		public void Precache<T> () where T : new()
		{
			Precache<T> (GetGroupInfo (typeof(T)));
		}

		public void Precache<T> (GroupInfo info) where T : new()
		{
			return;
			if (info == null)
				info = GetGroupInfo<T> ();
			var type = typeof(T);
			var tuple = new Tuple<Type,string> (type, info.ToString());
			FillGroups (type, info);
			using(ThreadLock.Lock (groupLocker)) {
				if (Groups [tuple].Count () == 0)
					SetGroups (type, info);

				foreach (var group in Groups[tuple]) {
					if (group.Loaded)
						continue;
					cacheQueue.AddLast (delegate {
						LoadItemsForGroup<T> (group);
					});
				}
			}
			StartQueue ();

		}

		public void Precache<T> (int section) where T : new()
		{
			Precache<T> (GetGroupInfo (typeof(T)), section);
		}

		public void Precache<T> (GroupInfo info, int section) where T : new()
		{
			//return;
			if (info == null)
				info = GetGroupInfo<T> ();
			var type = typeof(T);
			var group = GetGroup (type, info, section);
			cacheQueue.AddFirst (delegate {
				LoadItemsForGroup<T> (group);
			});
			StartQueue ();
		}

		private void LoadItemsForGroup<T> (InstantDatabaseGroup group) where T : new()
		{
			if (group.Loaded)
				return;
			Console.WriteLine ("Loading items for group");
			var type = typeof(T);
			string query  = string.Format ("select * from {0} where {1} = ? {3} {2} LIMIT ? , 50", type.Name, group.GroupBy, group.OrderByString(true), group.FilterString (false));
			List<T> items;
			int current = 0;
			bool hasMore = true;
			while (hasMore) {
				
				if (string.IsNullOrEmpty (group.GroupBy))
					query = string.Format ("select * from {0} {1} {2} LIMIT {3}, 50", type.Name, group.FilterString (true), group.OrderByString(true), current);
				
				lock(Locker){
					items = connection.Query<T> (query, group.GroupString, current).ToList ();
				}
				{
					var tuple = new Tuple<Type,string> (type, group.ToString());
					using(ThreadLock.Lock (memStoreLocker)){
					if (!MemoryStore.ContainsKey (tuple)) {
						MemoryStore.Add (tuple, new Dictionary<int, Dictionary<int, object>> ());
						}
					}
					using(ThreadLock.Lock (memStoreLocker)){
					if (!MemoryStore [tuple].ContainsKey (group.Order))
							try{
						MemoryStore [tuple].Add (group.Order, new Dictionary<int, object> ());
						}
						catch(Exception ex)
						{

						}

					}


					Dictionary<int,object> memoryGroup;
					using(ThreadLock.Lock (memStoreLocker)){
						memoryGroup = MemoryStore [tuple] [group.Order];
					}
					for (int i = 0; i< items.Count; i++) {
						lock (groupLocker)
						{
							if (memoryGroup.ContainsKey (i + current))
								memoryGroup [i + current] = items [i];
							else
								memoryGroup.Add (i + current, items [i]);
						}
						AddObjectToDict (items [i]);

					}

				}
				current += items.Count;
				if (current == group.RowCount)
					hasMore = false;
			}
			Console.WriteLine ("group loaded");
			group.Loaded = true;
		}

		LinkedList<Action> cacheQueue = new LinkedList<Action> ();
		object locker = new object ();
		bool queueIsRunning = false;

		private void StartQueue ()
		{
			lock (locker) {
				if (queueIsRunning)
					return;
				if (cacheQueue.Count == 0)
					return;
				queueIsRunning = true;
			}
			Thread thread = new Thread (runQueue);
			thread.Start ();
		}

		void runQueue ()
		{
			Action action;
			lock (locker) {
				if (cacheQueue.Count == 0) {
					queueIsRunning = false;
					return;
				}


				try{
				//Task.Factory.StartNew (delegate {
				action = cacheQueue.First ();
				cacheQueue.Remove (action);
				}
				catch(Exception ex)
				{
					runQueue();
					return;
				}
			}
			if(action  != null)
				action ();
			//}).ContinueWith (delegate {
			runQueue ();
		}


		private PropertyInfo GetGroupByProperty (Type type)
		{
			foreach (var prop in type.GetProperties()) {
				var attribtues = prop.GetCustomAttributes (false);
				var visibleAtt = attribtues.Where (x => x is GroupByAttribute).FirstOrDefault () as GroupByAttribute;
				if (visibleAtt != null)
					return prop;
			}
			return null;
		}

		private PropertyInfo GetOrderByProperty (Type type)
		{
			foreach (var prop in type.GetProperties()) {
				var attribtues = prop.GetCustomAttributes (false);
				var visibleAtt = attribtues.Where (x => x is OrderByAttribute).FirstOrDefault () as OrderByAttribute;
				if (visibleAtt != null)
					return prop;
			}
			return null;
		}

		private PropertyInfo GetPrimaryKeyProperty (Type type)
		{
			foreach (var prop in type.GetProperties()) {
				var attribtues = prop.GetCustomAttributes (false);
				var visibleAtt = attribtues.Where (x => x is PrimaryKeyAttribute).FirstOrDefault () as PrimaryKeyAttribute;
				if (visibleAtt != null)
					return prop;
			}
			return null;
		}
		#region sqlite

		public int InsertAll (System.Collections.IEnumerable objects)
		{
			lock(Locker){
				return connection.InsertAll (objects);
			}
		}
		
		/// <summary>
		/// Inserts all specified objects.
		/// </summary>
		/// <param name="objects">
		/// An <see cref="IEnumerable"/> of the objects to insert.
		/// </param>
		/// <param name="extra">
		/// Literal SQL code that gets placed into the command. INSERT {extra} INTO ...
		/// </param>
		/// <returns>
		/// The number of rows added to the table.
		/// </returns>
		public int InsertAll (System.Collections.IEnumerable objects, string extra)
		{
			lock(Locker)
				return connection.InsertAll (objects,extra);
		}
		
		/// <summary>
		/// Inserts all specified objects.
		/// </summary>
		/// <param name="objects">
		/// An <see cref="IEnumerable"/> of the objects to insert.
		/// </param>
		/// <param name="objType">
		/// The type of object to insert.
		/// </param>
		/// <returns>
		/// The number of rows added to the table.
		/// </returns>
		public int InsertAll (System.Collections.IEnumerable objects, Type objType)
		{
			
			lock(Locker)
				return connection.InsertAll (objects,objType);
		}
		
		/// <summary>
		/// Inserts the given object and retrieves its
		/// auto incremented primary key if it has one.
		/// </summary>
		/// <param name="obj">
		/// The object to insert.
		/// </param>
		/// <returns>
		/// The number of rows added to the table.
		/// </returns>
		public int Insert (object obj)
		{
			lock(Locker)
				return connection.Insert (obj);
		}
		
		/// <summary>
		/// Inserts the given object and retrieves its
		/// auto incremented primary key if it has one.
		/// If a UNIQUE constraint violation occurs with
		/// some pre-existing object, this function deletes
		/// the old object.
		/// </summary>
		/// <param name="obj">
		/// The object to insert.
		/// </param>
		/// <returns>
		/// The number of rows modified.
		/// </returns>
		public int InsertOrReplace (object obj)
		{
			lock(Locker)
				return connection.InsertOrReplace (obj);
		}
		
		/// <summary>
		/// Inserts the given object and retrieves its
		/// auto incremented primary key if it has one.
		/// </summary>
		/// <param name="obj">
		/// The object to insert.
		/// </param>
		/// <param name="objType">
		/// The type of object to insert.
		/// </param>
		/// <returns>
		/// The number of rows added to the table.
		/// </returns>
		public int Insert (object obj, Type objType)
		{
			lock(Locker)
				return connection.Insert (obj,objType);
		}
		
		/// <summary>
		/// Inserts the given object and retrieves its
		/// auto incremented primary key if it has one.
		/// If a UNIQUE constraint violation occurs with
		/// some pre-existing object, this function deletes
		/// the old object.
		/// </summary>
		/// <param name="obj">
		/// The object to insert.
		/// </param>
		/// <param name="objType">
		/// The type of object to insert.
		/// </param>
		/// <returns>
		/// The number of rows modified.
		/// </returns>
		public int InsertOrReplace (object obj, Type objType)
		{
			lock(Locker)
				return connection.InsertOrReplace (obj,objType);
		}
		
		/// <summary>
		/// Inserts the given object and retrieves its
		/// auto incremented primary key if it has one.
		/// </summary>
		/// <param name="obj">
		/// The object to insert.
		/// </param>
		/// <param name="extra">
		/// Literal SQL code that gets placed into the command. INSERT {extra} INTO ...
		/// </param>
		/// <returns>
		/// The number of rows added to the table.
		/// </returns>
		public int Insert (object obj, string extra)
		{
			lock(Locker)
				return connection.Insert (obj,extra);
		}
		
		/// <summary>
		/// Inserts the given object and retrieves its
		/// auto incremented primary key if it has one.
		/// </summary>
		/// <param name="obj">
		/// The object to insert.
		/// </param>
		/// <param name="extra">
		/// Literal SQL code that gets placed into the command. INSERT {extra} INTO ...
		/// </param>
		/// <param name="objType">
		/// The type of object to insert.
		/// </param>
		/// <returns>
		/// The number of rows added to the table.
		/// </returns>
		public int Insert (object obj, string extra, Type objType)
		{
			lock(Locker)
				return connection.Insert (obj,extra,objType);
		}

		public int Execute (string query, params object[] args)
		{
			lock(Locker)
				return connection.Execute (query, args);
		}
		public List<T> Query<T> (string query, params object[] args) where T : new()
		{
			//lock(Locker)
				return connection.Query<T> (query, args);

		}
		
		public int Delete (object objectToDelete)
		{
			lock(Locker)
				return connection.Delete (objectToDelete);
		}
		public int Update (object obj)
		{
			lock(Locker)
				return connection.Update (obj);
		}

		
		public int UpdateAll (System.Collections.IEnumerable objects)
		{
			lock (Locker)
				return connection.UpdateAll (objects);
		}
		public int CreateTable<T> () where T : new()
		{
			lock(Locker)
				return connection.CreateTable<T> ();
		}
		public T ExecuteScalar<T> (string query, params object[] args) where T : new()
		{
			lock(Locker)
				return connection.ExecuteScalar<T>(query,args);
		}

		#endregion

	}

	[AttributeUsage (AttributeTargets.Property)]
	public class GroupByAttribute : Attribute
	{

	}
	
	[AttributeUsage (AttributeTargets.Property)]
	public class OrderByAttribute : Attribute
	{

	}
}

