using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using SimpleDatabase;

namespace Sample
{
	public class Database : SimpleDatabaseConnection
	{
		public Database () : base(dbPath)
		{
			CreateTable<Person> ();
		}

		static Database main;
		public static readonly string dbPath = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "contacts.db");

		public static Database Main {
			get {
				if (main == null)
					main = new Database ();
				return main;
			}
		}

		public Task InsertPeople (List<Person> people)
		{
			return Task.Factory.StartNew (delegate {

				this.InsertAll (people);
			});
		}
	}
}

