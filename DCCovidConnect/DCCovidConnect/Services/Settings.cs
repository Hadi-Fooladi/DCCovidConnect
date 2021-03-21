using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace DCCovidConnect.Services
{
    public static class Settings
    {
        private static ISettings AppSettings => CrossSettings.Current;

        public static string DefaultState
        {
            get => AppSettings.GetValueOrDefault(nameof(DefaultState), "District of Columbia");
            set => AppSettings.AddOrUpdateValue(nameof(DefaultState), value);
        }
        
        public static string DarkMode
        {
            get => AppSettings.GetValueOrDefault(nameof(DarkMode), "Off");
            set => AppSettings.AddOrUpdateValue(nameof(DarkMode), value);
        }
    }
}