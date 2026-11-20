using System;
using System.Linq;
using System.Windows;

namespace WpfLab2.Services;

public sealed class ThemeService
{
    private const string ThemePackPrefix = "pack://application:,,,/WpfResourceLibrary;component/Themes/";

    public void ApplyTheme(string themeName)
    {
        string fileName = themeName switch
        {
            "Тёмная" => "DarkTheme.xaml",
            "Высокий контраст" => "HighContrastTheme.xaml",
            _ => "LightTheme.xaml"
        };

        var dictionaries = Application.Current.Resources.MergedDictionaries;
        var oldTheme = dictionaries.FirstOrDefault(dictionary =>
            dictionary.Source?.OriginalString.Contains("/Themes/") == true &&
            dictionary.Source.OriginalString.EndsWith("Theme.xaml", StringComparison.OrdinalIgnoreCase));

        if (oldTheme is not null)
            dictionaries.Remove(oldTheme);

        dictionaries.Add(new ResourceDictionary
        {
            Source = new Uri(ThemePackPrefix + fileName, UriKind.Absolute)
        });
    }
}
