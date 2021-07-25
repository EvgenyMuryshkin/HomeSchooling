using System;
using System.IO;

namespace PhotoServer
{
    public class Settings
    {
        public static string Location => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HomeSchoolingServer");
    }
}
