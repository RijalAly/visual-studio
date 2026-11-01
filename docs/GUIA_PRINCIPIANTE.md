# Guia do principiante — Sentinela

## Objectivo

Este guia explica como preparar o computador, abrir o projecto, utilizar os ecrãs e apresentar o trabalho.

## A. Preparar o computador

### Instalar o Visual Studio 2022 Community

1. Abra o instalador do Visual Studio.
2. Seleccione a carga de trabalho Desenvolvimento de ambiente de trabalho .NET.
3. Confirme que o SDK do .NET 8 está incluído.
4. Termine a instalação e abra o Visual Studio.

### Preparar a base de dados

Pode utilizar o XAMPP ou o MySQL Server. No XAMPP, abra o Control Panel e carregue em Start na linha MySQL. O serviço deve ficar activo.

A aplicação cria automaticamente a base de dados sentinela_ia e as tabelas na primeira ligação. O ficheiro sentinela_ia.sql permite criar a estrutura manualmente.

### Abrir o projecto

1. Descarregue o repositório RijalAly/visual-studio.
2. Extraia o ZIP para uma pasta do computador.
3. Abra Sentinela.sln.
4. Aguarde a restauração dos pacotes.
5. Carregue em F5.

Se a janela de ligação aparecer, preencha os dados do MySQL. Para uma instalação XAMPP sem palavra-passe root, utilize localhost, porta 3306, utilizador root e deixe a palavra-passe vazia.

## B. Iniciar sessão

A aplicação cria duas contas quando a tabela utilizadores ainda está vazia:

- Administrador: utilizador admin, palavra-passe inicial Sentinela#2026.
- Analista: utilizador analista, palavra-passe inicial Analista#2026.

São credenciais de demonstração.

## C. Percorrer os ecrãs

### Painel

Apresenta o número de transacções registadas, o número de classificadas como potencialmente suspeitas, a taxa calculada e os alertas pendentes. O gráfico resume a actividade dos últimos 14 dias.

### Analisar transacção

1. Escolha um cliente.
2. Consulte a média de operação e a cidade habitual.
3. Introduza o valor em MZN.
4. Escolha a data/hora, tipo, canal e cidade.
5. Assinale se o beneficiário é novo.
6. Verifique o número de operações na última hora. Esse valor é consultado na base de dados.
7. Carregue em Analisar transacção.

O resultado apresenta Normal, Atenção ou Transacção potencialmente suspeita, a probabilidade, a confiança e os factores de atenção. A operação pode ser guardada no histórico.

Os exemplos preenchem cenários didácticos. A classificação continua a ser produzida pelo modelo treinado.

### Transacções

A lista permite pesquisar por nome ou conta e filtrar por classificação e revisão. Seleccione uma linha para confirmar suspeita, marcar falso alerta ou repor pendente. O duplo clique apresenta detalhes. A exportação cria um CSV compatível com o Excel.

### Clientes

Pode criar, editar e eliminar clientes. A aplicação avisa que eliminar um cliente também elimina as transacções ligadas a esse cliente.

### Modelo de IA

1. Defina o número de clientes e transacções. Os valores iniciais são 40 e 6 000.
2. Carregue em Gerar dados simulados.
3. Aguarde a mensagem de conclusão.
4. Carregue em Treinar / reaprender.
5. Consulte os registos recebidos, inválidos, repetidos e usados.
6. Compare os algoritmos e consulte as métricas calculadas.
7. No ecrã Transacções, reveja alguns alertas e volte a treinar para incorporar as decisões.
8. A opção Eliminar dados simulados remove os clientes simulados e as transacções associadas.

O treino é executado em segundo plano. O modelo fica guardado em Documentos\Sentinela\modelo.zip.

### Sobre e ajuda

Apresenta o contexto e as limitações do projecto. Explique que dados simulados não provam desempenho em situações bancárias reais.

## D. Como interpretar as métricas

- Exactidão: percentagem de todas as previsões que estão correctas.
- Precisão: dos casos identificados como suspeitos pelo modelo, quantos são suspeitos segundo a etiqueta do teste.
- Recall: dos casos suspeitos no conjunto de teste, quantos foram identificados.
- F1: medida que equilibra precisão e recall.
- AUC: mede a capacidade de distinguir as duas classes em diferentes limiares.
- VP: suspeito correctamente identificado.
- FP: normal identificado incorrectamente como suspeito.
- VN: normal correctamente identificado.
- FN: suspeito que o modelo não identificou.

Quando existem muito mais transacções normais do que suspeitas, a exactidão pode parecer elevada mesmo que o modelo falhe muitos casos suspeitos. Por isso, também devem ser observados precisão, recall, F1, AUC e matriz de confusão.

## E. Roteiro de demonstração de cinco minutos

**Minuto 1 — Apresentação**
Explique o problema: muitas transacções podem ser analisadas num curto período. O Sentinela procura comportamentos fora do padrão e apresenta alertas para revisão humana.

**Minuto 2 — Dados**
Abra Modelo de IA e mostre a criação de clientes e transacções fictícias. Explique que os registos incluem valor, data/hora, localização, tipo, canal, beneficiário novo e frequência.

**Minuto 3 — Treino**
Carregue em Treinar / reaprender. Mostre os registos recebidos, inválidos, repetidos e utilizados. Explique que o programa compara FastTree com regressão logística.

**Minuto 4 — Análise**
Abra Analisar transacção. Utilize um exemplo normal e outro com vários sinais de atenção. Mostre a classificação, a probabilidade e a explicação dos factores.

**Minuto 5 — Revisão e aprendizagem**
Abra Transacções, marque um alerta como Confirmada ou Descartada, volte a Modelo de IA e treine novamente. Termine mostrando o histórico de treino e o script SQL.

## F. Limites que devem ser referidos

Os dados são simulados e foram criados segundo padrões definidos para a experiência. O modelo aprende esses padrões; as métricas não provam que detectaria fraude real. Uma operação suspeita é apenas um alerta para avaliação por uma pessoa.
## G. Funções acrescentadas na interface

**Pesquisar e filtrar operações:** no ecrã Transacções, introduza nome, número de conta ou ID. Pode combinar os filtros de classificação e revisão com um período e intervalo de valores. A exportação CSV contém apenas os resultados visíveis.

**Rever e justificar um alerta:** seleccione uma operação e carregue em Rever operação. Escolha Pendente, Confirmada ou Descartada. Para confirmar ou descartar, escreva uma justificação com pelo menos oito caracteres. O histórico de auditoria identifica o analista, a data/hora, o estado anterior, o novo estado e a observação.

**Consultar auditoria:** seleccione a operação e abra Histórico. A janela apresenta todas as decisões registadas para aquela operação.

**Alterar a palavra-passe:** abra Conta e segurança, introduza a palavra-passe actual e escolha a nova. A nova palavra-passe exige pelo menos dez caracteres, uma maiúscula, uma minúscula e um número.

**Limpar dados de demonstração:** no ecrã Modelo de IA, a opção de eliminação remove as operações simuladas. Os registos manuais são preservados; clientes de demonstração que ainda tenham operações associadas permanecem na base de dados.
