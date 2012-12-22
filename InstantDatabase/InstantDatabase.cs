using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using System.Reflection;
using System.Collections;
using System.Threading.Tasks;

namespace Xamarin.Data
{
	public class InstantDatabase : SQLiteConnection
	{
		Dictionary<Type,Dictionary<int,Dictionary<int,Object>>> MemoryStore = new Dictionary<Type, Dictionary<int, Dictionary<int, object>>> ();
		Dictionary<Type,Dictionary<object,object>> ObjectsDict = new Dictionary<Type, Dictionary<object, object>> ();
		Dictionary<Type,List<object>> Objects = new Dictionary<Type, List<object>> ();
		Dictionary<Type,List<InstantDatabaseGroup>> Groups = new Dictionary<Type, List<InstantDatabaseGroup>> ();
		public static object DatabaseLocker = new object ();

		public InstantDatabase (string databasePath, bool storeDateTimeAsTicks = false) : base ( databasePath, storeDateTimeAsTicks)
		{
			init ();
		}

		public InstantDatabase (string databasePath, SQLiteOpenFlags openFlags, bool storeDateTimeAsTicks = false) : base (databasePath, openFlags, storeDateTimeAsTicks)
		{
			init ();
		}

		void init ()
		{
			lock (DatabaseLocker)
				this.CreateTable<InstantDatabaseGroup> ();
		}

		public void MakeClassInstant<T> ()
		{
			MakeClassInstant (typeof(T));
		}
		
		public void MakeClassInstant (Type type)
		{
			lock (DatabaseLocker) {
				var groupBy = GetGroupByProperty (type);
				var orderBy = GetOrderByProperty (type);
				if (groupBy == null)
					throw new Exception ("Objects must contain the GroupBy Attribute");
				if (orderBy == null)
					throw new Exception ("Objects must contain the OrderBy Attribute");
				SetGroups (type);
				FillGroups (type);
				if (Groups [type].Count () == 0)
					SetGroups (type);
			}

		}

		private void FillGroups (Type type)
		{
			lock (DatabaseLocker) {
				List<InstantDatabaseGroup> groups = this.Table<InstantDatabaseGroup> ().Where (x => x.ClassName == type.Name).OrderBy (x => x.Order).ToList ();
				if (Groups.ContainsKey (type))
					Groups [type] = groups;
				else
					Groups.Add (type, groups);
			}
		}

		private void SetGroups (Type type)
		{
			lock (DatabaseLocker) {
				var groupBy = GetGroupByProperty (type);
				var orderBy = GetOrderByProperty (type);

				var query = string.Format ("select distinct {1} as Grouping from {0} order by {2}", type.Name, groupBy.Name, orderBy.Name);
				List<InstantDatabaseGroup> groups = this.Query<InstantDatabaseGroup> (query).ToList ();
				var deleteQuery = string.Format ("delete from InstantDatabaseGroup where ClassName = ?");
				this.Execute (deleteQuery, type.Name);

				for (int i = 0; i < groups.Count(); i++) {
					var group = groups [i];
					group.ClassName = type.Name;
					group.Order = i;
					var rowQuery = string.Format ("select count(*) from {0} where {1} = ?", type.Name, groupBy.Name);
					lock (DatabaseLocker)
						group.RowCount = this.ExecuteScalar<int> (rowQuery, group.Grouping);
				}
		
				this.InsertAll (groups);

				if (Groups.ContainsKey (type))
					Groups [type] = groups;
				else
					Groups.Add (type, groups);
			}
		}

		public void UpdateInstant<T> ()
		{
			UpdateInstant (typeof(T));
		}

		public void UpdateInstant (Type type)
		{
			lock (DatabaseLocker) {
				if (MemoryStore.ContainsKey (type)) {
					MemoryStore [type] = new Dictionary<int, Dictionary<int, object>> ();
				}
				SetGroup (type);
			}
		}

		public void ClearMemory ()
		{
			lock (DatabaseLocker) {
				MemoryStore.Clear ();
				ObjectsDict.Clear ();
				Objects.Clear ();
				GC.Collect ();
			}
		}

		public string SectionHeader<T> (int section)
		{
			lock (DatabaseLocker) {
				var t = typeof(T);
				if (!Groups.ContainsKey (t))
					SetGroup (t);			
				return Groups [t] [section].Grouping;
			}
		}

		public string [] QuickJump<T> ()
		{
			lock (DatabaseLocker) {
				var t = typeof(T);
				if (!Groups.ContainsKey (t))
					SetGroup (t);
				var groups = Groups [t];
				var strings = groups.Select (x => x.Grouping [0].ToString ()).ToArray ();
				return strings;
			}
		}

		public int NumberOfSections<T> ()
		{
			lock (DatabaseLocker) {
				var t = typeof(T);
				if (!Groups.ContainsKey (t))
					SetGroup (t);			
				return Groups [t].Count;
			}
		}

		public int RowsInSection<T> (int section)
		{
			lock (DatabaseLocker) {
				var group = GetGroup<T> (section);
				return group.RowCount;
			}
		}

		private InstantDatabaseGroup GetGroup<T> (int section)
		{
			return GetGroup (typeof(T), section);

		}

		private InstantDatabaseGroup GetGroup (Type t, int section)
		{
			lock (DatabaseLocker) {
				if (!Groups.ContainsKey (t))
					SetGroup (t);
				var group = Groups [t];
				return group [section];
			}
		}

		private void SetGroup (Type t)
		{
			lock (DatabaseLocker) {
				List<InstantDatabaseGroup> groups = this.Table<InstantDatabaseGroup> ().Where (x => x.ClassName == t.Name).OrderBy (x => x.Order).ToList ();
				if (!Groups.ContainsKey (t))
					Groups.Add (t, groups);
				else
					Groups [t] = groups;
			}
		}

		public T ObjectForRow<T> (int section, int row) where T : new()
		{
			lock (DatabaseLocker) {
				var type = typeof(T);
				if (MemoryStore.ContainsKey (type)) {
					var groups = MemoryStore [type];
					if (groups.ContainsKey (section) && groups [section].ContainsKey (row)) {
						return (T)groups [section] [row];
					}
				}
				
				Precache<T> (section);
				return GetObject<T> (section, row);
			}
		}

		public T GetObject<T> (object primaryKey) where T : new()
		{
			lock (DatabaseLocker) {
				var type = typeof(T);
				if (!ObjectsDict.ContainsKey (type)) {
					ObjectsDict.Add (type, new Dictionary<object, object> ());
				}
				if (ObjectsDict [type].ContainsKey (primaryKey)) 
					return (T)ObjectsDict [type] [primaryKey];
				var pk = GetPrimaryKeyProperty (type);
				var query = string.Format ("select * from {0} where {1} = ? ", type.Name, pk.Name);
				var item = this.Query<T> (query, primaryKey).FirstOrDefault ();
				ObjectsDict [type].Add (primaryKey, item);
				return item;
			}

		}

		private T GetObject<T> (int section, int row) where T : new()
		{
			lock (DatabaseLocker) {
				var t = typeof(T);
				var groupBy = GetGroupByProperty (t);
				var orderBy = GetOrderByProperty (t);
				var group = GetGroup<T> (section);
				var query = string.Format ("select * from {0} where {1} = ? order by {2} LIMIT ? , 1", t.Name, groupBy.Name, orderBy.Name);
				T item;
				item = this.Query<T> (query, group.Grouping, row) [0];

				if (!MemoryStore.ContainsKey (t))
					MemoryStore.Add (t, new Dictionary<int, Dictionary<int, object>> ());
				var groups = MemoryStore [t];
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
			lock (DatabaseLocker) {
				var t = item.GetType ();
				var primaryKey = GetPrimaryKeyProperty (t);
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

		public List<T> GetObjects<T> ()
		{
			lock (DatabaseLocker) {
				var t = typeof(T);
				if (!Objects.ContainsKey (t))
					Objects.Add (t, new List<object> ());
				return Objects [t].OfType<T> ().ToList ();
			}
		}

		public void Precache<T> () where T : new()
		{
			var type = typeof(T);
			FillGroups (type);
			if (Groups [type].Count () == 0)
				SetGroups (type);

			foreach (var group in Groups[type]) {
				if (group.Loaded)
					continue;
				cacheQueue.AddLast (delegate {
					LoadItemsForGroup<T> (group);
				});
			}
			StartQueue ();

		}

		public void Precache<T> (int section) where T : new()
		{
			var type = typeof(T);
			var group = GetGroup (type, section);
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
			var groupBy = GetGroupByProperty (type);
			var orderBy = GetOrderByProperty (type);
			var query = string.Format ("select * from {0} where {1} = ? order by {2} LIMIT ? , 50", type.Name, groupBy.Name, orderBy.Name);
			List<T> items;
			bool hasMore = true;
			int current = 0;
			while (hasMore) {
				lock (DatabaseLocker)
					items = this.Query<T> (query, group.Grouping, current).ToList ();
			
			
					if (!MemoryStore.ContainsKey (type)) {
						MemoryStore.Add (type, new Dictionary<int, Dictionary<int, object>> ());
					}
					if (!MemoryStore [type].ContainsKey (group.Order))
						MemoryStore [type].Add (group.Order, new Dictionary<int, object> ());


					var memoryGroup = MemoryStore [type] [group.Order];
					for (int i = 0; i< items.Count; i++) {
						if (memoryGroup.ContainsKey (i + current))
							memoryGroup [i + current] = items [i];
						else
							memoryGroup.Add (i + current, items [i]);
						AddObjectToDict (items [i]);
						Console.WriteLine (i + current);

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
			runQueue ();
		}

		void runQueue ()
		{
			if (cacheQueue.Count == 0) {
				lock (locker)
					queueIsRunning = false;
				return;
			}
			Task.Factory.StartNew (delegate {
				Action action = cacheQueue.First ();
				cacheQueue.Remove (action);
				action ();
			}).ContinueWith (delegate {
				runQueue ();
			});
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

