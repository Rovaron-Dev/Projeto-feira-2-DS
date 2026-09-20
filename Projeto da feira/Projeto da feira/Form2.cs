using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Projeto_da_feira
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            
        }
        public static class Cores
        {
            public static Color Fundo = Color.FromArgb(255, 5, 5, 7);
            public static Color FundoSecundario = Color.FromArgb(255, 19, 18, 26);
            public static Color Roxo = Color.FromArgb(255, 101, 95, 188);


        }
        public class Conexao
        {
            private string connectionString =

                "Host=db.zjlnoxudmxjanibkptpf.supabase.co;Port=5432;" +
                "Database=postgres;" +
                "Username=postgres;" +
                "Password=reuna6genins;" +
                "SSL Mode=Require;" +
                "Trust Server Certificate=true";


            public NpgsqlConnection Abrir()
            {
                return new NpgsqlConnection(connectionString);
            }
        }

        private void guna2HtmlLabel2_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel3_Click(object sender, EventArgs e)
        {

        }

        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2GradientButton1_Click(object sender, EventArgs e)
        {
            string email = emailtxt.Text;
            string senha = senhatxt.Text;

            using (var conexao = new Conexao().Abrir())
            {
                conexao.Open();

                string query = "SELECT * FROM \"user\" WHERE gmail_user = @Email AND senha_user = @Senha";

                try
                {
                    using (var comando = new NpgsqlCommand(query, conexao))
                    {
                        comando.Parameters.AddWithValue("@Email", email);
                        comando.Parameters.AddWithValue("@Senha", senha);

                        using (var leitor = comando.ExecuteReader())
                        {
                            if (leitor.Read())
                            {
                                MessageBox.Show("Login bem-sucedido!");

                                int idUsuario = Convert.ToInt32(leitor["pk_id_user"]);

                                // --- INÍCIO DA VERIFICAÇÃO DA PLAYLIST "CURTIDAS" ---
                                // Fechamos o leitor atual antes de abrir um novo comando na mesma conexão
                                leitor.Close();

                                // 1. Verifica se já existe a playlist "Curtidas" para este usuário
                                string queryVerifica = "SELECT COUNT(*) FROM playlist WHERE fk_id_user_playlist = @IdUsuario AND nome_playlist = 'Curtidas'";
                                using (var cmdVerifica = new NpgsqlCommand(queryVerifica, conexao))
                                {
                                    cmdVerifica.Parameters.AddWithValue("@IdUsuario", idUsuario);
                                    long quantidade = (long)cmdVerifica.ExecuteScalar();

                                    // 2. Se não existir (quantidade == 0), cria a playlist
                                    if (quantidade == 0)
                                    {
                                        string queryCria = "INSERT INTO playlist (music_playlist,nome_playlist, fk_id_user_playlist) VALUES (0,'Curtidas', @IdUsuario)";
                                        using (var cmdCria = new NpgsqlCommand(queryCria, conexao))
                                        {
                                            cmdCria.Parameters.AddWithValue("@IdUsuario", idUsuario);
                                            cmdCria.ExecuteNonQuery();
                                        }
                                    }
                                }
                                // --- FIM DA VERIFICAÇÃO ---

                                Form1 form1 = new Form1(idUsuario);
                                form1.Show();

                                this.Hide();
                            }
                            else
                            {
                                MessageBox.Show("Email ou senha incorretos.");
                                senhatxt.Clear();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("erro: " + ex.Message);
                }
            }
        }

        private void guna2CustomGradientPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2GradientButton1_Click_1(object sender, EventArgs e)
        {
            Form3 form = new Form3();
            form.Show();

            this.Hide();
        }
    }
}
