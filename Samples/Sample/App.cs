using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Sample
{
	public class App : Application
	{
		ActivityIndicator spinner;
		public App()
		{
			spinner = new ActivityIndicator();
			// The root page of your application
			var content = new ContentPage
			{
				Title = "Sample",
				Content = new TableView(
					new TableRoot {
						new TableSection(""){
							CreateCell("View Contacts",async ()=>{
								await MainPage.Navigation.PushAsync(new ContactsPage());
							}),
						},
						new TableSection("Populate database"){
							CreateCell("Add 100 people",async ()=>{
								await insertPeople (100);
							}),
							CreateCell("Add 1000 people",async ()=>{
								await insertPeople (1000);
							}),
							new TextCell{Text = "The first time will download a 4.1mb database used to generate real names"},
							new ViewCell{
								View = spinner,
							},
						},
					}
				),
			};

			MainPage = new NavigationPage(content);
		}

		TextCell CreateCell(string text, Action action)
		{
			var cell = new TextCell
			{
				Text = text,
			};
			cell.Tapped += (sender, e) => action?.Invoke();
			return cell;
		}
		async Task insertPeople(int numberOfPeople)
		{
			spinner.IsRunning = true;
			try
			{
				var people = await NameGenerator.GetPeopleAsync(numberOfPeople);
				var records = await Database.Main.InsertAllAsync(people);
				Database.Main.UpdateInstant<Person>();
			}
			catch (Exception ex)
			{
				this.MainPage.DisplayAlert("Error", ex.Message, "Ok");
			}
			finally
			{
				spinner.IsRunning = false;
			}
		}

		protected override void OnStart()
		{
			// Handle when your app starts
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}
	}
}
