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
        public Form1()
        {
            InitializeComponent();
            startGraph();
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
                }else if(controle is Guna.UI2.WinForms.Guna2Panel panel2 &&
                    panel2.Name.StartsWith("X"))
                {
                    panel2.Dock = DockStyle.None;
                    panel2.Width = 150;
                    panel2.Height = 180;
                    panel2.Margin = new Padding(10);

                    panel2.FillColor = Cores.FundoSecundario;


                    panel2.BorderRadius = 10;
                    panel2.BorderThickness = 0;
                }
                if (controle.HasChildren)
                {
                    Card(controle);
                }
            }

            
            
        }
        //teste commit
        private void CarregarImagem(PictureBox pictureBox, string url)
        {
            using (WebClient client = new WebClient())
            {
                byte[] dados = client.DownloadData(url);

                using (MemoryStream stream = new MemoryStream(dados))
                {
                    pictureBox.Image = Image.FromStream(stream);
                }
            }
        }
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
    }
}
