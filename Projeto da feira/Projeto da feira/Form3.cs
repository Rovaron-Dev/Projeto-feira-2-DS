using Guna.UI2.WinForms;
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
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
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

        private void guna2GradientButton1_Click(object sender, EventArgs e)
        {
            Form2 form1 = new Form2();
            form1.Show();

            this.Hide();
        }

        private void guna2HtmlLabel2_Click_1(object sender, EventArgs e)
        {

        }

        private void btnentrar_Click(object sender, EventArgs e)
        {
            string email = emailtxt.Text;
            string senha = senhatxt.Text;
            string nome = nometxt.Text; // Certifique-se de que a caixa de texto existe
            picture.ImageLocation = urltxt.Text;
            MessageBox.Show(email);
            if (picture.Image == picture.ErrorImage || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha) | string.IsNullOrWhiteSpace(nome))
            {
                MessageBox.Show("Verifique todos os campos ou se a imagem é valida");
            }
            else
            {
                using (var conexao = new Conexao().Abrir())
                {
                    try
                    {
                        conexao.Open();

                        // Adicionado o RETURNING para trazer os dados do usuário cadastrado
                        // ATENÇÃO: Inclua "@Nome" na query se a sua tabela tiver a coluna de nome. 
                        // Se a tabela tiver apenas email e senha, remova a parte do nome.
                        string query = "INSERT INTO \"user\" (gmail_user, senha_user, nome_user ,url_image_user) VALUES (@Email, @Senha, @Nome, @UrlImage) RETURNING pk_id_user, gmail_user;";

                        using (var comando = new NpgsqlCommand(query, conexao))
                        {
                            comando.Parameters.AddWithValue("@Email", email);
                            comando.Parameters.AddWithValue("@Senha", senha);
                            comando.Parameters.AddWithValue("@Nome", nome); // Corrigido para bater com a query
                            comando.Parameters.AddWithValue("@UrlImage", urltxt.Text);

                            using (var leitor = comando.ExecuteReader())
                            {
                                if (leitor.Read())
                                {
                                    MessageBox.Show("Registro bem-sucedido!");

                                    int idUsuario = Convert.ToInt32(leitor["pk_id_user"]);

                                    Form2 form1 = new Form2();
                                    form1.Show();

                                    this.Hide();
                                }
                                else
                                {
                                    MessageBox.Show("Não foi possível realizar o cadastro.");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erro: " + ex.Message);
                    }
                }
            }

        }

        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {
            picture.ImageLocation = urltxt.Text;    
        }

        private void guna2GradientButton2_Click(object sender, EventArgs e)
        {
            
        }

        private void guna2CustomGradientPanel1_Paint(object sender, PaintEventArgs e)
        {
            panel_fundo.Location = new Point((this.ClientSize.Width - panel_fundo.Width) / 2, ((this.ClientSize.Height - panel_fundo.Height) / 2) + 100);
        }

        private void Form3_Resize(object sender, EventArgs e)
        {
            panel_fundo.Location = new Point((this.ClientSize.Width - panel_fundo.Width) / 2, (this.ClientSize.Height - panel_fundo.Height) / 2);
            pb_beatCode.Location = new Point((this.ClientSize.Width - pb_beatCode.Width) / 2, pb_beatCode.Location.Y);
            
        }

        private void nometxt_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void emailtxt_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
