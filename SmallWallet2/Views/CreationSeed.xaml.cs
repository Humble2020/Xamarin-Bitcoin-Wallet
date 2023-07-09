using SmallWallet2.ViewModels.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SmallWallet2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreationSeed : ContentPage
    {
        public CreationSeedViewModel CsVm { get; set; }
        public CreationSeed(string walletSeed)
        {

            InitializeComponent();
            BindingContext = CsVm = new CreationSeedViewModel(Navigation, walletSeed);
            Device.BeginInvokeOnMainThread(() =>
            {
                seedInFullName.Text = walletSeed;
            });
         
        }
    }
}