using Microsoft.Maui.Controls;
using ProjetoFinalMVVM.Models;
using ProjetoFinalMVVM.ViewModels;

namespace ProjetoFinalMVVM.Views
{
    public partial class AluguelDetalhesView : ContentPage
    {
        public AluguelDetalhesView(Item console, ItemViewModel? itemViewModel = null)
        {
            InitializeComponent();
            BindingContext = new AluguelDetalhesViewModel(console, itemViewModel);
        }
    }
}
