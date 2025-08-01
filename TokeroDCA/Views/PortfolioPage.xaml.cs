using TokeroDCA.ViewModels;

namespace TokeroDCA.Views;

public partial class PortfolioPage : ContentPage
{
    public PortfolioPage(PortfolioViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}