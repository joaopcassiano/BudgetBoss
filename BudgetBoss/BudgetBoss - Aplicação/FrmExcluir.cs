using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BETA___BOXBALANCER
{
    public partial class FrmExcluir : Form
    {
        public FrmExcluir()
        {
            InitializeComponent();
        }

        private Conexao objetoConexao = new Conexao();
        private InserindoValor Remove = new InserindoValor();

        private void btnExcluir_Click(object sender, EventArgs e)
        {

            if (dataGridView1.SelectedRows.Count > 0)
            {
                int codigoMovimentacao = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["Codigo_Movimentacao"].Value);
                Remove.ExcluirRegistro(codigoMovimentacao); // Chama o método para excluir do banco de dados
                dataGridView1.Rows.RemoveAt(dataGridView1.SelectedRows[0].Index); // Remove do DataGridView
                MessageBox.Show("Deseja mesmo excluir ?", "Excluir", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                MessageBox.Show("Registro excluído com sucesso!");
                AtualizarDataGridView(); // Chama o método para atualizar o DataGridView depois da exclusão
            }
            else
            {
                MessageBox.Show("Selecione uma linha para excluir.");
            }
        }

        private void AtualizarDataGridView()
        {
            string sqlQuery = "SELECT Codigo_Movimentacao, Data_Movimentacao, Vendedor_Movimentacao, TipoEntrada_Movimentacao, Valor_Movimentacao, FormaPagamento_Movimentacao FROM Movimentacao";
            using (SqlDataAdapter da = new SqlDataAdapter(sqlQuery, objetoConexao.GetConnection()))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                dataGridView1.DataSource = dt;
            }
        }

        private void FrmExcluir_Load(object sender, EventArgs e)
        {
            AtualizarDataGridView();
        }

        private void BtnSair_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Deseja mesmo sair?", "Sair", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            Close();
        }
    }
}
