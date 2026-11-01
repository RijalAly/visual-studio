-- Sentinela: estrutura da base de dados
-- Compatível com MySQL 8 e MariaDB do XAMPP.

CREATE DATABASE IF NOT EXISTS sentinela_ia
    CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE sentinela_ia;

CREATE TABLE IF NOT EXISTS utilizadores (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    NomeUtilizador VARCHAR(60) NOT NULL UNIQUE,
    NomeCompleto VARCHAR(140) NOT NULL,
    Sal CHAR(24) NOT NULL,
    HashSenha CHAR(44) NOT NULL,
    Perfil VARCHAR(30) NOT NULL,
    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS clientes (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(140) NOT NULL,
    NumeroConta VARCHAR(40) NOT NULL UNIQUE,
    CidadeHabitual VARCHAR(80) NOT NULL,
    MediaValor DECIMAL(15,2) NOT NULL,
    Simulado BOOLEAN NOT NULL DEFAULT FALSE,
    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS transaccoes (
    Id BIGINT AUTO_INCREMENT PRIMARY KEY,
    ClienteId INT NOT NULL,
    DataHora DATETIME NOT NULL,
    Valor DECIMAL(15,2) NOT NULL,
    Tipo VARCHAR(35) NOT NULL,
    Canal VARCHAR(35) NOT NULL,
    Cidade VARCHAR(80) NOT NULL,
    CidadeDiferente BOOLEAN NOT NULL,
    BeneficiarioNovo BOOLEAN NOT NULL,
    OperacoesUltimaHora INT NOT NULL,
    RelacaoMedia FLOAT NOT NULL,
    Pontuacao DOUBLE NULL,
    Classificacao VARCHAR(50) NOT NULL DEFAULT 'Por analisar',
    Revisao VARCHAR(20) NOT NULL DEFAULT 'Pendente',
    Origem VARCHAR(20) NOT NULL,
    Etiqueta TINYINT NULL,
    CONSTRAINT fk_transaccoes_clientes
        FOREIGN KEY (ClienteId) REFERENCES clientes(Id) ON DELETE CASCADE,
    INDEX ix_transaccoes_cliente_data (ClienteId, DataHora),
    INDEX ix_transaccoes_data (DataHora),
    INDEX ix_transaccoes_revisao (Revisao, Classificacao)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS treinos (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    DataHora DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Algoritmo VARCHAR(60) NOT NULL,
    Escolhido BOOLEAN NOT NULL,
    NumExemplos INT NOT NULL,
    NumRevisoes INT NOT NULL,
    NumTeste INT NOT NULL,
    Exactidao DOUBLE NOT NULL,
    Precisao DOUBLE NOT NULL,
    Recall DOUBLE NOT NULL,
    F1 DOUBLE NOT NULL,
    Auc DOUBLE NOT NULL,
    VP INT NOT NULL,
    FP INT NOT NULL,
    VN INT NOT NULL,
    FN INT NOT NULL,
    Duracao DOUBLE NOT NULL
) ENGINE=InnoDB;

-- 1. Quantidade total de clientes e transacções.
SELECT (SELECT COUNT(*) FROM clientes) AS TotalClientes,
       (SELECT COUNT(*) FROM transaccoes) AS TotalTransaccoes;

-- 2. Transacções mais recentes.
SELECT t.Id, c.Nome, c.NumeroConta, t.DataHora, t.Valor, t.Tipo, t.Cidade,
       t.Classificacao, t.Pontuacao, t.Revisao
FROM transaccoes t
INNER JOIN clientes c ON c.Id = t.ClienteId
ORDER BY t.DataHora DESC
LIMIT 20;

-- 3. Operações classificadas como potencialmente suspeitas.
SELECT t.Id, c.Nome, t.DataHora, t.Valor, t.Cidade, t.Pontuacao
FROM transaccoes t
INNER JOIN clientes c ON c.Id = t.ClienteId
WHERE t.Classificacao = 'Transacção potencialmente suspeita'
ORDER BY t.Pontuacao DESC;

-- 4. Alertas que ainda aguardam revisão.
SELECT t.Id, c.Nome, c.NumeroConta, t.DataHora, t.Valor, t.Pontuacao
FROM transaccoes t
INNER JOIN clientes c ON c.Id = t.ClienteId
WHERE t.Revisao = 'Pendente'
  AND t.Classificacao = 'Transacção potencialmente suspeita'
ORDER BY t.DataHora DESC;

-- 5. Resultado das revisões feitas pelos analistas.
SELECT Revisao, COUNT(*) AS Quantidade
FROM transaccoes
GROUP BY Revisao
ORDER BY Quantidade DESC;

-- 6. Distribuição das classificações por cidade.
SELECT Cidade, Classificacao, COUNT(*) AS Quantidade
FROM transaccoes
GROUP BY Cidade, Classificacao
ORDER BY Cidade, Quantidade DESC;

-- 7. Clientes com maior volume de operações registadas.
SELECT c.Nome, c.NumeroConta, COUNT(t.Id) AS NumeroOperacoes,
       SUM(t.Valor) AS ValorAcumulado
FROM clientes c
LEFT JOIN transaccoes t ON t.ClienteId = c.Id
GROUP BY c.Id, c.Nome, c.NumeroConta
ORDER BY NumeroOperacoes DESC
LIMIT 10;

-- 8. Histórico de treino e comparação de algoritmos.
SELECT DataHora, Algoritmo, Escolhido, NumExemplos, NumRevisoes,
       Exactidao, Precisao, Recall, F1, Auc, VP, FP, VN, FN, Duracao
FROM treinos
ORDER BY DataHora DESC, Id DESC
LIMIT 20;
