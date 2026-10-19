using SQLite;

namespace ProjetoFinalMVVM.Models
{
    /// <summary>
    /// Representa os usuários do sistema (Administradores ou Clientes).
    /// Mapeado para a tabela 'Usuarios' no SQLite.
    /// </summary>
    [Table("Usuarios")]
    public class Usuario
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Cpf { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Senha { get; set; } = string.Empty;

        /// <summary>
        /// Define as permissões de acesso: "Administrador", "Locadora" ou "Cliente".
        /// </summary>
        public string Tipo { get; set; } = "Cliente";

        /// <summary>
        /// Quando o usuário for do tipo "Locadora", vincula à filial específica que ele administra.
        /// </summary>
        public string LocadoraVinculadaNome { get; set; } = string.Empty;

        // Propriedades visuais auxiliares (ignoradas pelo SQLite)
        [Ignore]
        public string TipoBadgeBg => Tipo switch
        {
            "Administrador" => "#FEF3C7",
            "Locadora" => "#E0E7FF",
            _ => "#DCFCE7"
        };

        [Ignore]
        public string TipoBadgeText => Tipo switch
        {
            "Administrador" => "#B45309",
            "Locadora" => "#4338CA",
            _ => "#15803D"
        };

        [Ignore]
        public string SubtituloDescricao => Tipo == "Locadora"
            ? $"Gerente da unidade: {LocadoraVinculadaNome}"
            : (Tipo == "Administrador" ? "Acesso total ao sistema" : $"CPF: {Cpf}");
    }
}
