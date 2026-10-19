using Microsoft.Maui.Controls;
using ProjetoFinalMVVM.ViewModels;

namespace ProjetoFinalMVVM.Views
{
    public partial class ConfiguracoesContaView : ContentPage
    {
        private readonly UsuarioViewModel _viewModel;

        public ConfiguracoesContaView()
        {
            InitializeComponent();
            _viewModel = new UsuarioViewModel();
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.CarregarDadosUsuarioLogado();
        }
    }
}
