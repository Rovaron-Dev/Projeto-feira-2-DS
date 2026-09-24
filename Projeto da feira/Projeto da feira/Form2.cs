using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Projeto_da_feira
{
    public partial class Form2 : Form
    {
        PrivateFontCollection fonteCollection = new PrivateFontCollection();
        public Form2()
        {
            InitializeComponent();

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            CarregarFonteInter();
        }
        private void CarregarFonteInter()
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

                "Host=aws-0-sa-east-1.pooler.supabase.com;" +
                "Database=postgres;" +
                "Username=postgres.zjlnoxudmxjanibkptpf;" +
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
                                // Verifica se existe a playlist "Curtidas"
                                string queryVerificaCurtidas = @"
                                        SELECT COUNT(*) 
                                        FROM playlist 
                                        WHERE fk_id_user_playlist = @IdUsuario 
                                          AND nome_playlist = 'Curtidas'";

                                using (var cmdVerificaCurtidas = new NpgsqlCommand(queryVerificaCurtidas, conexao))
                                {
                                    cmdVerificaCurtidas.Parameters.AddWithValue("@IdUsuario", idUsuario);

                                    long quantidadeCurtidas = (long)cmdVerificaCurtidas.ExecuteScalar();

                                    if (quantidadeCurtidas == 0)
                                    {
                                        string queryCriaCurtidas = @"
                                            INSERT INTO playlist 
                                                (music_playlist, nome_playlist, fk_id_user_playlist) 
                                            VALUES 
                                                (0, 'Curtidas', @IdUsuario)";

                                        using (var cmdCriaCurtidas = new NpgsqlCommand(queryCriaCurtidas, conexao))
                                        {
                                            cmdCriaCurtidas.Parameters.AddWithValue("@IdUsuario", idUsuario);
                                            cmdCriaCurtidas.ExecuteNonQuery();
                                        }
                                    }
                                }


                                // Verifica se existe a playlist "Histórico"
                                string queryVerificaHistorico = @"
                                            SELECT COUNT(*) 
                                            FROM playlist 
                                            WHERE fk_id_user_playlist = @IdUsuario 
                                              AND nome_playlist = 'Histórico'";

                                using (var cmdVerificaHistorico = new NpgsqlCommand(queryVerificaHistorico, conexao))
                                {
                                    cmdVerificaHistorico.Parameters.AddWithValue("@IdUsuario", idUsuario);

                                    long quantidadeHistorico = (long)cmdVerificaHistorico.ExecuteScalar();

                                    if (quantidadeHistorico == 0)
                                    {
                                        string queryCriaHistorico = @"
                                        INSERT INTO playlist 
                                            (music_playlist, nome_playlist, fk_id_user_playlist) 
                                        VALUES 
                                            (0, 'Histórico', @IdUsuario)";

                                        using (var cmdCriaHistorico = new NpgsqlCommand(queryCriaHistorico, conexao))
                                        {
                                            cmdCriaHistorico.Parameters.AddWithValue("@IdUsuario", idUsuario);
                                            cmdCriaHistorico.ExecuteNonQuery();
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
            guna2CustomGradientPanel1.Location = new Point((this.ClientSize.Width - guna2CustomGradientPanel1.Width) / 2, ((this.ClientSize.Height - guna2CustomGradientPanel1.Height) / 2) + 100);
            emailtxt.Location = new Point((guna2CustomGradientPanel1.Width - emailtxt.Width) / 2, emailtxt.Location.Y);
            senhatxt.Location = new Point((guna2CustomGradientPanel1.Width - senhatxt.Width) / 2, senhatxt.Location.Y);
            pb_beatCode.Location = new Point((this.ClientSize.Width - pb_beatCode.Width) / 2, pb_beatCode.Location.Y);
        }

        private void guna2GradientButton1_Click_1(object sender, EventArgs e)
        {
            Form3 form = new Form3();
            form.Show();

            this.Hide();
        }

        private void Form2_Resize(object sender, EventArgs e)
        {
            guna2CustomGradientPanel1.Location = new Point((this.ClientSize.Width - guna2CustomGradientPanel1.Width) / 2, (this.ClientSize.Height - guna2CustomGradientPanel1.Height) / 2);
        }

        private void emailtxt_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void guna2HtmlLabel1_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel1_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel3_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox2_Click(object sender, EventArgs e)
        {
            
        }

        private void pb_beatCode_Click(object sender, EventArgs e)
        {
            
        }

        private void guna2HtmlLabel3_Click_2(object sender, EventArgs e)
        {

        }
    }
}
