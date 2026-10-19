# 📘 Documentação Técnica e Arquitetural — PegaLá (Alugaki)

> **Marketplace Inteligente de Locação e Compartilhamento de Hardware & Consoles de Videogame**  
> *Arquitetura MVVM Pura, Persistência Local SQLite, Geolocalização Reativa e Handshake de Custódia por PIN de 6 Dígitos.*

---

## 📑 Sumário Executivo

1. [Visão Geral do Sistema](#1-visão-geral-do-sistema)
2. [Arquitetura de Software em Camadas (MVVM)](#2-arquitetura-de-software-em-camadas-mvvm)
3. [Diagrama de Casos de Uso (UML Use Case)](#3-diagrama-de-casos-de-uso-uml-use-case)
4. [Diagrama de Classes (UML Class Diagram)](#4-diagrama-de-classes-uml-class-diagram)
5. [Modelo Entidade-Relacionamento (DER / SQLite)](#5-modelo-entidade-relacionamento-der--sqlite)
6. [Diagrama de Sequência: Handshake de Custódia (PIN de 6 Dígitos)](#6-diagrama-de-sequência-handshake-de-custódia-pin-de-6-dígitos)
7. [Diagrama de Sequência: Gestão e Seleção de Serviços Opcionais](#7-diagrama-de-sequência-gestão-e-seleção-de-serviços-opcionais)
8. [Diagrama de Máquina de Estados: Ciclo de Vida da Locação](#8-diagrama-de-máquina-de-estados-ciclo-de-vida-da-locação)
9. [Matriz de Permissões e Perfis de Acesso (RBAC)](#9-matriz-de-permissões-e-perfis-de-acesso-rbac)
10. [Módulos e Serviços de Infraestrutura](#10-módulos-e-serviços-de-infraestrutura)

---

## 1. Visão Geral do Sistema

O **PegaLá (Alugaki)** é uma plataforma mobile de locação de consoles de videogame e equipamentos de entretenimento desenvolvida sobre o framework **.NET MAUI 9.0** e linguagem **C# 13**. O ecossistema atua como um hub logístico e marketplace conectando três atores principais:

1. **Administrador Geral**: Gestor global da plataforma, responsável pelo cadastro e auditoria de filiais/locadoras, catálogo mestre de consoles, gerenciamento de usuários e monitoramento das métricas financeiras consolidadas.
2. **Gerente de Locadora (Unidade/Filial)**: Administra o estoque local de aparelhos, configura planos de locação (diária, semanal, mensal), gerencia o catálogo exclusivo de **Serviços Opcionais** da sua filial, valida a retirada física de consoles no balcão através do código de segurança PIN e dá baixa em devoluções via calendário interativo de 14 dias.
3. **Cliente**: Explora o catálogo de consoles e filiais ordenados dinamicamente pela proximidade GPS (Fórmula de Haversine), simula e contrata aluguéis customizados, escolhe opcionais e forma de entrega, e obtém seu PIN de segurança com acompanhamento via widget flutuante estilo iFood.

---

## 2. Arquitetura de Software em Camadas (MVVM)

O projeto adota rigorosamente o padrão de arquitetura **MVVM (Model-View-ViewModel)** em sua forma **pura e manual**, sem dependência de bibliotecas geradoras de código ou reflexão mágica (como CommunityToolkit), garantindo previsibilidade, portabilidade e aderência estrita aos conceitos de orientação a objetos:

- **Views (XAML & Code-behind Limpo)**: Telas declarativas com DataBinding compilado (`x:DataType`), estilizadas com paleta Light Mode moderna e ícones vetoriais da biblioteca FontAwesome Solid (`FontFamily="FontAwesomeSolid"`). O code-behind (`.xaml.cs`) é estritamente restrito à inicialização de componentes e chamadas de ciclo de vida (`OnAppearing`).
- **ViewModels (`BaseViewModel`)**: Camada intermediária que gerencia estado e regras de apresentação. Herdam de `BaseViewModel`, que encapsula a interface `INotifyPropertyChanged` e o método `SetProperty<T>()`.
- **Models**: Classes POCO anotadas com atributos SQLite (`[Table]`, `[PrimaryKey]`, `[AutoIncrement]`, `[Ignore]`).
- **Services (Singleton)**: Componentes assíncronos que isolam persistência em banco local, consultas de CEP, geolocalização nativa e armazenamento de imagens.

```mermaid
graph TD
    subgraph UI ["Camada de Apresentação (Views - XAML)"]
        V_Login["LoginView"]
        V_Admin["AdminMainView"]
        V_Locadora["LocadoraMainView"]
        V_Cliente["ClienteMainView"]
        V_Aluguel["AluguelDetalhesView"]
        V_CadItem["ConsoleCadastroView"]
        V_CadLoc["LocadoraCadastroView"]
        V_Historico["HistoricoView"]
    end

    subgraph VM ["Camada de Apresentação Lógica (ViewModels)"]
        BaseVM["BaseViewModel (INotifyPropertyChanged)"]
        VM_Item["ItemViewModel"]
        VM_Aluguel["AluguelDetalhesViewModel"]
        VM_User["UsuarioViewModel"]
        BaseVM --> VM_Item
        BaseVM --> VM_Aluguel
        BaseVM --> VM_User
    end

    subgraph Services ["Camada de Serviços (Services - Singleton)"]
        S_DB["SQLiteDatabaseHelper (SQLite-net-pcl)"]
        S_GPS["GeolocalizacaoService (GPS & Haversine)"]
        S_CEP["ViaCepService (HTTP REST)"]
        S_IMG["ImageService (File System)"]
    end

    subgraph Domain ["Camada de Domínio e Dados (Models)"]
        M_User["Usuario"]
        M_Loc["Locadora"]
        M_Item["Item (Console)"]
        M_Locacao["Locacao"]
        M_Servico["ServicoOpcional"]
        M_Hist["Historico"]
        M_Dia["DiaCalendario"]
    end

    subgraph Storage ["Persistência Local"]
        DB[("locadora_videogame.db3")]
    end

    UI -->|DataBinding & Commands| VM
    VM -->|Regras de Negócio & Consultas| Services
    Services -->|Mapeamento Objeto-Relacional| Domain
    S_DB -->|Leitura e Escrita Assíncrona| DB
```

---

## 3. Diagrama de Casos de Uso (UML Use Case)

O diagrama abaixo detalha todos os casos de uso do ecossistema, segmentados pelos 3 perfis de atores:

```mermaid
flowchart LR
    %% Atores
    Admin((Administrador))
    Locadora((Gerente Locadora))
    Cliente((Cliente))

    %% Casos de Uso Administrador
    subgraph UC_Admin ["Módulo de Administração Geral"]
        UC1["Visualizar Dashboard Financeiro Geral"]
        UC2["Cadastrar / Editar Filiais de Locadoras"]
        UC3["Consultar Endereço por CEP (ViaCEP)"]
        UC4["Obter Coordenadas GPS da Unidade"]
        UC5["Gerenciar Catálogo Geral de Consoles"]
        UC6["Excluir Usuários / Locatários"]
        UC7["Restaurar / Limpar Base de Dados (Seed)"]
    end

    %% Casos de Uso Locadora
    subgraph UC_Locadora ["Módulo Operacional da Filial"]
        UC8["Acompanhar Faturamento da Filial"]
        UC9["Cadastrar / Editar Consoles da Própria Loja"]
        UC10["Configurar Planos (Diária, Semanal, Mensal)"]
        UC11["Cadastrar Serviços Opcionais da Loja"]
        UC12["Ativar / Pausar Serviços Opcionais"]
        UC13["Validar Retirada via PIN de 6 Dígitos"]
        UC14["Acompanhar Calendário de Devoluções (14 Dias)"]
        UC15["Registrar Recebimento e Aplicar Taxa de Atraso"]
    end

    %% Casos de Uso Cliente
    subgraph UC_Cliente ["Módulo do Cliente"]
        UC16["Autenticar / Criar Nova Conta"]
        UC17["Explorar Consoles Ordenados por GPS"]
        UC18["Filtrar Consoles por Locadora Específica"]
        UC19["Traçar Rota no Mapa até a Loja"]
        UC20["Ligar para a Unidade"]
        UC21["Configurar Aluguel (Período & Quantidade)"]
        UC22["Selecionar Serviços Opcionais da Unidade"]
        UC23["Escolher Modalidade de Entrega (Balcão / Delivery)"]
        UC24["Gerar PIN de Confirmação de 6 Dígitos"]
        UC25["Acompanhar Retirada via Widget Flutuante / PIN"]
        UC26["Consultar Histórico de Locações"]
    end

    %% Conexões Administrador
    Admin --> UC1
    Admin --> UC2
    Admin --> UC3
    Admin --> UC4
    Admin --> UC5
    Admin --> UC6
    Admin --> UC7

    %% Conexões Locadora
    Locadora --> UC8
    Locadora --> UC9
    Locadora --> UC10
    Locadora --> UC11
    Locadora --> UC12
    Locadora --> UC13
    Locadora --> UC14
    Locadora --> UC15

    %% Conexões Cliente
    Cliente --> UC16
    Cliente --> UC17
    Cliente --> UC18
    Cliente --> UC19
    Cliente --> UC20
    Cliente --> UC21
    Cliente --> UC22
    Cliente --> UC23
    Cliente --> UC24
    Cliente --> UC25
    Cliente --> UC26
```

### Especificação dos Casos de Uso Críticos

| ID | Caso de Uso | Ator Primário | Pré-condição | Fluxo Principal | Pós-condição |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **UC13** | **Validar Retirada com PIN** | Gerente Locadora | Locação no status `AguardandoRetirada` | Gerente digita PIN de 6 dígitos informado pelo cliente no balcão; sistema confere integridade atômica no SQLite | Status da locação muda para `Alugado`, console é marcado como indisponível no catálogo |
| **UC21** | **Configuração da Locação** | Cliente | Console disponível | Cliente define modalidade habilitada pela loja (Diária, Semanal ou Mensal), escolhe quantidade de períodos e revisa data de devolução | Totais de diárias calculados reativamente |
| **UC22** | **Seleção de Opcionais** | Cliente | Loja possuir opcionais ativos | Cliente marca opcionais (ex: controle extra, headset, seguro); sistema atualiza `SubtotalExtras` e `TotalGeral` | Opcionais anexados ao resumo e persistidos |
| **UC11** | **Gestão de Serviços Opcionais** | Gerente Locadora | Usuário autenticado como Locadora | Gerente informa nome, descrição e valor diário; escolhe ícone glyph | Opcional fica disponível no checkout de todos os consoles daquela filial |

---

## 4. Diagrama de Classes (UML Class Diagram)

O diagrama a seguir exibe as relações de herança, associações de domínio, coleções observáveis e métodos de persistência:

```mermaid
classDiagram
    %% Herança e Base
    class BaseViewModel {
        <<abstract>>
        +SetProperty~T~(ref T backingStore, T value, string propertyName) bool
        +OnPropertyChanged(string propertyName) void
        #IsBusy: bool
    }

    %% Modelos de Dados
    class Usuario {
        +int Id
        +string Nome
        +string Cpf
        +string Email
        +string Senha
        +string Tipo
        +string LocadoraVinculadaNome
        +string TipoBadgeBg
        +string TipoBadgeText
    }

    class Locadora {
        +int Id
        +string Nome
        +string Endereco
        +string Telefone
        +string Cep
        +string Rua
        +string Numero
        +string Bairro
        +string Cidade
        +string Estado
        +double Latitude
        +double Longitude
        +double DistanciaKm
        +int QuantidadeConsolesDisponiveis
        +string DistanciaFormatada
        +MontarEnderecoCompleto() string
    }

    class Item {
        +int Id
        +string Nome
        +string ChaveSerial
        +string ImgDiretorio
        +string Desc
        +string ContaVinculada
        +bool PermiteDiaria
        +decimal ValorDiaria
        +bool PermiteSemanal
        +decimal ValorSemanal
        +bool PermiteMensal
        +decimal ValorMensal
        +string Status
        +string LocadoraNome
        +double LocadoraDistanciaKm
        +bool IsDisponivel
    }

    class ServicoOpcional {
        +int Id
        +int LocadoraId
        +string LocadoraNome
        +string Nome
        +string Descricao
        +string IconeGlyph
        +decimal Valor
        +bool Ativo
        +bool Selecionado
        +Action OnSelecaoAlterada
        +string ValorFormatado
        +string StatusTexto
    }

    class StatusLocacaoEnum {
        <<enumeration>>
        AguardandoRetirada
        Alugado
        Finalizado
        Cancelado
    }

    class Locacao {
        +int Id
        +int UsuarioId
        +string UsuarioNome
        +int ItemId
        +string ItemNome
        +string ChaveSerialConsole
        +int LocadoraId
        +string LocadoraNome
        +DateTime DataLocacao
        +DateTime DataDevolucaoPrevista
        +string CodigoConfirmacao
        +StatusLocacaoEnum StatusLocacao
        +decimal ValorTotal
        +decimal TaxaAdicional
        +string ModalidadeAluguel
        +int QuantidadePeriodo
        +string FormaEntrega
        +string DescricaoExtras
        +string CodigoDigitado
    }

    class Historico {
        +int Id
        +int UsuarioId
        +string UsuarioNome
        +int ItemId
        +string ItemNome
        +string ChaveSerialConsole
        +string LocadoraNome
        +string ModalidadeAluguel
        +int QuantidadePeriodo
        +string DescricaoExtras
        +string FormaEntrega
        +DateTime DataHora
        +DateTime DataDevolucaoPrevista
        +decimal ValorTotal
        +decimal TaxaAdicional
        +string StatusLocacao
    }

    class DiaCalendario {
        +DateTime Data
        +string DiaSemana
        +string DiaNumero
        +int QuantidadeDevolucoes
        +bool TemDevolucoes
        +bool IsSelecionado
    }

    %% ViewModels
    class ItemViewModel {
        +ObservableCollection~Item~ Itens
        +ObservableCollection~Item~ ItensDaUnidade
        +ObservableCollection~Item~ ConsolesDisponiveis
        +ObservableCollection~Locadora~ Locadoras
        +ObservableCollection~Locacao~ LocacoesPendentesRetirada
        +ObservableCollection~ServicoOpcional~ ServicosOpcionaisDaUnidade
        +ObservableCollection~DiaCalendario~ DiasCalendario
        +ICommand CarregarDadosClienteCommand
        +ICommand CarregarDadosLocadoraCommand
        +ICommand SalvarNovoServicoOpcionalCommand
        +ICommand ExcluirServicoOpcionalCommand
        +ICommand ValidarCodigoRetiradaCommand
        +ICommand AlugarConsoleCommand
        +ICommand ReceberConsoleCommand
        +CarregarDadosLocadoraAsync() Task
        +CarregarDadosClienteAsync() Task
    }

    class AluguelDetalhesViewModel {
        -Item _console
        +ObservableCollection~ServicoOpcional~ ServicosOpcionaisDisponiveis
        +string ModalidadeSelecionada
        +int QuantidadePeriodo
        +string FormaEntrega
        +decimal SubtotalPlano
        +decimal SubtotalExtras
        +decimal TotalGeral
        +DateTime DataDevolucaoCalculada
        +ICommand SelecionarPlanoCommand
        +ICommand AlternarServicoCommand
        +ICommand ConfirmarLocacaoCommand
    }

    %% Serviços
    class SQLiteDatabaseHelper {
        <<Singleton>>
        -SQLiteAsyncConnection _banco
        +InicializarAsync() Task
        +ObterItensAsync() Task~List~Item~~
        +ObterLocadorasAsync() Task~List~Locadora~~
        +ObterServicosOpcionaisPorLocadoraAsync(string) Task~List~ServicoOpcional~~
        +SalvarServicoOpcionalAsync(ServicoOpcional) Task~int~
        +ValidarCodigoRetiradaAsync(int, string) Task~bool~
        +SalvarLocacaoAsync(Locacao) Task~int~
        +RestaurarBancoDadosAsync() Task
    }

    class GeolocalizacaoService {
        <<Singleton>>
        +CalcularDistanciaKm(lat1, lon1, lat2, lon2) double
        +ObterLocalizacaoAsync() Task~Location~
        +AbrirRotaNoMapaAsync(lat, lon, nome) Task
        +RealizarLigacao(telefone) void
    }

    %% Relações de Herança
    BaseViewModel <|-- ItemViewModel
    BaseViewModel <|-- AluguelDetalhesViewModel
    BaseViewModel <|-- ServicoOpcional

    %% Associações
    Locacao --> StatusLocacaoEnum
    ItemViewModel --> SQLiteDatabaseHelper : utiliza
    ItemViewModel --> GeolocalizacaoService : utiliza
    AluguelDetalhesViewModel --> SQLiteDatabaseHelper : utiliza
    Locadora "1" *-- "0..*" Item : possui
    Locadora "1" *-- "0..*" ServicoOpcional : gerencia
    Locacao --> Item : referencia
    Locacao --> Usuario : referencia
    Locacao --> Locadora : referencia
    Historico --> Item : auditoria
```

---

## 5. Modelo Entidade-Relacionamento (DER / SQLite)

O banco de dados SQLite local (`locadora_videogame.db3`) é estruturado em tabelas normalizadas com chaves primárias e relacionamentos conceituais:

```mermaid
erDiagram
    USUARIOS ||--o{ LOCACOES : "realiza"
    USUARIOS ||--o{ HISTORICOS : "audita"
    LOCADORAS ||--o{ ITENS : "estoca"
    LOCADORAS ||--o{ SERVICOS_OPCIONAIS : "oferece"
    LOCADORAS ||--o{ LOCACOES : "atende"
    ITENS ||--o{ LOCACOES : "alugado_em"
    ITENS ||--o{ HISTORICOS : "registrado_em"

    USUARIOS {
        int Id PK
        string Nome
        string Cpf
        string Email
        string Senha
        string Tipo
        string LocadoraVinculadaNome
    }

    LOCADORAS {
        int Id PK
        string Nome
        string Endereco
        string Telefone
        string Cep
        string Rua
        string Numero
        string Bairro
        string Cidade
        string Estado
        double Latitude
        double Longitude
        double DistanciaKm
    }

    ITENS {
        int Id PK
        string Nome
        string ChaveSerial
        string ImgDiretorio
        string Desc
        string ContaVinculada
        bool PermiteDiaria
        decimal ValorDiaria
        bool PermiteSemanal
        decimal ValorSemanal
        bool PermiteMensal
        decimal ValorMensal
        string Status
        string LocadoraNome
    }

    SERVICOS_OPCIONAIS {
        int Id PK
        int LocadoraId FK
        string LocadoraNome
        string Nome
        string Descricao
        string IconeGlyph
        decimal Valor
        bool Ativo
    }

    LOCACOES {
        int Id PK
        int UsuarioId FK
        string UsuarioNome
        int ItemId FK
        string ItemNome
        string ChaveSerialConsole
        int LocadoraId FK
        string LocadoraNome
        datetime DataLocacao
        datetime DataDevolucaoPrevista
        string CodigoConfirmacao
        int StatusLocacao
        decimal ValorTotal
        decimal TaxaAdicional
        string ModalidadeAluguel
        int QuantidadePeriodo
        int DiasLocados
        string FormaEntrega
        string DescricaoExtras
    }

    HISTORICOS {
        int Id PK
        int UsuarioId FK
        string UsuarioNome
        int ItemId FK
        string ItemNome
        string ChaveSerialConsole
        string LocadoraNome
        string ModalidadeAluguel
        int QuantidadePeriodo
        string DescricaoExtras
        string FormaEntrega
        datetime DataHora
        datetime DataDevolucaoPrevista
        decimal ValorTotal
        decimal TaxaAdicional
        string StatusLocacao
    }
```

---

## 6. Diagrama de Sequência: Handshake de Custódia (PIN de 6 Dígitos)

O mecanismo de segurança inspirado no protocolo de retenção e custódia de plataformas de entrega e transporte (ex: iFood, BlaBlaCar) garante que o console físico só seja entregue após validação do código numérico de 6 dígitos gerado criptograficamente na reserva:

```mermaid
sequenceDiagram
    autonumber
    actor Cliente
    participant ViewCheckout as AluguelDetalhesView
    participant VM_Checkout as AluguelDetalhesViewModel
    participant DB as SQLiteDatabaseHelper
    actor Gerente as Gerente Locadora
    participant ViewLocadora as LocadoraMainView
    participant VM_Locadora as ItemViewModel

    Cliente->>ViewCheckout: Clica em "Confirmar e Alugar Agora"
    ViewCheckout->>VM_Checkout: ExecutarConfirmarLocacaoAsync()
    
    Note over VM_Checkout: Gera PIN numérico aleatório de 6 dígitos:<br/>RandomNumberGenerator.GetInt32(100000, 1000000)<br/>Ex: "849201"
    
    VM_Checkout->>DB: SalvarLocacaoAsync(locacao: AguardandoRetirada)
    VM_Checkout->>DB: AtualizarStatusConsoleAsync(Id, "Reservado")
    VM_Checkout->>DB: CriarRegistroHistoricoAsync()
    
    DB-->>VM_Checkout: Confirmação de Persistência
    VM_Checkout-->>Cliente: Exibe Alerta com o PIN e orientações de retirada
    
    Note over Cliente, Gerente: O cliente se desloca até o balcão físico da filial da Locadora
    
    Cliente->>Gerente: Informa verbalmente ou exibe o PIN de 6 dígitos
    Gerente->>ViewLocadora: Digita o PIN no card da reserva pendente
    ViewLocadora->>VM_Locadora: ValidarCodigoRetiradaCommand(locacao)
    
    VM_Locadora->>DB: ValidarCodigoRetiradaAsync(locacaoId, "849201")
    
    alt Código Correto e Status == AguardandoRetirada
        DB->>DB: Update Locacao Set StatusLocacao = Alugado
        DB->>DB: Update Itens Set Status = 'Alugado'
        DB->>DB: Update Historicos Set StatusLocacao = 'Em Andamento'
        DB-->>VM_Locadora: Retorna true (Sucesso)
        VM_Locadora-->>ViewLocadora: Remove item de LocacoesPendentesRetirada
        VM_Locadora-->>Gerente: Exibe alerta: "Retirada Autorizada! ✅ Entregar console."
        Note over Gerente, Cliente: O gerente entrega o aparelho e acessórios ao cliente com segurança total
    else Código Incorreto ou Divergente
        DB-->>VM_Locadora: Retorna false
        VM_Locadora-->>Gerente: Exibe alerta: "Código Incorreto! ❌ Solicitar conferência no app."
    end
```

---

## 7. Diagrama de Sequência: Gestão e Seleção de Serviços Opcionais

Fluxo que permite à unidade cadastrar serviços e adicionais customizados e ao cliente selecioná-los no checkout:

```mermaid
sequenceDiagram
    autonumber
    actor Gerente as Gerente Locadora
    participant ViewLoc as LocadoraMainView
    participant VM_Item as ItemViewModel
    participant DB as SQLiteDatabaseHelper
    actor Cliente
    participant ViewCli as AluguelDetalhesView
    participant VM_Aluguel as AluguelDetalhesViewModel

    Note over Gerente, ViewLoc: 1. Gestão e Cadastro do Serviço Opcional pela Filial
    Gerente->>ViewLoc: Clica em "+ Novo Serviço"
    ViewLoc->>VM_Item: AbrirFormularioNovoServicoCommand
    Gerente->>ViewLoc: Preenche Nome ("Controle Extra DualSense"), Valor ("25,00")
    Gerente->>ViewLoc: Clica em "Salvar Serviço"
    VM_Item->>DB: SalvarServicoOpcionalAsync(novoServico com LocadoraId e LocadoraNome)
    DB-->>VM_Item: Sucesso
    VM_Item->>ViewLoc: Atualiza ServicosOpcionaisDaUnidade na interface

    Note over Cliente, ViewCli: 2. Exibição e Seleção Dinâmica no Checkout pelo Cliente
    Cliente->>ViewCli: Abre tela de locação do PlayStation 5 desta unidade
    ViewCli->>VM_Aluguel: CarregarServicosOpcionaisAsync()
    VM_Aluguel->>DB: ObterServicosOpcionaisPorLocadoraAsync("GameZone Express")
    DB-->>VM_Aluguel: Retorna apenas os opcionais cadastrados por esta unidade
    VM_Aluguel-->>ViewCli: Popula ServicosOpcionaisDisponiveis com switches

    Cliente->>ViewCli: Ativa o Switch "Controle Extra DualSense (+R$ 25,00)"
    ViewCli->>VM_Aluguel: servico.Selecionado = true (OnSelecaoAlterada)
    VM_Aluguel->>VM_Aluguel: AtualizarRecalculoCompleto()
    Note over VM_Aluguel: Recalcula SubtotalExtras (Sum dos selecionados) e TotalGeral
    VM_Aluguel-->>ViewCli: Atualiza Total Geral na barra de checkout em tempo real
```

---

## 8. Diagrama de Máquina de Estados: Ciclo de Vida da Locação

Controle estrito dos estados da locação definidos pelo enum `StatusLocacaoEnum`:

```mermaid
stateDiagram-v2
    [*] --> AguardandoRetirada: Cliente conclui checkout no app (PIN gerado)
    
    AguardandoRetirada --> Alugado: Gerente valida PIN de 6 dígitos no balcão da filial
    AguardandoRetirada --> Cancelado: Cliente ou Administrador cancela a reserva
    
    Alugado --> Finalizado: Console devolvido fisicamente no balcão (Baixa registrada)
    
    note right of AguardandoRetirada
        O console fica bloqueado no catálogo
        PIN de 6 dígitos ativo no app do cliente
    end note
    
    note right of Alugado
        Aparelho em posse do cliente
        Data limite monitorada no Calendário
    end note

    note right of Finalizado
        Console retorna para 'Disponível' no estoque
        Possibilidade de aplicação de taxa de atraso
    end note

    Finalizado --> [*]
    Cancelado --> [*]
```

---

## 9. Matriz de Permissões e Perfis de Acesso (RBAC)

O sistema conta com autenticação e autorização por perfil (`Usuario.Tipo`), garantindo isolamento total de recursos:

| Funcionalidade / Recurso | Administrador (`Admin`) | Gerente Locadora (`Locadora`) | Cliente (`Cliente`) |
| :--- | :---: | :---: | :---: |
| **Dashboard Consolidado da Rede** | ✅ Total | ❌ | ❌ |
| **Dashboard Operacional da Unidade** | ❌ (Redirecionado) | ✅ Apenas sua filial | ❌ |
| **Cadastro e Edição de Filiais** | ✅ Total | ❌ | ❌ |
| **Consulta de Endereço por CEP (ViaCEP)** | ✅ Sim | ❌ | ❌ |
| **Cadastro de Consoles no Estoque** | ✅ Para qualquer loja | ✅ Apenas para sua loja | ❌ |
| **Definição de Preços (Diária/Semanal/Mensal)**| ✅ Sim | ✅ Sim | ❌ |
| **Cadastro de Serviços Opcionais** | ❌ | ✅ Exclusivo de sua filial | ❌ |
| **Validação de Retirada (PIN 6 Dígitos)** | ❌ | ✅ Exclusivo de sua filial | ❌ |
| **Baixa de Devoluções / Calendário 14 Dias** | ❌ | ✅ Exclusivo de sua filial | ❌ |
| **Catálogo de Consoles por GPS mais Próximo** | ❌ | ❌ | ✅ Sim |
| **Seleção de Opcionais & Checkout** | ❌ | ❌ | ✅ Sim |
| **Widget Flutuante de Retirada (PIN)** | ❌ | ❌ | ✅ Sim |
| **Histórico de Locações** | ✅ Geral | ✅ Da filial | ✅ Próprias locações |
| **Restaurar Banco de Dados (Seed Inicial)** | ✅ Sim | ❌ | ❌ |

---

## 10. Módulos e Serviços de Infraestrutura

### 10.1. `SQLiteDatabaseHelper` (Persistência Transacional)
- **Tecnologia**: Pacote NuGet `sqlite-net-pcl`.
- **Padrão**: Singleton assíncrono com conexão thread-safe compartilhada (`_banco`).
- **Resiliência**: Tratamento de exceções, criação automática de índices por chave primária, seeds controlados por contagem inicial (`CountAsync() == 0`).
- **Caminho Físico**: `FileSystem.AppDataDirectory/locadora_videogame.db3`.

### 10.2. `GeolocalizacaoService` (Cálculo Espacial e Mapas)
- **Fórmula de Haversine**:
  $$\Delta\sigma = 2 \cdot \arcsin\left(\sqrt{\sin^2\left(\frac{\Delta\phi}{2}\right) + \cos(\phi_1)\cos(\phi_2)\sin^2\left(\frac{\Delta\lambda}{2}\right)}\right)$$
  $$d = R \cdot \Delta\sigma \quad (\text{onde } R = 6.371\text{ km})$$
- **Integração com Sistema Operacional**: Disparo nativo de rotas no aplicativo de mapas padrão (`Map.Default.OpenAsync`) e discagem telefônica direta (`PhoneDialer.Default.Open`).

### 10.3. `ViaCepService` (Automação Cadastral)
- Consumo assíncrono da API aberta `https://viacep.com.br/ws/{cep}/json/` via `HttpClient`.
- Extração de Logradouro, Bairro, Localidade e UF com sanitização de caracteres não-numéricos.

### 10.4. `ImageService` (Gerenciamento de Ativos Gráficos)
- Interface de seleção nativa (`FilePicker.PickAsync`) para imagens `.png`, `.jpg`, `.jpeg` e `.webp`.
- Cópia segura e persistência no diretório de dados da aplicação (`FileSystem.AppDataDirectory`), desacoplando o sistema de caminhos temporários voláteis.

---

<div align="center">

**Projeto Desenvolvido com Padrões de Engenharia de Software de Alto Nível**  
*Documentação técnica homologada para fins acadêmicos, de portfólio e auditoria de código.*

</div>
