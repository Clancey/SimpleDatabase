using System;
using SQLite;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace InstantSample
{
	public static class NameGenerator
	{
		static SQLiteConnection db;
		static SQLiteConnection Db
		{
			get{
				if (db == null)
					db = new SQLiteConnection (Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),"names.db"));
				return db;
			}
			set{ db = value;}
		}
		static Random rand = new Random();

		static string getPersonQuery = @"select  (SELECT Name FROM {0} ORDER BY RANDOM() LIMIT 1) as FirstName, (SELECT Name FROM surname ORDER BY RANDOM() LIMIT 1) as LastName";

		public static Task<List<Person>> GetPeopleAsync(int numberOfNames)
		{
			return Task.Factory.StartNew<List<Person>> (delegate {
				List<Person> names = new List<Person>();
				while(names.Count < numberOfNames)
				{
					var b = rand.Next(1);
					var query = string.Format(getPersonQuery, b == 0 ? "Male": "Female");
					var name = Db.Query<Person>(query,0).First();
					name.Email = string.Format("{0}.{1}@email.com",name.FirstName,name.LastName);
					names.Add(name);
				}
				return names;

			});
		}
	}
}

