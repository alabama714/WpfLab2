using System.Windows;
using WpfLab2.Services;
using WpfLab2.ViewModels;

namespace WpfLab2;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(new ThemeService());
    }
}
