using System;
using Xamarin.Data;
using SQLite;

namespace InstantSample
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
		string lastName;
		public string LastName { 
			get{ return lastName;}
			set{ 
				lastName = value;
				if(!string.IsNullOrEmpty(lastName))
					IndexCharacter =  lastName.Substring(0,1);
			}
		}
		[GroupBy]
		[Indexed]
		public string IndexCharacter { get; set; }
		public string Email { get; set; }
		public string PhoneNumber {get;set;}
		public override string ToString ()
		{
			return string.Format ("{0} , {1}",  LastName, FirstName);
		}
	}
}

