using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PhotosClient
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class NewServerPage : ContentPage
	{
		public NewServerPage ()
		{
			InitializeComponent ();
			Title = "New Server";
		}

        private async void add_Clicked(object sender, EventArgs e)
        {
			if (string.IsNullOrWhiteSpace(serverName.Text) || string.IsNullOrWhiteSpace(serverAddress.Text))
            {
				await DisplayAlert("Add New Server", $"Please fill in server details", "Close");
				return;
			}

			try
            {
				var state = await HomeSchoolingStateManager.LoadFromStorage();
				if (state.Servers.Find(s => s.Name == serverName.Text || s.Address == serverAddress.Text) != null)
                {
					await DisplayAlert("Add New Server", $"Server with name '{serverName.Text}' or address '{serverAddress.Text}' already exists", "Close");
					return;
				}

				state.Servers.Add(new HomeScroolingServer() { Name = serverName.Text, Address = serverAddress.Text });
				await HomeSchoolingStateManager.SaveToStorage(state);
				await MainPage.Instance.Navigation.PopAsync();
			}
			catch
            {

            }
        }
    }
}