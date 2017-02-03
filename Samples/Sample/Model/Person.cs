using System;
using SimpleDatabase;
using SQLite;

namespace Sample
{
	public class Person
	{
		public Person ()
		{
		}
		[PrimaryKeyAttribute, AutoIncrement]
		public int Id {get;set;}
		public string FirstName {get;set;}
		[OrderByAttribute]
		public string MiddleName { get; set; }
		public string LastName { get; set; }
		string indexCharacter;
		[GroupBy]
		public string IndexCharacter { 
			get {
				if(string.IsNullOrWhiteSpace(indexCharacter) && !string.IsNullOrWhiteSpace(LastName))
					indexCharacter = LastName.Substring(0, 1);
				return indexCharacter;
			}
			set { indexCharacter = value;}
		}
		public string Email { get; set; }
		public string PhoneNumber {get;set;}
		public string DisplayName => $"{LastName}, {FirstName}";
		public override string ToString ()
		{
			return string.Format ("{0} , {1}",  LastName, FirstName);
		}
	}
}

