using SQLite;
using System;

namespace ProjetoFinalMVVM.Models
{
    /// <summary>
    /// Registra todas as operações de locação e devolução no sistema.
    /// Mapeado para a tabela 'Historicos' no SQLite.
    /// Contém dados completos da modalidade (Diária, Semanal, Mensal), adicionais contratados e entrega.
    /// </summary>
    [Table("Historicos")]
    public class Historico
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public string UsuarioNome { get; set; } = string.Empty;

        public int ItemId { get; set; }

        public string ItemNome { get; set; } = string.Empty;

        public string ChaveSerialConsole { get; set; } = string.Empty;

        public string LocadoraNome { get; set; } = string.Empty;

        /// <summary>
        /// Modalidade contratada: "Diário", "Semanal" ou "Mensal".
        /// </summary>
        public string ModalidadeAluguel { get; set; } = "Diário";

        /// <summary>
        /// Quantidade de unidades do período contratado (ex: 3 dias, 2 semanas, 1 mês).
        /// </summary>
        public int QuantidadePeriodo { get; set; } = 1;

        /// <summary>
        /// Descrição dos serviços e acessórios adicionais contratados.
        /// </summary>
        public string DescricaoExtras { get; set; } = string.Empty;

        /// <summary>
        /// Forma de recebimento: "Retirada no Balcão" ou "Entrega Delivery".
        /// </summary>
        public string FormaEntrega { get; set; } = "Retirada no Balcão";

        /// <summary>
        /// Data e hora exatas em que a locação foi formalizada.
        /// </summary>
        public DateTime DataHora { get; set; }

        /// <summary>
        /// Data prevista em que o console deve retornar à locadora.
        /// </summary>
        public DateTime DataDevolucaoPrevista { get; set; }

        /// <summary>
        /// Quantidade total de dias calculados para a locação.
        /// </summary>
        public int DiasLocados { get; set; }

        /// <summary>
        /// Taxa adicional aplicada para controles, seguro, frete ou multas.
        /// </summary>
        public decimal TaxaAdicional { get; set; }

        /// <summary>
        /// Valor monetário total cobrado pela locação.
        /// </summary>
        public decimal ValorTotal { get; set; }

        /// <summary>
        /// Situação da locação: "Em Andamento" ou "Devolvido".
        /// </summary>
        public string StatusLocacao { get; set; } = "Em Andamento";

        // Propriedades auxiliares de exibição na interface XAML (ignoradas pelo SQLite)
        [Ignore]
        public string DataHoraFormatada => DataHora.ToString("dd/MM/yyyy HH:mm");

        [Ignore]
        public string DataDevolucaoFormatada => DataDevolucaoPrevista.ToString("dd/MM/yyyy");

        [Ignore]
        public string ValorTotalFormatado => $"R$ {ValorTotal:F2}";

        [Ignore]
        public string TaxaAdicionalFormatada => $"R$ {TaxaAdicional:F2}";

        [Ignore]
        public string ResumoPlanoFormatado => $"{ModalidadeAluguel} ({QuantidadePeriodo}x) - {DiasLocados} dia(s)";

        [Ignore]
        public bool IsEmAndamento => StatusLocacao == "Em Andamento";

        [Ignore]
        public string StatusCor => StatusLocacao == "Em Andamento" ? "#D97706" : "#16A34A";

        [Ignore]
        public string StatusBadgeBg => StatusLocacao == "Em Andamento" ? "#FEF3C7" : "#DCFCE7";

        [Ignore]
        public string StatusBadgeText => StatusLocacao == "Em Andamento" ? "#B45309" : "#15803D";
    }
}
