using Microsoft.Maui.Controls;
using ProjetoFinalMVVM.ViewModels;

namespace ProjetoFinalMVVM.Views
{
    public partial class LocadoraCadastroView : ContentPage
    {
        public LocadoraCadastroView(ItemViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
