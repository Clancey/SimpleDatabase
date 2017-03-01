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
						new TableSection("Download database"){
							CreateCell("1,000 people",async ()=>{
								await SetupDatabase(Database.SetDatabase1000());
							}),
							CreateCell("10,000 people",async ()=>{
								await SetupDatabase(Database.SetDatabase10000());
							}),
							CreateCell("20,000 people",async ()=>{
							await SetupDatabase(Database.SetDatabase20000());
							}),
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
		async Task SetupDatabase(Task setupDatabase)
		{
			spinner.IsRunning = true;
			try
			{
				await setupDatabase;
				await MainPage.Navigation.PushAsync(new ContactsPage());

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
