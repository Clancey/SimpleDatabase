using System;
using SimpleDatabase;
using SQLite;

namespace Sample
{
	public class Song
	{
		[PrimaryKey]
		public string Id { get; set; }
		public string Title {get;set;}
		[OrderBy]
		public string TitleNorm { get; set; }
		public string Artist {get;set;}
		[GroupBy]
		public string IndexCharacter { get; set; }
	}
}

