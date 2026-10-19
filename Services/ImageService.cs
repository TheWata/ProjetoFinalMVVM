using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace ProjetoFinalMVVM.Services
{
    /// <summary>
    /// Serviço responsável pela seleção de imagens no computador ou celular
    /// e armazenamento seguro no diretório de recursos/dados do aplicativo.
    /// </summary>
    public class ImageService
    {
        private static ImageService? _instancia;
        public static ImageService Instancia => _instancia ??= new ImageService();

        private ImageService()
        {
        }

        /// <summary>
        /// Abre o seletor de arquivos do sistema operacional (Windows/Android/iOS),
        /// permite ao usuário selecionar uma imagem e a salva na pasta local de recursos do aplicativo.
        /// Retorna o caminho absoluto do arquivo salvo para exibição no componente Image.
        /// </summary>
        public async Task<string?> SelecionarESalvarImagemAsync()
        {
            try
            {
                var options = new PickOptions
                {
                    PickerTitle = "Selecione a Imagem do Console",
                    FileTypes = FilePickerFileType.Images
                };

                var resultado = await FilePicker.Default.PickAsync(options);
                if (resultado == null)
                    return null;

                // Cria o diretório de destino 'Resources/Images' no armazenamento local do app
                string pastaDestino = Path.Combine(FileSystem.AppDataDirectory, "Resources", "Images");
                if (!Directory.Exists(pastaDestino))
                {
                    Directory.CreateDirectory(pastaDestino);
                }

                // Gera um nome seguro e único para o arquivo
                string extensao = Path.GetExtension(resultado.FileName);
                if (string.IsNullOrWhiteSpace(extensao))
                    extensao = ".png";

                string nomeArquivoUnico = $"console_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N")[..6]}{extensao}";
                string caminhoCompletoDestino = Path.Combine(pastaDestino, nomeArquivoUnico);

                // Copia os dados do arquivo selecionado para a pasta resources do app
                using (var streamOrigem = await resultado.OpenReadAsync())
                using (var streamDestino = File.Create(caminhoCompletoDestino))
                {
                    await streamOrigem.CopyToAsync(streamDestino);
                }

                return caminhoCompletoDestino;
            }
            catch (Exception)
            {
                // Fallback seguro em caso de permissão negada ou cancelamento
                return null;
            }
        }
    }
}
