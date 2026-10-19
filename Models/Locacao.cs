using SQLite;
using System;

namespace ProjetoFinalMVVM.Models
{
    /// <summary>
    /// Enumeração estrita para controle do ciclo de vida da locação.
    /// </summary>
    public enum StatusLocacaoEnum
    {
        AguardandoRetirada = 0,
        Alugado = 1,
        Finalizado = 2,
        Cancelado = 3
    }

    /// <summary>
    /// Modelo que representa a transação de locação de videogame com validação de entrega/retirada via código PIN.
    /// Mapeado para a tabela 'Locacoes' no SQLite.
    /// </summary>
    [Table("Locacoes")]
    public class Locacao
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public string UsuarioNome { get; set; } = string.Empty;

        public int ItemId { get; set; }

        public string ItemNome { get; set; } = string.Empty;

        public string ChaveSerialConsole { get; set; } = string.Empty;

        public int LocadoraId { get; set; }

        public string LocadoraNome { get; set; } = string.Empty;

        public DateTime DataLocacao { get; set; } = DateTime.Now;

        public DateTime DataDevolucaoPrevista { get; set; }

        /// <summary>
        /// Código numérico aleatório de 6 dígitos (PIN de segurança) gerado na reserva para validação no balcão.
        /// </summary>
        public string CodigoConfirmacao { get; set; } = string.Empty;

        /// <summary>
        /// Status da locação utilizando o Enum estrito: AguardandoRetirada, Alugado, Finalizado ou Cancelado.
        /// </summary>
        public StatusLocacaoEnum StatusLocacao { get; set; } = StatusLocacaoEnum.AguardandoRetirada;

        public decimal ValorTotal { get; set; }

        public decimal TaxaAdicional { get; set; }

        public string ModalidadeAluguel { get; set; } = "Diária";

        public int QuantidadePeriodo { get; set; } = 1;

        public int DiasLocados { get; set; }

        public string FormaEntrega { get; set; } = "Retirada no Balcão";

        public string DescricaoExtras { get; set; } = string.Empty;

        // ==========================================
        // PROPRIEDADES DE EXIBIÇÃO E DATABINDING (IGNORADAS NO BANCO)
        // ==========================================

        [Ignore]
        public string DataLocacaoFormatada => DataLocacao.ToString("dd/MM/yyyy HH:mm");

        [Ignore]
        public string DataDevolucaoFormatada => DataDevolucaoPrevista.ToString("dd/MM/yyyy");

        [Ignore]
        public string ValorTotalFormatado => $"R$ {ValorTotal:F2}";

        [Ignore]
        public string StatusTexto => StatusLocacao switch
        {
            StatusLocacaoEnum.AguardandoRetirada => "Aguardando Retirada",
            StatusLocacaoEnum.Alugado => "Alugado (Em Andamento)",
            StatusLocacaoEnum.Finalizado => "Finalizado (Devolvido)",
            StatusLocacaoEnum.Cancelado => "Cancelado",
            _ => StatusLocacao.ToString()
        };

        [Ignore]
        public string StatusCor => StatusLocacao switch
        {
            StatusLocacaoEnum.AguardandoRetirada => "#D97706", // Laranja / Âmbar
            StatusLocacaoEnum.Alugado => "#0284C7",            // Azul
            StatusLocacaoEnum.Finalizado => "#16A34A",         // Verde
            _ => "#DC2626"
        };

        [Ignore]
        public string StatusBadgeBg => StatusLocacao switch
        {
            StatusLocacaoEnum.AguardandoRetirada => "#FEF3C7",
            StatusLocacaoEnum.Alugado => "#E0F2FE",
            StatusLocacaoEnum.Finalizado => "#DCFCE7",
            _ => "#FEE2E2"
        };

        [Ignore]
        public string StatusBadgeText => StatusLocacao switch
        {
            StatusLocacaoEnum.AguardandoRetirada => "#B45309",
            StatusLocacaoEnum.Alugado => "#0369A1",
            StatusLocacaoEnum.Finalizado => "#15803D",
            _ => "#B91C1C"
        };

        [Ignore]
        public bool IsAguardandoRetirada => StatusLocacao == StatusLocacaoEnum.AguardandoRetirada;

        [Ignore]
        public bool IsAlugado => StatusLocacao == StatusLocacaoEnum.Alugado;

        /// <summary>
        /// Propriedade temporária em memória para o campo Entry onde a locadora digita o código informado pelo cliente.
        /// </summary>
        [Ignore]
        public string CodigoDigitado { get; set; } = string.Empty;
    }
}
