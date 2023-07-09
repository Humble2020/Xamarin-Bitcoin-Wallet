using Newtonsoft.Json;
using SmallWallet2.src;
using SmallWallet2.ViewModels.VM;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SmallWallet2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SendPage : ContentPage
    {
        public walletViewModel Model { get; set; }
        public SendViewModel sVm { get; set; }
        public INavigation Navigationx { get; set; }
        public SendPage(walletViewModel model)
        {
            Model = model;
            InitializeComponent();
            BindingContext = sVm = new SendViewModel(Navigationx, model);
        }
      
       protected override void OnAppearing()
        {

            base.OnAppearing();
            IsBusy = true;
            sVm.checkfirst();
        }

        private void ChangeToUsd(object sender, EventArgs e)
        {

            if (sVm.USD_amount == null)
            {
                return;
            }

            if (sVm.USD_amount != null)
            {
                sVm.ConvertUSD_toBTC();
            }
            else
            {
                return;
            }

        }

        private async void OnSetMaxAmountButtonClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(amountsendBTC.Text) && !string.IsNullOrEmpty(amountsendUSD.Text))
                amountsendBTC.CursorPosition = amountsendBTC.Text.Length;
     
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                amountsendBTC.Unfocus();
                amountsendUSD.Unfocus();

            });
        }

        private async void OnSetMaxAmount2ButtonClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(amountsendBTC.Text) && !string.IsNullOrEmpty(amountsendUSD.Text))
                amountsendBTC.CursorPosition = amountsendBTC.Text.Length;
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                amountsendBTC.Unfocus();
                amountsendUSD.Unfocus();

            });
        }
    }
}