using Microsoft.Maui.Controls;
using ProjetoFinalMVVM.ViewModels;

namespace ProjetoFinalMVVM.Views
{
    public partial class LocadoraMainView : ContentPage
    {
        private readonly ItemViewModel _viewModel;

        public LocadoraMainView()
        {
            InitializeComponent();
            _viewModel = new ItemViewModel();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.CarregarDadosLocadoraAsync();
        }
    }
}
