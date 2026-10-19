using Microsoft.Maui.Controls;
using ProjetoFinalMVVM.ViewModels;

namespace ProjetoFinalMVVM.Views
{
    public partial class AdminMainView : ContentPage
    {
        private readonly ItemViewModel _viewModel;

        public AdminMainView()
        {
            InitializeComponent();
            _viewModel = new ItemViewModel();
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Recarrega os dados do SQLite ao exibir a página
            _viewModel.CarregarDadosAdminCommand.Execute(null);
        }
    }
}
