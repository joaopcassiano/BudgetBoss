using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;

namespace BETA___BOXBALANCER
{
    internal class InserindoValor
    {
        private Conexao objetoConexao = new Conexao();
        private int codigo_Movimentacao;
        private string tipoDeEntrada_Movimentacao;
        private double valor_Movimentacao;
        private string formaPagamento_Movimentacao;
        private string vendedor_Movimentacao;
        private string data_Movimentacao;
        

        public int Codigo_Movimentacao { get => codigo_Movimentacao; set => codigo_Movimentacao = value; }
        public string TipoDeEntrada_Movimentacao { get => tipoDeEntrada_Movimentacao; set => tipoDeEntrada_Movimentacao = value; }
        public double Valor_Movimentacao { get => valor_Movimentacao; set => valor_Movimentacao = value; }
        public string FormaPagamento_Movimentacao { get => formaPagamento_Movimentacao; set => formaPagamento_Movimentacao = value; }
        public string Vendedor_Movimentacao { get => vendedor_Movimentacao; set => vendedor_Movimentacao = value; }
        public string Data_Movimentacao { get => data_Movimentacao; set => data_Movimentacao = value; }

        public void Inserir()
        {
            string sql = "";
            sql += $"Insert into Movimentacao (Vendedor_Movimentacao, TipoEntrada_Movimentacao, Valor_Movimentacao, FormaPagamento_Movimentacao, Data_Movimentacao)" +
                $"values ('{Vendedor_Movimentacao}','{TipoDeEntrada_Movimentacao}', {Valor_Movimentacao.ToString().Replace(',', '.')}, '{FormaPagamento_Movimentacao}', '{Data_Movimentacao}' )";
            objetoConexao.Conectar();
            objetoConexao.Executar(sql);
            objetoConexao.Desconectar();
        }

        public void ExcluirRegistro(int codigoMovimentacao)
        {
            SqlConnection conn = objetoConexao.GetConnection();

            string sql = "DELETE FROM Movimentacao WHERE Codigo_Movimentacao = @Codigo_Movimentacao";
            try
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Codigo_Movimentacao", codigoMovimentacao);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao excluir registro: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

        }
        public void InserirInicial()
        {
            string data = DateTime.Now.ToShortDateString();
            string sqlInicial = $"Insert into Caixa (Data_Caixa)" + $"values ('{data}')";
            objetoConexao.Conectar();
            objetoConexao.Executar(sqlInicial);
            objetoConexao.Desconectar();

        }


        public double FecharCaixa()
        {
            double saldoFinal = 0;
            SqlConnection conn = objetoConexao.GetConnection();
            conn.Open();

            // Calcular entrada dinheiro
            string queryEntradaDinheiro = "SELECT SUM(Valor_Movimentacao) FROM Movimentacao WHERE TipoEntrada_Movimentacao = 'Entrada' AND FormaPagamento_Movimentacao = 'Dinheiro'";
            double entradaDinheiro = ExecutarConsulta(queryEntradaDinheiro, conn);

            // Calcular saída dinheiro
            string querySaidaDinheiro = "SELECT SUM(Valor_Movimentacao) FROM Movimentacao WHERE TipoEntrada_Movimentacao = 'Saída' AND FormaPagamento_Movimentacao = 'Dinheiro'";
            double saidaDinheiro = ExecutarConsulta(querySaidaDinheiro, conn);

            // Calcular entrada cartão de crédito
            string queryEntradaCredito = "SELECT SUM(Valor_Movimentacao) FROM Movimentacao WHERE TipoEntrada_Movimentacao = 'Entrada' AND FormaPagamento_Movimentacao = 'Crédito'";
            double entradaCredito = ExecutarConsulta(queryEntradaCredito, conn);

            // Calcular saída cartão de crédito
            string querySaidaCredito = "SELECT SUM(Valor_Movimentacao) FROM Movimentacao WHERE TipoEntrada_Movimentacao = 'Saída' AND FormaPagamento_Movimentacao = 'Crédito'";
            double saidaCredito = ExecutarConsulta(querySaidaCredito, conn);

            // Calcular entrada débito
            string queryEntradaDebito = "SELECT SUM(Valor_Movimentacao) FROM Movimentacao WHERE TipoEntrada_Movimentacao = 'Entrada' AND FormaPagamento_Movimentacao = 'Débito'";
            double entradaDebito = ExecutarConsulta(queryEntradaDebito, conn);

            // Calcular saída débito
            string querySaidaDebito = "SELECT SUM(Valor_Movimentacao) FROM Movimentacao WHERE TipoEntrada_Movimentacao = 'Saída' AND FormaPagamento_Movimentacao = 'Débito'";
            double saidaDebito = ExecutarConsulta(querySaidaDebito, conn);

            // Calcular entrada PIX
            string queryEntradaPix = "SELECT SUM(Valor_Movimentacao) FROM Movimentacao WHERE TipoEntrada_Movimentacao = 'Entrada' AND FormaPagamento_Movimentacao = 'PIX'";
            double entradaPix = ExecutarConsulta(queryEntradaPix, conn);

            // Calcular saída PIX
            string querySaidaPix = "SELECT SUM(Valor_Movimentacao) FROM Movimentacao WHERE TipoEntrada_Movimentacao = 'Saída' AND FormaPagamento_Movimentacao = 'PIX'";
            double saidaPix = ExecutarConsulta(querySaidaPix, conn);

            // Calcular saldo final
            saldoFinal = entradaDinheiro - saidaDinheiro + entradaCredito - saidaCredito + entradaDebito - saidaDebito + entradaPix - saidaPix;

            string sqlInsertCaixa = $"INSERT INTO Caixa (Data_Caixa, Saldo_Caixa) VALUES (GETDATE(), @SaldoFinal)";
            using (SqlCommand cmd = new SqlCommand(sqlInsertCaixa, conn))
            {
                cmd.Parameters.AddWithValue("@SaldoFinal", saldoFinal);
                cmd.ExecuteNonQuery();
            }

            conn.Close();

            MessageBox.Show($"Caixa fechado com sucesso! Saldo Final: {saldoFinal:C}");

            return saldoFinal;
        }

        public double ExecutarConsulta(string query, SqlConnection conn)
        {
            double resultado = 0;


            try
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        resultado = Convert.ToDouble(result);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao executar consulta: {ex.Message}");
            }
            return resultado;
        }
        public DataSet PesquisaDados()
        {
            string sql = $"Select * from Movimentacao";
            objetoConexao.Conectar();
            DataSet ds = objetoConexao.ListarDados(sql);
            objetoConexao.Desconectar();
            return ds;
        }



    }


}