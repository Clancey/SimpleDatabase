using System;
using System.IO;
using System.Net;
using SimpleDatabase;

namespace Sample
{
	public class SongDatabase: SimpleDatabaseConnection
	{
		public SongDatabase () : base(dbPath)
		{
			CreateTable<Song> ();
			MakeClassInstant<Song> ();
		}
		
		static SongDatabase main;
		public static readonly string dbPath =  Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "music.db");
		public static SongDatabase Main
		{
			get{
				if (main == null)
				{
					downloadIfNeeded();
					main = new SongDatabase ();

				}
				return main;
			}
		}
		static void downloadIfNeeded()
		{
			if (File.Exists(dbPath))
				File.Delete(dbPath);
			   return;
			//var client = new WebClient ();
			//client.DownloadFile ("https://www.dropbox.com/s/r25qjjae25mk29g/music.db", dbPath);
		}

	}
}


