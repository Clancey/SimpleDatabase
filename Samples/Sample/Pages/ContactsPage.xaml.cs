using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Sample
{
	public partial class ContactsPage : ContentPage
	{
		public ContactsPage()
		{
			InitializeComponent();
			BindingContext = new ContactsViewModel();
		}
	}
}
