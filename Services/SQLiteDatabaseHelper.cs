using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ProjetoFinalMVVM.Models;

namespace ProjetoFinalMVVM.Services
{
    /// <summary>
    /// Serviço de persistência local gerenciando operações SQLite através do pacote 'sqlite-net-pcl'.
    /// Implementa o padrão Singleton para manter uma conexão assíncrona única durante o ciclo de vida do app.
    /// </summary>
    public class SQLiteDatabaseHelper
    {
        private static SQLiteDatabaseHelper? _instancia;
        public static SQLiteDatabaseHelper Instancia => _instancia ??= new SQLiteDatabaseHelper();

        private SQLiteAsyncConnection? _banco;

        private SQLiteDatabaseHelper()
        {
        }

        /// <summary>
        /// Inicializa a conexão SQLite no diretório de dados do aplicativo
        /// e cria as tabelas caso ainda não existam.
        /// </summary>
        public async Task InicializarAsync()
        {
            if (_banco != null)
                return;

            string caminhoBanco = Path.Combine(FileSystem.AppDataDirectory, "locadora_videogame.db3");
            _banco = new SQLiteAsyncConnection(caminhoBanco);

            // Criação das tabelas da arquitetura
            await _banco.CreateTableAsync<Usuario>();
            await _banco.CreateTableAsync<Locadora>();
            await _banco.CreateTableAsync<Item>();
            await _banco.CreateTableAsync<Historico>();

            // Executa o Seed inicial de dados para a primeira execução
            await PopularDadosIniciaisAsync();
        }

        /// <summary>
        /// Popula o banco com as 3 locadoras e os consoles solicitados na especificação
        /// caso as tabelas estejam vazias.
        /// </summary>
        private async Task PopularDadosIniciaisAsync()
        {
            if (_banco == null) return;

            // 1. Seed de Locadoras com Coordenadas Reais e Telefones
            var locadorasExistentes = await _banco.Table<Locadora>().CountAsync();
            if (locadorasExistentes == 0)
            {
                var locadorasIniciais = new List<Locadora>
                {
                    new Locadora
                    {
                        Nome = "GameZone Express - Centro",
                        Endereco = "Av. Central, 500 - Centro, São Paulo - SP",
                        Telefone = "(11) 3105-1020",
                        Latitude = -23.548900,
                        Longitude = -46.638800,
                        DistanciaKm = 0.8
                    },
                    new Locadora
                    {
                        Nome = "Pixel Play Retro & NextGen",
                        Endereco = "Av. Paulista, 1200 - Bela Vista, São Paulo - SP",
                        Telefone = "(11) 3288-4450",
                        Latitude = -23.561414,
                        Longitude = -46.655881,
                        DistanciaKm = 2.4
                    },
                    new Locadora
                    {
                        Nome = "Arena Gamer Pro - Zona Sul",
                        Endereco = "Av. Eng. Luís Carlos Berrini, 890 - Itaim Bibi, São Paulo - SP",
                        Telefone = "(11) 5093-8870",
                        Latitude = -23.601200,
                        Longitude = -46.692300,
                        DistanciaKm = 7.1
                    }
                };

                await _banco.InsertAllAsync(locadorasIniciais);
            }
            else
            {
                // Garante que bases já criadas recebam coordenadas reais
                var todasLocadoras = await _banco.Table<Locadora>().ToListAsync();
                foreach (var loc in todasLocadoras)
                {
                    if (loc.Latitude == 0 && loc.Longitude == 0)
                    {
                        if (loc.Nome.Contains("Centro")) { loc.Latitude = -23.548900; loc.Longitude = -46.638800; loc.Telefone = "(11) 3105-1020"; }
                        else if (loc.Nome.Contains("Pixel")) { loc.Latitude = -23.561414; loc.Longitude = -46.655881; loc.Telefone = "(11) 3288-4450"; }
                        else { loc.Latitude = -23.601200; loc.Longitude = -46.692300; loc.Telefone = "(11) 5093-8870"; }
                        await _banco.UpdateAsync(loc);
                    }
                }
            }

            // 2. Seed de Consoles (Itens) com Valores Diário, Semanal e Mensal
            var itensExistentes = await _banco.Table<Item>().CountAsync();
            if (itensExistentes == 0)
            {
                var consolesIniciais = new List<Item>
                {
                    new Item
                    {
                        Nome = "Xbox 360",
                        ChaveSerial = "XB360-9842-BR",
                        ImgDiretorio = "https://images.unsplash.com/photo-1621259182978-fbf93132d53d?w=400",
                        Desc = "Console Xbox 360 Slim com 2 controles sem fio, HD 250GB e coletânea de clássicos.",
                        ContaVinculada = "xbox.retro360@gamerental.com",
                        PermiteDiaria = true,
                        ValorDiaria = 35.00m,
                        PermiteSemanal = true,
                        ValorSemanal = 180.00m,
                        PermiteMensal = true,
                        ValorMensal = 520.00m,
                        Status = "Disponível",
                        LocadoraNome = "GameZone Express - Centro"
                    },
                    new Item
                    {
                        Nome = "Nintendo Wii",
                        ChaveSerial = "WII-4412-BR",
                        ImgDiretorio = "https://images.unsplash.com/photo-1578632767115-351597cf2477?w=400",
                        Desc = "Nintendo Wii com sensor bar, 2 controles Wii Remote Plus, Nunchuk e Wii Sports.",
                        ContaVinculada = "nintendo.fun@gamerental.com",
                        PermiteDiaria = true,
                        ValorDiaria = 30.00m,
                        PermiteSemanal = true,
                        ValorSemanal = 150.00m,
                        PermiteMensal = true,
                        ValorMensal = 450.00m,
                        Status = "Disponível",
                        LocadoraNome = "GameZone Express - Centro"
                    },
                    new Item
                    {
                        Nome = "Xbox One",
                        ChaveSerial = "XBO-7719-BR",
                        ImgDiretorio = "https://images.unsplash.com/photo-1607604276583-eef5d076aa5f?w=400",
                        Desc = "Xbox One S 1TB Branco com controle sem fio e assinatura Game Pass Ultimate inclusa.",
                        ContaVinculada = "gamepass.ultimate@gamerental.com",
                        PermiteDiaria = true,
                        ValorDiaria = 50.00m,
                        PermiteSemanal = true,
                        ValorSemanal = 260.00m,
                        PermiteMensal = true,
                        ValorMensal = 790.00m,
                        Status = "Disponível",
                        LocadoraNome = "Pixel Play Retro & NextGen"
                    },
                    new Item
                    {
                        Nome = "PlayStation 5",
                        ChaveSerial = "PS5-5520-BR",
                        ImgDiretorio = "https://images.unsplash.com/photo-1606813907291-d86efa9b94db?w=400",
                        Desc = "PlayStation 5 Edição Digital com controle sem fio DualSense e PS Plus Extra.",
                        ContaVinculada = "psn.plus@gamerental.com",
                        PermiteDiaria = true,
                        ValorDiaria = 70.00m,
                        PermiteSemanal = true,
                        ValorSemanal = 370.00m,
                        PermiteMensal = true,
                        ValorMensal = 1100.00m,
                        Status = "Disponível",
                        LocadoraNome = "Arena Gamer Pro - Zona Sul"
                    }
                };

                await _banco.InsertAllAsync(consolesIniciais);
            }
            else
            {
                // Migração suave: se a base já possui itens sem valores semanais/mensais, inicializa com descontos proporcionais
                var itensDoBanco = await _banco.Table<Item>().ToListAsync();
                foreach (var item in itensDoBanco)
                {
                    if (item.ValorSemanal <= 0 || item.ValorMensal <= 0)
                    {
                        item.PermiteDiaria = true;
                        item.PermiteSemanal = true;
                        item.PermiteMensal = true;
                        item.ValorSemanal = Math.Round(item.ValorDiaria * 7 * 0.75m, 2);
                        item.ValorMensal = Math.Round(item.ValorDiaria * 30 * 0.55m, 2);
                        await _banco.UpdateAsync(item);
                    }
                }
            }

            // 3. Seed de Usuários (Administrador, Locadora e Cliente padrão de teste)
            var usuariosExistentes = await _banco.Table<Usuario>().CountAsync();
            if (usuariosExistentes == 0)
            {
                var usuariosIniciais = new List<Usuario>
                {
                    new Usuario
                    {
                        Nome = "Admin Master",
                        Cpf = "111.222.333-44",
                        Email = "admin@gamer.com",
                        Senha = "123",
                        Tipo = "Administrador"
                    },
                    new Usuario
                    {
                        Nome = "Gerente GameZone",
                        Cpf = "333.444.555-66",
                        Email = "locadora@gamer.com",
                        Senha = "123",
                        Tipo = "Locadora",
                        LocadoraVinculadaNome = "GameZone Express - Centro"
                    },
                    new Usuario
                    {
                        Nome = "Lucas Aluno",
                        Cpf = "222.333.444-55",
                        Email = "cliente@gamer.com",
                        Senha = "123",
                        Tipo = "Cliente"
                    }
                };

                await _banco.InsertAllAsync(usuariosIniciais);
            }
            else
            {
                // Garante que o perfil Locadora exista mesmo em bancos já criados
                var locadoraUser = await _banco.Table<Usuario>().Where(u => u.Email == "locadora@gamer.com").FirstOrDefaultAsync();
                if (locadoraUser == null)
                {
                    await _banco.InsertAsync(new Usuario
                    {
                        Nome = "Gerente GameZone",
                        Cpf = "333.444.555-66",
                        Email = "locadora@gamer.com",
                        Senha = "123",
                        Tipo = "Locadora",
                        LocadoraVinculadaNome = "GameZone Express - Centro"
                    });
                }
            }

            // 4. Seed de Histórico inicial para alimentar o Dashboard e Calendário
            var historicosExistentes = await _banco.Table<Historico>().CountAsync();
            if (historicosExistentes == 0)
            {
                var historicoInicial = new List<Historico>
                {
                    new Historico
                    {
                        UsuarioId = 2,
                        UsuarioNome = "Lucas Aluno",
                        ItemId = 1,
                        ItemNome = "Xbox 360",
                        ChaveSerialConsole = "XB360-9842-BR",
                        LocadoraNome = "GameZone Express - Centro",
                        DataHora = DateTime.Now.AddDays(-3),
                        DataDevolucaoPrevista = DateTime.Today.AddDays(1), // Amanhã
                        DiasLocados = 4,
                        TaxaAdicional = 15.00m,
                        ValorTotal = 155.00m,
                        StatusLocacao = "Em Andamento"
                    },
                    new Historico
                    {
                        UsuarioId = 2,
                        UsuarioNome = "Lucas Aluno",
                        ItemId = 2,
                        ItemNome = "Nintendo Wii",
                        ChaveSerialConsole = "WII-4412-BR",
                        LocadoraNome = "GameZone Express - Centro",
                        DataHora = DateTime.Now.AddDays(-10),
                        DataDevolucaoPrevista = DateTime.Now.AddDays(-3),
                        DiasLocados = 7,
                        TaxaAdicional = 0.00m,
                        ValorTotal = 210.00m,
                        StatusLocacao = "Devolvido"
                    }
                };

                await _banco.InsertAllAsync(historicoInicial);
            }
        }

        // ==========================================
        // MÉTODOS CRUD: USUÁRIOS
        // ==========================================

        public async Task<Usuario?> ObterUsuarioPorCredenciaisAsync(string email, string senha)
        {
            await InicializarAsync();
            return await _banco!.Table<Usuario>()
                                .Where(u => u.Email.ToLower() == email.ToLower() && u.Senha == senha)
                                .FirstOrDefaultAsync();
        }

        public async Task<Usuario?> ObterUsuarioPorEmailAsync(string email)
        {
            await InicializarAsync();
            return await _banco!.Table<Usuario>()
                                .Where(u => u.Email.ToLower() == email.ToLower())
                                .FirstOrDefaultAsync();
        }

        public async Task<int> SalvarUsuarioAsync(Usuario usuario)
        {
            await InicializarAsync();
            if (usuario.Id != 0)
            {
                return await _banco!.UpdateAsync(usuario);
            }
            return await _banco!.InsertAsync(usuario);
        }

        public async Task<List<Usuario>> ObterUsuariosAsync()
        {
            await InicializarAsync();
            return await _banco!.Table<Usuario>().ToListAsync();
        }

        // ==========================================
        // MÉTODOS CRUD: LOCADORAS
        // ==========================================

        public async Task<List<Locadora>> ObterLocadorasAsync()
        {
            await InicializarAsync();
            return await _banco!.Table<Locadora>()
                                .OrderBy(l => l.DistanciaKm)
                                .ToListAsync();
        }

        public async Task<int> SalvarLocadoraAsync(Locadora locadora)
        {
            await InicializarAsync();
            if (locadora.Id != 0)
            {
                return await _banco!.UpdateAsync(locadora);
            }
            return await _banco!.InsertAsync(locadora);
        }

        // ==========================================
        // MÉTODOS CRUD: CONSOLES (ITENS)
        // ==========================================

        public async Task<List<Item>> ObterItensAsync()
        {
            await InicializarAsync();
            return await _banco!.Table<Item>().ToListAsync();
        }

        public async Task<List<Item>> ObterItensPorLocadoraAsync(string locadoraNome)
        {
            await InicializarAsync();
            return await _banco!.Table<Item>()
                                .Where(i => i.LocadoraNome == locadoraNome)
                                .ToListAsync();
        }

        public async Task<Item?> ObterItemPorIdAsync(int id)
        {
            await InicializarAsync();
            return await _banco!.Table<Item>()
                                .Where(i => i.Id == id)
                                .FirstOrDefaultAsync();
        }

        public async Task<int> SalvarItemAsync(Item item)
        {
            await InicializarAsync();
            if (item.Id != 0)
            {
                return await _banco!.UpdateAsync(item);
            }
            return await _banco!.InsertAsync(item);
        }

        public async Task<int> ExcluirItemAsync(Item item)
        {
            await InicializarAsync();
            return await _banco!.DeleteAsync(item);
        }

        // ==========================================
        // MÉTODOS CRUD: HISTÓRICO DE LOCAÇÕES
        // ==========================================

        public async Task<List<Historico>> ObterHistoricosAsync()
        {
            await InicializarAsync();
            return await _banco!.Table<Historico>()
                                .OrderByDescending(h => h.DataHora)
                                .ToListAsync();
        }

        public async Task<List<Historico>> ObterHistoricoPorUsuarioAsync(int usuarioId)
        {
            await InicializarAsync();
            return await _banco!.Table<Historico>()
                                .Where(h => h.UsuarioId == usuarioId)
                                .OrderByDescending(h => h.DataHora)
                                .ToListAsync();
        }

        public async Task<List<Historico>> ObterDevolucoesPorDataAsync(DateTime data)
        {
            await InicializarAsync();
            var todos = await _banco!.Table<Historico>().ToListAsync();
            return todos.FindAll(h => h.DataDevolucaoPrevista.Date == data.Date && h.StatusLocacao == "Em Andamento");
        }

        public async Task<int> SalvarHistoricoAsync(Historico historico)
        {
            await InicializarAsync();
            if (historico.Id != 0)
            {
                return await _banco!.UpdateAsync(historico);
            }
            return await _banco!.InsertAsync(historico);
        }

        public async Task<int> AtualizarHistoricoAsync(Historico historico)
        {
            await InicializarAsync();
            return await _banco!.UpdateAsync(historico);
        }

        public async Task<int> ExcluirUsuarioAsync(Usuario usuario)
        {
            await InicializarAsync();
            return await _banco!.DeleteAsync(usuario);
        }

        public async Task<int> ExcluirLocadoraAsync(Locadora locadora)
        {
            await InicializarAsync();
            return await _banco!.DeleteAsync(locadora);
        }

        public async Task<Locadora?> ObterLocadoraPorNomeAsync(string nome)
        {
            await InicializarAsync();
            return await _banco!.Table<Locadora>().Where(l => l.Nome == nome).FirstOrDefaultAsync();
        }

        public async Task<List<Historico>> ObterHistoricosPorLocadoraAsync(string locadoraNome)
        {
            await InicializarAsync();
            return await _banco!.Table<Historico>()
                                .Where(h => h.LocadoraNome == locadoraNome)
                                .OrderByDescending(h => h.DataHora)
                                .ToListAsync();
        }

        public async Task<List<Historico>> ObterDevolucoesPorDataELocadoraAsync(DateTime data, string locadoraNome)
        {
            await InicializarAsync();
            var todos = await _banco!.Table<Historico>()
                                     .Where(h => h.LocadoraNome == locadoraNome)
                                     .ToListAsync();
            return todos.FindAll(h => h.DataDevolucaoPrevista.Date == data.Date && h.StatusLocacao == "Em Andamento");
        }
    }
}
