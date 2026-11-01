using MySqlConnector;
using Sentinela.Util;

namespace Sentinela.Dados;

public static class BaseDados
{
    public static void Inicializar(ConfiguracaoApp configuracao)
    {
        using var ligacao = ConexaoBD.Abrir(configuracao);
        string[] comandos =
        {
            @"CREATE TABLE IF NOT EXISTS utilizadores (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                NomeUtilizador VARCHAR(60) NOT NULL UNIQUE,
                NomeCompleto VARCHAR(140) NOT NULL,
                Sal CHAR(24) NOT NULL,
                HashSenha CHAR(44) NOT NULL,
                Perfil VARCHAR(30) NOT NULL,
                CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
            ) ENGINE=InnoDB",
            @"CREATE TABLE IF NOT EXISTS clientes (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Nome VARCHAR(140) NOT NULL,
                NumeroConta VARCHAR(40) NOT NULL UNIQUE,
                CidadeHabitual VARCHAR(80) NOT NULL,
                MediaValor DECIMAL(15,2) NOT NULL,
                Simulado BOOLEAN NOT NULL DEFAULT FALSE,
                CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
            ) ENGINE=InnoDB",
            @"CREATE TABLE IF NOT EXISTS transaccoes (
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
                CONSTRAINT fk_transaccoes_clientes FOREIGN KEY (ClienteId)
                    REFERENCES clientes(Id) ON DELETE CASCADE,
                INDEX ix_transaccoes_cliente_data (ClienteId, DataHora),
                INDEX ix_transaccoes_data (DataHora),
                INDEX ix_transaccoes_revisao (Revisao, Classificacao)
            ) ENGINE=InnoDB",
            @"CREATE TABLE IF NOT EXISTS treinos (
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
            ) ENGINE=InnoDB"
        };

        foreach (string sql in comandos)
        {
            using var comando = new MySqlCommand(sql, ligacao);
            comando.ExecuteNonQuery();
        }

        CriarUtilizadoresIniciais(ligacao);
    }

    private static void CriarUtilizadoresIniciais(MySqlConnection ligacao)
    {
        using var verificar = new MySqlCommand(
            "SELECT COUNT(*) FROM utilizadores", ligacao);
        if (Convert.ToInt32(verificar.ExecuteScalar()) > 0) return;

        AdicionarUtilizador(ligacao, "admin", "Administrador", "Administrador", "Sentinela#2026");
        AdicionarUtilizador(ligacao, "analista", "Analista do Sistema", "Analista", "Analista#2026");
    }

    private static void AdicionarUtilizador(MySqlConnection ligacao,
        string nome, string completo, string perfil, string palavraPasse)
    {
        var (sal, hash) = Seguranca.CriarHash(palavraPasse);
        using var comando = new MySqlCommand(
            @"INSERT INTO utilizadores (NomeUtilizador, NomeCompleto, Sal, HashSenha, Perfil)
              VALUES (@nome, @completo, @sal, @hash, @perfil)", ligacao);
        comando.Parameters.AddWithValue("@nome", nome);
        comando.Parameters.AddWithValue("@completo", completo);
        comando.Parameters.AddWithValue("@sal", sal);
        comando.Parameters.AddWithValue("@hash", hash);
        comando.Parameters.AddWithValue("@perfil", perfil);
        comando.ExecuteNonQuery();
    }
}
