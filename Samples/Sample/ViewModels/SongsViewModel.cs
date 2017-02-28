using System;
namespace Sample
{
	public class SongsViewModel
	{
		public SimpleDatabaseSource<Song> Songs { get; set; } = new SimpleDatabaseSource<Song> (SongDatabase.Main);
	}
}
