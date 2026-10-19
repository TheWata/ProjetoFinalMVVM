using Microsoft.Maui.Controls;
using ProjetoFinalMVVM.ViewModels;

namespace ProjetoFinalMVVM.Views
{
    public partial class ClienteMainView : ContentPage
    {
        private readonly ItemViewModel _viewModel;

        public ClienteMainView()
        {
            InitializeComponent();
            _viewModel = new ItemViewModel();
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.CarregarDadosClienteCommand.Execute(null);
        }
    }
}
