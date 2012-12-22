using System;
using Xamarin.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace InstantSample
{
	public class Database : InstantDatabase
	{
		public Database () : base(dbPath,true)
		{
			CreateTable<Person> ();
		}

		static Database main;
		public static readonly string dbPath =  Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "contacts.db");
		public static Database Main
		{
			get{
				if (main == null)
					main = new Database ();
				return main;
			}
		}
		public Task InsertPeople(List<Person> people)
		{
			return Task.Factory.StartNew (delegate {
				lock(DatabaseLocker)
				{
					this.InsertAll(people);
				}
			});
		}
	}
}

