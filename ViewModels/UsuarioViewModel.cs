using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using ProjetoFinalMVVM.Helpers;
using ProjetoFinalMVVM.Models;
using ProjetoFinalMVVM.Services;
using ProjetoFinalMVVM.Views;

namespace ProjetoFinalMVVM.ViewModels
{
    /// <summary>
    /// ViewModel responsável pelo controle de autenticação e registro de novos usuários.
    /// Implementa INotifyPropertyChanged manualmente e utiliza ICommand com a classe nativa Command.
    /// </summary>
    public class UsuarioViewModel : BaseViewModel
    {
        private string _nome = string.Empty;
        private string _cpf = string.Empty;
        private string _email = string.Empty;
        private string _senha = string.Empty;
        private string _tipo = "Cliente";
        private bool _isBusy;

        public string Nome
        {
            get => _nome;
            set => SetProperty(ref _nome, value);
        }

        public string Cpf
        {
            get => _cpf;
            set => SetProperty(ref _cpf, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Senha
        {
            get => _senha;
            set => SetProperty(ref _senha, value);
        }

        public string Tipo
        {
            get => _tipo;
            set
            {
                if (SetProperty(ref _tipo, value))
                {
                    OnPropertyChanged(nameof(IsTipoLocadora));
                }
            }
        }

        public bool IsTipoLocadora => Tipo == "Locadora";

        private string _locadoraVinculadaNome = string.Empty;
        public string LocadoraVinculadaNome
        {
            get => _locadoraVinculadaNome;
            set => SetProperty(ref _locadoraVinculadaNome, value);
        }

        // ==========================================
        // DADOS EXCLUSIVOS DO CADASTRO DE LOCADORA
        // ==========================================
        private string _locadoraNomeCadastro = string.Empty;
        public string LocadoraNomeCadastro
        {
            get => _locadoraNomeCadastro;
            set => SetProperty(ref _locadoraNomeCadastro, value);
        }

        private bool _usarLocalizacaoGpsAtual;
        public bool UsarLocalizacaoGpsAtual
        {
            get => _usarLocalizacaoGpsAtual;
            set
            {
                if (SetProperty(ref _usarLocalizacaoGpsAtual, value))
                {
                    OnPropertyChanged(nameof(IsEnderecoManualVisivel));
                    if (value)
                    {
                        _ = ExecutarAtivarLocalizacaoAtualAsync();
                    }
                }
            }
        }

        public bool IsEnderecoManualVisivel => !UsarLocalizacaoGpsAtual;

        private string _locadoraCepCadastro = string.Empty;
        public string LocadoraCepCadastro
        {
            get => _locadoraCepCadastro;
            set
            {
                if (SetProperty(ref _locadoraCepCadastro, value))
                {
                    string cepLimpo = value?.Replace("-", "").Replace(".", "").Trim() ?? "";
                    if (cepLimpo.Length == 8)
                    {
                        _ = ExecutarConsultarCepAsync(cepLimpo);
                    }
                }
            }
        }

        private string _locadoraRuaCadastro = string.Empty;
        public string LocadoraRuaCadastro
        {
            get => _locadoraRuaCadastro;
            set => SetProperty(ref _locadoraRuaCadastro, value);
        }

        private string _locadoraNumeroCadastro = string.Empty;
        public string LocadoraNumeroCadastro
        {
            get => _locadoraNumeroCadastro;
            set => SetProperty(ref _locadoraNumeroCadastro, value);
        }

        private string _locadoraBairroCadastro = string.Empty;
        public string LocadoraBairroCadastro
        {
            get => _locadoraBairroCadastro;
            set => SetProperty(ref _locadoraBairroCadastro, value);
        }

        private string _locadoraCidadeCadastro = string.Empty;
        public string LocadoraCidadeCadastro
        {
            get => _locadoraCidadeCadastro;
            set => SetProperty(ref _locadoraCidadeCadastro, value);
        }

        private string _locadoraEstadoCadastro = string.Empty;
        public string LocadoraEstadoCadastro
        {
            get => _locadoraEstadoCadastro;
            set => SetProperty(ref _locadoraEstadoCadastro, value);
        }

        private string _locadoraEnderecoCadastro = string.Empty;
        public string LocadoraEnderecoCadastro
        {
            get => _locadoraEnderecoCadastro;
            set => SetProperty(ref _locadoraEnderecoCadastro, value);
        }

        private string _locadoraTelefoneCadastro = string.Empty;
        public string LocadoraTelefoneCadastro
        {
            get => _locadoraTelefoneCadastro;
            set => SetProperty(ref _locadoraTelefoneCadastro, value);
        }

        private string _locadoraLatitudeCadastroTexto = "-23.550520";
        public string LocadoraLatitudeCadastroTexto
        {
            get => _locadoraLatitudeCadastroTexto;
            set => SetProperty(ref _locadoraLatitudeCadastroTexto, value);
        }

        private string _locadoraLongitudeCadastroTexto = "-46.633308";
        public string LocadoraLongitudeCadastroTexto
        {
            get => _locadoraLongitudeCadastroTexto;
            set => SetProperty(ref _locadoraLongitudeCadastroTexto, value);
        }

        public ObservableCollection<string> LocadorasDisponiveis { get; } = new ObservableCollection<string>();

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        /// <summary>
        /// Lista de perfis para exibição no Picker do formulário de cadastro.
        /// </summary>
        public List<string> TiposDisponiveis { get; } = new List<string>
        {
            "Cliente",
            "Locadora",
            "Administrador"
        };

        // Comandos nativos do .NET MAUI
        public ICommand EntrarCommand { get; }
        public ICommand SalvarCommand { get; }
        public ICommand IrParaCadastroCommand { get; }
        public ICommand VoltarCommand { get; }
        public ICommand PreencherLoginTesteCommand { get; }
        public ICommand SalvarConfiguracoesCommand { get; }
        public ICommand PreencherGpsLocadoraCadastroCommand { get; }
        public ICommand ConsultarCepCadastroCommand { get; }
        public ICommand RestaurarBancoDadosCommand { get; }

        public UsuarioViewModel()
        {
            EntrarCommand = new Command(async () => await ExecutarEntrarAsync());
            SalvarCommand = new Command(async () => await ExecutarSalvarAsync());
            IrParaCadastroCommand = new Command(async () => await ExecutarIrParaCadastroAsync());
            VoltarCommand = new Command(async () => await ExecutarVoltarAsync());
            PreencherLoginTesteCommand = new Command<string>(ExecutarPreencherLoginTeste);
            SalvarConfiguracoesCommand = new Command(async () => await ExecutarSalvarConfiguracoesAsync());
            PreencherGpsLocadoraCadastroCommand = new Command(async () => await ExecutarPreencherGpsLocadoraCadastroAsync());
            ConsultarCepCadastroCommand = new Command(async () => await ExecutarConsultarCepAsync(LocadoraCepCadastro));
            RestaurarBancoDadosCommand = new Command(async () => await ExecutarRestaurarBancoDadosAsync());

            _ = CarregarLocadorasDisponiveisAsync();
        }

        public async Task CarregarLocadorasDisponiveisAsync()
        {
            try
            {
                var locs = await SQLiteDatabaseHelper.Instancia.ObterLocadorasAsync();
                LocadorasDisponiveis.Clear();
                foreach (var l in locs)
                {
                    LocadorasDisponiveis.Add(l.Nome);
                }
                if (LocadorasDisponiveis.Count > 0 && string.IsNullOrEmpty(LocadoraVinculadaNome))
                {
                    LocadoraVinculadaNome = LocadorasDisponiveis[0];
                }
            }
            catch
            {
                // Tratamento seguro
            }
        }

        private Page? ObterPaginaAtual()
        {
            if (Application.Current?.Windows != null && Application.Current.Windows.Count > 0)
            {
                var windowPage = Application.Current.Windows[0].Page;
                if (windowPage is NavigationPage navPage && navPage.CurrentPage != null)
                    return navPage.CurrentPage;
                return windowPage;
            }
            return null;
        }

        private INavigation? ObterNavegacao()
        {
            var page = ObterPaginaAtual();
            return page?.Navigation;
        }

        /// <summary>
        /// Valida as credenciais no SQLite e direciona para a tela correspondente ao tipo de usuário.
        /// </summary>
        private async Task ExecutarEntrarAsync()
        {
            if (IsBusy) return;

            var pagina = ObterPaginaAtual();
            var nav = ObterNavegacao();

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
            {
                if (pagina != null)
                    await pagina.DisplayAlert("Atenção", "Por favor, preencha o E-mail e a Senha para continuar.", "OK");
                return;
            }

            try
            {
                IsBusy = true;
                var usuario = await SQLiteDatabaseHelper.Instancia.ObterUsuarioPorCredenciaisAsync(Email.Trim(), Senha);

                if (usuario == null)
                {
                    if (pagina != null)
                        await pagina.DisplayAlert("Acesso Negado", "E-mail ou senha incorretos. Verifique suas credenciais.", "OK");
                    return;
                }

                // Armazena no Singleton global para acesso em todas as telas
                UsuarioAtual.Instancia.DefinirUsuario(usuario);

                // Limpa campos sensíveis da tela de login
                Senha = string.Empty;

                if (nav != null)
                {
                    if (usuario.Tipo == "Administrador")
                    {
                        await nav.PushAsync(new AdminMainView());
                    }
                    else if (usuario.Tipo == "Locadora")
                    {
                        await nav.PushAsync(new LocadoraMainView());
                    }
                    else
                    {
                        await nav.PushAsync(new ClienteMainView());
                    }
                }
            }
            catch (Exception ex)
            {
                if (pagina != null)
                    await pagina.DisplayAlert("Erro de Login", $"Falha ao autenticar: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Ativa o modo de localização atual por GPS, obtendo as coordenadas e tentando preencher o endereço por geocodificação reversa.
        /// </summary>
        private async Task ExecutarAtivarLocalizacaoAtualAsync()
        {
            try
            {
                var loc = await GeolocalizacaoService.Instancia.ObterLocalizacaoAsync(forcarAtualizacao: true);
                LocadoraLatitudeCadastroTexto = loc.Latitude.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
                LocadoraLongitudeCadastroTexto = loc.Longitude.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);

                var placemark = await GeolocalizacaoService.Instancia.ObterEnderecoPorCoordenadasAsync(loc.Latitude, loc.Longitude);
                if (placemark != null)
                {
                    if (!string.IsNullOrWhiteSpace(placemark.Thoroughfare))
                        LocadoraRuaCadastro = placemark.Thoroughfare;
                    if (!string.IsNullOrWhiteSpace(placemark.SubLocality))
                        LocadoraBairroCadastro = placemark.SubLocality;
                    if (!string.IsNullOrWhiteSpace(placemark.Locality))
                        LocadoraCidadeCadastro = placemark.Locality;
                    if (!string.IsNullOrWhiteSpace(placemark.AdminArea))
                        LocadoraEstadoCadastro = placemark.AdminArea;
                    if (!string.IsNullOrWhiteSpace(placemark.PostalCode))
                        LocadoraCepCadastro = placemark.PostalCode;

                    LocadoraEnderecoCadastro = $"{LocadoraRuaCadastro}, {(string.IsNullOrWhiteSpace(LocadoraNumeroCadastro) ? "S/N" : LocadoraNumeroCadastro)} - {LocadoraCidadeCadastro}/{LocadoraEstadoCadastro}";
                }
                else
                {
                    LocadoraEnderecoCadastro = $"Localização GPS ({loc.Latitude:F4}, {loc.Longitude:F4})";
                }
            }
            catch
            {
                LocadoraEnderecoCadastro = "Localização Atual (GPS)";
            }
        }

        /// <summary>
        /// Realiza a busca de endereço pelo CEP através do serviço ViaCEP e preenche rua, bairro, cidade e estado.
        /// </summary>
        private async Task ExecutarConsultarCepAsync(string? cep)
        {
            if (string.IsNullOrWhiteSpace(cep)) return;

            try
            {
                var resultado = await ViaCepService.Instancia.ConsultarCepAsync(cep);
                if (resultado != null)
                {
                    LocadoraRuaCadastro = resultado.Logradouro ?? "";
                    LocadoraBairroCadastro = resultado.Bairro ?? "";
                    LocadoraCidadeCadastro = resultado.Localidade ?? "";
                    LocadoraEstadoCadastro = resultado.Uf ?? "";

                    LocadoraEnderecoCadastro = $"{LocadoraRuaCadastro}, {LocadoraNumeroCadastro} - {LocadoraBairroCadastro}, {LocadoraCidadeCadastro} - {LocadoraEstadoCadastro}";
                }
            }
            catch
            {
                // Fallback gracioso
            }
        }

        /// <summary>
        /// Captura a localização GPS do dispositivo e preenche a latitude e longitude da locadora em cadastro.
        /// </summary>
        private async Task ExecutarPreencherGpsLocadoraCadastroAsync()
        {
            var pagina = ObterPaginaAtual();
            var loc = await GeolocalizacaoService.Instancia.ObterLocalizacaoAsync(forcarAtualizacao: true);

            LocadoraLatitudeCadastroTexto = loc.Latitude.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
            LocadoraLongitudeCadastroTexto = loc.Longitude.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);

            if (pagina != null)
            {
                await pagina.DisplayAlert(
                    "GPS Atualizado",
                    $"Coordenadas capturadas com sucesso:\nLat: {LocadoraLatitudeCadastroTexto}\nLon: {LocadoraLongitudeCadastroTexto}",
                    "OK");
            }
        }

        /// <summary>
        /// Cadastra um novo usuário no banco SQLite com validações de campos obrigatórios.
        /// Se o perfil selecionado for 'Locadora', cria simultaneamente a nova filial com endereço estruturado e GPS.
        /// </summary>
        private async Task ExecutarSalvarAsync()
        {
            if (IsBusy) return;

            var pagina = ObterPaginaAtual();
            var nav = ObterNavegacao();

            if (string.IsNullOrWhiteSpace(Nome) ||
                string.IsNullOrWhiteSpace(Cpf) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Senha))
            {
                if (pagina != null)
                    await pagina.DisplayAlert("Campos Obrigatórios", "Todos os campos do responsável (Nome, CPF, E-mail e Senha) devem ser preenchidos.", "OK");
                return;
            }

            string perfil = string.IsNullOrWhiteSpace(Tipo) ? "Cliente" : Tipo;

            if (perfil == "Locadora")
            {
                if (string.IsNullOrWhiteSpace(LocadoraNomeCadastro) ||
                    string.IsNullOrWhiteSpace(LocadoraTelefoneCadastro))
                {
                    if (pagina != null)
                        await pagina.DisplayAlert("Dados da Locadora", "Por favor, preencha o Nome da Locadora e o Telefone de contato.", "OK");
                    return;
                }

                if (!UsarLocalizacaoGpsAtual && string.IsNullOrWhiteSpace(LocadoraRuaCadastro) && string.IsNullOrWhiteSpace(LocadoraEnderecoCadastro))
                {
                    if (pagina != null)
                        await pagina.DisplayAlert("Endereço Obrigatório", "Informe o CEP ou a Rua e Cidade da locadora, ou marque a opção de usar a localização atual.", "OK");
                    return;
                }
            }

            try
            {
                IsBusy = true;

                // Validação de duplicação de e-mail
                var existente = await SQLiteDatabaseHelper.Instancia.ObterUsuarioPorEmailAsync(Email.Trim());
                if (existente != null)
                {
                    if (pagina != null)
                        await pagina.DisplayAlert("Atenção", "Já existe uma conta cadastrada com este e-mail.", "OK");
                    return;
                }

                string locadoraVinculada = string.Empty;

                if (perfil == "Locadora")
                {
                    double lat = GeolocalizacaoService.LatitudePadrao;
                    double lon = GeolocalizacaoService.LongitudePadrao;

                    if (double.TryParse(LocadoraLatitudeCadastroTexto.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedLat))
                        lat = parsedLat;

                    if (double.TryParse(LocadoraLongitudeCadastroTexto.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedLon))
                        lon = parsedLon;

                    string enderecoFinal;
                    if (UsarLocalizacaoGpsAtual)
                    {
                        enderecoFinal = !string.IsNullOrWhiteSpace(LocadoraRuaCadastro)
                            ? $"{LocadoraRuaCadastro}, {(string.IsNullOrWhiteSpace(LocadoraNumeroCadastro) ? "S/N" : LocadoraNumeroCadastro)} - {LocadoraCidadeCadastro}/{LocadoraEstadoCadastro}"
                            : (!string.IsNullOrWhiteSpace(LocadoraEnderecoCadastro) ? LocadoraEnderecoCadastro : $"Localização GPS ({lat:F4}, {lon:F4})");
                    }
                    else
                    {
                        string num = string.IsNullOrWhiteSpace(LocadoraNumeroCadastro) ? "S/N" : LocadoraNumeroCadastro.Trim();
                        string cidUf = !string.IsNullOrWhiteSpace(LocadoraCidadeCadastro) && !string.IsNullOrWhiteSpace(LocadoraEstadoCadastro)
                            ? $"{LocadoraCidadeCadastro.Trim()} - {LocadoraEstadoCadastro.Trim()}"
                            : LocadoraCidadeCadastro.Trim();
                        string cepStr = !string.IsNullOrWhiteSpace(LocadoraCepCadastro) ? $", CEP: {LocadoraCepCadastro.Trim()}" : "";
                        string bairroStr = !string.IsNullOrWhiteSpace(LocadoraBairroCadastro) ? $" - {LocadoraBairroCadastro.Trim()}" : "";

                        enderecoFinal = $"{LocadoraRuaCadastro.Trim()}, {num}{bairroStr}, {cidUf}{cepStr}";
                    }

                    // Cria e cadastra a nova Locadora / Filial no banco de dados SQLite
                    var novaLocadora = new Locadora
                    {
                        Nome = LocadoraNomeCadastro.Trim(),
                        Endereco = enderecoFinal,
                        Telefone = LocadoraTelefoneCadastro.Trim(),
                        Cep = LocadoraCepCadastro?.Trim() ?? "",
                        Rua = LocadoraRuaCadastro?.Trim() ?? "",
                        Numero = LocadoraNumeroCadastro?.Trim() ?? "",
                        Bairro = LocadoraBairroCadastro?.Trim() ?? "",
                        Cidade = LocadoraCidadeCadastro?.Trim() ?? "",
                        Estado = LocadoraEstadoCadastro?.Trim() ?? "",
                        Latitude = lat,
                        Longitude = lon
                    };

                    await SQLiteDatabaseHelper.Instancia.SalvarLocadoraAsync(novaLocadora);
                    locadoraVinculada = novaLocadora.Nome;
                }

                var novoUsuario = new Usuario
                {
                    Nome = Nome.Trim(),
                    Cpf = Cpf.Trim(),
                    Email = Email.Trim(),
                    Senha = Senha,
                    Tipo = perfil,
                    LocadoraVinculadaNome = locadoraVinculada
                };

                await SQLiteDatabaseHelper.Instancia.SalvarUsuarioAsync(novoUsuario);

                if (pagina != null)
                {
                    string msgSucesso = perfil == "Locadora"
                        ? $"Locadora '{locadoraVinculada}' e usuário cadastrados com sucesso!\nFaça login para gerenciar sua unidade."
                        : "Cadastro realizado com sucesso! Faça login para continuar.";
                    await pagina.DisplayAlert("Sucesso", msgSucesso, "OK");
                }

                // Limpa os campos do formulário
                Nome = string.Empty;
                Cpf = string.Empty;
                Email = string.Empty;
                Senha = string.Empty;
                LocadoraNomeCadastro = string.Empty;
                LocadoraEnderecoCadastro = string.Empty;
                LocadoraTelefoneCadastro = string.Empty;
                LocadoraCepCadastro = string.Empty;
                LocadoraRuaCadastro = string.Empty;
                LocadoraNumeroCadastro = string.Empty;
                LocadoraBairroCadastro = string.Empty;
                LocadoraCidadeCadastro = string.Empty;
                LocadoraEstadoCadastro = string.Empty;
                UsarLocalizacaoGpsAtual = false;

                if (nav != null)
                    await nav.PopAsync();
            }
            catch (Exception ex)
            {
                if (pagina != null)
                    await pagina.DisplayAlert("Erro ao Salvar", $"Falha ao gravar usuário: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExecutarIrParaCadastroAsync()
        {
            var nav = ObterNavegacao();
            if (nav != null)
            {
                await nav.PushAsync(new CadastroUsuarioView());
            }
        }

        private async Task ExecutarVoltarAsync()
        {
            var nav = ObterNavegacao();
            if (nav != null)
            {
                await nav.PopAsync();
            }
        }

        /// <summary>
        /// Popula os campos da ViewModel com os dados do usuário autenticado no Singleton.
        /// </summary>
        public void CarregarDadosUsuarioLogado()
        {
            var usuario = UsuarioAtual.Instancia;
            Nome = usuario.Nome;
            Cpf = usuario.Cpf;
            Email = usuario.Email;
            Tipo = usuario.Tipo;
            Senha = string.Empty;
        }

        /// <summary>
        /// Atualiza as configurações de perfil do usuário no SQLite e no Singleton.
        /// </summary>
        private async Task ExecutarSalvarConfiguracoesAsync()
        {
            if (IsBusy) return;

            var pagina = ObterPaginaAtual();
            var nav = ObterNavegacao();

            if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Email))
            {
                if (pagina != null)
                    await pagina.DisplayAlert("Atenção", "Nome e E-mail são obrigatórios.", "OK");
                return;
            }

            try
            {
                IsBusy = true;
                int id = UsuarioAtual.Instancia.Id;
                var usuarioBanco = await SQLiteDatabaseHelper.Instancia.ObterUsuarioPorEmailAsync(UsuarioAtual.Instancia.Email);
                if (usuarioBanco != null)
                {
                    usuarioBanco.Nome = Nome.Trim();
                    usuarioBanco.Cpf = Cpf.Trim();
                    usuarioBanco.Email = Email.Trim();
                    if (!string.IsNullOrWhiteSpace(Senha))
                    {
                        usuarioBanco.Senha = Senha;
                    }
                    await SQLiteDatabaseHelper.Instancia.SalvarUsuarioAsync(usuarioBanco);
                    UsuarioAtual.Instancia.DefinirUsuario(usuarioBanco);
                }

                if (pagina != null)
                    await pagina.DisplayAlert("Sucesso", "Configurações da conta salvas com sucesso!", "OK");

                if (nav != null)
                    await nav.PopAsync();
            }
            catch (Exception ex)
            {
                if (pagina != null)
                    await pagina.DisplayAlert("Erro", $"Falha ao salvar configurações: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ExecutarPreencherLoginTeste(string? tipo)
        {
            if (string.Equals(tipo, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                Email = "admin@gamer.com";
                Senha = "123";
            }
            else if (string.Equals(tipo, "Locadora", StringComparison.OrdinalIgnoreCase))
            {
                Email = "locadora@gamer.com";
                Senha = "123";
            }
            else
            {
                Email = "cliente@gamer.com";
                Senha = "123";
            }
        }

        private async Task ExecutarRestaurarBancoDadosAsync()
        {
            var pagina = ObterPaginaAtual();
            if (pagina == null) return;

            bool confirmou = await pagina.DisplayAlert(
                "Restaurar Banco de Dados",
                "Tem certeza de que deseja limpar e restaurar o banco de dados?\n\nTodas as locações, históricos e alterações serão redefinidos para os valores padrão de fábrica.",
                "Sim, Restaurar",
                "Cancelar");

            if (!confirmou) return;

            try
            {
                IsBusy = true;
                await SQLiteDatabaseHelper.Instancia.RestaurarBancoDadosAsync();
                await pagina.DisplayAlert(
                    "Banco Restaurado! 🔄",
                    "O banco de dados SQLite foi limpo e reconstruído com sucesso!\n\nDados padrão recriados:\n- Admin (admin@gamer.com / 123)\n- Locadora (locadora@gamer.com / 123)\n- Cliente (cliente@gamer.com / 123)",
                    "OK");

                await CarregarLocadorasDisponiveisAsync();
            }
            catch (Exception ex)
            {
                await pagina.DisplayAlert("Erro", $"Falha ao restaurar banco: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
