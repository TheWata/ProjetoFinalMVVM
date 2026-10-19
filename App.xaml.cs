using Microsoft.Maui.Controls;
using ProjetoFinalMVVM.Services;
using ProjetoFinalMVVM.Views;

namespace ProjetoFinalMVVM
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Inicializa a base SQLite de forma assíncrona na inicialização
            Task.Run(async () =>
            {
                await SQLiteDatabaseHelper.Instancia.InicializarAsync();
            });
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Cria a pilha de navegação clássica via NavigationPage
            var paginaInicial = new NavigationPage(new LoginView());

            // Estilização do cabeçalho da barra de navegação no tema Claro Amigável
            paginaInicial.BarBackgroundColor = Color.FromArgb("#FFFFFF");
            paginaInicial.BarTextColor = Color.FromArgb("#0F172A");

            return new Window(paginaInicial);
        }
    }
}