using SQLite;

namespace ProjetoFinalMVVM.Models
{
    /// <summary>
    /// Representa a entidade Console disponível para locação no sistema.
    /// Mapeado para a tabela 'Itens' no SQLite.
    /// Suporta múltiplas modalidades de locação definidas pela loja: Diária, Semanal e Mensal.
    /// </summary>
    [Table("Itens")]
    public class Item
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Nome do console (ex: "Xbox 360", "Xbox One", "Nintendo Wii", "PlayStation 5")
        /// </summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Código serial exclusivo do aparelho físico.
        /// </summary>
        public string ChaveSerial { get; set; } = string.Empty;

        /// <summary>
        /// Caminho ou URL da imagem ilustrativa do console.
        /// </summary>
        public string ImgDiretorio { get; set; } = string.Empty;

        /// <summary>
        /// Descrição detalhada dos acessórios, estado e jogos inclusos.
        /// </summary>
        public string Desc { get; set; } = string.Empty;

        /// <summary>
        /// Conta de serviço vinculada (ex: Game Pass, PSN Plus, Nintendo Online).
        /// </summary>
        public string ContaVinculada { get; set; } = string.Empty;

        /// <summary>
        /// Define se a loja disponibiliza locação na modalidade diária.
        /// </summary>
        public bool PermiteDiaria { get; set; } = true;

        /// <summary>
        /// Preço cobrado por dia de locação.
        /// </summary>
        public decimal ValorDiaria { get; set; } = 35.00m;

        /// <summary>
        /// Define se a loja disponibiliza locação na modalidade semanal.
        /// </summary>
        public bool PermiteSemanal { get; set; } = true;

        /// <summary>
        /// Preço de pacote cobrado por semana de locação (7 dias).
        /// </summary>
        public decimal ValorSemanal { get; set; } = 180.00m;

        /// <summary>
        /// Define se a loja disponibiliza locação na modalidade mensal.
        /// </summary>
        public bool PermiteMensal { get; set; } = true;

        /// <summary>
        /// Preço de pacote cobrado por mês de locação (30 dias).
        /// </summary>
        public decimal ValorMensal { get; set; } = 550.00m;

        /// <summary>
        /// Situação atual: "Disponível" ou "Alugado".
        /// </summary>
        public string Status { get; set; } = "Disponível";

        /// <summary>
        /// Nome da unidade/locadora onde o console está alocado.
        /// </summary>
        public string LocadoraNome { get; set; } = string.Empty;

        // Propriedades auxiliares de exibição na interface XAML (ignoradas pelo SQLite)
        [Ignore]
        public double LocadoraDistanciaKm { get; set; }

        [Ignore]
        public string LocadoraDistanciaFormatada => $"{LocadoraDistanciaKm:F1} km de você";

        [Ignore]
        public string ValorDiariaFormatado => $"R$ {ValorDiaria:F2} / dia";

        [Ignore]
        public string ValorSemanalFormatado => $"R$ {ValorSemanal:F2} / semana";

        [Ignore]
        public string ValorMensalFormatado => $"R$ {ValorMensal:F2} / mês";

        [Ignore]
        public string PlanosDisponiveisTexto
        {
            get
            {
                var planos = new System.Collections.Generic.List<string>();
                if (PermiteDiaria && ValorDiaria > 0) planos.Add($"Diária: R$ {ValorDiaria:F2}");
                if (PermiteSemanal && ValorSemanal > 0) planos.Add($"Semanal: R$ {ValorSemanal:F2}");
                if (PermiteMensal && ValorMensal > 0) planos.Add($"Mensal: R$ {ValorMensal:F2}");
                return planos.Count > 0 ? string.Join(" • ", planos) : $"Diária: R$ {ValorDiaria:F2}";
            }
        }

        [Ignore]
        public bool IsDisponivel => Status == "Disponível";

        [Ignore]
        public string StatusCor => Status == "Disponível" ? "#16A34A" : "#DC2626";

        [Ignore]
        public string StatusBadgeBg => Status == "Disponível" ? "#DCFCE7" : "#FEE2E2";

        [Ignore]
        public string StatusBadgeText => Status == "Disponível" ? "#15803D" : "#B91C1C";
    }
}
