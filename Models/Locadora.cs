using SQLite;

namespace ProjetoFinalMVVM.Models
{
    /// <summary>
    /// Representa as filiais/unidades físicas da locadora de videogame.
    /// Mapeado para a tabela 'Locadoras' no SQLite.
    /// Contém endereço estruturado (CEP, Rua, Número, Bairro, Cidade, Estado) e coordenadas GPS.
    /// </summary>
    [Table("Locadoras")]
    public class Locadora
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Endereço textual formatado completo.
        /// </summary>
        public string Endereco { get; set; } = string.Empty;

        public string Telefone { get; set; } = string.Empty;

        // Campos detalhados de endereço
        public string Cep { get; set; } = string.Empty;
        public string Rua { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;

        /// <summary>
        /// Coordenadas geográficas reais para localização e mapas.
        /// </summary>
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        /// <summary>
        /// Distância calculada dinamicamente via GPS em relação ao cliente.
        /// </summary>
        public double DistanciaKm { get; set; }

        [Ignore]
        public string DistanciaFormatada => DistanciaKm < 1.0 && DistanciaKm > 0
            ? $"{(int)(DistanciaKm * 1000)} m de você"
            : $"{DistanciaKm:F1} km de você";

        [Ignore]
        public int QuantidadeConsolesDisponiveis { get; set; }

        [Ignore]
        public string ConsolesDisponiveisFormatado => $"{QuantidadeConsolesDisponiveis} consoles livres";

        [Ignore]
        public string CoordenadasTexto => $"{Latitude:F4}, {Longitude:F4}";

        /// <summary>
        /// Monta o endereço formatado a partir dos campos estruturados.
        /// </summary>
        public string MontarEnderecoFormatado()
        {
            if (string.IsNullOrWhiteSpace(Rua) && string.IsNullOrWhiteSpace(Cidade))
                return Endereco;

            string num = string.IsNullOrWhiteSpace(Numero) ? "S/N" : Numero.Trim();
            string cidUf = !string.IsNullOrWhiteSpace(Cidade) && !string.IsNullOrWhiteSpace(Estado)
                ? $"{Cidade.Trim()} - {Estado.Trim()}"
                : Cidade.Trim();

            string cepStr = !string.IsNullOrWhiteSpace(Cep) ? $", CEP: {Cep.Trim()}" : "";
            string bairroStr = !string.IsNullOrWhiteSpace(Bairro) ? $" - {Bairro.Trim()}" : "";

            return $"{Rua.Trim()}, {num}{bairroStr}, {cidUf}{cepStr}";
        }
    }
}
