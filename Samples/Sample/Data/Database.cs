using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using SimpleDatabase;
using System.Net;

namespace Sample
{
	public class Database : SimpleDatabaseConnection
	{
		public Database () : this(dbPath)
		{
		}
		public Database(string path) : base(path)
		{
			CreateTable<Person>();
		}

		static Database main;
		public static readonly string dbPath = Path.Combine (Environment.GetFolderPath(Environment.SpecialFolder.Personal), "contacts.db");

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

		public static Task SetDatabase1000()
		{
			return DownloadDAtabase("https://www.dropbox.com/s/kob54oioz56rsnb/contacts-1000.db?dl=1", "contacts-1000.db");

		}

		public static Task SetDatabase10000()
		{
			return DownloadDAtabase("https://www.dropbox.com/s/llxh3zafm16lcrr/contacts-10000.db?dl=1", "contacts-10000.db");

		}

		public static Task SetDatabase20000()
		{
			return DownloadDAtabase("https://www.dropbox.com/s/foyjdl9yv894ssx/contacts-20000.db?dl=1", "contacts-20000.db");
		}

		static async Task DownloadDAtabase(string url, string database)
		{
			var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), database);
			if (!File.Exists(path))
			{
				var client = new WebClient();
				await client.DownloadFileTaskAsync(url, path);
			}
			main = new Database(path);
		}
	}
}

