# LEIA-ME — Sentinela

## Requisitos

- Windows 10 ou Windows 11 de 64 bits.
- Visual Studio 2022 Community com Desenvolvimento de ambiente de trabalho .NET.
- SDK do .NET 8.
- MySQL 8 Community Server ou MariaDB do XAMPP.
- Internet na primeira restauração dos pacotes NuGet.

Pacotes usados: MySqlConnector 2.6.2, Microsoft.ML 5.0.1, Microsoft.ML.FastTree 5.0.1 e System.Security.Cryptography.ProtectedData 8.0.0.

## Preparar o MySQL

No XAMPP, abra o Control Panel e carregue em Start na linha MySQL. Confirme que está a usar a porta 3306.

No MySQL Server, inicie o serviço e tenha disponíveis as credenciais de uma conta com permissão para criar a base de dados e as tabelas.

A primeira execução cria automaticamente a base de dados sentinela_ia. Em alternativa, execute o ficheiro sentinela_ia.sql com uma conta administrativa.

## Abrir e executar

1. Descarregue o repositório e extraia a pasta.
2. Abra Sentinela.sln no Visual Studio 2022.
3. Aguarde a restauração automática dos pacotes.
4. Confirme que Sentinela é o projecto de arranque.
5. Carregue em F5.
6. Configure a ligação ao MySQL quando solicitado.

Com XAMPP sem palavra-passe root, a configuração habitual é servidor localhost, porta 3306, utilizador root e palavra-passe vazia.

Comandos alternativos no Developer PowerShell:

    dotnet restore Sentinela.sln
    dotnet build Sentinela.sln --configuration Release

## Credenciais de demonstração

As contas são criadas apenas se a tabela utilizadores estiver vazia:

- admin — Sentinela#2026 — Administrador.
- analista — Analista#2026 — Analista.

Estas palavras-passe são exemplos académicos. Não as reutilize noutros serviços.

## Utilização

Painel: mostra totais, taxa de alertas e actividade recente.

Analisar transacção: escolha o cliente e introduza os dados. A frequência na última hora é consultada no histórico do cliente. Treine o modelo antes de efectuar previsões.

Transacções: pesquise, filtre, abra detalhes, reveja alertas e exporte CSV.

Clientes: crie, edite ou elimine clientes. A eliminação de um cliente apaga também as transacções associadas.

Modelo de IA: gere os dados simulados, treine os dois algoritmos, consulte as métricas e elimine os clientes simulados e respectivas operações.

Sobre e ajuda: apresenta o objectivo e as limitações.

## Ficheiros locais

A ligação fica em Documentos\Sentinela\ligacao.json e o modelo em Documentos\Sentinela\modelo.zip. A palavra-passe da ligação é cifrada com Windows DPAPI para o utilizador actual.

## Resolução de problemas

**Falha na ligação MySQL:** confirme se o serviço está iniciado, se servidor e porta estão correctos e se a palavra-passe está certa.

**Poucos dados para treinar:** gere os dados simulados e confirme que existem etiquetas de ambas as classes.

**Modelo ainda não treinado:** abra Modelo de IA e carregue em Treinar / reaprender.

**Falha ao restaurar pacotes:** confirme a Internet e o SDK .NET 8. No Visual Studio, use a opção Restaurar pacotes NuGet da solução.

## Limitação

Os dados e padrões de treino são simulados. As métricas são calculadas pelo programa sobre o conjunto de teste e não provam desempenho com fraude bancária real. O resultado é apoio à decisão e precisa de revisão humana.
