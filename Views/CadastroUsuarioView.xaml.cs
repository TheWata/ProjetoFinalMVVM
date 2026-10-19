using Microsoft.Maui.Controls;
using ProjetoFinalMVVM.ViewModels;

namespace ProjetoFinalMVVM.Views
{
    public partial class CadastroUsuarioView : ContentPage
    {
        public CadastroUsuarioView()
        {
            InitializeComponent();
            BindingContext = new UsuarioViewModel();
        }
    }
}
