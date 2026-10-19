using System;

namespace ProjetoFinalMVVM.Models
{
    /// <summary>
    /// Modelo auxiliar utilizado no calendário interativo do Administrador
    /// para exibir os próximos 14 dias e suas respectivas devoluções previstas.
    /// </summary>
    public class DiaCalendario
    {
        public DateTime Data { get; set; }

        /// <summary>
        /// Abreviação do dia da semana (ex: "SEG", "TER", "DOM").
        /// </summary>
        public string DiaSemana { get; set; } = string.Empty;

        /// <summary>
        /// Número do dia do mês (ex: "19", "20").
        /// </summary>
        public string DiaNumero { get; set; } = string.Empty;

        /// <summary>
        /// Contagem de devoluções programadas para esta data específica.
        /// </summary>
        public int QuantidadeDevolucoes { get; set; }

        /// <summary>
        /// Cor hexadecimal do indicador ou borda.
        /// </summary>
        public string CorDestaque { get; set; } = "#E2E8F0";

        /// <summary>
        /// Indica se o dia está atualmente selecionado pelo administrador.
        /// </summary>
        public bool IsSelecionado { get; set; }

        // Cores semânticas adaptadas para o Modo Claro amigável
        public string CorFundoCard => IsSelecionado ? "#DCFCE7" : (QuantidadeDevolucoes > 0 ? "#F0FDF4" : "#FFFFFF");

        public string CorBordaCard => IsSelecionado ? "#16A34A" : (QuantidadeDevolucoes > 0 ? "#86EFAC" : "#E2E8F0");

        public string CorTextoBadge => QuantidadeDevolucoes > 0 ? "#15803D" : "#94A3B8";

        public string CorBadgeFundo => QuantidadeDevolucoes > 0 ? "#DCFCE7" : "#F1F5F9";

        public bool TemDevolucoes => QuantidadeDevolucoes > 0;
    }
}
