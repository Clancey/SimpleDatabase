using System;
namespace Sample
{
	public class ContactsViewModel
	{
		public SimpleDatabaseSource<Person> Contacts { get; set; } = new SimpleDatabaseSource<Person> (Database.Main);
	}
}
