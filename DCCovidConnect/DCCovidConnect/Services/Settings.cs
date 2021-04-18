using DCCovidConnect.ViewModels;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace DCCovidConnect.Services
{
    public class Settings : BaseViewModel
    {
        private static ISettings AppSettings => CrossSettings.Current;

        private static Settings settings;
        public static Settings Current => settings ??= new Settings();
        
        public string DefaultState
        {
            get => AppSettings.GetValueOrDefault(nameof(DefaultState), "District of Columbia");
            set
            {
                var original = DefaultState;
                if (AppSettings.AddOrUpdateValue(nameof(DefaultState), value))
                    SetProperty(ref original, value);
            }
        }

        public string DarkMode
        {
            get => AppSettings.GetValueOrDefault(nameof(DarkMode), "Off");
            set => AppSettings.AddOrUpdateValue(nameof(DarkMode), value);
        }
    }
}