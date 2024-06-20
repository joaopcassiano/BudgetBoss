using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;


namespace BETA___BOXBALANCER
{
    public partial class FrmPrincipal : Form
    {
        private Conexao objetoConexao = new Conexao();
        private InserindoValor objetoInserindoValor = new InserindoValor();
        private DataGridView mainDataGridView;
        private DataTable dt = new DataTable();
        private int i;

        public FrmPrincipal()
        {
            InitializeComponent();
            LimparMovimentacoes();
            AtualizarCamposResumo();
            AtualizarDataGridView();
        }

        private void BtnAbrirCaixa_Click(object sender, EventArgs e)
        {
            SqlConnection conn = objetoConexao.GetConnection();
            conn.Open();

            if (string.IsNullOrEmpty(TxtSaldoInicial.Text))
            {
                MessageBox.Show("Insira um valor inicial, mesmo que seja 'R$ 00,00'", "Saldo Inicial", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(cbUsuario.Text))
            {
                MessageBox.Show("Selecione um funcionário para a abertura do caixa.", "Funcionário", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (double.TryParse(TxtSaldoInicial.Text, out double saldoInicial))
            {
                // Verificar se já existe um caixa aberto na data atual
                int codigoCaixaAberto = VerificarCaixaAberto(conn);
                if (codigoCaixaAberto == -1)
                {
                    InserirNovoCaixaAberto(saldoInicial);

                    // Limpar a tabela Movimentacao ao abrir um novo caixa
                    LimparMovimentacoes();
                }
                else
                {
                    /*MessageBox.Show("Já existe um caixa aberto na data atual.", "Caixa Aberto", MessageBoxButtons.OK, MessageBoxIcon.Information);*/
                }


                pictureBox1.Image = Properties.Resources.caixaaberto;
                LblCaixa.Text = "ABERTO";
                TxtBSaldoInicialResumo.Text = saldoInicial.ToString("C2");
                labelgrid.Text = saldoInicial.ToString("C2");
                LblData.Text = DateTime.Now.ToShortDateString();

                PnlMovimentacoes.Enabled = true;
                PnlResumo.Enabled = true;
                cbUsuario.Enabled = false;
                TxtSaldoInicial.Enabled = false;
                BtnAbrirCaixa.Enabled = false;
                dataGridView1.Visible = true;

                // Adicionar a coluna de data à tabela
                dt.Columns.Add("Data", typeof(DateTime));

                // Atualizar os campos de resumo
                AtualizarCamposResumo();
            }
            else
            {
                MessageBox.Show("O valor inserido não é válido. Insira um número válido.", "Valor Inválido", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            conn.Close();
        }

        private void LimparMovimentacoes()
        {
            string sqlLimparMovimentacoes = "DELETE FROM Movimentacao";
            objetoConexao.Conectar();
            objetoConexao.Executar(sqlLimparMovimentacoes);
            objetoConexao.Desconectar();
        }


        private void BtnIncluir_Click(object sender, EventArgs e)
        {

            FrmIncluir incluir = new FrmIncluir(dataGridView1);
            incluir.ShowDialog();
            AtualizarDataGridView();
            AtualizarCamposResumo();
        }


        private void BtnExluir_Click(object sender, EventArgs e)
        {
            FrmExcluir Excluir = new FrmExcluir();
            Excluir.ShowDialog();
            AtualizarDataGridView();
            AtualizarCamposResumo();

        }

        private void BtnFecharCaixa_Click(object sender, EventArgs e)
        {

            SqlConnection conn = objetoConexao.GetConnection();
            double saldoFinal = 0;
            try
            {
                conn.Open();
                saldoFinal = objetoInserindoValor.FecharCaixa();
                MessageBox.Show("Caixa diário fechado com sucesso!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao fechar o caixa: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            txtSaldoFinal.Text = saldoFinal.ToString("C2");
            AtualizarCamposResumo();
        }

        private void BtnSair_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Deseja mesmo sair?", "Sair", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            Close();
        }

        private void AtualizarCamposResumo()
        {
            SqlConnection conn = objetoConexao.GetConnection();
            conn.Open();

            // Calcular e atualizar os campos de resumo
            txtEntradaDinheiro.Text = CalcularValorMovimentacao("Entrada", "Dinheiro", conn).ToString("C2");
            txtEntradaCredito.Text = CalcularValorMovimentacao("Entrada", "Crédito", conn).ToString("C2");
            txtEntradaDebito.Text = CalcularValorMovimentacao("Entrada", "Débito", conn).ToString("C2");
            txtEntradaPix.Text = CalcularValorMovimentacao("Entrada", "PIX", conn).ToString("C2");

            txtSaidaDinheiro.Text = CalcularValorMovimentacao("Saída", "Dinheiro", conn).ToString("C2");
            txtSaidaCredito.Text = CalcularValorMovimentacao("Saída", "Crédito", conn).ToString("C2");
            txtSaidaDebito.Text = CalcularValorMovimentacao("Saída", "Débito", conn).ToString("C2");
            txtSaidaPix.Text = CalcularValorMovimentacao("Saída", "PIX", conn).ToString("C2");

            txtEntradaTotal.Text = AtualizaFinal("Entrada", conn).ToString("C2");
            txtSaidaTotal.Text = AtualizaFinal("Saída", conn).ToString("C2");


            conn.Close();
        }

        private double AtualizaFinal(string tipoEntrada, SqlConnection conn)
        {
            string query = $"SELECT SUM(Valor_Movimentacao) FROM Movimentacao WHERE TipoEntrada_Movimentacao = '{tipoEntrada}'";
            return ExecutarConsulta(query, conn);
        }
        private double CalcularValorMovimentacao(string tipoEntrada, string formaPagamento, SqlConnection conn)
        {
            string query = $"SELECT SUM(Valor_Movimentacao) FROM Movimentacao WHERE TipoEntrada_Movimentacao = '{tipoEntrada}' AND FormaPagamento_Movimentacao = '{formaPagamento}'";
            return ExecutarConsulta(query, conn);
        }

        private double ExecutarConsulta(string query, SqlConnection conn)
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

        private int VerificarCaixaAberto(SqlConnection conn)
        {
            int codigoCaixa = -1;
            string query = "SELECT TOP 1 Codigo_Caixa FROM Caixa WHERE Estado_Caixa = 'Aberto' ORDER BY Data_Caixa DESC";

            try
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        codigoCaixa = Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao verificar caixa aberto: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }

            return codigoCaixa;
        }
        private void InserirNovoCaixaAberto(double saldoInicial)
        {
            SqlConnection conn = objetoConexao.GetConnection();

            try
            {
                conn.Open();
                string sqlInsertCaixa = $"INSERT INTO Caixa (Data_Caixa, Saldo_Caixa, Estado_Caixa) VALUES (GETDATE(), @SaldoInicial, 'Aberto')";

                using (SqlCommand cmd = new SqlCommand(sqlInsertCaixa, conn))
                {
                    cmd.Parameters.AddWithValue("@SaldoInicial", saldoInicial);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao inserir novo caixa aberto: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }

        private void AtualizarDataGridView()
        {
            string sqlQuery = "SELECT Vendedor_Movimentacao, TipoEntrada_Movimentacao, FormaPagamento_Movimentacao, Valor_Movimentacao, GETDATE() AS Data FROM Movimentacao";
            using (SqlDataAdapter da = new SqlDataAdapter(sqlQuery, objetoConexao.GetConnection()))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                dataGridView1.DataSource = dt;
            }
        }

        private void FrmPrincipal_Load(object sender, EventArgs e)
        {
            PnlMovimentacoes.Enabled = false;
            PnlResumo.Enabled = false;
            dataGridView1.Visible = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {

            PrintDocument pd = new PrintDocument();
            pd.DocumentName = "Relatorio";
            pd.BeginPrint += Pd_BeginPrint;
            pd.PrintPage += Imprimir;

            //cria objeto de preview

            PrintPreviewDialog ppd = new PrintPreviewDialog();
            ppd.Document = pd;
            ppd.ShowDialog();

        }
        private void Pd_BeginPrint(object sender, PrintEventArgs e)
        {
            i = 0;
        }

        private void Imprimir(object sender, PrintPageEventArgs ev)
        {
            //Configuração de Página
            float linhaPorPagina = 0;
            float posicaoVertical = 0;
            float contador = 0;
            float margemEsquerda = 20;
            float margemSuperior = 20;
            float alturaFonte = 0;

            string linha = "";

            Font fonte = new Font("Arial", 14);
            alturaFonte = fonte.GetHeight(ev.Graphics);
            linhaPorPagina = Convert.ToInt32(ev.MarginBounds.Height / alturaFonte);

            //Título

            linha = "Movimentações do Caixa Diário";
            posicaoVertical = margemSuperior + contador * alturaFonte;
            ev.Graphics.DrawString(linha, fonte, Brushes.Black, margemEsquerda, posicaoVertical);

            contador += 4;

            linha = "Vendedor";
            posicaoVertical = margemSuperior + contador * alturaFonte;
            ev.Graphics.DrawString(linha, fonte, Brushes.Black, margemEsquerda, posicaoVertical);

            linha = "Forma de Pagamento";
            ev.Graphics.DrawString(linha, fonte, Brushes.Black, margemEsquerda + 250, posicaoVertical);

            linha = "Valor";
            ev.Graphics.DrawString(linha, fonte, Brushes.Black, margemEsquerda + 500, posicaoVertical);

            linha = "--------------------------------------------------------------------------------------";
            contador += 1;
            posicaoVertical = margemSuperior + contador * alturaFonte;
            ev.Graphics.DrawString(linha, fonte, Brushes.Black, margemEsquerda, posicaoVertical);

            contador++;
            DataSet ds = objetoInserindoValor.PesquisaDados();

            if (ds.Tables[0] != null)
            {
                while (i < ds.Tables[0].Rows.Count && contador < linhaPorPagina)
                {
                    DataRow item = ds.Tables[0].Rows[i];
                    linha = item["Vendedor_Movimentacao"].ToString();
                    posicaoVertical = margemSuperior + contador * alturaFonte;
                    ev.Graphics.DrawString(linha, fonte, Brushes.Black, margemEsquerda, posicaoVertical);
                    linha = item["FormaPagamento_Movimentacao"].ToString();
                    ev.Graphics.DrawString(linha, fonte, Brushes.Black, margemEsquerda + 250, posicaoVertical);
                    linha = item["Valor_Movimentacao"].ToString();
                    posicaoVertical = margemSuperior + contador * alturaFonte;
                    ev.Graphics.DrawString(linha, fonte, Brushes.Black, margemEsquerda + 500, posicaoVertical);
                    contador += 2;
                    i++;

                }
            }
            else MessageBox.Show("Sem Cadastro");

            if (contador > linhaPorPagina)
            {
                ev.HasMorePages = true;
            }
            else
            {
                ev.HasMorePages = false;
            }





        }
    }
}