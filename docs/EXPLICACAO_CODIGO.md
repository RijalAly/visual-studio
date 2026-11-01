# Explicação do código — Sentinela

## 1. Organização geral

O código está na pasta src/Sentinela. A aplicação separa a interface, os dados, a lógica de Inteligência Artificial, os modelos e as ferramentas auxiliares.

### Ficheiros de arranque

**Program.cs** — define a cultura pt-PT, prepara o Windows Forms, lê a configuração, testa a ligação ao MySQL, cria as tabelas e abre o formulário de início de sessão. Também cria uma única instância de ServicoIA para toda a aplicação.

**Sentinela.csproj** — define que a aplicação é Windows Forms em net8.0-windows e declara os pacotes NuGet.

**app.manifest** — contém as definições de execução e de escala DPI do Windows.

**Sentinela.sln** — solução que abre o projecto no Visual Studio.

## 2. Modelos

As classes desta pasta representam os dados guardados ou utilizados pela aplicação.

**Cliente.cs** — representa um cliente com nome, número de conta, cidade habitual e média de valor por operação.

**Transaccao.cs** — representa uma operação financeira, incluindo valor, data/hora, tipo, canal, cidade, beneficiário, frequência, classificação, probabilidade e estado de revisão.

**Utilizador.cs** — representa a conta que iniciou sessão e o respectivo perfil.

**DadosIA.cs** — contém os campos que o modelo de aprendizagem automática recebe. A propriedade Label é a resposta conhecida usada durante o treino.

**ResultadoTreino.cs** — guarda as métricas e a matriz de confusão calculadas no conjunto de teste.

**ResultadoAnalise.cs** — contém a probabilidade, a confiança, a classificação final e a lista de factores de atenção apresentados no ecrã.

## 3. Util

**ConfiguracaoApp.cs** — lê e guarda o servidor, a porta, o utilizador e a base de dados. A palavra-passe é protegida com Windows DPAPI antes de ser guardada no computador.

**Seguranca.cs** — cria um sal aleatório e calcula o hash da palavra-passe com PBKDF2, SHA-256 e 100 000 iterações. A verificação compara os hashes em tempo constante.

## 4. Dados

**ConexaoBD.cs** — constrói a cadeia de ligação e abre ligações MySQL. O nome da base de dados é validado antes de ser usado como identificador.

**BaseDados.cs** — cria as tabelas quando ainda não existem e prepara as contas iniciais se a tabela de utilizadores estiver vazia.

**UtilizadorDAO.cs** — procura um utilizador e valida a palavra-passe através da classe Seguranca.

**ClienteDAO.cs** — lista, pesquisa por identificador, insere, actualiza e elimina clientes.

**TransaccaoDAO.cs** — insere transacções em lote, calcula o número de operações do cliente na hora anterior, lista o histórico, guarda revisões, obtém os dados etiquetados para treino e grava as classificações calculadas pelo modelo.

**TreinoDAO.cs** — grava e consulta as métricas de cada treino.

**GestaoDadosDAO.cs** — elimina os clientes simulados e as transacções associadas quando o utilizador pede essa operação.

DAO significa Data Access Object. É uma classe responsável por comunicar com a base de dados. Os formulários chamam métodos DAO em vez de escreverem consultas SQL directamente.

## 5. Inteligencia

**GeradorDados.cs** — cria clientes e transacções fictícias. Cerca de 7% das operações recebem a etiqueta de suspeita. Os padrões são usados para construir o conjunto de demonstração, não para decidir a previsão final.

**PreparadorDados.cs** — verifica os dados, rejeita valores inválidos, remove linhas repetidas e conta as classes normal e suspeita.

**ResultadoPreparacao.cs** — transporta os dados preparados e os totais de validação.

**ServicoIA.cs** — é o serviço central de aprendizagem automática. Prepara as categorias com codificação one-hot, cria as características, divide os dados em 80% para treino e 20% para teste, treina FastTree e regressão logística, calcula métricas e escolhe o modelo com melhor F1. Em caso de empate, utiliza AUC. Guarda e carrega o modelo e executa previsões.

**ExplicadorFactores.cs** — cria textos simples para ajudar a pessoa a interpretar as características da transacção. Estes textos não calculam a probabilidade nem substituem o modelo.

## 6. Controlos

**Tema.cs** — centraliza as cores, fontes, estilos dos botões, títulos e cartões. Isto evita que cada formulário tenha uma paleta diferente.

## 7. Ecras

**FormularioLigacao.cs** — permite configurar e testar o acesso ao MySQL.

**FormularioLogin.cs** — recolhe o utilizador e a palavra-passe e inicia a sessão quando os dados são válidos.

**FormularioPrincipal.cs** — contém a navegação lateral e abre as páginas de trabalho.

**FormularioCliente.cs** — recolhe os campos de um cliente para criar ou editar o registo.

**PaginaPainel.cs** — apresenta indicadores gerais, actividade recente e alertas.

**PaginaAnalise.cs** — recolhe os dados de uma operação, calcula a frequência no histórico e pede uma previsão ao serviço de IA.

**PaginaTransaccoes.cs** — mostra o histórico, aplica filtros, apresenta detalhes, guarda revisões e exporta CSV.

**PaginaClientes.cs** — gere os registos de clientes através de ClienteDAO.

**PaginaModeloIA.cs** — permite gerar dados, eliminar dados simulados, preparar os registos e executar o treino sem bloquear a janela.

**PaginaSobre.cs** — apresenta o objectivo académico e as limitações do sistema.

## 8. Percurso dos dados

1. O gerador cria clientes e transacções fictícias e guarda-os no MySQL.
2. O preparador lê as etiquetas conhecidas e valida os registos.
3. O serviço de IA divide os dados em treino e teste.
4. Os dois algoritmos são treinados e avaliados.
5. O modelo com melhor F1 (AUC como desempate) é guardado em Documentos\Sentinela\modelo.zip.
6. A análise manual envia as características para o modelo e recebe a probabilidade.
7. A revisão humana fica gravada na coluna Revisao.
8. No treino seguinte, uma revisão Confirmada ou Descartada substitui a etiqueta simulada daquela transacção.

## 9. O que significa a previsão

A probabilidade é calculada pelo modelo ML.NET treinado. O ecrã apresenta três níveis: abaixo de 25% — Normal; de 25% até menos de 50% — Atenção; a partir de 50% — Transacção potencialmente suspeita.

A confiança é a probabilidade da classe prevista. Os factores de atenção são informação explicativa. O programa não afirma que uma transacção é fraude e não substitui a revisão humana.

## 10. Limitação de validação

A estrutura e os ficheiros devem ser confirmados por compilação no Visual Studio 2022 num Windows com SDK .NET 8 e MySQL/MariaDB activo. As métricas só devem ser apresentadas depois de o programa concluir um treino real.
## 11. Melhorias recentes da aplicação

**CartaoMetrica.cs** — componente reutilizável para apresentar indicadores em cartões com cor, ícone, valor e descrição.

**DashboardDAO.cs / DadosPainel.cs** — consultas agregadas para o painel: contagens, volume diário, actividade dos últimos 30 dias e alertas recentes. Evitam carregar toda a tabela de transacções só para calcular os indicadores.

**RevisaoDAO.cs / RevisaoHistorico.cs** — regista a decisão do analista numa transacção de base de dados, guarda o estado anterior, o novo estado, o utilizador, a data/hora e a justificação; também consulta o histórico.

**FormularioRevisao.cs** — formulário para decidir o estado e escrever a justificação.

**FormularioHistoricoRevisoes.cs** — apresenta as decisões anteriores de uma operação.

**FormularioDetalheTransaccao.cs** — mostra os dados principais, probabilidade e factores de atenção de uma operação.

**PaginaConta.cs** — permite ao utilizador alterar a própria palavra-passe, confirmando a actual e cumprindo requisitos mínimos.

**TransaccaoDAO.InserirComPredicao** — grava uma nova operação juntamente com a probabilidade e classificação já calculadas, sem voltar a classificar o histórico inteiro após cada análise manual.

**PaginaTransaccoes.cs** — acrescenta filtros por texto, classificação, estado, período e intervalo de valores, auditoria e exportação apenas do resultado filtrado.

**PaginaClientes.cs** — acrescenta pesquisa, filtro dos registos simulados e exportação CSV.

**PaginaModeloIA.cs** — apresenta o histórico comparativo dos algoritmos e as métricas dos treinos anteriores.

A tabela revisoes_transaccao é criada automaticamente por BaseDados.cs e também está documentada no script sentinela_ia.sql.
