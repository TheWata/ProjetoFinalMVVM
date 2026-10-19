using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using ProjetoFinalMVVM.Helpers;
using ProjetoFinalMVVM.Models;
using ProjetoFinalMVVM.Services;
using ProjetoFinalMVVM.Views;

namespace ProjetoFinalMVVM.ViewModels
{
    /// <summary>
    /// ViewModel central do aplicativo para gerenciamento de Consoles, Dashboard,
    /// Calendário de Devoluções, Abas de Locação de Clientes e Histórico.
    /// Implementa INotifyPropertyChanged de forma manual e pura através de BaseViewModel.
    /// </summary>
    public class ItemViewModel : BaseViewModel
    {
        // ==========================================
        // PROPRIEDADES DO USUÁRIO LOGADO (SINGLETON)
        // ==========================================
        public string NomeUsuarioLogado => UsuarioAtual.Instancia.Nome;
        public string TipoUsuarioLogado => UsuarioAtual.Instancia.Tipo;
        public string EmailUsuarioLogado => UsuarioAtual.Instancia.Email;
        public string LocadoraNomeUsuarioLogado => UsuarioAtual.Instancia.LocadoraVinculadaNome;
        public bool IsAdmin => UsuarioAtual.Instancia.IsAdministrador;
        public bool IsLocadora => UsuarioAtual.Instancia.IsLocadora;
        public bool PodeEscolherLocadora => UsuarioAtual.Instancia.IsAdministrador;
        public string StatusGpsTexto => GeolocalizacaoService.Instancia.StatusGpsTexto;

        // ==========================================
        // CONTROLE DE ABAS DA VISÃO DO ADMINISTRADOR
        // ==========================================
        private string _abaAdminAtiva = "Dashboard"; // "Dashboard", "Consoles", "Locadoras", "Usuarios"
        public string AbaAdminAtiva
        {
            get => _abaAdminAtiva;
            set
            {
                if (SetProperty(ref _abaAdminAtiva, value))
                {
                    OnPropertyChanged(nameof(IsAbaAdminDashboardAtiva));
                    OnPropertyChanged(nameof(IsAbaAdminConsolesAtiva));
                    OnPropertyChanged(nameof(IsAbaAdminLocadorasAtiva));
                    OnPropertyChanged(nameof(IsAbaAdminUsuariosAtiva));
                    OnPropertyChanged(nameof(CorBotaoAdminDashboard));
                    OnPropertyChanged(nameof(CorTextoAdminDashboard));
                    OnPropertyChanged(nameof(CorBotaoAdminConsoles));
                    OnPropertyChanged(nameof(CorTextoAdminConsoles));
                    OnPropertyChanged(nameof(CorBotaoAdminLocadoras));
                    OnPropertyChanged(nameof(CorTextoAdminLocadoras));
                    OnPropertyChanged(nameof(CorBotaoAdminUsuarios));
                    OnPropertyChanged(nameof(CorTextoAdminUsuarios));
                }
            }
        }

        public bool IsAbaAdminDashboardAtiva => AbaAdminAtiva == "Dashboard";
        public bool IsAbaAdminConsolesAtiva => AbaAdminAtiva == "Consoles";
        public bool IsAbaAdminLocadorasAtiva => AbaAdminAtiva == "Locadoras";
        public bool IsAbaAdminUsuariosAtiva => AbaAdminAtiva == "Usuarios";

        public Color CorBotaoAdminDashboard => IsAbaAdminDashboardAtiva ? Color.FromArgb("#16A34A") : Color.FromArgb("#F1F5F9");
        public Color CorTextoAdminDashboard => IsAbaAdminDashboardAtiva ? Colors.White : Color.FromArgb("#475569");

        public Color CorBotaoAdminConsoles => IsAbaAdminConsolesAtiva ? Color.FromArgb("#16A34A") : Color.FromArgb("#F1F5F9");
        public Color CorTextoAdminConsoles => IsAbaAdminConsolesAtiva ? Colors.White : Color.FromArgb("#475569");

        public Color CorBotaoAdminLocadoras => IsAbaAdminLocadorasAtiva ? Color.FromArgb("#16A34A") : Color.FromArgb("#F1F5F9");
        public Color CorTextoAdminLocadoras => IsAbaAdminLocadorasAtiva ? Colors.White : Color.FromArgb("#475569");

        public Color CorBotaoAdminUsuarios => IsAbaAdminUsuariosAtiva ? Color.FromArgb("#16A34A") : Color.FromArgb("#F1F5F9");
        public Color CorTextoAdminUsuarios => IsAbaAdminUsuariosAtiva ? Colors.White : Color.FromArgb("#475569");

        // ==========================================
        // CONTROLE DE ABAS DA VISÃO DO CLIENTE
        // ==========================================
        private string _abaAtiva = "Consoles"; // "Consoles" ou "Locadoras"
        public string AbaAtiva
        {
            get => _abaAtiva;
            set
            {
                if (SetProperty(ref _abaAtiva, value))
                {
                    OnPropertyChanged(nameof(IsAbaConsolesAtiva));
                    OnPropertyChanged(nameof(IsAbaLocadorasAtiva));
                    OnPropertyChanged(nameof(CorBotaoAbaConsoles));
                    OnPropertyChanged(nameof(CorTextoAbaConsoles));
                    OnPropertyChanged(nameof(CorBotaoAbaLocadoras));
                    OnPropertyChanged(nameof(CorTextoAbaLocadoras));
                }
            }
        }

        public bool IsAbaConsolesAtiva => AbaAtiva == "Consoles";
        public bool IsAbaLocadorasAtiva => AbaAtiva == "Locadoras";

        public Color CorBotaoAbaConsoles => IsAbaConsolesAtiva ? Color.FromArgb("#16A34A") : Color.FromArgb("#E2E8F0");
        public Color CorTextoAbaConsoles => IsAbaConsolesAtiva ? Colors.White : Color.FromArgb("#334155");

        public Color CorBotaoAbaLocadoras => IsAbaLocadorasAtiva ? Color.FromArgb("#16A34A") : Color.FromArgb("#E2E8F0");
        public Color CorTextoAbaLocadoras => IsAbaLocadorasAtiva ? Colors.White : Color.FromArgb("#334155");

        // Filtro opcional por locadora
        private string _filtroLocadoraNome = string.Empty;
        public string FiltroLocadoraNome
        {
            get => _filtroLocadoraNome;
            set
            {
                if (SetProperty(ref _filtroLocadoraNome, value))
                {
                    OnPropertyChanged(nameof(TemFiltroAtivo));
                    OnPropertyChanged(nameof(NaoTemFiltroAtivo));
                    OnPropertyChanged(nameof(TextoFiltroAtivo));
                }
            }
        }

        public bool TemFiltroAtivo => !string.IsNullOrEmpty(FiltroLocadoraNome);
        public bool NaoTemFiltroAtivo => string.IsNullOrEmpty(FiltroLocadoraNome);
        public string TextoFiltroAtivo => $"Filtrando: {FiltroLocadoraNome}";

        // ==========================================
        // PROPRIEDADES DA GESTÃO DA LOCADORA (FILIAL)
        // ==========================================
        private decimal _receitaUnidade;
        public decimal ReceitaUnidade
        {
            get => _receitaUnidade;
            set
            {
                if (SetProperty(ref _receitaUnidade, value))
                    OnPropertyChanged(nameof(ReceitaUnidadeFormatada));
            }
        }
        public string ReceitaUnidadeFormatada => $"R$ {ReceitaUnidade:F2}";

        private int _locacoesAtivasUnidadeCount;
        public int LocacoesAtivasUnidadeCount
        {
            get => _locacoesAtivasUnidadeCount;
            set => SetProperty(ref _locacoesAtivasUnidadeCount, value);
        }

        private int _consolesDisponiveisUnidadeCount;
        public int ConsolesDisponiveisUnidadeCount
        {
            get => _consolesDisponiveisUnidadeCount;
            set => SetProperty(ref _consolesDisponiveisUnidadeCount, value);
        }

        private int _consolesAlugadosUnidadeCount;
        public int ConsolesAlugadosUnidadeCount
        {
            get => _consolesAlugadosUnidadeCount;
            set => SetProperty(ref _consolesAlugadosUnidadeCount, value);
        }

        // ==========================================
        // CAMPOS DO CADASTRO/EDIÇÃO DE LOCADORA
        // ==========================================
        private int _locadoraIdEdicao;
        public int LocadoraIdEdicao
        {
            get => _locadoraIdEdicao;
            set
            {
                if (SetProperty(ref _locadoraIdEdicao, value))
                {
                    OnPropertyChanged(nameof(TituloFormularioLocadora));
                    OnPropertyChanged(nameof(TextoBotaoSalvarLocadora));
                }
            }
        }

        private string _locadoraNomeEdicao = string.Empty;
        public string LocadoraNomeEdicao
        {
            get => _locadoraNomeEdicao;
            set => SetProperty(ref _locadoraNomeEdicao, value);
        }

        private bool _usarLocalizacaoGpsAtualEdicao;
        public bool UsarLocalizacaoGpsAtualEdicao
        {
            get => _usarLocalizacaoGpsAtualEdicao;
            set
            {
                if (SetProperty(ref _usarLocalizacaoGpsAtualEdicao, value))
                {
                    OnPropertyChanged(nameof(NaoUsaGpsEdicao));
                    if (value)
                    {
                        _ = ObterEnderecoGpsEdicaoAsync();
                    }
                }
            }
        }
        public bool NaoUsaGpsEdicao => !UsarLocalizacaoGpsAtualEdicao;

        private string _locadoraCepEdicao = string.Empty;
        public string LocadoraCepEdicao
        {
            get => _locadoraCepEdicao;
            set
            {
                if (SetProperty(ref _locadoraCepEdicao, value))
                {
                    string limpo = (value ?? "").Replace("-", "").Trim();
                    if (limpo.Length == 8)
                    {
                        _ = ConsultarCepLocadoraEdicaoAsync();
                    }
                }
            }
        }

        private string _locadoraRuaEdicao = string.Empty;
        public string LocadoraRuaEdicao
        {
            get => _locadoraRuaEdicao;
            set => SetProperty(ref _locadoraRuaEdicao, value);
        }

        private string _locadoraNumeroEdicao = string.Empty;
        public string LocadoraNumeroEdicao
        {
            get => _locadoraNumeroEdicao;
            set => SetProperty(ref _locadoraNumeroEdicao, value);
        }

        private string _locadoraBairroEdicao = string.Empty;
        public string LocadoraBairroEdicao
        {
            get => _locadoraBairroEdicao;
            set => SetProperty(ref _locadoraBairroEdicao, value);
        }

        private string _locadoraCidadeEdicao = string.Empty;
        public string LocadoraCidadeEdicao
        {
            get => _locadoraCidadeEdicao;
            set => SetProperty(ref _locadoraCidadeEdicao, value);
        }

        private string _locadoraEstadoEdicao = string.Empty;
        public string LocadoraEstadoEdicao
        {
            get => _locadoraEstadoEdicao;
            set => SetProperty(ref _locadoraEstadoEdicao, value);
        }

        private string _locadoraEnderecoEdicao = string.Empty;
        public string LocadoraEnderecoEdicao
        {
            get => _locadoraEnderecoEdicao;
            set => SetProperty(ref _locadoraEnderecoEdicao, value);
        }

        private string _locadoraTelefoneEdicao = string.Empty;
        public string LocadoraTelefoneEdicao
        {
            get => _locadoraTelefoneEdicao;
            set => SetProperty(ref _locadoraTelefoneEdicao, value);
        }

        private string _locadoraLatitudeEdicaoTexto = "-23.550520";
        public string LocadoraLatitudeEdicaoTexto
        {
            get => _locadoraLatitudeEdicaoTexto;
            set => SetProperty(ref _locadoraLatitudeEdicaoTexto, value);
        }

        private string _locadoraLongitudeEdicaoTexto = "-46.633308";
        public string LocadoraLongitudeEdicaoTexto
        {
            get => _locadoraLongitudeEdicaoTexto;
            set => SetProperty(ref _locadoraLongitudeEdicaoTexto, value);
        }

        public string TituloFormularioLocadora => LocadoraIdEdicao == 0 ? "Cadastrar Filial / Locadora" : $"Editar Filial #{LocadoraIdEdicao}";
        public string TextoBotaoSalvarLocadora => LocadoraIdEdicao == 0 ? "Cadastrar Filial" : "Salvar Alterações";

        // ==========================================
        // COLEÇÕES OBSERVÁVEIS
        // ==========================================
        public ObservableCollection<Item> Itens { get; } = new ObservableCollection<Item>();
        public ObservableCollection<Item> ItensDaUnidade { get; } = new ObservableCollection<Item>();
        public ObservableCollection<Item> ConsolesDisponiveis { get; } = new ObservableCollection<Item>();
        public ObservableCollection<Locadora> Locadoras { get; } = new ObservableCollection<Locadora>();
        public ObservableCollection<Locadora> TodasLocadoras { get; } = new ObservableCollection<Locadora>();
        public ObservableCollection<Usuario> TodosUsuarios { get; } = new ObservableCollection<Usuario>();
        public ObservableCollection<DiaCalendario> DiasCalendario { get; } = new ObservableCollection<DiaCalendario>();
        public ObservableCollection<Historico> DevolucoesDoDia { get; } = new ObservableCollection<Historico>();
        public ObservableCollection<Historico> Historicos { get; } = new ObservableCollection<Historico>();
        public ObservableCollection<string> LocadorasNomes { get; } = new ObservableCollection<string>();

        // Coleção de locações pendentes de retirada (aguardando conferência do PIN de 6 dígitos)
        public ObservableCollection<Locacao> LocacoesPendentesRetirada { get; } = new ObservableCollection<Locacao>();

        private int _locacoesPendentesCount;
        public int LocacoesPendentesCount
        {
            get => _locacoesPendentesCount;
            set => SetProperty(ref _locacoesPendentesCount, value);
        }

        // Coleção de serviços opcionais gerenciados pela locadora vinculada
        public ObservableCollection<ServicoOpcional> ServicosOpcionaisDaUnidade { get; } = new ObservableCollection<ServicoOpcional>();

        private int _servicosOpcionaisCount;
        public int ServicosOpcionaisCount
        {
            get => _servicosOpcionaisCount;
            set => SetProperty(ref _servicosOpcionaisCount, value);
        }

        // Controle do Formulário de Cadastro de Novo Serviço Opcional
        private bool _isFormularioNovoServicoVisivel;
        public bool IsFormularioNovoServicoVisivel
        {
            get => _isFormularioNovoServicoVisivel;
            set => SetProperty(ref _isFormularioNovoServicoVisivel, value);
        }

        private string _novoServicoNome = string.Empty;
        public string NovoServicoNome
        {
            get => _novoServicoNome;
            set => SetProperty(ref _novoServicoNome, value);
        }

        private string _novoServicoDescricao = string.Empty;
        public string NovoServicoDescricao
        {
            get => _novoServicoDescricao;
            set => SetProperty(ref _novoServicoDescricao, value);
        }

        private string _novoServicoValorTexto = string.Empty;
        public string NovoServicoValorTexto
        {
            get => _novoServicoValorTexto;
            set => SetProperty(ref _novoServicoValorTexto, value);
        }

        // ==========================================
        // WIDGET FLUTUANTE ESTILO IFOOD (PIN DO CLIENTE)
        // ==========================================
        private bool _temLocacaoPendenteRetirada;
        public bool TemLocacaoPendenteRetirada
        {
            get => _temLocacaoPendenteRetirada;
            set => SetProperty(ref _temLocacaoPendenteRetirada, value);
        }

        private string _codigoPinPendente = string.Empty;
        public string CodigoPinPendente
        {
            get => _codigoPinPendente;
            set => SetProperty(ref _codigoPinPendente, value);
        }

        private string _itemNomePendente = string.Empty;
        public string ItemNomePendente
        {
            get => _itemNomePendente;
            set => SetProperty(ref _itemNomePendente, value);
        }

        private string _locadoraNomePendente = string.Empty;
        public string LocadoraNomePendente
        {
            get => _locadoraNomePendente;
            set => SetProperty(ref _locadoraNomePendente, value);
        }

        private bool _isModalPinVisivel;
        public bool IsModalPinVisivel
        {
            get => _isModalPinVisivel;
            set => SetProperty(ref _isModalPinVisivel, value);
        }

        // Cache local de todos os consoles para filtros rápidos
        private readonly List<Item> _todosConsolesDisponiveisCache = new List<Item>();

        // ==========================================
        // CAMPOS DO FORMULÁRIO DE CONSOLE (ADMIN)
        // ==========================================
        private int _itemId;
        private string _nomeConsole = string.Empty;
        private string _chaveSerial = string.Empty;
        private string _imgDiretorio = string.Empty;
        private string _desc = string.Empty;
        private string _contaVinculada = string.Empty;
        private decimal _valorDiaria = 35.00m;
        private string _statusConsole = "Disponível";
        private string _locadoraSelecionadaNome = string.Empty;

        public int ItemId
        {
            get => _itemId;
            set
            {
                if (SetProperty(ref _itemId, value))
                {
                    OnPropertyChanged(nameof(TituloFormulario));
                    OnPropertyChanged(nameof(TextoBotaoSalvar));
                    OnPropertyChanged(nameof(IsEditando));
                }
            }
        }

        public string NomeConsole
        {
            get => _nomeConsole;
            set => SetProperty(ref _nomeConsole, value);
        }

        public string ChaveSerial
        {
            get => _chaveSerial;
            set => SetProperty(ref _chaveSerial, value);
        }

        public string ImgDiretorio
        {
            get => _imgDiretorio;
            set => SetProperty(ref _imgDiretorio, value);
        }

        public string Desc
        {
            get => _desc;
            set => SetProperty(ref _desc, value);
        }

        public string ContaVinculada
        {
            get => _contaVinculada;
            set => SetProperty(ref _contaVinculada, value);
        }

        private bool _permiteDiaria = true;
        public bool PermiteDiaria
        {
            get => _permiteDiaria;
            set => SetProperty(ref _permiteDiaria, value);
        }

        public decimal ValorDiaria
        {
            get => _valorDiaria;
            set => SetProperty(ref _valorDiaria, value);
        }

        private bool _permiteSemanal = true;
        public bool PermiteSemanal
        {
            get => _permiteSemanal;
            set => SetProperty(ref _permiteSemanal, value);
        }

        private decimal _valorSemanal = 180.00m;
        public decimal ValorSemanal
        {
            get => _valorSemanal;
            set => SetProperty(ref _valorSemanal, value);
        }

        private bool _permiteMensal = true;
        public bool PermiteMensal
        {
            get => _permiteMensal;
            set => SetProperty(ref _permiteMensal, value);
        }

        private decimal _valorMensal = 450.00m;
        public decimal ValorMensal
        {
            get => _valorMensal;
            set => SetProperty(ref _valorMensal, value);
        }

        public string StatusConsole
        {
            get => _statusConsole;
            set => SetProperty(ref _statusConsole, value);
        }

        public string LocadoraSelecionadaNome
        {
            get => _locadoraSelecionadaNome;
            set => SetProperty(ref _locadoraSelecionadaNome, value);
        }

        public string TituloFormulario => ItemId == 0 ? "Cadastrar Console" : $"Editar Console #{ItemId}";
        public string TextoBotaoSalvar => ItemId == 0 ? "Cadastrar no Estoque" : "Salvar Alterações";
        public bool IsEditando => ItemId > 0;

        // ==========================================
        // SELEÇÃO DO CALENDÁRIO
        // ==========================================
        private DiaCalendario? _selecionadoDia;
        public DiaCalendario? SelecionadoDia
        {
            get => _selecionadoDia;
            set
            {
                if (SetProperty(ref _selecionadoDia, value) && value != null)
                {
                    _ = CarregarDevolucoesDoDiaAsync(value.Data);
                }
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        // ==========================================
        // PROPRIEDADES DO DASHBOARD FINANCEIRO (ADMIN)
        // ==========================================
        private decimal _receitaTotal;
        public decimal ReceitaTotal
        {
            get => _receitaTotal;
            set
            {
                if (SetProperty(ref _receitaTotal, value))
                    OnPropertyChanged(nameof(ReceitaTotalFormatada));
            }
        }

        private decimal _receitaExtras;
        public decimal ReceitaExtras
        {
            get => _receitaExtras;
            set
            {
                if (SetProperty(ref _receitaExtras, value))
                    OnPropertyChanged(nameof(ReceitaExtrasFormatada));
            }
        }

        private decimal _ticketMedio;
        public decimal TicketMedio
        {
            get => _ticketMedio;
            set
            {
                if (SetProperty(ref _ticketMedio, value))
                    OnPropertyChanged(nameof(TicketMedioFormatado));
            }
        }

        private int _consolesAlugadosCount;
        public int ConsolesAlugadosCount
        {
            get => _consolesAlugadosCount;
            set => SetProperty(ref _consolesAlugadosCount, value);
        }

        private int _consolesDisponiveisCount;
        public int ConsolesDisponiveisCount
        {
            get => _consolesDisponiveisCount;
            set => SetProperty(ref _consolesDisponiveisCount, value);
        }

        private int _totalLocacoesCount;
        public int TotalLocacoesCount
        {
            get => _totalLocacoesCount;
            set => SetProperty(ref _totalLocacoesCount, value);
        }

        public string ReceitaTotalFormatada => $"R$ {ReceitaTotal:F2}";
        public string ReceitaExtrasFormatada => $"R$ {ReceitaExtras:F2}";
        public string TicketMedioFormatado => $"R$ {TicketMedio:F2}";

        // ==========================================
        // COMANDOS NATIVOS (.NET MAUI Command)
        // ==========================================
        public ICommand CarregarDadosAdminCommand { get; }
        public ICommand CarregarDadosClienteCommand { get; }
        public ICommand CarregarDadosLocadoraCommand { get; }
        public ICommand CarregarHistoricoCommand { get; }
        public ICommand SalvarItemCommand { get; }
        public ICommand EditarItemCommand { get; }
        public ICommand ExcluirItemCommand { get; }
        public ICommand LimparFormularioCommand { get; }

        public ICommand TrocarAbaCommand { get; }
        public ICommand TrocarAbaAdminCommand { get; }
        public ICommand VerConsolesDaLocadoraCommand { get; }
        public ICommand VerDetalhesLocadoraCommand { get; }
        public ICommand LimparFiltroLocadoraCommand { get; }

        public ICommand IrParaNovaLocadoraCommand { get; }
        public ICommand EditarLocadoraCommand { get; }
        public ICommand SalvarLocadoraCommand { get; }
        public ICommand ExcluirLocadoraCommand { get; }
        public ICommand PreencherGpsLocadoraEdicaoCommand { get; }
        public ICommand ConsultarCepLocadoraEdicaoCommand { get; }
        public ICommand SelecionarImagemConsoleCommand { get; }
        public ICommand ExcluirUsuarioCommand { get; }

        public ICommand AbrirRotaNoMapaCommand { get; }
        public ICommand LigarLocadoraCommand { get; }
        public ICommand AtualizarGpsCommand { get; }

        public ICommand AlugarConsoleCommand { get; }
        public ICommand ReceberConsoleCommand { get; }
        public ICommand IrParaHistoricoCommand { get; }
        public ICommand IrParaNovoConsoleCommand { get; }
        public ICommand IrParaConfiguracoesCommand { get; }
        public ICommand VoltarCommand { get; }
        public ICommand DeslogarCommand { get; }
        public ICommand ValidarCodigoRetiradaCommand { get; }

        public ICommand AbrirModalPinCommand { get; }
        public ICommand FecharModalPinCommand { get; }
        public ICommand CopiarPinCommand { get; }
        public ICommand RestaurarBancoDadosCommand { get; }

        public ICommand AbrirFormularioNovoServicoCommand { get; }
        public ICommand FecharFormularioNovoServicoCommand { get; }
        public ICommand SalvarNovoServicoOpcionalCommand { get; }
        public ICommand ExcluirServicoOpcionalCommand { get; }
        public ICommand AlternarAtivoServicoOpcionalCommand { get; }

        public ItemViewModel()
        {
            CarregarDadosAdminCommand = new Command(async () => await CarregarDadosAdminAsync());
            CarregarDadosClienteCommand = new Command(async () => await CarregarDadosClienteAsync());
            CarregarDadosLocadoraCommand = new Command(async () => await CarregarDadosLocadoraAsync());
            CarregarHistoricoCommand = new Command(async () => await CarregarHistoricoAsync());

            SalvarItemCommand = new Command(async () => await ExecutarSalvarItemAsync());
            EditarItemCommand = new Command<Item>(ExecutarEditarItem);
            ExcluirItemCommand = new Command<Item>(async item => await ExecutarExcluirItemAsync(item));
            LimparFormularioCommand = new Command(LimparFormulario);

            TrocarAbaCommand = new Command<string>(aba =>
            {
                if (!string.IsNullOrEmpty(aba))
                    AbaAtiva = aba;
            });

            TrocarAbaAdminCommand = new Command<string>(aba =>
            {
                if (!string.IsNullOrEmpty(aba))
                    AbaAdminAtiva = aba;
            });

            VerConsolesDaLocadoraCommand = new Command<Locadora>(ExecutarVerConsolesDaLocadora);
            VerDetalhesLocadoraCommand = new Command<Locadora>(async loc => await ExecutarVerDetalhesLocadoraAsync(loc));
            LimparFiltroLocadoraCommand = new Command(ExecutarLimparFiltroLocadora);

            IrParaNovaLocadoraCommand = new Command(async () => await ExecutarIrParaNovaLocadoraAsync());
            EditarLocadoraCommand = new Command<Locadora>(async loc => await ExecutarEditarLocadoraAsync(loc));
            SalvarLocadoraCommand = new Command(async () => await ExecutarSalvarLocadoraAsync());
            ExcluirLocadoraCommand = new Command<Locadora>(async loc => await ExecutarExcluirLocadoraAsync(loc));
            PreencherGpsLocadoraEdicaoCommand = new Command(async () => await ExecutarPreencherGpsLocadoraEdicaoAsync());
            ConsultarCepLocadoraEdicaoCommand = new Command(async () => await ConsultarCepLocadoraEdicaoAsync());
            SelecionarImagemConsoleCommand = new Command(async () => await ExecutarSelecionarImagemConsoleAsync());
            ExcluirUsuarioCommand = new Command<Usuario>(async user => await ExecutarExcluirUsuarioAsync(user));

            AbrirRotaNoMapaCommand = new Command<Locadora>(async loc => await ExecutarAbrirRotaNoMapaAsync(loc));
            LigarLocadoraCommand = new Command<Locadora>(async loc => await ExecutarLigarLocadoraAsync(loc));
            AtualizarGpsCommand = new Command(async () => await CarregarDadosClienteAsync());

            AlugarConsoleCommand = new Command<Item>(async console => await ExecutarAlugarConsoleAsync(console));
            ReceberConsoleCommand = new Command<Historico>(async historico => await ExecutarReceberConsoleAsync(historico));

            IrParaHistoricoCommand = new Command(async () => await ExecutarIrParaHistoricoAsync());
            IrParaNovoConsoleCommand = new Command(async () => await ExecutarIrParaNovoConsoleAsync());
            IrParaConfiguracoesCommand = new Command(async () => await ExecutarIrParaConfiguracoesAsync());
            VoltarCommand = new Command(async () => await ExecutarVoltarAsync());
            DeslogarCommand = new Command(async () => await ExecutarDeslogarAsync());
            ValidarCodigoRetiradaCommand = new Command<Locacao>(async loc => await ExecutarValidarCodigoRetiradaAsync(loc));

            AbrirModalPinCommand = new Command(() => IsModalPinVisivel = true);
            FecharModalPinCommand = new Command(() => IsModalPinVisivel = false);
            CopiarPinCommand = new Command(async () => await ExecutarCopiarPinAsync());
            RestaurarBancoDadosCommand = new Command(async () => await ExecutarRestaurarBancoDadosAsync());

            AbrirFormularioNovoServicoCommand = new Command(() =>
            {
                NovoServicoNome = string.Empty;
                NovoServicoDescricao = string.Empty;
                NovoServicoValorTexto = string.Empty;
                IsFormularioNovoServicoVisivel = true;
            });
            FecharFormularioNovoServicoCommand = new Command(() =>
            {
                IsFormularioNovoServicoVisivel = false;
            });
            SalvarNovoServicoOpcionalCommand = new Command(async () => await ExecutarSalvarNovoServicoOpcionalAsync());
            ExcluirServicoOpcionalCommand = new Command<ServicoOpcional>(async serv => await ExecutarExcluirServicoOpcionalAsync(serv));
            AlternarAtivoServicoOpcionalCommand = new Command<ServicoOpcional>(async serv => await ExecutarAlternarAtivoServicoOpcionalAsync(serv));
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

        // ==========================================
        // FLUXO DO CLIENTE (COM DUAS ABAS)
        // ==========================================

        /// <summary>
        /// Carrega locadoras e consoles disponíveis ordenados pela localização mais próxima.
        /// </summary>
        public async Task CarregarDadosClienteAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                // 1. Obtém a localização GPS atual do usuário (ou referência)
                var localizacao = await GeolocalizacaoService.Instancia.ObterLocalizacaoAsync();
                OnPropertyChanged(nameof(StatusGpsTexto));

                // 2. Carrega as locadoras e calcula a distância real em Km para cada uma
                var listaLocadoras = await SQLiteDatabaseHelper.Instancia.ObterLocadorasAsync();
                foreach (var loc in listaLocadoras)
                {
                    if (loc.Latitude != 0 && loc.Longitude != 0)
                    {
                        loc.DistanciaKm = GeolocalizacaoService.Instancia.CalcularDistanciaKm(
                            localizacao.Latitude, localizacao.Longitude,
                            loc.Latitude, loc.Longitude);
                    }
                }

                // Ordena as filiais pela proximidade do usuário
                listaLocadoras = listaLocadoras.OrderBy(l => l.DistanciaKm).ToList();
                var mapaDistanciaLocadoras = listaLocadoras.ToDictionary(l => l.Nome, l => l.DistanciaKm);

                // 3. Carrega todos os consoles disponíveis
                var todosItens = await SQLiteDatabaseHelper.Instancia.ObterItensAsync();
                var disponiveis = todosItens.Where(i => i.Status == "Disponível").ToList();

                // 4. Atualiza a contagem de consoles livres em cada locadora
                Locadoras.Clear();
                foreach (var loc in listaLocadoras)
                {
                    loc.QuantidadeConsolesDisponiveis = disponiveis.Count(c => c.LocadoraNome == loc.Nome);
                    Locadoras.Add(loc);
                }

                // 5. Atribui a distância da locadora a cada console para ordenação por proximidade
                _todosConsolesDisponiveisCache.Clear();
                foreach (var console in disponiveis)
                {
                    if (mapaDistanciaLocadoras.TryGetValue(console.LocadoraNome, out double dist))
                    {
                        console.LocadoraDistanciaKm = dist;
                    }
                    _todosConsolesDisponiveisCache.Add(console);
                }

                // 6. Aplica filtro inicial (ou todos ordenados por proximidade)
                AplicarFiltroConsoles();

                // 7. Busca se o cliente logado possui alguma locação pendente de retirada (para o botão estilo iFood)
                var locacoesPendentes = await SQLiteDatabaseHelper.Instancia.ObterLocacoesPorStatusAsync(
                    StatusLocacaoEnum.AguardandoRetirada);

                var pendenteCliente = locacoesPendentes.Find(l => l.UsuarioId == UsuarioAtual.Instancia.Id);
                if (pendenteCliente != null)
                {
                    TemLocacaoPendenteRetirada = true;
                    CodigoPinPendente = pendenteCliente.CodigoConfirmacao;
                    ItemNomePendente = pendenteCliente.ItemNome;
                    LocadoraNomePendente = pendenteCliente.LocadoraNome;
                }
                else
                {
                    TemLocacaoPendenteRetirada = false;
                    CodigoPinPendente = string.Empty;
                    ItemNomePendente = string.Empty;
                    LocadoraNomePendente = string.Empty;
                    IsModalPinVisivel = false;
                }
            }
            catch (Exception ex)
            {
                var pagina = ObterPaginaAtual();
                if (pagina != null)
                    await pagina.DisplayAlert("Erro", $"Falha ao carregar catálogo: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Exibe os consoles da lista ordenando pela distância mais próxima da locadora.
        /// </summary>
        private void AplicarFiltroConsoles()
        {
            ConsolesDisponiveis.Clear();

            IEnumerable<Item> query = _todosConsolesDisponiveisCache;

            if (!string.IsNullOrEmpty(FiltroLocadoraNome))
            {
                query = query.Where(c => c.LocadoraNome == FiltroLocadoraNome);
            }

            // Ordena sempre pela distância mais próxima!
            var ordenados = query.OrderBy(c => c.LocadoraDistanciaKm).ToList();

            foreach (var item in ordenados)
            {
                ConsolesDisponiveis.Add(item);
            }
        }

        /// <summary>
        /// Ação da Aba 2 (Locadoras): Leva o usuário diretamente à lista de consoles daquela locadora na Aba 1.
        /// </summary>
        private void ExecutarVerConsolesDaLocadora(Locadora? locadora)
        {
            if (locadora == null) return;

            FiltroLocadoraNome = locadora.Nome;
            AplicarFiltroConsoles();

            // Alterna para a primeira aba (Consoles)
            AbaAtiva = "Consoles";
        }

        private void ExecutarLimparFiltroLocadora()
        {
            FiltroLocadoraNome = string.Empty;
            AplicarFiltroConsoles();
        }

        private async Task ExecutarVerDetalhesLocadoraAsync(Locadora? locadora)
        {
            if (locadora == null) return;

            var pagina = ObterPaginaAtual();
            if (pagina == null) return;

            string mensagem =
                $"Unidade: {locadora.Nome}\n\n" +
                $"Endereço: {locadora.Endereco}\n\n" +
                $"Distância: {locadora.DistanciaFormatada}\n\n" +
                $"Consoles disponíveis: {locadora.QuantidadeConsolesDisponiveis}\n\n" +
                $"Horário de Funcionamento: Seg a Sáb das 09h às 21h.\n\n" +
                $"Retirada imediata de consoles mediante apresentação de documento com foto.";

            bool verConsoles = await pagina.DisplayAlert(
                locadora.Nome,
                mensagem,
                "Ver Consoles Desta Unidade",
                "Fechar");

            if (verConsoles)
            {
                ExecutarVerConsolesDaLocadora(locadora);
            }
        }

        /// <summary>
        /// Direciona o cliente para a página profissional de configuração do aluguel
        /// onde o cliente escolhe modalidades (diária, semanal, mensal) e serviços extras.
        /// </summary>
        private async Task ExecutarAlugarConsoleAsync(Item? console)
        {
            if (console == null) return;

            var nav = ObterNavegacao();
            if (nav != null)
            {
                await nav.PushAsync(new AluguelDetalhesView(console, this));
            }
        }

        // ==========================================
        // FLUXO DO ADMINISTRADOR
        // ==========================================

        public async Task CarregarDadosAdminAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                // 1. Carrega locadoras para o picker do cadastro e lista de filiais
                var locadoras = await SQLiteDatabaseHelper.Instancia.ObterLocadorasAsync();
                LocadorasNomes.Clear();
                TodasLocadoras.Clear();
                foreach (var loc in locadoras)
                {
                    LocadorasNomes.Add(loc.Nome);
                    TodasLocadoras.Add(loc);
                }

                if (string.IsNullOrEmpty(LocadoraSelecionadaNome) && LocadorasNomes.Count > 0)
                {
                    LocadoraSelecionadaNome = LocadorasNomes[0];
                }

                // 2. Carrega lista completa de consoles
                var listaItens = await SQLiteDatabaseHelper.Instancia.ObterItensAsync();
                Itens.Clear();
                foreach (var item in listaItens)
                {
                    Itens.Add(item);
                }

                // 3. Atualiza contadores
                ConsolesAlugadosCount = listaItens.Count(i => i.Status == "Alugado");
                ConsolesDisponiveisCount = listaItens.Count(i => i.Status == "Disponível");

                // 4. Carrega histórico e calcula Dashboard Financeiro Geral
                var historicos = await SQLiteDatabaseHelper.Instancia.ObterHistoricosAsync();
                TotalLocacoesCount = historicos.Count;
                ReceitaTotal = historicos.Sum(h => h.ValorTotal);
                ReceitaExtras = historicos.Sum(h => h.TaxaAdicional);
                TicketMedio = TotalLocacoesCount > 0 ? (ReceitaTotal / TotalLocacoesCount) : 0.00m;

                // 5. Gera os 14 dias do calendário geral
                await GerarCalendarioInterativoAsync(historicos);

                // 6. Carrega todos os usuários da plataforma (Clientes, Locadoras e Admins)
                var usuarios = await SQLiteDatabaseHelper.Instancia.ObterUsuariosAsync();
                TodosUsuarios.Clear();
                foreach (var u in usuarios)
                {
                    TodosUsuarios.Add(u);
                }
            }
            catch (Exception ex)
            {
                var pagina = ObterPaginaAtual();
                if (pagina != null)
                    await pagina.DisplayAlert("Erro", $"Falha ao carregar painel admin: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ==========================================
        // FLUXO DA LOCADORA (FILIAL / UNIDADE)
        // ==========================================

        /// <summary>
        /// Carrega os dados exclusivos da filial vinculada ao usuário com perfil Locadora:
        /// estoque de consoles da própria unidade, métricas financeiras locais e calendário de devoluções local.
        /// </summary>
        public async Task CarregarDadosLocadoraAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                string filialNome = UsuarioAtual.Instancia.LocadoraVinculadaNome;

                // 1. Carrega apenas os consoles pertencentes a esta filial
                var todosItens = await SQLiteDatabaseHelper.Instancia.ObterItensAsync();
                var itensFilial = todosItens.Where(i => i.LocadoraNome == filialNome).ToList();

                ItensDaUnidade.Clear();
                foreach (var item in itensFilial)
                {
                    ItensDaUnidade.Add(item);
                }

                // 2. Atualiza contadores locais de estoque
                ConsolesDisponiveisUnidadeCount = itensFilial.Count(i => i.Status == "Disponível");
                ConsolesAlugadosUnidadeCount = itensFilial.Count(i => i.Status == "Alugado");

                // 3. Carrega histórico e métricas financeiras locais
                var historicos = await SQLiteDatabaseHelper.Instancia.ObterHistoricosPorLocadoraAsync(filialNome);
                LocacoesAtivasUnidadeCount = historicos.Count(h => h.StatusLocacao == "Em Andamento");
                ReceitaUnidade = historicos.Sum(h => h.ValorTotal);

                // 4. Preenche o calendário de devoluções exclusivo desta filial
                await GerarCalendarioInterativoAsync(historicos);

                // 5. Carrega as locações pendentes de retirada nesta filial para validação com PIN de 6 dígitos
                var pendentes = await SQLiteDatabaseHelper.Instancia.ObterLocacoesPorStatusAsync(
                    StatusLocacaoEnum.AguardandoRetirada, filialNome);

                LocacoesPendentesRetirada.Clear();
                foreach (var p in pendentes)
                {
                    LocacoesPendentesRetirada.Add(p);
                }
                LocacoesPendentesCount = LocacoesPendentesRetirada.Count;

                // 6. Carrega os serviços opcionais cadastrados e gerenciados exclusivamente por esta filial
                var servicosFilial = await SQLiteDatabaseHelper.Instancia.ObterServicosOpcionaisPorLocadoraAsync(filialNome);
                ServicosOpcionaisDaUnidade.Clear();
                foreach (var s in servicosFilial)
                {
                    ServicosOpcionaisDaUnidade.Add(s);
                }
                ServicosOpcionaisCount = ServicosOpcionaisDaUnidade.Count;
            }
            catch (Exception ex)
            {
                var pagina = ObterPaginaAtual();
                if (pagina != null)
                    await pagina.DisplayAlert("Erro", $"Falha ao carregar dados da filial: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task GerarCalendarioInterativoAsync(List<Historico> historicos)
        {
            DiasCalendario.Clear();
            var hoje = DateTime.Today;

            var nomesDiasSemana = new Dictionary<DayOfWeek, string>
            {
                { DayOfWeek.Sunday, "DOM" },
                { DayOfWeek.Monday, "SEG" },
                { DayOfWeek.Tuesday, "TER" },
                { DayOfWeek.Wednesday, "QUA" },
                { DayOfWeek.Thursday, "QUI" },
                { DayOfWeek.Friday, "SEX" },
                { DayOfWeek.Saturday, "SÁB" }
            };

            for (int i = 0; i < 14; i++)
            {
                var data = hoje.AddDays(i);
                int qtdDevolucoes = historicos.Count(h =>
                    h.StatusLocacao == "Em Andamento" &&
                    h.DataDevolucaoPrevista.Date == data.Date);

                string corDestaque = qtdDevolucoes > 0 ? "#16A34A" : "#E2E8F0";

                DiasCalendario.Add(new DiaCalendario
                {
                    Data = data,
                    DiaSemana = nomesDiasSemana[data.DayOfWeek],
                    DiaNumero = data.Day.ToString("00"),
                    QuantidadeDevolucoes = qtdDevolucoes,
                    CorDestaque = corDestaque,
                    IsSelecionado = i == 0
                });
            }

            if (DiasCalendario.Count > 0)
            {
                SelecionadoDia = DiasCalendario[0];
            }
        }

        public async Task CarregarDevolucoesDoDiaAsync(DateTime data)
        {
            try
            {
                foreach (var dia in DiasCalendario)
                {
                    dia.IsSelecionado = (dia.Data.Date == data.Date);
                }

                List<Historico> devolucoes;
                if (UsuarioAtual.Instancia.IsLocadora)
                {
                    devolucoes = await SQLiteDatabaseHelper.Instancia.ObterDevolucoesPorDataELocadoraAsync(data, UsuarioAtual.Instancia.LocadoraVinculadaNome);
                }
                else
                {
                    devolucoes = await SQLiteDatabaseHelper.Instancia.ObterDevolucoesPorDataAsync(data);
                }

                DevolucoesDoDia.Clear();
                foreach (var dev in devolucoes)
                {
                    DevolucoesDoDia.Add(dev);
                }
            }
            catch (Exception ex)
            {
                var pagina = ObterPaginaAtual();
                if (pagina != null)
                    await pagina.DisplayAlert("Erro", $"Falha ao carregar devoluções da data: {ex.Message}", "OK");
            }
        }

        private async Task ExecutarReceberConsoleAsync(Historico? historico)
        {
            if (historico == null) return;

            var pagina = ObterPaginaAtual();
            if (pagina == null) return;

            bool confirmouRecebimento = await pagina.DisplayAlert(
                "Receber Console",
                $"Deseja registrar o retorno do console {historico.ItemNome} (Serial: {historico.ChaveSerialConsole}) locado por {historico.UsuarioNome}?",
                "Sim, Receber",
                "Cancelar");

            if (!confirmouRecebimento) return;

            if (DateTime.Today > historico.DataDevolucaoPrevista.Date)
            {
                int diasAtraso = (DateTime.Today - historico.DataDevolucaoPrevista.Date).Days;
                bool aplicarTaxa = await pagina.DisplayAlert(
                    "Devolução com Atraso",
                    $"A locação está com atraso de {diasAtraso} dia(s)!\nDeseja aplicar uma taxa de atraso no valor de R$ 20,00?",
                    "Sim, Aplicar Taxa (+ R$ 20)",
                    "Não, Dispensar Taxa");

                if (aplicarTaxa)
                {
                    historico.TaxaAdicional += 20.00m;
                    historico.ValorTotal += 20.00m;
                }
            }

            historico.StatusLocacao = "Devolvido";
            await SQLiteDatabaseHelper.Instancia.AtualizarHistoricoAsync(historico);

            var itemConsole = await SQLiteDatabaseHelper.Instancia.ObterItemPorIdAsync(historico.ItemId);
            if (itemConsole != null)
            {
                itemConsole.Status = "Disponível";
                await SQLiteDatabaseHelper.Instancia.SalvarItemAsync(itemConsole);
            }

            await pagina.DisplayAlert("Sucesso", $"Console {historico.ItemNome} devolvido ao estoque com sucesso!", "OK");

            if (UsuarioAtual.Instancia.IsLocadora)
            {
                await CarregarDadosLocadoraAsync();
            }
            else
            {
                await CarregarDadosAdminAsync();
            }

            if (SelecionadoDia != null)
            {
                await CarregarDevolucoesDoDiaAsync(SelecionadoDia.Data);
            }
        }

        /// <summary>
        /// Validação de entrega/retirada na visão da Locadora baseada em PIN de 6 dígitos.
        /// A locadora digita o código de 6 dígitos que o cliente informou.
        /// Se o código for correto, o status da locação é alterado no SQLite para Alugado
        /// e a lista é atualizada via ObservableCollection.
        /// Se for incorreto, exibe um alerta de validação amigável via DisplayAlert.
        /// </summary>
        private async Task ExecutarValidarCodigoRetiradaAsync(Locacao? locacao)
        {
            if (locacao == null) return;

            var pagina = ObterPaginaAtual();
            if (pagina == null) return;

            string codigoDigitado = locacao.CodigoDigitado?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(codigoDigitado))
            {
                await pagina.DisplayAlert(
                    "Código Obrigatório",
                    "Por favor, digite o PIN numérico de 6 dígitos apresentado pelo cliente para validar a retirada.",
                    "OK");
                return;
            }

            if (codigoDigitado.Length != 6)
            {
                await pagina.DisplayAlert(
                    "Formato Inválido",
                    "O código de confirmação deve possuir exatamente 6 dígitos numéricos (ex: 849201).",
                    "OK");
                return;
            }

            bool validado = await SQLiteDatabaseHelper.Instancia.ValidarCodigoRetiradaAsync(locacao.Id, codigoDigitado);

            if (validado)
            {
                await pagina.DisplayAlert(
                    "Retirada Autorizada! ✅",
                    $"Código {codigoDigitado} validado com sucesso!\n\n" +
                    $"Cliente: {locacao.UsuarioNome}\n" +
                    $"Console: {locacao.ItemNome} (Serial: {locacao.ChaveSerialConsole})\n\n" +
                    "O status da locação foi atualizado para 'Alugado' e o console pode ser entregue.",
                    "Concluir");

                // Atualiza a lista na interface via ObservableCollection
                LocacoesPendentesRetirada.Remove(locacao);
                LocacoesPendentesCount = LocacoesPendentesRetirada.Count;

                // Recarrega contadores e estoque da unidade
                await CarregarDadosLocadoraAsync();
            }
            else
            {
                await pagina.DisplayAlert(
                    "Código Incorreto! ❌",
                    $"O código '{codigoDigitado}' não confere com o PIN gerado para esta locação.\n\n" +
                    "Por favor, solicite ao cliente que confira o código de 6 dígitos na tela de confirmação ou no Histórico de Locações do app.",
                    "Tentar Novamente");
            }
        }

        private async Task ExecutarCopiarPinAsync()
        {
            if (string.IsNullOrEmpty(CodigoPinPendente)) return;

            await Clipboard.Default.SetTextAsync(CodigoPinPendente);
            var pagina = ObterPaginaAtual();
            if (pagina != null)
            {
                await pagina.DisplayAlert("Copiado! 📋", $"Código PIN '{CodigoPinPendente}' copiado com sucesso!\nApresente no balcão da filial {LocadoraNomePendente}.", "OK");
            }
        }

        // ==========================================
        // GESTÃO DE SERVIÇOS OPCIONAIS PELA LOCADORA
        // ==========================================

        private async Task ExecutarSalvarNovoServicoOpcionalAsync()
        {
            var pagina = ObterPaginaAtual();
            if (pagina == null) return;

            if (string.IsNullOrWhiteSpace(NovoServicoNome))
            {
                await pagina.DisplayAlert("Campo Obrigatório", "Por favor, informe o nome do serviço ou acessório adicional.", "OK");
                return;
            }

            string valorLimpo = NovoServicoValorTexto.Replace("R$", "").Trim();
            if (!decimal.TryParse(valorLimpo, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out decimal valor) &&
                !decimal.TryParse(valorLimpo, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out valor))
            {
                await pagina.DisplayAlert("Valor Inválido", "Por favor, digite um valor numérico válido para o serviço (ex: 20,00).", "OK");
                return;
            }

            if (valor <= 0)
            {
                await pagina.DisplayAlert("Valor Inválido", "O valor do serviço opcional deve ser maior que zero.", "OK");
                return;
            }

            string filialNome = UsuarioAtual.Instancia.LocadoraVinculadaNome;
            var todasLocadoras = await SQLiteDatabaseHelper.Instancia.ObterLocadorasAsync();
            var loc = todasLocadoras.FirstOrDefault(l => l.Nome == filialNome);
            int locId = loc?.Id ?? 0;

            // Define ícone temático FontAwesomeSolid baseado no tipo de serviço
            string glyph = "\uf7a9";
            string nomeLower = NovoServicoNome.ToLowerInvariant();
            if (nomeLower.Contains("controle") || nomeLower.Contains("joystick"))
                glyph = "\uf11b";
            else if (nomeLower.Contains("headset") || nomeLower.Contains("fone") || nomeLower.Contains("audio"))
                glyph = "\uf025";
            else if (nomeLower.Contains("seguro") || nomeLower.Contains("garantia") || nomeLower.Contains("protecao"))
                glyph = "\uf3ed";
            else if (nomeLower.Contains("jogo") || nomeLower.Contains("midia") || nomeLower.Contains("disco"))
                glyph = "\uf02d";
            else if (nomeLower.Contains("cabo") || nomeLower.Contains("hdmi") || nomeLower.Contains("fonte"))
                glyph = "\uf0c1";

            var novoServico = new ServicoOpcional
            {
                LocadoraId = locId,
                LocadoraNome = filialNome,
                Nome = NovoServicoNome.Trim(),
                Descricao = string.IsNullOrWhiteSpace(NovoServicoDescricao) ? "Serviço adicional oferecido pela unidade." : NovoServicoDescricao.Trim(),
                IconeGlyph = glyph,
                Valor = valor,
                Ativo = true
            };

            await SQLiteDatabaseHelper.Instancia.SalvarServicoOpcionalAsync(novoServico);

            NovoServicoNome = string.Empty;
            NovoServicoDescricao = string.Empty;
            NovoServicoValorTexto = string.Empty;
            IsFormularioNovoServicoVisivel = false;

            await pagina.DisplayAlert("Sucesso", $"Serviço '{novoServico.Nome}' cadastrado com sucesso para a filial {filialNome}!", "OK");
            await CarregarDadosLocadoraAsync();
        }

        private async Task ExecutarExcluirServicoOpcionalAsync(ServicoOpcional? servico)
        {
            if (servico == null) return;

            var pagina = ObterPaginaAtual();
            if (pagina == null) return;

            bool confirma = await pagina.DisplayAlert(
                "Excluir Serviço Opcional",
                $"Deseja realmente remover o opcional '{servico.Nome}' ({servico.ValorFormatado}) da sua filial?",
                "Sim, Excluir",
                "Cancelar");

            if (!confirma) return;

            await SQLiteDatabaseHelper.Instancia.ExcluirServicoOpcionalAsync(servico);
            ServicosOpcionaisDaUnidade.Remove(servico);
            ServicosOpcionaisCount = ServicosOpcionaisDaUnidade.Count;

            await pagina.DisplayAlert("Removido", "Serviço adicional removido com sucesso.", "OK");
        }

        private async Task ExecutarAlternarAtivoServicoOpcionalAsync(ServicoOpcional? servico)
        {
            if (servico == null) return;

            servico.Ativo = !servico.Ativo;
            await SQLiteDatabaseHelper.Instancia.SalvarServicoOpcionalAsync(servico);
            await CarregarDadosLocadoraAsync();
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

                if (UsuarioAtual.Instancia.IsAdministrador)
                    await CarregarDadosAdminAsync();
                else if (UsuarioAtual.Instancia.IsLocadora)
                    await CarregarDadosLocadoraAsync();
                else
                    await CarregarDadosClienteAsync();
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

        private async Task ExecutarSalvarItemAsync()
        {
            var pagina = ObterPaginaAtual();

            if (UsuarioAtual.Instancia.IsLocadora)
            {
                LocadoraSelecionadaNome = UsuarioAtual.Instancia.LocadoraVinculadaNome;
            }

            if (string.IsNullOrWhiteSpace(NomeConsole) ||
                string.IsNullOrWhiteSpace(ChaveSerial) ||
                string.IsNullOrWhiteSpace(LocadoraSelecionadaNome))
            {
                if (pagina != null)
                    await pagina.DisplayAlert("Campos Obrigatórios", "Informe o Nome, Chave Serial e Locadora do console.", "OK");
                return;
            }

            if (!PermiteDiaria && !PermiteSemanal && !PermiteMensal)
            {
                if (pagina != null)
                    await pagina.DisplayAlert("Planos de Locação", "Habilite ao menos uma modalidade de aluguel (Diária, Semanal ou Mensal).", "OK");
                return;
            }

            if (PermiteDiaria && ValorDiaria <= 0)
            {
                if (pagina != null)
                    await pagina.DisplayAlert("Valor Inválido", "Informe um valor positivo para a diária.", "OK");
                return;
            }

            if (PermiteSemanal && ValorSemanal <= 0)
            {
                if (pagina != null)
                    await pagina.DisplayAlert("Valor Inválido", "Informe um valor positivo para o aluguel semanal.", "OK");
                return;
            }

            if (PermiteMensal && ValorMensal <= 0)
            {
                if (pagina != null)
                    await pagina.DisplayAlert("Valor Inválido", "Informe um valor positivo para o aluguel mensal.", "OK");
                return;
            }

            try
            {
                var item = new Item
                {
                    Id = ItemId,
                    Nome = NomeConsole.Trim(),
                    ChaveSerial = ChaveSerial.Trim(),
                    ImgDiretorio = string.IsNullOrWhiteSpace(ImgDiretorio) ? "https://images.unsplash.com/photo-1621259182978-fbf93132d53d?w=400" : ImgDiretorio.Trim(),
                    Desc = Desc?.Trim() ?? string.Empty,
                    ContaVinculada = ContaVinculada?.Trim() ?? string.Empty,
                    ValorDiaria = ValorDiaria,
                    PermiteDiaria = PermiteDiaria,
                    ValorSemanal = ValorSemanal,
                    PermiteSemanal = PermiteSemanal,
                    ValorMensal = ValorMensal,
                    PermiteMensal = PermiteMensal,
                    Status = string.IsNullOrWhiteSpace(StatusConsole) ? "Disponível" : StatusConsole,
                    LocadoraNome = LocadoraSelecionadaNome
                };

                await SQLiteDatabaseHelper.Instancia.SalvarItemAsync(item);

                if (pagina != null)
                    await pagina.DisplayAlert("Sucesso", ItemId == 0 ? "Console cadastrado com sucesso!" : "Console atualizado com sucesso!", "OK");

                LimparFormulario();

                if (UsuarioAtual.Instancia.IsLocadora)
                {
                    await CarregarDadosLocadoraAsync();
                }
                else
                {
                    await CarregarDadosAdminAsync();
                }

                var nav = ObterNavegacao();
                if (nav != null)
                {
                    await nav.PopAsync();
                }
            }
            catch (Exception ex)
            {
                if (pagina != null)
                    await pagina.DisplayAlert("Erro", $"Falha ao salvar console: {ex.Message}", "OK");
            }
        }

        private async void ExecutarEditarItem(Item? item)
        {
            if (item == null) return;

            ItemId = item.Id;
            NomeConsole = item.Nome;
            ChaveSerial = item.ChaveSerial;
            ImgDiretorio = item.ImgDiretorio;
            Desc = item.Desc;
            ContaVinculada = item.ContaVinculada;
            ValorDiaria = item.ValorDiaria;
            PermiteDiaria = item.PermiteDiaria;
            ValorSemanal = item.ValorSemanal;
            PermiteSemanal = item.PermiteSemanal;
            ValorMensal = item.ValorMensal;
            PermiteMensal = item.PermiteMensal;
            StatusConsole = item.Status;
            LocadoraSelecionadaNome = item.LocadoraNome;

            var nav = ObterNavegacao();
            if (nav != null)
            {
                await nav.PushAsync(new ConsoleCadastroView(this));
            }
        }

        private async Task ExecutarIrParaNovoConsoleAsync()
        {
            LimparFormulario();
            var nav = ObterNavegacao();
            if (nav != null)
            {
                await nav.PushAsync(new ConsoleCadastroView(this));
            }
        }

        private async Task ExecutarIrParaConfiguracoesAsync()
        {
            var nav = ObterNavegacao();
            if (nav != null)
            {
                await nav.PushAsync(new ConfiguracoesContaView());
            }
        }

        private async Task ExecutarExcluirItemAsync(Item? item)
        {
            if (item == null) return;

            var pagina = ObterPaginaAtual();
            if (pagina == null) return;

            bool confirma = await pagina.DisplayAlert(
                "Confirmação",
                $"Deseja realmente excluir o console '{item.Nome}' (Serial: {item.ChaveSerial})?",
                "Sim, Excluir",
                "Cancelar");

            if (!confirma) return;

            await SQLiteDatabaseHelper.Instancia.ExcluirItemAsync(item);
            await pagina.DisplayAlert("Removido", "Console removido com sucesso.", "OK");

            if (ItemId == item.Id)
            {
                LimparFormulario();
            }

            if (UsuarioAtual.Instancia.IsLocadora)
            {
                await CarregarDadosLocadoraAsync();
            }
            else
            {
                await CarregarDadosAdminAsync();
            }
        }

        private void LimparFormulario()
        {
            ItemId = 0;
            NomeConsole = string.Empty;
            ChaveSerial = string.Empty;
            ImgDiretorio = string.Empty;
            Desc = string.Empty;
            ContaVinculada = string.Empty;
            ValorDiaria = 35.00m;
            PermiteDiaria = true;
            ValorSemanal = 180.00m;
            PermiteSemanal = true;
            ValorMensal = 450.00m;
            PermiteMensal = true;
            StatusConsole = "Disponível";

            if (UsuarioAtual.Instancia.IsLocadora)
            {
                LocadoraSelecionadaNome = UsuarioAtual.Instancia.LocadoraVinculadaNome;
            }
            else if (LocadorasNomes.Count > 0)
            {
                LocadoraSelecionadaNome = LocadorasNomes[0];
            }
        }

        private async Task ExecutarSelecionarImagemConsoleAsync()
        {
            var pagina = ObterPaginaAtual();
            try
            {
                string? caminhoSalvo = await ImageService.Instancia.SelecionarESalvarImagemAsync();
                if (!string.IsNullOrEmpty(caminhoSalvo))
                {
                    ImgDiretorio = caminhoSalvo;
                }
            }
            catch (Exception ex)
            {
                if (pagina != null)
                    await pagina.DisplayAlert("Imagem", $"Erro ao selecionar imagem: {ex.Message}", "OK");
            }
        }

        // ==========================================
        // GESTÃO DE FILIAIS / LOCADORAS (ADMIN)
        // ==========================================

        private void LimparFormularioLocadora()
        {
            LocadoraIdEdicao = 0;
            LocadoraNomeEdicao = string.Empty;
            LocadoraEnderecoEdicao = string.Empty;
            LocadoraTelefoneEdicao = string.Empty;
            LocadoraCepEdicao = string.Empty;
            LocadoraRuaEdicao = string.Empty;
            LocadoraNumeroEdicao = string.Empty;
            LocadoraBairroEdicao = string.Empty;
            LocadoraCidadeEdicao = string.Empty;
            LocadoraEstadoEdicao = string.Empty;
            UsarLocalizacaoGpsAtualEdicao = false;
            LocadoraLatitudeEdicaoTexto = "-23.550520";
            LocadoraLongitudeEdicaoTexto = "-46.633308";
        }

        private async Task ExecutarIrParaNovaLocadoraAsync()
        {
            LimparFormularioLocadora();
            var nav = ObterNavegacao();
            if (nav != null)
            {
                await nav.PushAsync(new LocadoraCadastroView(this));
            }
        }

        private async Task ExecutarEditarLocadoraAsync(Locadora? loc)
        {
            if (loc == null) return;

            LocadoraIdEdicao = loc.Id;
            LocadoraNomeEdicao = loc.Nome;
            LocadoraEnderecoEdicao = loc.Endereco;
            LocadoraTelefoneEdicao = loc.Telefone;
            LocadoraCepEdicao = loc.Cep ?? string.Empty;
            LocadoraRuaEdicao = loc.Rua ?? string.Empty;
            LocadoraNumeroEdicao = loc.Numero ?? string.Empty;
            LocadoraBairroEdicao = loc.Bairro ?? string.Empty;
            LocadoraCidadeEdicao = loc.Cidade ?? string.Empty;
            LocadoraEstadoEdicao = loc.Estado ?? string.Empty;
            UsarLocalizacaoGpsAtualEdicao = false;
            LocadoraLatitudeEdicaoTexto = loc.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
            LocadoraLongitudeEdicaoTexto = loc.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);

            var nav = ObterNavegacao();
            if (nav != null)
            {
                await nav.PushAsync(new LocadoraCadastroView(this));
            }
        }

        public async Task ConsultarCepLocadoraEdicaoAsync()
        {
            if (string.IsNullOrWhiteSpace(LocadoraCepEdicao)) return;
            var pagina = ObterPaginaAtual();
            try
            {
                var endereco = await ViaCepService.Instancia.ConsultarCepAsync(LocadoraCepEdicao);
                if (endereco != null)
                {
                    if (!string.IsNullOrEmpty(endereco.Logradouro)) LocadoraRuaEdicao = endereco.Logradouro;
                    if (!string.IsNullOrEmpty(endereco.Bairro)) LocadoraBairroEdicao = endereco.Bairro;
                    if (!string.IsNullOrEmpty(endereco.Localidade)) LocadoraCidadeEdicao = endereco.Localidade;
                    if (!string.IsNullOrEmpty(endereco.Uf)) LocadoraEstadoEdicao = endereco.Uf;
                }
                else
                {
                    if (pagina != null)
                        await pagina.DisplayAlert("Aviso", "CEP não localizado pelo ViaCEP. Preencha manualmente.", "OK");
                }
            }
            catch (Exception ex)
            {
                if (pagina != null)
                    await pagina.DisplayAlert("Erro", $"Falha ao consultar CEP: {ex.Message}", "OK");
            }
        }

        public async Task ObterEnderecoGpsEdicaoAsync()
        {
            var pagina = ObterPaginaAtual();
            try
            {
                var loc = await GeolocalizacaoService.Instancia.ObterLocalizacaoAsync(forcarAtualizacao: true);
                LocadoraLatitudeEdicaoTexto = loc.Latitude.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
                LocadoraLongitudeEdicaoTexto = loc.Longitude.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);

                var enderecoGps = await GeolocalizacaoService.Instancia.ObterEnderecoPorCoordenadasAsync(loc.Latitude, loc.Longitude);
                if (enderecoGps != null)
                {
                    if (!string.IsNullOrEmpty(enderecoGps.PostalCode)) LocadoraCepEdicao = enderecoGps.PostalCode;
                    if (!string.IsNullOrEmpty(enderecoGps.Thoroughfare)) LocadoraRuaEdicao = enderecoGps.Thoroughfare;
                    if (!string.IsNullOrEmpty(enderecoGps.SubLocality)) LocadoraBairroEdicao = enderecoGps.SubLocality;
                    if (!string.IsNullOrEmpty(enderecoGps.Locality)) LocadoraCidadeEdicao = enderecoGps.Locality;
                    if (!string.IsNullOrEmpty(enderecoGps.AdminArea)) LocadoraEstadoEdicao = enderecoGps.AdminArea;
                }
            }
            catch (Exception ex)
            {
                if (pagina != null)
                    await pagina.DisplayAlert("GPS", $"Não foi possível obter endereço por GPS: {ex.Message}", "OK");
            }
        }

        private async Task ExecutarSalvarLocadoraAsync()
        {
            var pagina = ObterPaginaAtual();

            if (string.IsNullOrWhiteSpace(LocadoraNomeEdicao) ||
                string.IsNullOrWhiteSpace(LocadoraTelefoneEdicao))
            {
                if (pagina != null)
                    await pagina.DisplayAlert("Atenção", "Preencha o Nome e o Telefone da filial.", "OK");
                return;
            }

            // Se campos estruturados estão preenchidos, monta endereço formatado
            string enderecoFinal = LocadoraEnderecoEdicao;
            if (!string.IsNullOrWhiteSpace(LocadoraRuaEdicao) || !string.IsNullOrWhiteSpace(LocadoraCidadeEdicao))
            {
                var partes = new List<string>();
                if (!string.IsNullOrWhiteSpace(LocadoraRuaEdicao))
                {
                    string ruaNum = LocadoraRuaEdicao.Trim();
                    if (!string.IsNullOrWhiteSpace(LocadoraNumeroEdicao))
                        ruaNum += $", {LocadoraNumeroEdicao.Trim()}";
                    partes.Add(ruaNum);
                }
                if (!string.IsNullOrWhiteSpace(LocadoraBairroEdicao))
                    partes.Add(LocadoraBairroEdicao.Trim());
                if (!string.IsNullOrWhiteSpace(LocadoraCidadeEdicao))
                {
                    string cid = LocadoraCidadeEdicao.Trim();
                    if (!string.IsNullOrWhiteSpace(LocadoraEstadoEdicao))
                        cid += $" - {LocadoraEstadoEdicao.Trim()}";
                    partes.Add(cid);
                }
                if (!string.IsNullOrWhiteSpace(LocadoraCepEdicao))
                    partes.Add($"CEP: {LocadoraCepEdicao.Trim()}");

                enderecoFinal = string.Join(" - ", partes);
            }

            if (string.IsNullOrWhiteSpace(enderecoFinal))
            {
                if (pagina != null)
                    await pagina.DisplayAlert("Atenção", "Informe o endereço ou ative a localização GPS atual.", "OK");
                return;
            }

            double lat = GeolocalizacaoService.LatitudePadrao;
            double lon = GeolocalizacaoService.LongitudePadrao;

            if (double.TryParse(LocadoraLatitudeEdicaoTexto.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedLat))
                lat = parsedLat;

            if (double.TryParse(LocadoraLongitudeEdicaoTexto.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedLon))
                lon = parsedLon;

            var loc = new Locadora
            {
                Id = LocadoraIdEdicao,
                Nome = LocadoraNomeEdicao.Trim(),
                Endereco = enderecoFinal,
                Telefone = LocadoraTelefoneEdicao.Trim(),
                Cep = LocadoraCepEdicao?.Trim() ?? string.Empty,
                Rua = LocadoraRuaEdicao?.Trim() ?? string.Empty,
                Numero = LocadoraNumeroEdicao?.Trim() ?? string.Empty,
                Bairro = LocadoraBairroEdicao?.Trim() ?? string.Empty,
                Cidade = LocadoraCidadeEdicao?.Trim() ?? string.Empty,
                Estado = LocadoraEstadoEdicao?.Trim() ?? string.Empty,
                Latitude = lat,
                Longitude = lon
            };

            await SQLiteDatabaseHelper.Instancia.SalvarLocadoraAsync(loc);

            if (pagina != null)
                await pagina.DisplayAlert("Sucesso", LocadoraIdEdicao == 0 ? "Filial cadastrada com sucesso!" : "Filial atualizada com sucesso!", "OK");

            LimparFormularioLocadora();
            await CarregarDadosAdminAsync();

            var nav = ObterNavegacao();
            if (nav != null)
            {
                await nav.PopAsync();
            }
        }

        private async Task ExecutarExcluirLocadoraAsync(Locadora? loc)
        {
            if (loc == null) return;

            var pagina = ObterPaginaAtual();
            if (pagina == null) return;

            bool confirma = await pagina.DisplayAlert(
                "Excluir Filial",
                $"Deseja realmente remover a unidade '{loc.Nome}'?\nTodos os consoles vinculados a ela permanecerão no banco.",
                "Sim, Excluir",
                "Cancelar");

            if (!confirma) return;

            await SQLiteDatabaseHelper.Instancia.ExcluirLocadoraAsync(loc);
            await pagina.DisplayAlert("Sucesso", "Filial removida com sucesso.", "OK");
            await CarregarDadosAdminAsync();
        }

        private async Task ExecutarPreencherGpsLocadoraEdicaoAsync()
        {
            var pagina = ObterPaginaAtual();
            var loc = await GeolocalizacaoService.Instancia.ObterLocalizacaoAsync(forcarAtualizacao: true);

            LocadoraLatitudeEdicaoTexto = loc.Latitude.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
            LocadoraLongitudeEdicaoTexto = loc.Longitude.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);

            if (pagina != null)
            {
                await pagina.DisplayAlert(
                    "GPS Atualizado",
                    $"Coordenadas obtidas do dispositivo:\nLat: {LocadoraLatitudeEdicaoTexto}\nLon: {LocadoraLongitudeEdicaoTexto}",
                    "OK");
            }
        }

        // ==========================================
        // GESTÃO DE USUÁRIOS (ADMIN)
        // ==========================================

        private async Task ExecutarExcluirUsuarioAsync(Usuario? user)
        {
            if (user == null) return;

            var pagina = ObterPaginaAtual();
            if (pagina == null) return;

            if (user.Id == UsuarioAtual.Instancia.Id)
            {
                await pagina.DisplayAlert("Ação Bloqueada", "Você não pode excluir sua própria conta de administrador em uso.", "OK");
                return;
            }

            bool confirma = await pagina.DisplayAlert(
                "Excluir Usuário",
                $"Deseja realmente remover a conta de '{user.Nome}' ({user.Tipo} - {user.Email})?",
                "Sim, Excluir",
                "Cancelar");

            if (!confirma) return;

            await SQLiteDatabaseHelper.Instancia.ExcluirUsuarioAsync(user);
            await pagina.DisplayAlert("Sucesso", "Usuário removido com sucesso.", "OK");
            await CarregarDadosAdminAsync();
        }

        // ==========================================
        // MAPAS E TELEFONIA
        // ==========================================

        private async Task ExecutarAbrirRotaNoMapaAsync(Locadora? loc)
        {
            if (loc == null) return;
            await GeolocalizacaoService.Instancia.AbrirRotaNoMapaAsync(loc.Latitude, loc.Longitude, loc.Nome);
        }

        private async Task ExecutarLigarLocadoraAsync(Locadora? loc)
        {
            if (loc == null || string.IsNullOrWhiteSpace(loc.Telefone)) return;

            var pagina = ObterPaginaAtual();
            if (pagina != null)
            {
                bool ligar = await pagina.DisplayAlert(
                    "Ligar para Unidade",
                    $"Deseja discar para {loc.Nome}?\nTelefone: {loc.Telefone}",
                    "Ligar Agora",
                    "Cancelar");

                if (!ligar) return;
            }

            GeolocalizacaoService.Instancia.RealizarLigacao(loc.Telefone);
        }

        // ==========================================
        // HISTÓRICO DE LOCAÇÕES
        // ==========================================

        public async Task CarregarHistoricoAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                List<Historico> lista;

                if (UsuarioAtual.Instancia.IsAdministrador)
                {
                    lista = await SQLiteDatabaseHelper.Instancia.ObterHistoricosAsync();
                }
                else if (UsuarioAtual.Instancia.IsLocadora)
                {
                    lista = await SQLiteDatabaseHelper.Instancia.ObterHistoricosPorLocadoraAsync(UsuarioAtual.Instancia.LocadoraVinculadaNome);
                }
                else
                {
                    lista = await SQLiteDatabaseHelper.Instancia.ObterHistoricoPorUsuarioAsync(UsuarioAtual.Instancia.Id);
                }

                Historicos.Clear();
                foreach (var h in lista)
                {
                    Historicos.Add(h);
                }
            }
            catch (Exception ex)
            {
                var pagina = ObterPaginaAtual();
                if (pagina != null)
                    await pagina.DisplayAlert("Erro", $"Falha ao carregar histórico: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ==========================================
        // NAVEGAÇÃO
        // ==========================================

        private async Task ExecutarIrParaHistoricoAsync()
        {
            var nav = ObterNavegacao();
            if (nav != null)
            {
                await nav.PushAsync(new HistoricoView());
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

        private async Task ExecutarDeslogarAsync()
        {
            var pagina = ObterPaginaAtual();
            var nav = ObterNavegacao();

            if (pagina != null)
            {
                bool confirma = await pagina.DisplayAlert("Encerrar Sessão", "Deseja realmente sair da sua conta?", "Sim, Sair", "Cancelar");
                if (!confirma) return;
            }

            UsuarioAtual.Instancia.Deslogar();

            if (nav != null)
            {
                await nav.PopToRootAsync();
            }
        }
    }
}
