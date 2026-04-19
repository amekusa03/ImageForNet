using Avalonia.Controls;
using ImageForNet.ViewModels;

namespace ImageForNet.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        Loaded += (sender, args) =>
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.StorageProvider = TopLevel.GetTopLevel(this)?.StorageProvider;
            }
        };
    }
}