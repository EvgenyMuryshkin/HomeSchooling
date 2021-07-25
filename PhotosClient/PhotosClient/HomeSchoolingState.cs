using System.Collections.Generic;

namespace PhotosClient
{
    public class HomeScroolingServer
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }

    public class HomeSchoolingState
    {
        public List<HomeScroolingServer> Servers { get; set; } = new List<HomeScroolingServer>();
    }
}
