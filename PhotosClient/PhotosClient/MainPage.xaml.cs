using Newtonsoft.Json;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PhotosClient
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public static MainPage Instance;

        public MainPage()
        {
            Instance = this;
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadState();
        }

        void SelectServer(HomeScroolingServer server)
        {
            serverName.Text = server?.Name ?? "";
            URL.Text = server?.Address ?? "";
        }

        async Task LoadState()
        {
            try
            {
                var state = await HomeSchoolingStateManager.LoadFromStorage();

                servers.Children.Clear();

                foreach (var server in state.Servers)
                {
                    var button = new Button()
                    {
                        Text = server.Name
                    };

                    button.Clicked += (s, a) =>
                    {
                        SelectServer(server);
                    };

                    servers.Children.Add(button);
                }

                SelectServer(state.Servers.FirstOrDefault());
            }
            catch
            {

            }
        }

        async Task DeleteFile(string path)
        {
            try
            {
                if (path != null)
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        async Task SetStatus(string value)
        {
            if (App.IsOnMainThread())
            {
                StatusLabel.Text = value;
            }
            else
            {
                var tcs = new TaskCompletionSource<bool>();

                Dispatcher.BeginInvokeOnMainThread(() =>
                {
                    StatusLabel.Text = value;
                    tcs.SetResult(true);
                });

                await tcs.Task;
            }
        }


        async Task LoadPreview(Stream source)
        {
            if (App.IsOnMainThread())
            {
                PhotoImage.Source = ImageSource.FromStream(() => { return source; });
            }
            else
            {
                var tcs = new TaskCompletionSource<bool>();

                Dispatcher.BeginInvokeOnMainThread(() =>
                {
                    PhotoImage.Source = ImageSource.FromStream(() => { return source; });
                    tcs.SetResult(true);
                });

                await tcs.Task;
            }
        }
        async Task Upload(string name, Stream source)
        {
            var endPoint = $"http://{URL.Text}:5002/photos";

            var client = new HttpClient(
                new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                })
            {
                Timeout = TimeSpan.FromSeconds(5)
            };

            await SetStatus("Status check ...");

            /*
            var tcs = new CancellationTokenSource();
            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                tcs.Cancel();
            });
            */

            var content = await client.GetStringAsync(endPoint);
            //var resp = await client.GetAsync(endPoint, tcs.Token);
            //var content = await resp.Content.ReadAsStringAsync();

            client.DefaultRequestHeaders.Add("FileName", name);

            await SetStatus("Uploading ...");

            await client.PostAsync(endPoint, new StreamContent(source));
            source.Seek(0, SeekOrigin.Begin);
        }

        private async void CameraButton_Clicked(object sender, EventArgs e)
        {
            string path = null;

            try
            {
                IsEnabled = false;
                PhotoImage.Source = null;
                await SetStatus("Waiting for image ...");

                using (var photo = await CrossMedia.Current.TakePhotoAsync(
                    new StoreCameraMediaOptions() 
                    {
                        CompressionQuality = 75,
                        MaxWidthHeight = 1024
                    }))
                {
                    if (photo != null)
                    {
                        path = photo.Path;

                        await Upload(Path.GetFileName(path), photo.GetStream());

                        await SetStatus("Loading preview ...");

                        await LoadPreview(photo.GetStream());
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                await DeleteFile(path);
                IsBusy = false;
                IsEnabled = true;
                //await DisplayAlert("Uploaded", "All good", "OK");
                await SetStatus("Ready");
            }
        }

        private async void Delete_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(serverName.Text))
                return;

            if (!await DisplayAlert("Home Schooling", $"Delete server '{serverName.Text}'?", "OK", "Cancel"))
                return;

            try
            {
                var state = await HomeSchoolingStateManager.LoadFromStorage();
                state.Servers.RemoveAll(s => s.Name == serverName.Text);
                await HomeSchoolingStateManager.SaveToStorage(state);
                await LoadState();
            }
            catch
            {

            }
        }

        private async void Add_Clicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new NavigationPage(new NewServerPage()) { Title = "Add New Server" });
            }
            catch(Exception ex)
            {

            }            
        }

        private void Dad_Clicked(object sender, EventArgs e)
        {
            URL.Text = "192.168.1.109";
        }

        private void Mom_Clicked(object sender, EventArgs e)
        {
            URL.Text = "192.168.1.110";
        }

        private async void ExistingButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                IsEnabled = false;
                PhotoImage.Source = null;
                await SetStatus("Waiting for image ...");

                var photos = await CrossMedia.Current.PickPhotosAsync(
                    new PickMediaOptions()
                    {
                        CompressionQuality = 75,
                        MaxWidthHeight = 1024
                    });

                if (photos != null)
                {
                    foreach (var photo in photos)
                    {
                        var path = photo.Path;

                        await Upload(Path.GetFileName(path), photo.GetStream());

                        await SetStatus("Loading preview ...");

                        await LoadPreview(photo.GetStream());

                        photo.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
                IsEnabled = true;
                await DisplayAlert("Uploaded", "All good", "OK");
                await SetStatus("Ready");
            }
        }
    }
}
