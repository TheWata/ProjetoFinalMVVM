using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using ProjetoFinalMVVM.Helpers;
using ProjetoFinalMVVM.Models;
using ProjetoFinalMVVM.Services;

namespace ProjetoFinalMVVM.ViewModels
{
    /// <summary>
    /// ViewModel da tela profissional de configuração e contratação do aluguel de videogame.
    /// Permite ao cliente escolher modalidades (diária, semanal, mensal) definidas pela loja,
    /// definir quantidade de períodos, selecionar acessórios e serviços opcionais, e escolher frete.
    /// Implementa MVVM 100% puro e manual através de BaseViewModel.
    /// </summary>
    public class AluguelDetalhesViewModel : BaseViewModel
    {
        private readonly Item _console;
        private readonly ItemViewModel? _itemViewModel;

        public Item Console => _console;

        // ==========================================
        // SELEÇÃO DE MODALIDADE DEFINIDA PELA LOJA
        // ==========================================
        private string _modalidadeSelecionada = "Diaria"; // "Diaria", "Semanal", "Mensal"
        public string ModalidadeSelecionada
        {
            get => _modalidadeSelecionada;
            set
            {
                if (SetProperty(ref _modalidadeSelecionada, value))
                {
                    AtualizarRecalculoCompleto();
                    OnPropertyChanged(nameof(IsDiariaSelecionada));
                    OnPropertyChanged(nameof(IsSemanalSelecionada));
                    OnPropertyChanged(nameof(IsMensalSelecionada));
                    OnPropertyChanged(nameof(CorBordaDiaria));
                    OnPropertyChanged(nameof(CorFundoDiaria));
                    OnPropertyChanged(nameof(CorBordaSemanal));
                    OnPropertyChanged(nameof(CorFundoSemanal));
                    OnPropertyChanged(nameof(CorBordaMensal));
                    OnPropertyChanged(nameof(CorFundoMensal));
                    OnPropertyChanged(nameof(TextoUnidadePeriodo));
                    OnPropertyChanged(nameof(PrecoUnitarioPlanoFormatado));
                }
            }
        }

        public bool IsDiariaSelecionada => ModalidadeSelecionada == "Diaria";
        public bool IsSemanalSelecionada => ModalidadeSelecionada == "Semanal";
        public bool IsMensalSelecionada => ModalidadeSelecionada == "Mensal";

        public Color CorBordaDiaria => IsDiariaSelecionada ? Color.FromArgb("#16A34A") : Color.FromArgb("#E2E8F0");
        public Color CorFundoDiaria => IsDiariaSelecionada ? Color.FromArgb("#F0FDF4") : Colors.White;

        public Color CorBordaSemanal => IsSemanalSelecionada ? Color.FromArgb("#16A34A") : Color.FromArgb("#E2E8F0");
        public Color CorFundoSemanal => IsSemanalSelecionada ? Color.FromArgb("#F0FDF4") : Colors.White;

        public Color CorBordaMensal => IsMensalSelecionada ? Color.FromArgb("#16A34A") : Color.FromArgb("#E2E8F0");
        public Color CorFundoMensal => IsMensalSelecionada ? Color.FromArgb("#F0FDF4") : Colors.White;

        public bool PodeDiaria => _console.PermiteDiaria;
        public bool PodeSemanal => _console.PermiteSemanal;
        public bool PodeMensal => _console.PermiteMensal;

        public double OpacidadeDiaria => PodeDiaria ? 1.0 : 0.45;
        public double OpacidadeSemanal => PodeSemanal ? 1.0 : 0.45;
        public double OpacidadeMensal => PodeMensal ? 1.0 : 0.45;

        public string StatusDiariaTexto => PodeDiaria ? "por dia" : "Indisponível";
        public string StatusSemanalTexto => PodeSemanal ? "por 7 dias" : "Indisponível";
        public string StatusMensalTexto => PodeMensal ? "por 30 dias" : "Indisponível";

        // ==========================================
        // DURAÇÃO DO PERÍODO
        // ==========================================
        private int _quantidadePeriodo = 1;
        public int QuantidadePeriodo
        {
            get => _quantidadePeriodo;
            set
            {
                if (value < 1) value = 1;
                if (value > 90) value = 90;
                if (SetProperty(ref _quantidadePeriodo, value))
                {
                    AtualizarRecalculoCompleto();
                }
            }
        }

        public string TextoUnidadePeriodo => ModalidadeSelecionada switch
        {
            "Semanal" => QuantidadePeriodo == 1 ? "semana" : "semanas",
            "Mensal" => QuantidadePeriodo == 1 ? "mês" : "meses",
            _ => QuantidadePeriodo == 1 ? "dia" : "dias"
        };

        // ==========================================
        // PACOTES DE SERVIÇOS & ACESSÓRIOS ADICIONAIS
        // ==========================================
        private bool _incluirControleExtra;
        public bool IncluirControleExtra
        {
            get => _incluirControleExtra;
            set
            {
                if (SetProperty(ref _incluirControleExtra, value))
                    AtualizarRecalculoCompleto();
            }
        }

        private bool _incluirHeadsetGamer;
        public bool IncluirHeadsetGamer
        {
            get => _incluirHeadsetGamer;
            set
            {
                if (SetProperty(ref _incluirHeadsetGamer, value))
                    AtualizarRecalculoCompleto();
            }
        }

        private bool _incluirJogosExtras;
        public bool IncluirJogosExtras
        {
            get => _incluirJogosExtras;
            set
            {
                if (SetProperty(ref _incluirJogosExtras, value))
                    AtualizarRecalculoCompleto();
            }
        }

        private bool _incluirSeguroProtecao;
        public bool IncluirSeguroProtecao
        {
            get => _incluirSeguroProtecao;
            set
            {
                if (SetProperty(ref _incluirSeguroProtecao, value))
                    AtualizarRecalculoCompleto();
            }
        }

        // ==========================================
        // FORMA DE ENTREGA / LOGÍSTICA
        // ==========================================
        private string _formaEntrega = "Balcao"; // "Balcao" ou "Domicilio"
        public string FormaEntrega
        {
            get => _formaEntrega;
            set
            {
                if (SetProperty(ref _formaEntrega, value))
                {
                    AtualizarRecalculoCompleto();
                    OnPropertyChanged(nameof(IsRetiradaBalcao));
                    OnPropertyChanged(nameof(IsEntregaDomicilio));
                    OnPropertyChanged(nameof(CorBordaBalcao));
                    OnPropertyChanged(nameof(CorFundoBalcao));
                    OnPropertyChanged(nameof(CorBordaDomicilio));
                    OnPropertyChanged(nameof(CorFundoDomicilio));
                }
            }
        }

        public bool IsRetiradaBalcao => FormaEntrega == "Balcao";
        public bool IsEntregaDomicilio => FormaEntrega == "Domicilio";

        public Color CorBordaBalcao => IsRetiradaBalcao ? Color.FromArgb("#16A34A") : Color.FromArgb("#E2E8F0");
        public Color CorFundoBalcao => IsRetiradaBalcao ? Color.FromArgb("#F0FDF4") : Colors.White;

        public Color CorBordaDomicilio => IsEntregaDomicilio ? Color.FromArgb("#16A34A") : Color.FromArgb("#E2E8F0");
        public Color CorFundoDomicilio => IsEntregaDomicilio ? Color.FromArgb("#F0FDF4") : Colors.White;

        public decimal ValorTaxaEntrega => IsEntregaDomicilio ? 18.00m : 0.00m;

        // ==========================================
        // CÁLCULO E TOTAIS EM TEMPO REAL
        // ==========================================
        public decimal PrecoUnitarioPlano => ModalidadeSelecionada switch
        {
            "Semanal" => _console.ValorSemanal > 0 ? _console.ValorSemanal : (_console.ValorDiaria * 7),
            "Mensal" => _console.ValorMensal > 0 ? _console.ValorMensal : (_console.ValorDiaria * 30),
            _ => _console.ValorDiaria
        };

        public string PrecoUnitarioPlanoFormatado => $"R$ {PrecoUnitarioPlano:F2}";

        public decimal SubtotalPlano => PrecoUnitarioPlano * QuantidadePeriodo;
        public string SubtotalPlanoFormatado => $"R$ {SubtotalPlano:F2}";

        public decimal SubtotalExtras
        {
            get
            {
                decimal total = 0;
                if (IncluirControleExtra) total += 15.00m;
                if (IncluirHeadsetGamer) total += 12.00m;
                if (IncluirJogosExtras) total += 20.00m;
                if (IncluirSeguroProtecao) total += 25.00m;
                return total;
            }
        }
        public string SubtotalExtrasFormatado => $"R$ {SubtotalExtras:F2}";

        public string TaxaEntregaFormatada => IsEntregaDomicilio ? "R$ 18,00" : "Grátis (Balcão)";

        public decimal TotalGeral => SubtotalPlano + SubtotalExtras + ValorTaxaEntrega;
        public string TotalGeralFormatado => $"R$ {TotalGeral:F2}";

        public DateTime DataDevolucaoCalculada => ModalidadeSelecionada switch
        {
            "Semanal" => DateTime.Today.AddDays(QuantidadePeriodo * 7),
            "Mensal" => DateTime.Today.AddMonths(QuantidadePeriodo),
            _ => DateTime.Today.AddDays(QuantidadePeriodo)
        };

        public string DataDevolucaoFormatada => $"{DataDevolucaoCalculada:dd/MM/yyyy} ({DataDevolucaoCalculada:dddd})";

        public string ResumoExtrasTexto
        {
            get
            {
                var itens = new List<string>();
                if (IncluirControleExtra) itens.Add("Controle Extra (+R$ 15,00)");
                if (IncluirHeadsetGamer) itens.Add("Headset Gamer Pro (+R$ 12,00)");
                if (IncluirJogosExtras) itens.Add("2 Jogos Físicos Extras (+R$ 20,00)");
                if (IncluirSeguroProtecao) itens.Add("Seguro Proteção Acidental (+R$ 25,00)");
                return itens.Count > 0 ? string.Join("; ", itens) : "Nenhum adicional selecionado";
            }
        }

        public string ResumoPlanoTexto => $"{ModalidadeSelecionada} - {QuantidadePeriodo} {TextoUnidadePeriodo} (R$ {SubtotalPlano:F2})";

        public string ResumoEntregaTexto => IsEntregaDomicilio
            ? "Entrega VIP em Domicílio (Taxa: R$ 18,00)"
            : $"Retirada no Balcão da Loja: {_console.LocadoraNome}";

        // ==========================================
        // COMANDOS (.NET MAUI Command)
        // ==========================================
        public ICommand SelecionarPlanoCommand { get; }
        public ICommand SelecionarEntregaCommand { get; }
        public ICommand AumentarPeriodoCommand { get; }
        public ICommand DiminuirPeriodoCommand { get; }
        public ICommand ConfirmarLocacaoCommand { get; }
        public ICommand VoltarCommand { get; }

        public AluguelDetalhesViewModel(Item console, ItemViewModel? itemViewModel = null)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
            _itemViewModel = itemViewModel;

            // Define modalidade inicial de acordo com o que a loja habilitou
            if (_console.PermiteDiaria)
                _modalidadeSelecionada = "Diaria";
            else if (_console.PermiteSemanal)
                _modalidadeSelecionada = "Semanal";
            else if (_console.PermiteMensal)
                _modalidadeSelecionada = "Mensal";

            SelecionarPlanoCommand = new Command<string>(plano =>
            {
                if (plano == "Diaria" && !PodeDiaria) return;
                if (plano == "Semanal" && !PodeSemanal) return;
                if (plano == "Mensal" && !PodeMensal) return;

                if (!string.IsNullOrEmpty(plano))
                    ModalidadeSelecionada = plano;
            });

            SelecionarEntregaCommand = new Command<string>(entrega =>
            {
                if (!string.IsNullOrEmpty(entrega))
                    FormaEntrega = entrega;
            });

            AumentarPeriodoCommand = new Command(() =>
            {
                QuantidadePeriodo++;
            });

            DiminuirPeriodoCommand = new Command(() =>
            {
                if (QuantidadePeriodo > 1)
                    QuantidadePeriodo--;
            });

            ConfirmarLocacaoCommand = new Command(async () => await ExecutarConfirmarLocacaoAsync());
            VoltarCommand = new Command(async () => await ExecutarVoltarAsync());
        }

        private void AtualizarRecalculoCompleto()
        {
            OnPropertyChanged(nameof(PrecoUnitarioPlano));
            OnPropertyChanged(nameof(PrecoUnitarioPlanoFormatado));
            OnPropertyChanged(nameof(SubtotalPlano));
            OnPropertyChanged(nameof(SubtotalPlanoFormatado));
            OnPropertyChanged(nameof(SubtotalExtras));
            OnPropertyChanged(nameof(SubtotalExtrasFormatado));
            OnPropertyChanged(nameof(TaxaEntregaFormatada));
            OnPropertyChanged(nameof(TotalGeral));
            OnPropertyChanged(nameof(TotalGeralFormatado));
            OnPropertyChanged(nameof(DataDevolucaoCalculada));
            OnPropertyChanged(nameof(DataDevolucaoFormatada));
            OnPropertyChanged(nameof(ResumoExtrasTexto));
            OnPropertyChanged(nameof(ResumoPlanoTexto));
            OnPropertyChanged(nameof(ResumoEntregaTexto));
            OnPropertyChanged(nameof(TextoUnidadePeriodo));
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

        private async Task ExecutarConfirmarLocacaoAsync()
        {
            var pagina = ObterPaginaAtual();
            if (pagina == null) return;

            string mensagemResumo =
                $"Console: {_console.Nome}\n" +
                $"Serial: {_console.ChaveSerial}\n" +
                $"Filial: {_console.LocadoraNome} ({_console.LocadoraDistanciaFormatada})\n" +
                $"Plano: {ResumoPlanoTexto}\n" +
                $"Devolução: {DataDevolucaoFormatada}\n" +
                $"Adicionais: {ResumoExtrasTexto}\n" +
                $"Logística: {ResumoEntregaTexto}\n" +
                $"-----------------------------------\n" +
                $"Total da Locação: {TotalGeralFormatado}";

            bool confirmou = await pagina.DisplayAlert(
                "Confirmar Pedido de Locação",
                mensagemResumo,
                "Confirmar e Finalizar",
                "Revisar");

            if (!confirmou) return;

            int diasEquivalentes = ModalidadeSelecionada switch
            {
                "Semanal" => QuantidadePeriodo * 7,
                "Mensal" => QuantidadePeriodo * 30,
                _ => QuantidadePeriodo
            };

            var historico = new Historico
            {
                UsuarioId = UsuarioAtual.Instancia.Id,
                UsuarioNome = UsuarioAtual.Instancia.Nome,
                ItemId = _console.Id,
                ItemNome = _console.Nome,
                ChaveSerialConsole = _console.ChaveSerial,
                LocadoraNome = _console.LocadoraNome,
                DataHora = DateTime.Now,
                DataDevolucaoPrevista = DataDevolucaoCalculada,
                DiasLocados = diasEquivalentes,
                ModalidadeAluguel = ModalidadeSelecionada,
                QuantidadePeriodo = QuantidadePeriodo,
                DescricaoExtras = ResumoExtrasTexto,
                FormaEntrega = ResumoEntregaTexto,
                TaxaAdicional = SubtotalExtras + ValorTaxaEntrega,
                ValorTotal = TotalGeral,
                StatusLocacao = "Em Andamento"
            };

            await SQLiteDatabaseHelper.Instancia.SalvarHistoricoAsync(historico);

            // Marca o console como alugado no banco
            _console.Status = "Alugado";
            await SQLiteDatabaseHelper.Instancia.SalvarItemAsync(_console);

            await pagina.DisplayAlert(
                "Reserva Confirmada com Sucesso!",
                $"Parabéns, {UsuarioAtual.Instancia.Nome}!\nSeu console {_console.Nome} foi reservado.\n\n" +
                $"Instruções de Entrega/Retirada:\n{ResumoEntregaTexto}\n" +
                $"Data Limite de Devolução: {DataDevolucaoFormatada}",
                "Concluir");

            // Recarrega os dados do cliente se o ItemViewModel estiver vinculado
            if (_itemViewModel != null)
            {
                await _itemViewModel.CarregarDadosClienteAsync();
            }

            // Retorna à tela anterior
            if (pagina.Navigation != null)
            {
                await pagina.Navigation.PopAsync();
            }
        }

        private async Task ExecutarVoltarAsync()
        {
            var pagina = ObterPaginaAtual();
            if (pagina?.Navigation != null)
            {
                await pagina.Navigation.PopAsync();
            }
        }
    }
}
