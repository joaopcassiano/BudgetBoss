using System;
using System.Data;
using System.Data.SqlClient;

namespace BETA___BOXBALANCER
{
    internal class Conexao : IDisposable
    {
        private readonly string connectionString = "SERVER=.\\SQLEXPRESS;Database=BudgetBoss;UID=sa;PWD=t2e4x6h1";
        private SqlConnection conn;

        public Conexao()
        {
            conn = new SqlConnection(connectionString);
        }

        public void Conectar()
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
        }

        public void Desconectar()
        {
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }

        public SqlConnection GetConnection()
        {
            return conn;
        }

        public void Executar(string sql)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    Conectar();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao executar o comando SQL.", ex);
            }
            finally
            {
                Desconectar();
            }
        }

        public DataSet ListarDados(string sql)
        {
            try
            {
                using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                {
                    DataSet ds = new DataSet();
                    Conectar();
                    da.Fill(ds);
                    return ds;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao listar dados.", ex);
            }
            finally
            {
                Desconectar();
            }
        }

        public void Dispose()
        {
            if (conn != null)
            {
                conn.Dispose();
            }
        }
    }
}

