using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Sample
{
	public partial class SongsPage : ContentPage
	{
		public SongsPage()
		{
			InitializeComponent();
			BindingContext = new SongsViewModel();
		}
	}
}
