using Guna.UI2.WinForms;
using NAudio.Wave;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;


// Declare estas variáveis no escopo principal do seu Form (fora dos métodos)


namespace Projeto_da_feira
{
    public partial class Form1 : Form

    {
        private void Form1_Load(object sender, EventArgs e)
        {


        }
        public Form1(int id)
        {
            InitializeComponent();

            string nome = "";
            string email = "";
            string senha = "";
            string icon = "";
            bool dark = true;

            Conexao conexao = new Conexao();

            try
            {
                using (NpgsqlConnection conn = conexao.Abrir())
                {
                    conn.Open();

                    string query = "SELECT * FROM \"user\" WHERE pk_id_user = @Id";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);

                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                nome = reader["nome_user"].ToString();
                                email = reader["gmail_user"].ToString();
                                senha = reader["senha_user"].ToString();
                                icon = reader["url_image_user"].ToString();
                                dark = reader.GetBoolean(reader.GetOrdinal("dark_mode_user"));
                            }
                            else
                            {
                                MessageBox.Show("Usuário não encontrado.");
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
                return;
            }

            // Agora as variáveis estão disponíveis aqui
            Deezer req = new Deezer();

            user usuario = new user(id, nome, email, senha, icon, dark);
            Preferences.darkMode = usuario.dark;
            if (usuario.dark)
            {
                tableLayoutPanel1.BackColor = Cores.Fundo;
            }
            else
            {
                tableLayoutPanel1.BackColor = Color.White;
            }
            startGraph();
            CarregarMusicas(pesquisatxt.Text);
        }
        Deezer req = new Deezer();
        public static class Cores
        {
            public static Color Fundo = Color.FromArgb(255, 5, 5, 7);
            public static Color FundoSecundario = Color.FromArgb(255, 19, 18, 26);
            public static Color Roxo = Color.FromArgb(255, 101, 95, 188);


        }

        /*private MediaFoundationReader audioReader;
        private WaveOutEvent outputDevice;

        // Esta é a função que recebe a URL extraída e dá o Play
        private async void TocarPreviewNoForm(string urlPreview)
        {
            PararAudio();

            if (string.IsNullOrEmpty(urlPreview) || urlPreview.StartsWith("Erro"))
            {
                MessageBox.Show("URL de áudio inválida.");
                return;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // 1. Limpa cabeçalhos antigos para evitar conflito
                    client.DefaultRequestHeaders.Clear();

                    // 2. Adiciona o pacote completo de cabeçalhos de simulação de navegador (Anti-403)
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                    client.DefaultRequestHeaders.Add("Accept", "audio/webm,audio/ogg,audio/wav,audio/*;q=0.9,application/ogg;q=0.7,video/*;q=0.6,*//**;q=0.5");
                    client.DefaultRequestHeaders.Add("Accept-Language", "pt-BR,pt;q=0.9,en-US;q=0.8,en;q=0.7");
                    client.DefaultRequestHeaders.Add("Connection", "keep-alive");

                    // Caso o servidor exija uma origem para evitar bloqueio Cross-Origin (CORS)
                    client.DefaultRequestHeaders.Add("Referer", "https://www.deezer.com/");

                    // 3. Executa o download direto para os bytes da memória virtual do sistema
                    byte[] audioBytes = await client.GetByteArrayAsync(urlPreview);

                    var memoryStream = new System.IO.MemoryStream(audioBytes);

                    // 4. Inicializa o NAudio e executa o áudio do fluxo
                    outputDevice = new WaveOutEvent();
                    var mp3Reader = new StreamMediaFoundationReader(memoryStream);
                    audioReader = mp3Reader;

                    outputDevice.Init(audioReader);
                    outputDevice.Play();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao reproduzir o áudio: {ex.Message}", "Erro de Áudio", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void PararAudio()
        {
            if (outputDevice != null)
            {
                outputDevice.Stop();
                outputDevice.Dispose();
                outputDevice = null;
            }
            if (audioReader != null)
            {
                audioReader.Dispose();
                audioReader = null;
            }
        }*/

        // Garante que o som pare se o usuário fechar o programa do nada
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            
            base.OnFormClosing(e);
        }


        public class Deezer
        {
            private readonly HttpClient client;
            private const string BaseUrl = "https://api.deezer.com";

            public Deezer()
            {
                client = new HttpClient();
            }

            public async Task<JObject> PesquisarMusicas(string pesquisa)
            {
                string query = Uri.EscapeDataString(pesquisa);
                string url = $"{BaseUrl}/search?q={query}";
                return await FazerRequisicao(url);
            }

            public async Task<JObject> BuscarMusica(long id)
            {
                string url = $"{BaseUrl}/track/{id}";
                return await FazerRequisicao(url);
            }

            public async Task<JObject> PesquisarArtistas(string pesquisa)
            {
                string query = Uri.EscapeDataString(pesquisa);
                string url = $"{BaseUrl}/search/artist?q={query}";
                return await FazerRequisicao(url);
            }

            public async Task<JObject> BuscarArtista(long id)
            {
                string url = $"{BaseUrl}/artist/{id}";
                return await FazerRequisicao(url);
            }

            public async Task<JObject> BuscarTopArtista(long id, int limite = 30)
            {
                string url = $"{BaseUrl}/artist/{id}/top?limit={limite}";
                return await FazerRequisicao(url);
            }

            public async Task<JObject> BuscarAlbunsArtista(long id)
            {
                string url = $"{BaseUrl}/artist/{id}/albums";
                return await FazerRequisicao(url);
            }

            public async Task<JObject> PesquisarAlbuns(string pesquisa)
            {
                string query = Uri.EscapeDataString(pesquisa);
                string url = $"{BaseUrl}/search/album?q={query}";
                return await FazerRequisicao(url);
            }

            public async Task<JObject> BuscarAlbum(long id)
            {
                string url = $"{BaseUrl}/album/{id}";
                return await FazerRequisicao(url);
            }

            public async Task<JObject> BuscarMusicasAlbum(long id)
            {
                string url = $"{BaseUrl}/album/{id}/tracks";
                return await FazerRequisicao(url);
            }

            public async Task<JObject> PesquisarPlaylists(string pesquisa)
            {
                string query = Uri.EscapeDataString(pesquisa);
                string url = $"{BaseUrl}/search/playlist?q={query}";
                return await FazerRequisicao(url);
            }

            public async Task<JObject> BuscarPlaylist(long id)
            {
                string url = $"{BaseUrl}/playlist/{id}";
                return await FazerRequisicao(url);
            }

            public async Task<JObject> BuscarMusicasPlaylist(long id)
            {
                string url = $"{BaseUrl}/playlist/{id}/tracks";
                return await FazerRequisicao(url);
            }

            public async Task<JObject> BuscarGeneros()
            {
                string url = $"{BaseUrl}/genre";
                return await FazerRequisicao(url);
            }

            public async Task<JObject> BuscarGenero(long id)
            {
                string url = $"{BaseUrl}/genre/{id}";
                return await FazerRequisicao(url);


            } public async Task<JObject> BuscarMusicaNome(string nome)
            {
                string pesquisaUrl = Uri.EscapeDataString(nome);
                string url = $"{BaseUrl}/search?q={pesquisaUrl}";
                return await FazerRequisicao(url);
            }

            public async Task<JObject> BuscarTop30Musicas()
            {
                string url = $"{BaseUrl}/chart/0/tracks?limit=30";
                return await FazerRequisicao(url);
            }

            public async Task<JObject> BuscarTop30Artistas()
            {
                string url = $"{BaseUrl}/chart/0/artists?limit=30";
                return await FazerRequisicao(url);
            }

            public async Task<JObject> BuscarTop30Albuns()
            {
                string url = $"{BaseUrl}/chart/0/albums?limit=30";
                return await FazerRequisicao(url);
            }
            public async Task<JObject> BuscarTop30Genero(long id)
            {
                string url = $"{BaseUrl}/chart/{id}/tracks?limit=30";

                return await FazerRequisicao(url);
            }
            private async Task<JObject> FazerRequisicao(string url)
            {
                string json = await client.GetStringAsync(url);
                return JObject.Parse(json);
            }

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


        public class user
        {
            public int id { get; set; }
            public string nome { get; set; }
            public string email { get; set; }
            public string senha { get; set; }
            public string icon { get; set; }

            public bool dark { get; set; }

            public user(int id, string nome, string email, string senha, string icon, bool dark)
            {
                this.id = id;
                this.nome = nome;
                this.email = email;
                this.senha = senha;
                this.icon = icon;
                this.dark = dark;
            }
        }


            public static class Preferences{
             public static bool darkMode = true;
             public static bool loop = true;

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
       

        public void CarregarPlaylists()
        {
    
        }
        public async void CarregarMusicas(string pesquisa)
        {
            flowLayoutPanel1.Controls.Clear();

            pesquisa =  pesquisa.Replace(" ", "");
            if (string.IsNullOrWhiteSpace(pesquisa))
            {
                pesquisa = "pop";
            }

            JObject resultado = await req.BuscarMusicaNome(pesquisa);
            flowLayoutPanel1.SuspendLayout();
            flowLayoutPanel1.Visible = false;
            foreach (JToken token in resultado["data"])
            {
                long id = (long)token["id"];
                string nome = (string)token["title"];
                string url = (string)token["album"]["cover_medium"];
                string nomeArtista = (string)token["artist"]["name"];
                string urlmusica = (string)token["preview"];

                CriarCardsMusica((long)id, nome, url, nomeArtista);
            }
            flowLayoutPanel1.ResumeLayout();
            flowLayoutPanel1.Visible = true;

        }
        private void CriarCardsMusica(long id, string nome, string url, string nomeArtista)
        {
            Guna.UI2.WinForms.Guna2Panel panel = new Guna.UI2.WinForms.Guna2Panel();
            panel.Name = "Xcardmusic" + id;

            panel.Dock = DockStyle.None;
            panel.Width = 220;
            panel.Height = 280;
            panel.Margin = new Padding(30);

            panel.FillColor = Cores.FundoSecundario;
            panel.BorderRadius = 10;
            panel.BorderThickness = 0;


            TableLayoutPanel grid = new TableLayoutPanel();

            grid.Margin = new Padding(10);
            grid.Dock = DockStyle.Fill;
            grid.ColumnCount = 1;
            grid.BackColor = Color.Transparent;

            grid.RowCount = 3;

            grid.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100F)
            );

            grid.RowStyles.Add(
                new RowStyle(SizeType.Percent, 75F)
            );

            grid.RowStyles.Add(
                new RowStyle(SizeType.Percent, 10F)
            );

            grid.RowStyles.Add(
                new RowStyle(SizeType.Percent, 15F)
            );


            Label labelNome = new Label();

            labelNome.Text = nome;
            labelNome.Font = new Font("Inter", 12, FontStyle.Bold);
            labelNome.ForeColor = Color.White;
            labelNome.AutoSize = true;
            labelNome.Dock = DockStyle.Fill;
            labelNome.TextAlign = ContentAlignment.TopLeft;
            labelNome.BackColor = Color.Transparent;
            labelNome.Margin = new Padding(6, 0, 0, 0);


            Label labelartist = new Label();

            labelartist.Text = nomeArtista;
            labelartist.Font = new Font("Inter", 8, FontStyle.Bold);
            labelartist.ForeColor = Color.Purple;
            labelartist.AutoSize = true;
            labelartist.Dock = DockStyle.Fill;
            labelartist.TextAlign = ContentAlignment.TopLeft;
            labelartist.BackColor = Color.Transparent;
            labelartist.Margin = new Padding(15, 0, 0, 0);


            Guna2PictureBox pictureBox = new Guna2PictureBox();

            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.Name = "pictureBox" + id;
            pictureBox.Margin = new Padding(10, 10, 10, 0);
            pictureBox.ImageLocation = url;
            pictureBox.BorderRadius = 10;


            // MÉTODO QUE SERÁ EXECUTADO AO CLICAR
            void CliqueCard(object sender, EventArgs e)
            {
                Musica.id = (long)id;
               
                
                TocarMusica();

            }


            // Adiciona o mesmo clique em todos os elementos
            panel.Click += CliqueCard;
            grid.Click += CliqueCard;
            labelNome.Click += CliqueCard;
            labelartist.Click += CliqueCard;
            pictureBox.Click += CliqueCard;


            // Monta o card
            grid.Controls.Add(pictureBox, 0, 0);
            grid.Controls.Add(labelNome, 0, 1);
            grid.Controls.Add(labelartist, 0, 2);

            panel.Controls.Add(grid);

            flowLayoutPanel1.Controls.Add(panel);
        }

        public static class Musica
        {
            public static long id = 0;
            public static string preview = "";
            public static string cover = "";
           
            
            

        }

           


            private void startGraph()
        {

            this.DoubleBuffered = true;
            this.BackColor = Cores.Fundo;
            Card(this);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.SetRowSpan(Fpanel1, 2);
            Display.FillColor = Cores.Fundo;


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
                if (this.Width > 1200)
                {
                    controle.Width = 220;
                    controle.Height = 280;

                }
                else
                {
                    controle.Width = 170;
                    controle.Height = 200;
                }

            }
        }

        private void q(object sender, EventArgs e)
        {

        }

        private void FBiblioteca_MouseClick(object sender, MouseEventArgs e)
        {
           
        }

        private void guna2CircleButton1_Click(object sender, EventArgs e)
        {
            if(axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying)
            {
                axWindowsMediaPlayer1.Ctlcontrols.pause();
            }
            else
            {
                axWindowsMediaPlayer1.Ctlcontrols.play();
            }
        }
        private async void TocarMusica()
        {

            var musica = await req.BuscarMusica(Musica.id);

            
            string previewUrl = musica["preview"].ToString();

            timer1.Start();
            

            axWindowsMediaPlayer1.URL = previewUrl;
            axWindowsMediaPlayer1.Ctlcontrols.play();


        }
        
        
        private void timer1_Tick(object sender, EventArgs e)
        {

            if (axWindowsMediaPlayer1.currentMedia != null)
            {
                double atual = axWindowsMediaPlayer1.Ctlcontrols.currentPosition;
                double duracao = axWindowsMediaPlayer1.currentMedia.duration;
                
                guna2TrackBar1.Minimum = 0;
                guna2TrackBar1.Maximum = 29;

                guna2TrackBar1.Value = (int)atual;

                if (Preferences.loop && axWindowsMediaPlayer1.Ctlcontrols.currentPosition >= 29)
                {
                    TocarMusica();
                }


            }
            
        }

        private void guna2TrackBar1_Scroll(object sender, ScrollEventArgs e)
        {
            axWindowsMediaPlayer1.Ctlcontrols.currentPosition = guna2TrackBar1.Value;
        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2CircleButton2_Click(object sender, EventArgs e)
        {
            CarregarMusicas(pesquisatxt.Text);
        }
    } }


