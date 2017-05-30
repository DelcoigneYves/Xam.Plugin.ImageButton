using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sample
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void ImageButton_OnSelectedChanged(object sender, ImageButton.Abstractions.ImageButton.SelectedChangedArgs e)
        {
            Debug.WriteLine("Selected: " + e.Selected);
        }
    }
}
