using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
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
            await _banco.CreateTableAsync<Locacao>();
            await _banco.CreateTableAsync<ServicoOpcional>();

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

            // 5. Seed de Locação inicial em 'Aguardando Retirada' com código PIN de 6 dígitos para teste da locadora
            var locacoesExistentes = await _banco.Table<Locacao>().CountAsync();
            if (locacoesExistentes == 0)
            {
                var locacaoInicial = new Locacao
                {
                    UsuarioId = 2,
                    UsuarioNome = "Lucas Aluno",
                    ItemId = 1,
                    ItemNome = "Xbox 360",
                    ChaveSerialConsole = "XB360-9842-BR",
                    LocadoraId = 1,
                    LocadoraNome = "GameZone Express - Centro",
                    DataLocacao = DateTime.Now,
                    DataDevolucaoPrevista = DateTime.Today.AddDays(3),
                    CodigoConfirmacao = "849201",
                    StatusLocacao = StatusLocacaoEnum.AguardandoRetirada,
                    ValorTotal = 105.00m,
                    TaxaAdicional = 0m,
                    ModalidadeAluguel = "Diária",
                    QuantidadePeriodo = 3,
                    DiasLocados = 3,
                    FormaEntrega = "Retirada no Balcão",
                    DescricaoExtras = "Nenhum adicional selecionado"
                };

                await _banco.InsertAsync(locacaoInicial);
            }

            // 6. Seed de Serviços Opcionais vinculados às Locadoras
            var servicosExistentes = await _banco.Table<ServicoOpcional>().CountAsync();
            if (servicosExistentes == 0)
            {
                var servicosIniciais = new List<ServicoOpcional>
                {
                    // Serviços da GameZone Express - Centro
                    new ServicoOpcional
                    {
                        LocadoraId = 1,
                        LocadoraNome = "GameZone Express - Centro",
                        Nome = "Controle Extra Sem Fio",
                        Descricao = "Controle adicional original para partidas multiplayer local",
                        IconeGlyph = "\uf11b",
                        Valor = 15.00m,
                        Ativo = true
                    },
                    new ServicoOpcional
                    {
                        LocadoraId = 1,
                        LocadoraNome = "GameZone Express - Centro",
                        Nome = "Headset Gamer Pro",
                        Descricao = "Headset com microfone de alta fidelidade e áudio espacial",
                        IconeGlyph = "\uf025",
                        Valor = 12.00m,
                        Ativo = true
                    },
                    new ServicoOpcional
                    {
                        LocadoraId = 1,
                        LocadoraNome = "GameZone Express - Centro",
                        Nome = "Pacote 2 Jogos Físicos Extras",
                        Descricao = "Escolha 2 títulos em mídia física do acervo na retirada",
                        IconeGlyph = "\uf02d",
                        Valor = 20.00m,
                        Ativo = true
                    },
                    new ServicoOpcional
                    {
                        LocadoraId = 1,
                        LocadoraNome = "GameZone Express - Centro",
                        Nome = "Seguro Proteção Acidental Total",
                        Descricao = "Cobertura integral contra quedas e isenção de franquia",
                        IconeGlyph = "\uf3ed",
                        Valor = 25.00m,
                        Ativo = true
                    },

                    // Serviços da Pixel Play Retro & NextGen
                    new ServicoOpcional
                    {
                        LocadoraId = 2,
                        LocadoraNome = "Pixel Play Retro & NextGen",
                        Nome = "Controle Arcade Fight Stick",
                        Descricao = "Controle estilo fliperama clássico com botões Sanwa",
                        IconeGlyph = "\uf11b",
                        Valor = 30.00m,
                        Ativo = true
                    },
                    new ServicoOpcional
                    {
                        LocadoraId = 2,
                        LocadoraNome = "Pixel Play Retro & NextGen",
                        Nome = "Cabo HDMI 2.1 Ultra High Speed 4K",
                        Descricao = "Cabo blindado de 3 metros com suporte a 120Hz e HDR",
                        IconeGlyph = "\uf0c1",
                        Valor = 10.00m,
                        Ativo = true
                    },
                    new ServicoOpcional
                    {
                        LocadoraId = 2,
                        LocadoraNome = "Pixel Play Retro & NextGen",
                        Nome = "Seguro Especial Quedas",
                        Descricao = "Proteção básica contra riscos e avarias no transporte",
                        IconeGlyph = "\uf3ed",
                        Valor = 20.00m,
                        Ativo = true
                    },

                    // Serviços da Arena Gamer Pro - Zona Sul
                    new ServicoOpcional
                    {
                        LocadoraId = 3,
                        LocadoraNome = "Arena Gamer Pro - Zona Sul",
                        Nome = "Controle Pro Wireless Elite / DualSense Edge",
                        Descricao = "Controle nível e-sports com paddles e gatilhos ajustáveis",
                        IconeGlyph = "\uf11b",
                        Valor = 35.00m,
                        Ativo = true
                    },
                    new ServicoOpcional
                    {
                        LocadoraId = 3,
                        LocadoraNome = "Arena Gamer Pro - Zona Sul",
                        Nome = "Headset Sem Fio Wireless 7.1",
                        Descricao = "Headset wireless com cancelamento de ruído e 16h de bateria",
                        IconeGlyph = "\uf025",
                        Valor = 25.00m,
                        Ativo = true
                    },
                    new ServicoOpcional
                    {
                        LocadoraId = 3,
                        LocadoraNome = "Arena Gamer Pro - Zona Sul",
                        Nome = "Seguro Blindagem Gamer VIP",
                        Descricao = "Substituição imediata em caso de qualquer imprevisto",
                        IconeGlyph = "\uf3ed",
                        Valor = 35.00m,
                        Ativo = true
                    }
                };

                await _banco.InsertAllAsync(servicosIniciais);
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

        // ==========================================
        // MÉTODOS CRUD E LOGÍSTICA: LOCAÇÕES & PIN
        // ==========================================

        /// <summary>
        /// Salva uma nova locação no SQLite. Caso o código de confirmação de 6 dígitos
        /// ainda não tenha sido gerado, gera um PIN criptograficamente seguro entre 100000 e 999999.
        /// </summary>
        public async Task<int> SalvarLocacaoAsync(Locacao locacao)
        {
            await InicializarAsync();

            if (string.IsNullOrWhiteSpace(locacao.CodigoConfirmacao))
            {
                locacao.CodigoConfirmacao = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            }

            if (locacao.Id != 0)
            {
                return await _banco!.UpdateAsync(locacao);
            }

            return await _banco!.InsertAsync(locacao);
        }

        public async Task<List<Locacao>> ObterLocacoesAsync()
        {
            await InicializarAsync();
            return await _banco!.Table<Locacao>()
                                .OrderByDescending(l => l.DataLocacao)
                                .ToListAsync();
        }

        public async Task<List<Locacao>> ObterLocacoesPorLocadoraAsync(string locadoraNome)
        {
            await InicializarAsync();
            return await _banco!.Table<Locacao>()
                                .Where(l => l.LocadoraNome == locadoraNome)
                                .OrderByDescending(l => l.DataLocacao)
                                .ToListAsync();
        }

        /// <summary>
        /// Busca locações filtrando por status (ex: AguardandoRetirada) e opcionalmente por nome da locadora/filial.
        /// </summary>
        public async Task<List<Locacao>> ObterLocacoesPorStatusAsync(StatusLocacaoEnum status, string? locadoraNome = null)
        {
            await InicializarAsync();

            var query = _banco!.Table<Locacao>().Where(l => l.StatusLocacao == status);

            if (!string.IsNullOrWhiteSpace(locadoraNome))
            {
                query = query.Where(l => l.LocadoraNome == locadoraNome);
            }

            return await query.OrderByDescending(l => l.DataLocacao).ToListAsync();
        }

        public async Task<Locacao?> ObterLocacaoPorIdAsync(int id)
        {
            await InicializarAsync();
            return await _banco!.Table<Locacao>()
                                .Where(l => l.Id == id)
                                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Valida o PIN de 6 dígitos fornecido pelo cliente no momento da retirada.
        /// Se o código for idêntico e a locação estiver com status AguardandoRetirada:
        /// - Transiciona a locação para StatusLocacaoEnum.Alugado
        /// - Atualiza o status do console para 'Alugado'
        /// - Sincroniza o registro correspondente no Histórico
        /// - Retorna true em caso de sucesso e false se for incorreto ou inválido.
        /// </summary>
        public async Task<bool> ValidarCodigoRetiradaAsync(int locacaoId, string codigoDigitado)
        {
            await InicializarAsync();

            if (string.IsNullOrWhiteSpace(codigoDigitado))
                return false;

            var locacao = await _banco!.Table<Locacao>()
                                       .Where(l => l.Id == locacaoId)
                                       .FirstOrDefaultAsync();

            if (locacao == null)
                return false;

            // Validação estrita: status deve ser AguardandoRetirada e PIN de 6 dígitos idêntico
            if (locacao.StatusLocacao == StatusLocacaoEnum.AguardandoRetirada &&
                string.Equals(locacao.CodigoConfirmacao?.Trim(), codigoDigitado.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                // 1. Atualiza o status da locação para Alugado
                locacao.StatusLocacao = StatusLocacaoEnum.Alugado;
                await _banco.UpdateAsync(locacao);

                // 2. Atualiza o console/item físico para "Alugado"
                var console = await _banco.Table<Item>().Where(i => i.Id == locacao.ItemId).FirstOrDefaultAsync();
                if (console != null)
                {
                    console.Status = "Alugado";
                    await _banco.UpdateAsync(console);
                }

                // 3. Sincroniza o status no Histórico para acompanhamento no Dashboard e Calendário
                var historico = await _banco.Table<Historico>()
                                            .Where(h => h.ItemId == locacao.ItemId && h.UsuarioId == locacao.UsuarioId)
                                            .OrderByDescending(h => h.DataHora)
                                            .FirstOrDefaultAsync();

                if (historico != null)
                {
                    historico.StatusLocacao = "Em Andamento";
                    await _banco.UpdateAsync(historico);
                }

                return true;
            }

            return false;
        }

        // ==========================================
        // MÉTODOS CRUD: SERVIÇOS OPCIONAIS DA LOCADORA
        // ==========================================

        public async Task<List<ServicoOpcional>> ObterServicosOpcionaisPorLocadoraAsync(string locadoraNome)
        {
            await InicializarAsync();
            return await _banco!.Table<ServicoOpcional>()
                                .Where(s => s.LocadoraNome == locadoraNome)
                                .ToListAsync();
        }

        public async Task<int> SalvarServicoOpcionalAsync(ServicoOpcional servico)
        {
            await InicializarAsync();
            if (servico.Id != 0)
            {
                return await _banco!.UpdateAsync(servico);
            }
            return await _banco!.InsertAsync(servico);
        }

        public async Task<int> ExcluirServicoOpcionalAsync(ServicoOpcional servico)
        {
            await InicializarAsync();
            return await _banco!.DeleteAsync(servico);
        }

        /// <summary>
        /// Limpa e reconfigura todas as tabelas do banco de dados SQLite,
        /// restaurando os seeds padrão de locadoras, consoles, usuários e locações de teste.
        /// </summary>
        public async Task RestaurarBancoDadosAsync()
        {
            await InicializarAsync();

            // 1. Remove tabelas existentes
            await _banco!.DropTableAsync<ServicoOpcional>();
            await _banco!.DropTableAsync<Locacao>();
            await _banco!.DropTableAsync<Historico>();
            await _banco!.DropTableAsync<Item>();
            await _banco!.DropTableAsync<Locadora>();
            await _banco!.DropTableAsync<Usuario>();

            // 2. Recria todas as tabelas vazias
            await _banco.CreateTableAsync<Usuario>();
            await _banco.CreateTableAsync<Locadora>();
            await _banco.CreateTableAsync<Item>();
            await _banco.CreateTableAsync<Historico>();
            await _banco.CreateTableAsync<Locacao>();
            await _banco.CreateTableAsync<ServicoOpcional>();

            // 3. Executa a população com os dados padrão originais
            await PopularDadosIniciaisAsync();
        }
    }
}
