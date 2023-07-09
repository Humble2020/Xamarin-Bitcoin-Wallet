using NBitcoin;
using SmallWallet2.src;
using SmallWallet2.ViewModels.VM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SmallWallet2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreatePage : ContentPage
    {
        public walletViewModel Model { get; set; }
        public CreatePageViewModel cVm { get; set; }
        public INavigation navigation { get; set; }
        public CreatePage(INavigation Navigation)
        {
            navigation = Navigation;
            InitializeComponent();
            BindingContext = cVm = new CreatePageViewModel(navigation);
        }
        protected override void OnAppearing()
        {
          base.OnAppearing();
        }

        private async void MaterialFlatButton_Clicked(object sender, EventArgs e)
        {
           
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                bitName.Unfocus();

            });
        }
    }
}