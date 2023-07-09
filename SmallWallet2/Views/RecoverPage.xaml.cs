using NBitcoin;
using SmallWallet2.src;
using SmallWallet2.ViewModels.VM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SmallWallet2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RecoverPage : ContentPage
    {
        public walletViewModel Model { get; set; }
        public RecoverViewModel rVm { get; set; }
        public INavigation navigation { get; set; }
        public RecoverPage(INavigation Navigation)
        {
            InitializeComponent();
            BindingContext = rVm = new RecoverViewModel(navigation);
        }
        private async void OnSetMaxAmount2ButtonClicked(object sender, EventArgs e)
        {
          await  Device.InvokeOnMainThreadAsync(() =>
            {
                if (!string.IsNullOrEmpty(jsfs.Text) && !string.IsNullOrEmpty(mentd.Text))
                    jsfs.Unfocus();
                mentd.Unfocus();
            });
          
        }
    }
}