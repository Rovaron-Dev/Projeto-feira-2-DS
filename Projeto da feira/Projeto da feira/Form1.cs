using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Net;
using System.IO;
using Npgsql;
namespace Projeto_da_feira
{
    public partial class Form1 : Form

    {
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
    
    private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                Conexao banco = new Conexao();

                using (NpgsqlConnection conexao = banco.Abrir())
                {
                    conexao.Open();

                    MessageBox.Show("Conectado ao Supabase!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao conectar:\n\n" + ex.Message);
            }
        }
        public Form1()
        {
            InitializeComponent();
            startGraph();
            CarregarMusicas();
            
        }
        private void Card(Control container)
        {
            foreach (Control controle in container.Controls)
            {

                if (controle is Guna.UI2.WinForms.Guna2Panel panel &&
                    panel.Name.StartsWith("F"))
                {
                    panel.Dock = DockStyle.Fill;
                    panel.Margin = new Padding(10);

                    panel.FillColor = Cores.FundoSecundario;


                    panel.BorderRadius = 10;
                    panel.BorderThickness = 0;
                }
                else 
                if (controle.HasChildren)
                {
                    Card(controle);
                }
            }



        }
        public void CarregarMusicas()
        {
            Conexao banco = new Conexao();
            using (NpgsqlConnection conexao = banco.Abrir())
            {
                conexao.Open();
                string query = "SELECT pk_id_gender, nome_gender FROM gender";
                using (NpgsqlCommand comando = new NpgsqlCommand(query, conexao))
                {
                    using (NpgsqlDataReader leitor = comando.ExecuteReader())
                    {
                        while (leitor.Read())
                        {
                            int id = leitor.GetInt32(0);
                            string nome = leitor.GetString(1);
                            CriarCardsMusica(id, nome);
                        }
                        
                    }
                }
            }
        }
        private void CriarCardsMusica(int id, string nome)
        {
            Guna.UI2.WinForms.Guna2Panel panel = new Guna.UI2.WinForms.Guna2Panel();
            panel.Name = "Xcardmusic" + id;
            
                panel.Dock = DockStyle.None;
                panel.Width = 150;
                panel.Height = 180;
                panel.Margin = new Padding(10);

                panel.FillColor = Cores.FundoSecundario;


                panel.BorderRadius = 10;
                panel.BorderThickness = 0;


            Label labelNome = new Label();

            labelNome.Text = nome;
            labelNome.Font = new Font("Arial", 12, FontStyle.Bold);
            labelNome.AutoSize=true;
            labelNome.Location = new Point(10, 10);
            labelNome.BackColor = Cores.FundoSecundario;
            panel.Controls.Add(labelNome);

            flowLayoutPanel1.Controls.Add(panel);

        }

        //teste commit
        /*private void CarregarImagem(PictureBox pictureBox, string url)
        {
            using (WebClient client = new WebClient())
            {
                byte[] dados = client.DownloadData(url);

                using (MemoryStream stream = new MemoryStream(dados))
                {
                    pictureBox.Image = Image.FromStream(stream);
                }
            }
        }*/
        private void startGraph()
        {



            Card(this);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.SetRowSpan(Fpanel1, 2);


        }
        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2CustomGradientPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2CustomGradientPanel1_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel3_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            foreach (Control controle in flowLayoutPanel1.Controls)
            {

                //resize dos cards de acordo com o tamanho da tela
                if (this.Width >1000)
                    {
                        controle.Width = 200;
                        controle.Height = 250;

                    }
                    else
                    {
                        controle.Width = 150;
                        controle.Height = 180;
                    }
                
            }
        }

        private void q(object sender, EventArgs e)
        {

        }
    }
}
