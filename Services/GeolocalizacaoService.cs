using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;

namespace ProjetoFinalMVVM.Services
{
    /// <summary>
    /// Serviço de Geoposicionamento e Integração com Aplicativos de Mapas do dispositivo.
    /// Gerencia a solicitação em tempo de execução de permissões de localização (GPS),
    /// calcula distâncias reais em quilômetros até as unidades físicas da locadora
    /// e abre trajetos de navegação no app nativo de mapas (Google Maps / Apple Maps / Bing).
    /// </summary>
    public class GeolocalizacaoService
    {
        private static GeolocalizacaoService? _instancia;
        public static GeolocalizacaoService Instancia => _instancia ??= new GeolocalizacaoService();

        // Localização padrão de referência (Marco Zero de São Paulo - Praça da Sé)
        // Usada caso o usuário negue permissão de GPS ou o hardware não possua sensor (ex: desktop/máquina virtual)
        public const double LatitudePadrao = -23.550520;
        public const double LongitudePadrao = -46.633308;

        public Location LocalizacaoAtual { get; private set; }
        public bool IsUsandoGpsReal { get; private set; }
        public string StatusGpsTexto => IsUsandoGpsReal ? "GPS Ativo (Localização em tempo real)" : "Localização de Referência (Centro)";

        private GeolocalizacaoService()
        {
            LocalizacaoAtual = new Location(LatitudePadrao, LongitudePadrao);
            IsUsandoGpsReal = false;
        }

        /// <summary>
        /// Solicita permissão e obtém as coordenadas geográficas atuais do usuário.
        /// </summary>
        public async Task<Location> ObterLocalizacaoAsync(bool forcarAtualizacao = false)
        {
            try
            {
                // 1. Verifica e solicita permissão de localização ao usuário
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }

                if (status == PermissionStatus.Granted)
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(6));
                    var location = await Geolocation.Default.GetLocationAsync(request);

                    if (location != null)
                    {
                        LocalizacaoAtual = location;
                        IsUsandoGpsReal = true;
                        return LocalizacaoAtual;
                    }
                }
            }
            catch (Exception)
            {
                // Fallback gracioso em caso de falha de sensor ou timeout
            }

            // Fallback de referência caso GPS esteja desativado ou negado
            IsUsandoGpsReal = false;
            return LocalizacaoAtual;
        }

        /// <summary>
        /// Realiza geocodificação reversa para transformar coordenadas GPS em dados de endereço estruturado.
        /// </summary>
        public async Task<Placemark?> ObterEnderecoPorCoordenadasAsync(double latitude, double longitude)
        {
            try
            {
                var placemarks = await Geocoding.Default.GetPlacemarksAsync(latitude, longitude);
                return System.Linq.Enumerable.FirstOrDefault(placemarks);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Calcula a distância em quilômetros entre duas coordenadas geográficas.
        /// </summary>
        public double CalcularDistanciaKm(double lat1, double lon1, double lat2, double lon2)
        {
            if (lat1 == 0 && lon1 == 0) return 0;
            if (lat2 == 0 && lon2 == 0) return 0;

            var pontoOrigem = new Location(lat1, lon1);
            var pontoDestino = new Location(lat2, lon2);

            return Location.CalculateDistance(pontoOrigem, pontoDestino, DistanceUnits.Kilometers);
        }

        /// <summary>
        /// Abre a rota no aplicativo nativo de mapas instalado no aparelho (Google Maps, Apple Maps ou Bing).
        /// </summary>
        public async Task AbrirRotaNoMapaAsync(double destinoLat, double destinoLon, string nomeDestino)
        {
            try
            {
                var options = new MapLaunchOptions
                {
                    Name = nomeDestino,
                    NavigationMode = NavigationMode.Driving
                };

                await Map.Default.OpenAsync(new Location(destinoLat, destinoLon), options);
            }
            catch (Exception)
            {
                // Fallback universal via navegador / link direto do Google Maps caso o Map nativo falhe
                try
                {
                    string url = string.Format(
                        CultureInfo.InvariantCulture,
                        "https://www.google.com/maps/dir/?api=1&destination={0},{1}",
                        destinoLat, destinoLon);

                    await Launcher.Default.OpenAsync(new Uri(url));
                }
                catch
                {
                    // Tratamento seguro para nunca quebrar a interface
                }
            }
        }

        /// <summary>
        /// Inicia discagem para o número de telefone informado.
        /// </summary>
        public void RealizarLigacao(string telefone)
        {
            if (string.IsNullOrWhiteSpace(telefone)) return;

            try
            {
                if (Microsoft.Maui.ApplicationModel.Communication.PhoneDialer.Default.IsSupported)
                {
                    Microsoft.Maui.ApplicationModel.Communication.PhoneDialer.Default.Open(telefone);
                }
            }
            catch
            {
                // Tratamento seguro em dispositivos sem discador (ex: Windows Desktop sem aplicativo de telefone)
            }
        }
    }
}
