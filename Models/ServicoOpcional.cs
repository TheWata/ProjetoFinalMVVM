using SQLite;
using System;
using ProjetoFinalMVVM.ViewModels;

namespace ProjetoFinalMVVM.Models
{
    /// <summary>
    /// Modelo que representa os serviços e acessórios opcionais oferecidos pela locadora no checkout do aluguel.
    /// Cada locadora gerencia seu próprio catálogo de serviços adicionais (ex: controles extras, headsets, seguros).
    /// Mapeado para a tabela 'ServicosOpcionais' no SQLite.
    /// </summary>
    [Table("ServicosOpcionais")]
    public class ServicoOpcional : BaseViewModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int LocadoraId { get; set; }

        public string LocadoraNome { get; set; } = string.Empty;

        public string Nome { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        public string IconeGlyph { get; set; } = "\uf7a9"; // Ícone padrão FontAwesome

        public decimal Valor { get; set; }

        public bool Ativo { get; set; } = true;

        // ==========================================
        // PROPRIEDADES DE EXIBIÇÃO E BINDING TEMPORÁRIO
        // ==========================================

        private bool _selecionado;

        /// <summary>
        /// Controla a seleção pelo cliente na tela de contratação de aluguel.
        /// </summary>
        [Ignore]
        public bool Selecionado
        {
            get => _selecionado;
            set
            {
                if (SetProperty(ref _selecionado, value))
                {
                    OnSelecaoAlterada?.Invoke();
                }
            }
        }

        /// <summary>
        /// Callback executado quando o status de seleção muda, recalculando os totais na ViewModel.
        /// </summary>
        [Ignore]
        public Action? OnSelecaoAlterada { get; set; }

        [Ignore]
        public string ValorFormatado => $"R$ {Valor:F2}";

        [Ignore]
        public string StatusTexto => Ativo ? "Ativo" : "Pausado";

        [Ignore]
        public string StatusCor => Ativo ? "#16A34A" : "#94A3B8";

        [Ignore]
        public string StatusBadgeBg => Ativo ? "#DCFCE7" : "#F1F5F9";
    }
}
