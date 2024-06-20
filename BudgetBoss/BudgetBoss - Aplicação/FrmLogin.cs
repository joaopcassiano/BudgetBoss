using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace BETA___BOXBALANCER
{
    public partial class FrmLogin : Form
    {
        public FrmLogin()
        {
            InitializeComponent();
        }

        private Conexao objetoConexao = new Conexao();
        string usuario, senha;

        private void button1_Click(object sender, EventArgs e)
        {
            Login();
        }

        private void FrmLogin_Load(object sender, EventArgs e)
        {

        }

        private void Login()
        {
            string query = "SELECT Login_Usuario, Senha_Usuario FROM Usuario WHERE Login_Usuario=@usuario AND Senha_Usuario=@senha";
            try
            {
                objetoConexao.Conectar();
                using (SqlCommand comando = new SqlCommand(query, objetoConexao.GetConnection()))
                {
                    comando.Parameters.AddWithValue("@usuario", txtUsuario.Text);
                    comando.Parameters.AddWithValue("@senha", txtSenha.Text);

                    using (SqlDataReader reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = reader["Login_Usuario"].ToString();
                            senha = reader["Senha_Usuario"].ToString();
                            if (usuario == txtUsuario.Text && senha == txtSenha.Text)
                            {
                                FrmPrincipal frmPrincipal = new FrmPrincipal();
                                frmPrincipal.Show();
                                this.Hide();
                            }
                            else
                            {
                                MessageBox.Show("Usuário ou senha incorretos.","Login",MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Usuário ou senha incorretos.", "Login", MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao fazer login: " + ex.Message);
            }
            finally
            {
                objetoConexao.Desconectar();
            }
        }
    }
}
