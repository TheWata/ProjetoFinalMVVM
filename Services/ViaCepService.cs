using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProjetoFinalMVVM.Services
{
    /// <summary>
    /// Modelo de dados retornado pela API pública do ViaCEP.
    /// </summary>
    public class ViaCepResultado
    {
        [JsonPropertyName("cep")]
        public string? Cep { get; set; }

        [JsonPropertyName("logradouro")]
        public string? Logradouro { get; set; }

        [JsonPropertyName("complemento")]
        public string? Complemento { get; set; }

        [JsonPropertyName("bairro")]
        public string? Bairro { get; set; }

        [JsonPropertyName("localidade")]
        public string? Localidade { get; set; }

        [JsonPropertyName("uf")]
        public string? Uf { get; set; }

        [JsonPropertyName("erro")]
        public object? Erro { get; set; }

        public bool TemErro => Erro != null && Erro.ToString()?.ToLower() == "true";
    }

    /// <summary>
    /// Serviço de consulta e preenchimento automático de endereço via CEP (ViaCEP).
    /// </summary>
    public class ViaCepService
    {
        private static ViaCepService? _instancia;
        public static ViaCepService Instancia => _instancia ??= new ViaCepService();

        private readonly HttpClient _httpClient;

        private ViaCepService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
        }

        /// <summary>
        /// Consulta os dados de endereço a partir de um CEP informado (8 dígitos).
        /// </summary>
        public async Task<ViaCepResultado?> ConsultarCepAsync(string cep)
        {
            if (string.IsNullOrWhiteSpace(cep))
                return null;

            string cepLimpo = cep.Replace("-", "").Replace(".", "").Replace(" ", "").Trim();
            if (cepLimpo.Length != 8 || !long.TryParse(cepLimpo, out _))
                return null;

            try
            {
                string url = $"https://viacep.com.br/ws/{cepLimpo}/json/";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                string json = await response.Content.ReadAsStringAsync();
                var resultado = JsonSerializer.Deserialize<ViaCepResultado>(json);

                if (resultado != null && !resultado.TemErro)
                {
                    return resultado;
                }
            }
            catch
            {
                // Tratamento seguro em caso de indisponibilidade de internet ou timeout
            }

            return null;
        }
    }
}
