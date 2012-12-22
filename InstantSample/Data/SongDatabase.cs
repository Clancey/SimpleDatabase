using System;
using System.IO;
using Xamarin.Data;
using System.Net;

namespace InstantSample
{
	public class SongDatabase: InstantDatabase
	{
		public SongDatabase () : base(dbPath,true)
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
//			if(File.Exists(dbPath))
//			   return;
//			WebClient client = new WebClient ();
//			client.DownloadFile ("https://www.dropbox.com/s/r25qjjae25mk29g/music.db", dbPath);
		}

	}
}


