using Newtonsoft.Json;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace PhotosClient
{
    class HomeSchoolingStateManager
    {
        public static async Task<HomeSchoolingState> LoadFromStorage()
        {
            try
            {
                var payload = await SecureStorage.GetAsync("State");
                if (payload == null)
                    return new HomeSchoolingState();

                return JsonConvert.DeserializeObject<HomeSchoolingState>(payload);
            }
            catch
            {
                return new HomeSchoolingState();
            }
        }

        public static async Task SaveToStorage(HomeSchoolingState state)
        {
            try
            {
                await SecureStorage.SetAsync("State", JsonConvert.SerializeObject(state));
            }
            catch
            {

            }
        }
    }
}
