using Microsoft.Maui.Controls;
using ProjetoFinalMVVM.ViewModels;

namespace ProjetoFinalMVVM.Views
{
    public partial class ConsoleCadastroView : ContentPage
    {
        public ConsoleCadastroView(ItemViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        public ConsoleCadastroView()
        {
            InitializeComponent();
            BindingContext = new ItemViewModel();
        }
    }
}
