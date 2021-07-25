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

        async Task SelectServer(HomeScroolingServer server)
        {
            serverName.Text = server?.Name ?? "";
            URL.Text = server?.Address ?? "";

            await RunStatusCheck();
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

                    button.Clicked += async (s, a) =>
                    {
                        await SelectServer(server);
                    };

                    servers.Children.Add(button);
                }

                await SelectServer(state.Servers.FirstOrDefault());
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

        HttpClient NewClient
        {
            get
            {
                var client = new HttpClient(
                    new HttpClientHandler()
                    {
                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                    });

                return client;
            }
        }

        void TimeoutHandler(TaskCompletionSource<bool> tcs)
        {
            Task.Delay(TimeSpan.FromSeconds(3)).ContinueWith(t =>
            {
                tcs.TrySetResult(false);
            });
        }

        string PhotosEndpoint => $"http://{URL.Text}:5002/photos";

        Task<bool> StatusCheckAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            TimeoutHandler(tcs);

            Task.Factory.StartNew(async () =>
            {
                await NewClient.GetAsync(PhotosEndpoint);
                tcs.TrySetResult(true);
            });

            return tcs.Task;
        }

        async Task<bool> RunStatusCheck()
        {
            if (string.IsNullOrWhiteSpace(URL.Text))
            {
                await SetStatus("Please select server");
                return false;
            }

            await SetStatus("Status check ...");

            if (!await StatusCheckAsync())
            {
                await SetStatus($"Server is not reachable: {serverName.Text}");
                return false;
            }

            await SetStatus("Server ok");

            return true;
        }

        async Task<bool> Upload(string name, Stream source)
        {
            if (!await RunStatusCheck())
                return false;

            await SetStatus("Uploading ...");

            var client = NewClient;
            client.DefaultRequestHeaders.Add("FileName", name);

            await client.PostAsync(PhotosEndpoint, new StreamContent(source));
            source.Seek(0, SeekOrigin.Begin);

            return true;
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
            catch
            {

            }            
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

                        if (!await Upload(Path.GetFileName(path), photo.GetStream()))
                            return;

                        await SetStatus("Loading preview ...");

                        await LoadPreview(photo.GetStream());
                        await SetStatus("Ready");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
                await SetStatus(ex.Message);
            }
            finally
            {
                await DeleteFile(path);
                IsBusy = false;
                IsEnabled = true;
            }
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
                        using (photo)
                        {
                            if (!await Upload(Path.GetFileName(photo.Path), photo.GetStream()))
                                return;

                            await SetStatus("Loading preview ...");
                            await LoadPreview(photo.GetStream());
                        }
                    }

                    await SetStatus("Ready");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
                await SetStatus(ex.Message);
            }
            finally
            {
                IsBusy = false;
                IsEnabled = true;
            }
        }
    }
}
