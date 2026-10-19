using ProjetoFinalMVVM.Models;

namespace ProjetoFinalMVVM.Helpers
{
    /// <summary>
    /// Gerenciador de sessão global implementando o padrão de projeto Singleton.
    /// Armazena em memória as informações do usuário autenticado no aplicativo,
    /// permitindo que qualquer ViewModel ou View acesse o contexto da sessão.
    /// </summary>
    public class UsuarioAtual
    {
        // 1. Campo estático privado que mantém a instância única da classe
        private static UsuarioAtual? _instancia;

        // 2. Propriedade estática pública com acesso global (Lazy Initialization)
        public static UsuarioAtual Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    _instancia = new UsuarioAtual();
                }
                return _instancia;
            }
        }

        // 3. Construtor privado para impedir instanciação externa via 'new'
        private UsuarioAtual()
        {
        }

        // Propriedades do usuário atualmente autenticado
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; // "Administrador", "Locadora" ou "Cliente"
        public string LocadoraVinculadaNome { get; set; } = string.Empty;

        public bool IsAdministrador => Tipo == "Administrador";
        public bool IsLocadora => Tipo == "Locadora";
        public bool IsCliente => Tipo == "Cliente";
        public bool IsAutenticado => Id > 0;

        /// <summary>
        /// Popula o Singleton com as informações do usuário autenticado após o login.
        /// </summary>
        public void DefinirUsuario(Usuario usuario)
        {
            if (usuario == null) return;
            Id = usuario.Id;
            Nome = usuario.Nome;
            Cpf = usuario.Cpf;
            Email = usuario.Email;
            Tipo = usuario.Tipo;
            LocadoraVinculadaNome = usuario.LocadoraVinculadaNome;
        }

        /// <summary>
        /// Limpa o estado da sessão ao realizar logout.
        /// </summary>
        public void Deslogar()
        {
            Id = 0;
            Nome = string.Empty;
            Cpf = string.Empty;
            Email = string.Empty;
            Tipo = string.Empty;
            LocadoraVinculadaNome = string.Empty;
        }
    }
}
