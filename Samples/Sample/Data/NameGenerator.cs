using System;
using SQLite;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;

namespace Sample
{
	public static class NameGenerator
	{
		static SQLiteConnection db;
		static SQLiteConnection Db
		{
			get{
				if (db == null)
					db = new SQLiteConnection (dbPath);
				return db;
			}
			set{ db = value;}
		}

		public static readonly string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "names.db");
		static Random rand = new Random();

		static string getPersonQuery = @"select (SELECT Name FROM {0} ORDER BY RANDOM() LIMIT 1) as FirstName, (SELECT Name FROM surname ORDER BY RANDOM() LIMIT 1) as LastName";

		public static Task<List<Person>> GetPeopleAsync(int numberOfNames)
		{
			return Task.Factory.StartNew<List<Person>> (delegate {
				if (!File.Exists(dbPath))
				{
					var client = new WebClient();
					client.DownloadFile("https://www.dropbox.com/s/8iq6r5exmdt8f3d/names.db?dl=1",dbPath);
				}

				List<Person> names = new List<Person>();
				while(names.Count < numberOfNames)
				{
					var b = rand.Next(1);
					var query = string.Format(getPersonQuery, b == 0 ? "Male": "Female");
					var tempNames = Db.Query<Person>(query,50);
					foreach(var name in tempNames){
						name.Email = string.Format("{0}.{1}@email.com",name.FirstName,name.LastName);
						names.Add(name);
					}
				}
				return names;

			});
		}
	}
}

