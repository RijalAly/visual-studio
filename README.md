# Sentinela — IA para detecção de transacções suspeitas

Aplicação de ambiente de trabalho desenvolvida em C# com Windows Forms, .NET 8, MySQL e ML.NET. O objectivo é analisar operações financeiras e assinalar comportamentos fora do padrão para revisão humana.

## Tecnologias

- C# e Windows Forms sobre .NET 8
- MySQL 8 ou MariaDB do XAMPP
- MySqlConnector 2.6.2
- Microsoft.ML 5.0.1
- Microsoft.ML.FastTree 5.0.1
- System.Security.Cryptography.ProtectedData 8.0.0

As versões foram verificadas na NuGet Gallery. O projecto não utiliza APIs externas nem serviços na nuvem durante a análise.

## Funcionalidades

- Configuração e teste da ligação MySQL, com criação automática do esquema e das tabelas.
- Início de sessão com palavras-passe protegidas por PBKDF2/SHA-256.
- Cifração local da palavra-passe de ligação com Windows DPAPI.
- Cadastro de clientes e gestão de transacções.
- Geração de clientes e transacções de demonstração em MZN.
- Preparação dos dados, validação, remoção de duplicados e contagem das classes.
- Treino e comparação de FastTree e regressão logística (SDCA).
- Divisão 80/20, semente fixa, exactidão, precisão, recall, F1, AUC e matriz de confusão.
- Selecção do modelo por F1 e AUC como critério de desempate.
- Persistência e carregamento automático do modelo.
- Análise manual com frequência calculada a partir do histórico do cliente.
- Revisão de alertas e reaproveitamento das decisões no treino seguinte.
- Exportação do histórico para CSV.

## Abrir o projecto

1. Instale o Visual Studio 2022 Community com a carga de trabalho Desenvolvimento de ambiente de trabalho .NET e o SDK do .NET 8.
2. Instale e inicie o MySQL Server ou o MariaDB do XAMPP.
3. Abra o ficheiro Sentinela.sln.
4. Restaure os pacotes NuGet.
5. Carregue em F5.
6. Configure o acesso ao MySQL quando a janela de ligação aparecer.

## Contas iniciais de demonstração

Se a tabela utilizadores estiver vazia, são criadas as contas abaixo:

| Perfil | Utilizador | Palavra-passe |
|---|---|---|
| Administrador | admin | Sentinela#2026 |
| Analista | analista | Analista#2026 |

As credenciais são apenas para demonstração. Altere-as antes de qualquer utilização fora do ambiente académico.

## Documentação

- LEIA-ME.md — requisitos, instalação, execução e problemas comuns.
- docs/GUIA_PRINCIPIANTE.md — instruções para o estudante e roteiro de apresentação de cinco minutos.
- docs/EXPLICACAO_CODIGO.md — explicação das classes e do percurso dos dados.
- sentinela_ia.sql — estrutura da base de dados e oito consultas para a apresentação.

## Limitações e responsabilidade

Os dados de demonstração são fictícios e foram criados segundo padrões definidos para este projecto. O modelo aprende esses padrões; as métricas obtidas não provam que detectaria fraude real num banco. O Sentinela apresenta alertas para análise posterior e nunca confirma que uma transacção é fraude.

## Verificação da compilação

Foi adicionada uma rotina GitHub Actions para restaurar os pacotes e compilar a solução num executor Windows com .NET 8. A execução final da aplicação também deve ser confirmada localmente com MySQL ou MariaDB activo.

## Descarregar o código ou a aplicação

- **Código-fonte:** abra o repositório e escolha **Code → Download ZIP**. Extraia a pasta e abra `Sentinela.sln` no Visual Studio.
- **Aplicação Windows:** depois de uma execução bem-sucedida do GitHub Actions, abra [Compilar Sentinela](https://github.com/RijalAly/visual-studio/actions/workflows/build.yml), escolha a execução mais recente e descarregue o artefacto `Sentinela-Windows-x64`.

A versão publicada é dependente do .NET 8 Desktop Runtime e precisa de uma instância MySQL ou MariaDB activa. O pacote não inclui um servidor de base de dados.

