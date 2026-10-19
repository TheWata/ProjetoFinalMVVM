using Microsoft.Maui.Controls;
using ProjetoFinalMVVM.ViewModels;

namespace ProjetoFinalMVVM.Views
{
    public partial class HistoricoView : ContentPage
    {
        private readonly ItemViewModel _viewModel;

        public HistoricoView()
        {
            InitializeComponent();
            _viewModel = new ItemViewModel();
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.CarregarHistoricoCommand.Execute(null);
        }
    }
}
