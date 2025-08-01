using TokeroDCA.ViewModels;

namespace TokeroDCA.Views;

public partial class DCASetupPage : ContentPage
{
    public DCASetupPage(DCASetupViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}