using Guna.UI2.WinForms;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Projeto_da_feira.Form1;

namespace Projeto_da_feira
{
    public partial class Form_Playlist : Form
    {
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
        
        Deezer deezer = new Deezer();
        public class Deezer
        {
            private readonly HttpClient client;

            private const string BaseUrl =
                "https://api.deezer.com";

            public Deezer()
            {
                client = new HttpClient();
            }


            public async Task<JObject> PesquisarMusicas(string pesquisa)
            {
                string query =
                    Uri.EscapeDataString(pesquisa);

                string url =
                    $"{BaseUrl}/search?q={query}";

                return await FazerRequisicao(url);
            }


            public async Task<JObject> BuscarMusica(long id)
            {
                string url =
                    $"{BaseUrl}/track/{id}";

                return await FazerRequisicao(url);
            }


            public async Task<JObject> PesquisarArtistas(string pesquisa)
            {
                string query =
                    Uri.EscapeDataString(pesquisa);

                string url =
                    $"{BaseUrl}/search/artist?q={query}";

                return await FazerRequisicao(url);
            }


            public async Task<JObject> BuscarArtista(long id)
            {
                string url =
                    $"{BaseUrl}/artist/{id}";

                return await FazerRequisicao(url);
            }


            public async Task<JObject> BuscarTopArtista(
                long id,
                int limite = 30)
            {
                string url =
                    $"{BaseUrl}/artist/{id}/top?limit={limite}";

                return await FazerRequisicao(url);
            }


            public async Task<JObject> BuscarAlbunsArtista(long id)
            {
                string url =
                    $"{BaseUrl}/artist/{id}/albums";

                return await FazerRequisicao(url);
            }


            public async Task<JObject> PesquisarAlbuns(string pesquisa)
            {
                string query =
                    Uri.EscapeDataString(pesquisa);

                string url =
                    $"{BaseUrl}/search/album?q={query}";

                return await FazerRequisicao(url);
            }


            public async Task<JObject> BuscarAlbum(long id)
            {
                string url =
                    $"{BaseUrl}/album/{id}";

                return await FazerRequisicao(url);
            }


            public async Task<JObject> BuscarMusicasAlbum(long id)
            {
                string url =
                    $"{BaseUrl}/album/{id}/tracks";

                return await FazerRequisicao(url);
            }


            public async Task<JObject> PesquisarPlaylists(string pesquisa)
            {
                string query =
                    Uri.EscapeDataString(pesquisa);

                string url =
                    $"{BaseUrl}/search/playlist?q={query}";

                return await FazerRequisicao(url);
            }


            public async Task<JObject> BuscarPlaylist(long id)
            {
                string url =
                    $"{BaseUrl}/playlist/{id}";

                return await FazerRequisicao(url);
            }


            public async Task<JObject> BuscarMusicasPlaylist(long id)
            {
                string url =
                    $"{BaseUrl}/playlist/{id}/tracks";

                return await FazerRequisicao(url);
            }


            public async Task<JObject> BuscarGeneros()
            {
                string url =
                    $"{BaseUrl}/genre";

                return await FazerRequisicao(url);
            }


            public async Task<JObject> BuscarGenero(long id)
            {
                string url =
                    $"{BaseUrl}/genre/{id}";

                return await FazerRequisicao(url);
            }


            public async Task<JObject> BuscarMusicaNome(string nome)
            {
                string pesquisaUrl =
                    Uri.EscapeDataString(nome);

                string url =
                    $"{BaseUrl}/search?q={pesquisaUrl}";

                return await FazerRequisicao(url);
            }


            public async Task<JObject> BuscarTop30Musicas()
            {
                string url =
                    $"{BaseUrl}/chart/0/tracks?limit=30";

                return await FazerRequisicao(url);
            }


            public async Task<JObject> BuscarTop30Artistas()
            {
                string url =
                    $"{BaseUrl}/chart/0/artists?limit=30";

                return await FazerRequisicao(url);
            }


            public async Task<JObject> BuscarTop30Albuns()
            {
                string url =
                    $"{BaseUrl}/chart/0/albums?limit=30";

                return await FazerRequisicao(url);
            }


            public async Task<JObject> BuscarTop30Genero(long id)
            {
                string url =
                    $"{BaseUrl}/chart/{id}/tracks?limit=30";

                return await FazerRequisicao(url);
            }


            private async Task<JObject> FazerRequisicao(string url)
            {
                string json =
                    await client.GetStringAsync(url);

                return JObject.Parse(json);
            }
        }
        Conexao conexao = new Conexao();
        public Form_Playlist(string nome,long id, List<long> musicas, string imagem)
        {
            InitializeComponent();
            Playlist.id = id;
            Playlist.musicas = musicas;
            Playlist.nome = nome;
            Playlist.imagem = imagem;
            startGraph();

        }
        private async Task startGraph()
        {
            
            guna2PictureBox1.ImageLocation = Playlist.imagem;
            guna2HtmlLabel1.Text = Playlist.nome;
            if (Playlist.imagem != null)
            {
               
                   
                guna2GradientPanel1.FillColor = await ObterCorPredominanteAsync(Playlist.imagem); ;
            }
            BackColor = Cores.FundoSecundario;
        }
        // ID da música atualmente selecionada/ativa (por instância)
        public long idMusicaAtual { get; set; } = 0;
        public string nome { get; set; } = "";
        public string url { get; set; } = "";

        public static class Playlist
        {
            public static string nome;
            public static long id;

            public static List<long> musicas;

            public static string imagem;

        }
        public static class Cores
        {
            public static Color Fundo =
                Color.FromArgb(255, 5, 5, 7);

            public static Color FundoSecundario =
                Color.FromArgb(255, 19, 18, 26);

            public static Color Roxo =
                Color.FromArgb(255, 101, 95, 188);
        }
        private void Form_Playlist_Load(object sender, EventArgs e)
        {
            CarregarMusicas();
        }


        private async void CarregarMusicas()
        {
            foreach (long idMusica in Playlist.musicas)
            {
                JObject musica = await deezer.BuscarMusica(idMusica);
                long id = (long)musica["id"];
                string titulo = musica["title"].ToString();
                string artista = musica["artist"]["name"].ToString();
                string album = musica["album"]["title"].ToString();
                string imagem = musica["album"]["cover_medium"].ToString();
                // Cria um novo controle de música e adiciona ao FlowLayoutPanel
                CardMusica(id, titulo, artista, album, imagem);
                
            }
        }





        private void CardMusica(long id, string titulo, string artista, string album, string imagem, string duracao = "0.29")
        {
            Guna2Panel panel = new Guna2Panel();
            panel.Name = "Xcardmusic" + id;

            // Usamos a largura do flowLayoutPanel1 descontando uma margem segura para evitar quebra de linha
            panel.Width = flowLayoutPanel1.Width - 30;
            panel.Height = 60;
            panel.Margin = new Padding(5, 2, 5, 2);
            panel.FillColor = Cores.FundoSecundario;
            panel.BorderRadius = 8;
            panel.BorderThickness = 0;

            TableLayoutPanel grid = new TableLayoutPanel();
            grid.Dock = DockStyle.Fill;
            grid.RowCount = 1;
            grid.ColumnCount = 5;
            grid.BackColor = Color.Transparent;
            grid.Margin = new Padding(0);
            grid.Padding = new Padding(5, 2, 5, 2);

            // Definindo as colunas da lista horizontal
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));  // 1. Capa
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));   // 2. Título
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));   // 3. Artista
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));   // 4. Álbum
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));  // 5. Duração

            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            Guna2PictureBox pictureBox = new Guna2PictureBox();
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.Name = "pictureBox" + id;
            pictureBox.ImageLocation = imagem;
            pictureBox.BorderRadius = 6;
            pictureBox.Margin = new Padding(0);

            Label labelNome = new Label();
            labelNome.Text = titulo;
            labelNome.Font = new Font("Inter", 10, FontStyle.Bold);
            labelNome.ForeColor = Color.White;
            labelNome.Dock = DockStyle.Fill;
            labelNome.TextAlign = ContentAlignment.MiddleLeft;
            labelNome.AutoEllipsis = true;

            Label labelArtist = new Label();
            labelArtist.Text = artista;
            labelArtist.Font = new Font("Inter", 9, FontStyle.Regular);
            labelArtist.ForeColor = Color.DarkGray;
            labelArtist.Dock = DockStyle.Fill;
            labelArtist.TextAlign = ContentAlignment.MiddleLeft;
            labelArtist.AutoEllipsis = true;

            Label labelAlbum = new Label();
            labelAlbum.Text = album;
            labelAlbum.Font = new Font("Inter", 9, FontStyle.Regular);
            labelAlbum.ForeColor = Color.DarkGray;
            labelAlbum.Dock = DockStyle.Fill;
            labelAlbum.TextAlign = ContentAlignment.MiddleLeft;
            labelAlbum.AutoEllipsis = true;

            Label labelDuracao = new Label();
            labelDuracao.Text = duracao;
            labelDuracao.Font = new Font("Inter", 9, FontStyle.Regular);
            labelDuracao.ForeColor = Color.DarkGray;
            labelDuracao.Dock = DockStyle.Fill;
            labelDuracao.TextAlign = ContentAlignment.MiddleRight;

            void CliqueCard(object sender, EventArgs e)
            {
                idMusicaAtual = id;
                nome = titulo;
                url = imagem;
                tocarmusica?.Invoke(this, EventArgs.Empty);
            }

            panel.Click += CliqueCard;
            grid.Click += CliqueCard;
            labelNome.Click += CliqueCard;
            labelArtist.Click += CliqueCard;
            labelAlbum.Click += CliqueCard;
            labelDuracao.Click += CliqueCard;
            pictureBox.Click += CliqueCard;
            panel.Cursor = Cursors.Hand;
            grid.Cursor = Cursors.Hand;
            labelAlbum.Cursor = Cursors.Hand;
            labelNome.Cursor = Cursors.Hand;
            labelArtist.Cursor = Cursors.Hand;
            labelDuracao.Cursor = Cursors.Hand;
            pictureBox.Cursor = Cursors.Hand;
            grid.Controls.Add(pictureBox, 0, 0);
            grid.Controls.Add(labelNome, 1, 0);
            grid.Controls.Add(labelArtist, 2, 0);
            grid.Controls.Add(labelAlbum, 3, 0);
            grid.Controls.Add(labelDuracao, 4, 0);

            panel.Controls.Add(grid);

            // Retornando para o uso do seu flowLayoutPanel1 original
            flowLayoutPanel1.Controls.Add(panel);
        }
        private static readonly HttpClient httpClient = new HttpClient();

        public async Task<Color> ObterCorPredominanteAsync(string urlImagem)
        {
            if (string.IsNullOrEmpty(urlImagem))
                return Color.Black;

            try
            {
                // 1. Baixa a imagem da web diretamente para a memória (Stream) sem bloquear o disco
                using (var stream = await httpClient.GetStreamAsync(urlImagem))
                using (Bitmap bitmap = new Bitmap(stream))
                {
                    long rTotal = 0;
                    long gTotal = 0;
                    long bTotal = 0;
                    long totalPixels = 0;

                    // Dica de ouro: para imagens maiores (ex: 250x250), pular de 10 em 10 acelera muito 
                    // sem perder a precisão da cor predominante.
                    for (int x = 0; x < bitmap.Width; x += 10)
                    {
                        for (int y = 0; y < bitmap.Height; y += 10)
                        {
                            Color corPixel = bitmap.GetPixel(x, y);

                            if (corPixel.A > 10)
                            {
                                rTotal += corPixel.R;
                                gTotal += corPixel.G;
                                bTotal += corPixel.B;
                                totalPixels++;
                            }
                        }
                    }

                    if (totalPixels == 0) return Color.Black;

                    int mediaR = (int)(rTotal / totalPixels);
                    int mediaG = (int)(gTotal / totalPixels);
                    int mediaB = (int)(bTotal / totalPixels);

                    return Color.FromArgb(mediaR, mediaG, mediaB);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro ao calcular cor da imagem: " + ex.Message);
                return Color.Black; // Cor padrão caso dê erro na web
            }
        }
        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
        public event EventHandler BotaoFoiClicado;
        public event EventHandler tocarmusica;
        private void guna2Button1_Click(object sender, EventArgs e)
        {
            BotaoFoiClicado?.Invoke(this, EventArgs.Empty);
            
            this.Close();
        }
    }
}
