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
using System.Windows.Forms;
using System.Xml.Linq;
using static Projeto_da_feira.Form_Playlist;
using static System.Net.Mime.MediaTypeNames;

namespace Projeto_da_feira
{
    public partial class Form1 : Form
    {
        static class User
        {
            public static int id;
            public static string nome;
            public static string icon;
        }

        Conexao conexao = new Conexao();
        Deezer req = new Deezer();

        // NOVO: menu principal da música
        private Guna2Panel menuMusica;

        // NOVO: menu que mostra as playlists
        private Guna2Panel menuPlaylists;


        public Form1(int id)
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.DoubleBuffer |
                  ControlStyles.UserPaint |
                  ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            string nome = "";
            string email = "";
            string senha = "";
            string icon = "";
            bool dark = true;

            User.id = id;

            try
            {
                using (NpgsqlConnection conn = conexao.Abrir())
                {
                    conn.Open();

                    string query =
                        "SELECT * FROM \"user\" WHERE pk_id_user = @Id";

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

                                dark = reader.GetBoolean(
                                    reader.GetOrdinal("dark_mode_user")
                                );
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

            User.nome = nome;
            User.icon = icon;
            User.id = id;

            startGraph();

            CarregarMusicas(pesquisatxt.Text);
            CarregarBiblioteca();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
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

        public Color ObterCorPredominante(string caminhoImagem)
{
    using (Bitmap bitmap = new Bitmap(caminhoImagem))
    {
        long rTotal = 0;
        long gTotal = 0;
        long bTotal = 0;
        long totalPixels = 0;

        // Percorre a imagem (dica: para imagens grandes, pular de X em X pixels otimiza bastante)
        for (int x = 0; x < bitmap.Width; x += 5)
        {
            for (int y = 0; y < bitmap.Height; y += 5)
            {
                Color corPixel = bitmap.GetPixel(x, y);

                // Ignora pixels muito transparentes se houver canal alpha
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

        // Calcula a média das cores
        int mediaR = (int)(rTotal / totalPixels);
        int mediaG = (int)(gTotal / totalPixels);
        int mediaB = (int)(bTotal / totalPixels);

        return Color.FromArgb(mediaR, mediaG, mediaB);
    }
}
        public static class Preferences
        {
            public static bool darkMode = true;
            public static bool loop = true;
        }


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


        public class Conexao
        {
            private string connectionString =
                "Host=aws-0-sa-east-1.pooler.supabase.com;"+
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


        private async Task CarregarBiblioteca()
        {
            long id = 0;
            string nome = "";

            try
            {
                using (NpgsqlConnection conn = conexao.Abrir())
                {
                    await conn.OpenAsync();

                    // Pega o último registro inserido para cada id_playlist (para definir o nome e a capa)
                    string query = @"
    WITH UltimaLinha AS (
        SELECT 
            id_playlist, 
            music_playlist, 
            pk_id_playlist,
            ROW_NUMBER() OVER (PARTITION BY id_playlist ORDER BY pk_id_playlist DESC) as rn
        FROM playlist
        WHERE fk_id_user_playlist = @Id
    ),
    PrimeiroNome AS (
        SELECT DISTINCT ON (id_playlist) 
            id_playlist, 
            nome_playlist
        FROM playlist
        WHERE fk_id_user_playlist = @Id 
          AND nome_playlist IS NOT NULL
        ORDER BY id_playlist, pk_id_playlist ASC
    )
    SELECT 
        u.id_playlist, 
        p.nome_playlist, 
        u.music_playlist, 
        u.pk_id_playlist
    FROM UltimaLinha u
    JOIN PrimeiroNome p ON u.id_playlist = p.id_playlist
    WHERE u.rn = 1;";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", User.id);

                        using (NpgsqlDataReader reader = (NpgsqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                id = reader.GetInt64(reader.GetOrdinal("id_playlist"));
                                nome = reader["nome_playlist"].ToString();
                                
                                // ID da última música (usado para buscar a imagem de capa)
                                long idMusicUltima = reader.GetInt64(reader.GetOrdinal("music_playlist"));
                          
                                string urlCapa = await imagemMusica(idMusicUltima);

                                // 1. AQUI ESTÁ A LISTA: Pega todas as músicas que possuem esse mesmo id_playlist
                                List<long> todasAsMusicasDaPlaylist = await ObterMusicasDaPlaylistAsync(id);

                                // 2. Passa os dados para o card (pode ajustar o CardPlaylist para receber a lista se precisar dela lá dentro)
                                CardPlaylist(id, nome, urlCapa,todasAsMusicasDaPlaylist);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }

        // Método auxiliar que busca todas as músicas daquele id_playlist em ordem de inserção
        private async Task<List<long>> ObterMusicasDaPlaylistAsync(long idPlaylist)
        {
            List<long> listaMusicas = new List<long>();

            try
            {
                using (NpgsqlConnection conn = conexao.Abrir())
                {
                    await conn.OpenAsync();

                    string query = @"
                SELECT music_playlist 
                FROM playlist 
                WHERE id_playlist = @IdPlaylist 
                  AND music_playlist <> 0 
                ORDER BY pk_id_playlist ASC";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdPlaylist", idPlaylist);

                        using (NpgsqlDataReader reader = (NpgsqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                long idMusica = reader.GetInt64(reader.GetOrdinal("music_playlist"));
                                listaMusicas.Add(idMusica);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro ao buscar músicas da playlist: " + ex.Message);
            }

            return listaMusicas;
        }
        private async Task<string> imagemMusica(long id)
        {
            try
            {
                JObject resultado = await req.BuscarMusica(id);

                if (resultado != null && resultado["album"] != null)
                {
                    string coverUrl = (string)resultado["album"]["cover_medium"];
                    
                    return coverUrl ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro ao extrair imagem: " + ex.Message);
            }

            return "https://uxwing.com/wp-content/themes/uxwing/download/controller-and-music/music-player-playlist-round-black-icon.png";
        }

        private void CardPlaylist(long id, string nome, string imagem, List<long> musicas)
        {
            Guna2Panel panel = new Guna2Panel();
            panel.Name = "Fplaylist" + id;
            panel.FillColor = Cores.FundoSecundario;
            panel.Width = 150;
            panel.Height = 60;
            panel.Cursor = Cursors.Hand;
            panel.BorderRadius = 8; // Opcional: arredondar o painel também se desejar

            TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.ColumnCount = 2;
            tableLayoutPanel.RowCount = 1;
            tableLayoutPanel.BackColor = Color.Transparent; // Evita conflitos de cor de fundo
            tableLayoutPanel.Margin = new Padding(0);

            tableLayoutPanel.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 60f)
            );
            tableLayoutPanel.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100F)
            );

            // === 1. CRIANDO O PICTUREBOX PARA A IMAGEM ===
            Guna2PictureBox picBox = new Guna2PictureBox();
            picBox.Dock = DockStyle.None;
            picBox.Width = 55;
            picBox.Height = 55;
            picBox.Margin = new Padding(3);
            

            // MUDANÇA PRINCIPAL: Usar Zoom para evitar distorção da imagem
            picBox.SizeMode = PictureBoxSizeMode.StretchImage;

            picBox.ImageLocation = imagem;
            picBox.BorderRadius = 8; // Arredonda os cantos da imagem do Guna2
            picBox.Cursor = Cursors.Hand;
            picBox.BackColor = Color.Transparent;

            // === 2. CRIANDO O LABEL DO NOME ===
            Guna2HtmlLabel labelNome = new Guna2HtmlLabel();
            labelNome.Text = nome;
            labelNome.ForeColor = Color.White;
            labelNome.Font = new Font("Inter", 11, FontStyle.Bold); // Leve ajuste no tamanho se necessário para caber melhor
            labelNome.Dock = DockStyle.Fill;
            labelNome.TextAlignment = ContentAlignment.MiddleLeft;
            labelNome.Cursor = Cursors.Hand;
            labelNome.BackColor = Color.Transparent;

            // === 3. ADICIONANDO O EVENTO DE CLIQUE EM TUDO USANDO LAMBDA ===
            void AcaoClique(object sender, EventArgs e) => Abrirplaylist(nome, id, musicas, imagem);

            panel.Click += AcaoClique;
            tableLayoutPanel.Click += AcaoClique;
            picBox.Click += AcaoClique;
            labelNome.Click += AcaoClique;

            // === 4. POSICIONANDO NO TABLELAYOUTPANEL ===
            tableLayoutPanel.Controls.Add(picBox, 0, 0);
            tableLayoutPanel.Controls.Add(labelNome, 1, 0);

            panel.Controls.Add(tableLayoutPanel);
            flowLayoutPanel2.Controls.Add(panel);
        }

        public static class telaAtual
        {
            public static int tela = 1;
        }

        // Exemplo da sua função que recebe o ID
        private async Task Abrirplaylist(string nome, long id, List<long> id2, string url)
        {
            // 1. Limpa os formulários anteriores de dentro do painel para não sobrepor
            foreach (Control control in playlistForm.Controls)
            {
                if (control is Form formAntigo)
                {
                    formAntigo.Close();
                    formAntigo.Dispose();
                }
            }
            playlistForm.Controls.Clear();

            // 2. Instancia o novo formulário da playlist (evite usar o mesmo nome do painel para a variável)
            Form_Playlist playlistFormInstance = new Form_Playlist(nome, id, id2, url);

            // Ajustes do TableLayoutPanel (certifique-se de que 'formtable' é o nome correto da sua tabela)
            telaAtual.tela = 2;
            trocarPov();

            // Configura o form para se comportar como um controle interno
            playlistFormInstance.TopLevel = false;
            playlistFormInstance.FormBorderStyle = FormBorderStyle.None;
            playlistFormInstance.Dock = DockStyle.Fill;
            playlistFormInstance.BotaoFoiClicado += (sender, e) =>
            {
                // Chame aqui a função que está no seu formulário principal!
                telaAtual.tela = 1;
                trocarPov();
            };
            playlistFormInstance.tocarmusica += (sender, e) =>
            {
                // Chame aqui a função que está no seu formulário principal!
                Musica.id = playlistFormInstance.idMusicaAtual;
                Musica.nome = playlistFormInstance.nome;
                Musica.cover = playlistFormInstance.url;
                TocarMusica();
            };
            // Adiciona o formulário dentro do painel e o exibe
            playlistForm.Controls.Add(playlistFormInstance);
            playlistFormInstance.Show();
        }
        private void trocarPov()
        {
            if (telaAtual.tela == 1)
            {
                formtable.RowStyles[0].SizeType = SizeType.Percent;
                formtable.RowStyles[0].Height = 0f;
                formtable.RowStyles[1].SizeType = SizeType.Percent;
                formtable.RowStyles[1].Height = 85f;
                formtable.RowStyles[2].SizeType = SizeType.Percent;
                formtable.RowStyles[2].Height = 15f;

            }
            else
            {
                formtable.RowStyles[0].SizeType = SizeType.Percent;
                formtable.RowStyles[0].Height = 85f;
                formtable.RowStyles[1].SizeType = SizeType.Percent;
                formtable.RowStyles[1].Height = 0f;
                formtable.RowStyles[2].SizeType = SizeType.Percent;
                formtable.RowStyles[2].Height = 15f;
            }
            
            
        }
        private void Form1_KeyPress(
            object sender,
            KeyEventArgs e)
        {
            if (
                e.KeyCode == Keys.Enter &&
                pesquisatxt.Text != ""
            )
            {
                CarregarMusicas(
                    pesquisatxt.Text
                );
            }
        }

  
        private void Card(Control container)
        {
            foreach (Control controle
                in container.Controls)
            {
                if (
                    controle is
                    Guna.UI2.WinForms.Guna2Panel panel
                    &&
                    panel.Name.StartsWith("F")
                )
                {
                    panel.Dock =
                        DockStyle.Fill;
                
                    panel.BorderThickness = 0
                        ;
                    panel.BorderColor =
                        Color.White;
                    panel.Margin =
                        new Padding(10);

                    panel.FillColor =
                        Cores.FundoSecundario;

                    panel.BorderRadius = 10;
                   
                }
                else if (controle.HasChildren)
                {
                    Card(controle);
                }
            }
        }


        public async void CarregarMusicas(
            string pesquisa)
        {
            flowLayoutPanel1.Controls.Clear();

            pesquisa =
                pesquisa.Replace(" ", "");

            if (string.IsNullOrWhiteSpace(pesquisa))
            {
                pesquisa = "sahur";
            }

            JObject resultado =
                await req.BuscarMusicaNome(
                    pesquisa
                );

            flowLayoutPanel1.SuspendLayout();

            flowLayoutPanel1.Visible = false;

            foreach (
                JToken token
                in resultado["data"])
            {
                long id =
                    (long)token["id"];

                string nome =
                    (string)token["title"];

                string url =
                    (string)token["album"]
                    ["cover_medium"];

                string nomeArtista =
                    (string)token["artist"]
                    ["name"];

                string urlmusica =
                    (string)token["preview"];

                CriarCardsMusica(
                    id,
                    nome,
                    url,
                    nomeArtista
                );
            }

            flowLayoutPanel1.ResumeLayout();

            flowLayoutPanel1.Visible = true;
        }


        private void CriarCardsMusica(
     long id,
     string nome,
     string url,
     string nomeArtista)
        {
            Guna2Panel panel =
                new Guna2Panel();

            panel.Name =
                "Xcardmusic" + id;

            panel.Dock =
                DockStyle.None;

            panel.Width = 220;
            panel.Height = 280;

            panel.Margin =
                new Padding(15);
            panel.Padding =
                new Padding(14);
            panel.FillColor =
                Cores.FundoSecundario;

            panel.BorderRadius = 12;
            panel.BorderThickness = 0;
            panel.Cursor = Cursors.Hand;


            TableLayoutPanel grid =
                new TableLayoutPanel();

            

            grid.Dock =
                DockStyle.Fill;

            grid.ColumnCount = 1;

            grid.BackColor =
                Color.Transparent;

            grid.RowCount = 3;
            

            grid.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F
                )
            );

            // Proporção ideal: Mais espaço para a capa, e o restante para os dois textos
            grid.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    82F
                )
            );

            grid.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    10F
                )
            );

            grid.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    8f
                )
            );


            // === PICTURE BOX (CAPA) ===
            Guna2PictureBox pictureBox =
                new Guna2PictureBox();

            // ALTERADO PARA ZOOM: Evita que a imagem fique distorcida
            pictureBox.SizeMode =
                PictureBoxSizeMode.StretchImage;

            pictureBox.Dock =
                DockStyle.Fill;

            pictureBox.Name =
                "pictureBox" + id;

            // Margens equilibradas para a capa respirar dentro do card
            pictureBox.Margin =
                new Padding(0, 0, 0, 0);

            pictureBox.ImageLocation =
                url;

            pictureBox.BorderRadius = 8;
            pictureBox.BackColor = Color.Transparent;
            pictureBox.Cursor = Cursors.Hand;


            // === LABEL NOME DA MÚSICA ===
            Label labelNome =
                new Label();

            labelNome.Text =
                nome;

            labelNome.Font =
                new Font(
                    "Inter",
                    11,
                    FontStyle.Bold
                );

            labelNome.ForeColor =
                Color.White;

            labelNome.AutoSize = false; // Desativado para o Dock preencher direito a linha do TableLayout
            labelNome.Dock =
                DockStyle.Fill;

            labelNome.TextAlign =
                ContentAlignment.MiddleLeft;

            labelNome.BackColor =
                Color.Transparent;

            // Alinhado a zero na esquerda para ficar colado na bordinha interna correta
            labelNome.Margin =
                new Padding(0, 0, 0, 0);
            labelNome.Cursor = Cursors.Hand;


            // === LABEL NOME DO ARTISTA ===
            Label labelartist =
                new Label();

            labelartist.Text =
                nomeArtista;

            labelartist.Font =
                new Font(
                    "Inter",
                    9,
                    FontStyle.Regular
                );

            labelartist.ForeColor =
                Color.DarkGray; // Tom mais suave parecido com o estilo Spotify/Clean

            labelartist.AutoSize = false;
            labelartist.Dock =
                DockStyle.Fill;

            labelartist.TextAlign =
                ContentAlignment.MiddleLeft;

            labelartist.BackColor =
                Color.Transparent;

            labelartist.Margin =
                new Padding(0, 0, 0, 5);
            labelartist.Cursor = Cursors.Hand;


            // === EVENTO DE CLIQUE (DIREITO OU ESQUERDO) ===
            void CliqueCard(
                object sender,
                EventArgs e)
            {
                Musica.id = id;
                Musica.cover = url;
                Musica.nome = nome;

                if (
                    e is MouseEventArgs mouseEventArgs
                    &&
                    mouseEventArgs.Button ==
                    MouseButtons.Right
                )
                {
                    Point mousePosition =
                        panel.PointToScreen(
                            mouseEventArgs.Location
                        );

                    MostrarMenuMusica(
                        panel,
                        mousePosition
                    );
                }
                else
                {
                    TocarMusica();
                }
            }


            panel.Click += CliqueCard;
            grid.Click += CliqueCard;
            labelNome.Click += CliqueCard;
            labelartist.Click += CliqueCard;
            pictureBox.Click += CliqueCard;


            // Adicionando os controlos ao TableLayout nas respetivas linhas
            grid.Controls.Add(
                pictureBox,
                0,
                0
            );

            grid.Controls.Add(
                labelNome,
                0,
                1
            );

            grid.Controls.Add(
                labelartist,
                0,
                2
            );

            panel.Controls.Add(grid);

            flowLayoutPanel1.Controls.Add(
                panel
            );
        }


        public static class Musica
        {
            public static long id = 0;

            public static string nome = "";
            public static string preview = "";
            public static string cover = "";
        }


        // ALTERADO:
        // Fecha tanto o menu da música quanto o menu de playlists.
        protected override void OnMouseClick(
            MouseEventArgs e)
        {
            base.OnMouseClick(e);
            
            if (e.Button == MouseButtons.Right)
            {
                FecharMenus();
            }
        }


        private Guna2Button CriarOpcao(
            string texto)
        {
            Guna2Button label =
                new Guna2Button();
            label.BackColor = Color.Transparent;
            label.Text =
                texto;
            label.BorderRadius = 6;
            label.ForeColor =
                Color.White;
            label.FillColor = Cores.Fundo;
            label.Font =
                new Font(
                    "Segoe UI",
                    10
                );

            

            label.Size =
                new Size(
                    140,
                    35
                );

            

            label.Cursor =
                Cursors.Hand;

            return label;
        }


        // ALTERADO:
        // Menu principal da música.
        

        // NOVO:
        // Cria o segundo menu contendo todas
        // as playlists do usuário.
        private void MostrarMenuMusica(
    Control card,
    Point mousePosition)
        {
            FecharMenus();

            menuMusica =
                new Guna2Panel();

            menuMusica.Size =
                new Size(
                    180,
                    150
                );

            menuMusica.FillColor =
                Cores.FundoSecundario;
            menuMusica.BackColor = Color.Transparent;
            menuMusica.BorderRadius = 10;

            menuMusica.BorderThickness = 1;

            menuMusica.BorderColor =
                Color.FromArgb(
                    50,
                    50,
                    60
                );
            Guna2Button sair =
                criarfechar();

            Guna2Button curtir =
                CriarOpcao(
                    "♡  Curtir"
                );

            Guna2Button playlist =
                CriarOpcao(
                    "+  Playlist"
                );

            Guna2Button tocar =
                CriarOpcao(
                    "▶  Tocar"
                );


            curtir.Location =
                new Point(30, 10);

            playlist.Location =
                new Point(30, 55);

            tocar.Location =
                new Point(30, 100);
            sair.Location =
                new Point(0, 0);

            // NOVO:
            // Abre o menu com as playlists.
            playlist.Click +=
                (sender, e) =>
                {
                    MostrarPlaylists();
                };


            tocar.Click +=
                (sender, e) =>
                {
                    TocarMusica();

                    FecharMenus();
                };

            menuMusica.Controls.Add(
                sair
            );
            menuMusica.Controls.Add(
                curtir
            );

            menuMusica.Controls.Add(
                playlist
            );

            menuMusica.Controls.Add(
                tocar
            );


            this.Controls.Add(
                menuMusica
            );


            // Mantém o menu na posição do mouse.
            menuMusica.Location =
                this.PointToClient(
                    mousePosition
                );

            menuMusica.BringToFront();
        }
        private void MostrarPlaylists()
        {
            if (menuMusica != null)
            {
                Controls.Remove(
                    menuMusica
                );

                menuMusica.Dispose();

                menuMusica = null;
            }


            if (menuPlaylists != null)
            {
                Controls.Remove(
                    menuPlaylists
                );

                menuPlaylists.Dispose();
            }


            menuPlaylists =
                new Guna2Panel();


            menuPlaylists.Size =
                new Size(
                    220,
                    250
                );

            menuPlaylists.FillColor =
                Cores.FundoSecundario;
            menuPlaylists.BackColor = Color.Transparent;
            menuPlaylists.BorderRadius = 10;

            menuPlaylists.BorderThickness = 1;

            menuPlaylists.BorderColor =
                Color.FromArgb(
                    50,
                    50,
                    60
                );


            int y = 10;


            try
            {
                using (
                    NpgsqlConnection conn =
                    conexao.Abrir()
                )
                {
                    conn.Open();


                    // NOVO:
                    // Busca as playlists pertencentes
                    // ao usuário atualmente logado.
                    string query = @"                   SELECT                       id_playlist,                       nome_playlist                   FROM playlist                   WHERE fk_id_user_playlist = @Id AND nome_playlist IS NOT NULL                   ORDER BY id_playlist";


                    using (
                        NpgsqlCommand cmd =
                        new NpgsqlCommand(
                            query,
                            conn
                        )
                    )
                    {
                        cmd.Parameters.AddWithValue(
                            "@Id",
                            User.id
                        );


                        using (
                            NpgsqlDataReader reader =
                            cmd.ExecuteReader()
                        )
                        {
                            Guna2Button fechar =
                                    criarfechar();
                            menuPlaylists.Controls.Add(
                                    fechar
                                );

                            while (reader.Read())
                            {
                                long idPlaylist =
                                    reader.GetInt64(
                                        reader.GetOrdinal(
                                            "id_playlist"
                                        )
                                    );


                                string nomePlaylist =
                                    reader[
                                        "nome_playlist"
                                    ].ToString();

                                
                                Guna2Button playlist =
                                    CriarOpcao(
                                        nomePlaylist
                                    );

                                fechar.Location =
                                    new Point(
                                        1,
                                        1
                                    );
                                playlist.Location =
                                    new Point(
                                        30,
                                        y
                                    );


                                playlist.Size =
                                    new Size(
                                        180,
                                        35
                                    );


                                // NOVO:
                                // Ao clicar em uma playlist,
                                // adiciona a música selecionada.
                                playlist.Click +=
                                    (sender, e) =>
                                    {
                                        adicionarMusicaPlaylist(
                                            Musica.id,
                                            idPlaylist
                                        );

                                        FecharMenus();
                                    };


                                menuPlaylists.Controls.Add(
                                    playlist
                                );


                                y += 40;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao carregar playlists: "
                    + ex.Message
                );

                return;
            }


            // Ajusta a altura de acordo com
            // a quantidade de playlists.
            menuPlaylists.Height =
                Math.Max(
                    y + 10,
                    60
                );


            this.Controls.Add(
                menuPlaylists
            );


            // Faz o menu aparecer exatamente onde o cursor do mouse está
            menuPlaylists.Location = this.PointToClient(Cursor.Position);


            menuPlaylists.BringToFront();
        }

        // NOVO:
        // Adiciona a música à playlist escolhida.
        private void adicionarMusicaPlaylist(
            long idMusica,
            long idPlaylist)
        {
            try
            {
                using (
                    NpgsqlConnection conn =
                    conexao.Abrir()
                )
                {
                    conn.Open();


                    string query = @"
                        INSERT INTO playlist
                        (
                          
                            music_playlist,id_playlist,fk_id_user_playlist    
                        )
                        VALUES
                        (
                           
                            @Musica,
                            @idPlaylist
,@UserId
                        )";


                    using (
                        NpgsqlCommand cmd =
                        new NpgsqlCommand(
                            query,
                            conn
                        )
                    )
                    {
                      

                        cmd.Parameters.AddWithValue(
                            "@Musica",
                            idMusica
                        );

                        cmd.Parameters.AddWithValue(
                            "@idPlaylist",
                            idPlaylist
                        );

                        cmd.Parameters.AddWithValue(
                            "@UserId",
                            User.id
                        );

                        cmd.ExecuteNonQuery();
                    }
                }


                MessageBox.Show(
                    "Música adicionada à playlist!"
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao adicionar música: "
                    + ex.Message
                );
            }
            flowLayoutPanel2.Controls.Clear();
            CarregarBiblioteca();
        }

        private Guna2Button criarfechar()
        {
            Guna2Button label =
                new Guna2Button();

            label.Text = "X";
            label.BackColor = Color.Transparent;
            label.ForeColor =

                Color.White;

            label.Font =
                new Font(
                    "Segoe UI",
                    10
                );
            label.FillColor = Cores.FundoSecundario;
            label.BorderRadius = 6;
            label.AutoSize = false;

            label.Size =
                new Size(
                    30,
                    30
                );

            

            label.Cursor =
                Cursors.Hand;
            label.Click +=
                (sender, e) =>
                {
                    FecharMenus();
                };

            return label;
        }
        // NOVO:
        // Fecha os menus abertos.
        private void FecharMenus()
        {
            if (menuMusica != null)
            {
                Controls.Remove(
                    menuMusica
                );

                menuMusica.Dispose();

                menuMusica = null;
            }


            if (menuPlaylists != null)
            {
                Controls.Remove(
                    menuPlaylists
                );

                menuPlaylists.Dispose();

                menuPlaylists = null;
            }
        }


        private void startGraph()
        {
            trocarPov();
            
            musicImg.Visible = false;
            
            imagembbar.Height = 70;
            imagembbar.Width = 70;
            
            guna2TrackBar1.Location =
                new Point(
                    this.Width / 2 -
                    guna2TrackBar1.Width / 2,
                    50
                );
            volume.Location =
                new Point(
                    guna2TrackBar1.Location.X + guna2TrackBar1.Width + 270,
                    50
                );
            guna2Button1.Location =
                    new Point(
                        volume.Location.X - guna2Button1.Width - 10,
                        volume.Location.Y
                    );

            guna2CircleButton1.Location =
                new Point(
                    this.Width / 2 -
                    guna2CircleButton1.Width / 2,
                    10
                );
            imagembbar.Location =
                new Point(
                    30
                    , guna2TrackBar1.Location.Y - 30
                    );

            titulolbl.Location =
                new Point(
                    120
                    , imagembbar.Location.Y + imagembbar.Height / 2 -15
                    );
            titulolbl.Width = 300; // Ou a largura desejada
            titulolbl.TextAlignment = ContentAlignment.TopLeft;
            guna2TrackBar1.BackColor =
                Color.Transparent;

            this.DoubleBuffered =
                true;

            this.BackColor =
                Cores.Fundo;

            Card(this);

            guna2PictureBox1.ImageLocation =
                User.icon;

            if (
                guna2PictureBox1.Image ==
                guna2PictureBox1.ErrorImage
            )
            {
                guna2PictureBox1.ImageLocation =
                    @"https://cdn-icons-png.flaticon.com/512/149/149071.png";
            }

            guna2PictureBox1.SizeMode =
                PictureBoxSizeMode.StretchImage;

            guna2PictureBox1.Width = 60;
            guna2PictureBox1.Left = 2;

            guna2HtmlLabel1.Text =
                User.nome;

            guna2PictureBox1.Height = 60;

            flowLayoutPanel1.Dock =
                DockStyle.Fill;

            tableLayoutPanel1.SetRowSpan(
                Fpanel1,
                2
            );

            Display.FillColor =
                Cores.Fundo;
        }


        private void guna2CircleButton1_Click(
            object sender,
            EventArgs e)
        {
            if (
                axWindowsMediaPlayer1.playState ==
                WMPLib.WMPPlayState.wmppsPlaying
            )
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
            axWindowsMediaPlayer1.settings.volume = volume.Value;
            guna2Button1.Visible = true;
            titulolbl.Text = Musica.nome;
            imagembbar.ImageLocation = Musica.cover;
            var musica =
                await req.BuscarMusica(
                    Musica.id
                );

            string previewUrl =
                musica["preview"].ToString();

            timer1.Start();

            axWindowsMediaPlayer1.URL =
                previewUrl;

            axWindowsMediaPlayer1.Ctlcontrols.play();
        }


        private void timer1_Tick(
            object sender,
            EventArgs e)
        {
            if (
                axWindowsMediaPlayer1.currentMedia
                != null
            )
            {
                double atual =
                    axWindowsMediaPlayer1
                    .Ctlcontrols
                    .currentPosition;

                double duracao =
                    axWindowsMediaPlayer1
                    .currentMedia
                    .duration;


                guna2TrackBar1.Minimum = 0;

                guna2TrackBar1.Maximum = 29;

                guna2TrackBar1.Value =
                    (int)atual;


                if (
                    Preferences.loop &&
                    axWindowsMediaPlayer1
                    .Ctlcontrols
                    .currentPosition >= 29
                )
                {
                    TocarMusica();
                }
            }
        }


        private void guna2TrackBar1_Scroll(
            object sender,
            ScrollEventArgs e)
        {
            axWindowsMediaPlayer1
                .Ctlcontrols
                .currentPosition =
                guna2TrackBar1.Value;
        }


        private void guna2CircleButton2_Click(
            object sender,
            EventArgs e)
        {
            CarregarMusicas(
                pesquisatxt.Text
            );
        }


        int clickCount = 0;

        private void guna2Button1_Click(
            object sender,
            EventArgs e)
        {
            clickCount++;

            if (clickCount % 2 == 1)
            {
                Display2.FillColor = Cores.FundoSecundario;
                this.Visible = false;
                formtable.RowStyles[0].SizeType = SizeType.Percent;
                formtable.RowStyles[0].Height = 0f;
                formtable.RowStyles[1].SizeType = SizeType.Percent;
                formtable.RowStyles[1].Height = 0f;
                formtable.RowStyles[2].SizeType = SizeType.Percent;
                formtable.RowStyles[2].Height = 100f;

                // Define a nova porcentagem (ex: 70%)


                imagembbar.Visible = false;
                titulolbl.Visible = false;

                musicImg.Location =
                    new Point(
                        this.Width / 2 -
                        musicImg.Width / 2,
                        this.Height / 2 - musicImg.Height / 2 - 50
                    );
                lblMusicaNome.Width = 600; // Ou a largura desejada
                lblMusicaNome.TextAlignment = ContentAlignment.MiddleCenter;

                lblMusicaNome.Location = new Point(
                    (this.Width / 2) - (lblMusicaNome.Width / 2),
                    musicImg.Location.Y + musicImg.Height + 10
                );
                musicImg.Visible = true;
                lblMusicaNome.Visible = true;
                lblMusicaNome.Text =
                    Musica.nome;
                musicImg.ImageLocation =
                    Musica.cover;
            }
            else
            {
                imagembbar.Visible = true;
                titulolbl.Visible = true;

                Display2.FillColor = Color.Transparent;
                trocarPov();

                
            }
            this.Visible = true;
        }


        protected override void OnFormClosing(
            FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
        }


        private void Form1_Resize(
            object sender,
            EventArgs e)
        {
            foreach (
                Control controle
                in flowLayoutPanel1.Controls
            )
            {
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


        private void tableLayoutPanel1_Paint(
            object sender,
            PaintEventArgs e)
        {
        }

        private void guna2CustomGradientPanel1_Paint(
            object sender,
            PaintEventArgs e)
        {
        }

        private void guna2CustomGradientPanel1_Paint_1(
            object sender,
            PaintEventArgs e)
        {
        }

        private void panel3_Paint(
            object sender,
            PaintEventArgs e)
        {
        }

        private void guna2Panel1_Paint(
            object sender,
            PaintEventArgs e)
        {
        }

        private void panel3_Paint_1(
            object sender,
            PaintEventArgs e)
        {
        }

        private void flowLayoutPanel1_Paint(
            object sender,
            PaintEventArgs e)
        {
        }

        private void q(
            object sender,
            EventArgs e)
        {
        }

        private void FBiblioteca_MouseClick(
            object sender,
            MouseEventArgs e)
        {
        }

        private void guna2TextBox1_TextChanged(
            object sender,
            EventArgs e)
        {
        }

        private void guna2HtmlLabel1_Click(
            object sender,
            EventArgs e)
        {
        }

        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {
            // Limpa o painel caso já tenha outra tela aberta nele
            
        }

        private void guna2CustomGradientPanel1_Paint_2(
            object sender,
            PaintEventArgs e)
        {
        }

        private void Display_Paint(
            object sender,
            PaintEventArgs e)
        {
        }

        private void playlistForm_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2TrackBar2_Scroll(object sender, ScrollEventArgs e)
        {
            axWindowsMediaPlayer1.settings.volume = volume.Value;
        }

        private void guna2HtmlLabel2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {

        }
        private void CriarPlaylist()
        {
            // 1. Criar o painel container para o mini card
            Guna.UI2.WinForms.Guna2Panel panelCard = new Guna.UI2.WinForms.Guna2Panel();
            panelCard.Size = new System.Drawing.Size(250, 115); // Aumentei um pouco a altura para acomodar o botão de fechar
            panelCard.Location = new System.Drawing.Point(50, 50); // Defina a posição desejada no form
            panelCard.BorderRadius = 10;
            panelCard.FillColor = Cores.FundoSecundario; // Mantendo o padrão de cores que você usa
            panelCard.BorderThickness = 1;
            panelCard.BorderColor = Color.FromArgb(50, 50, 60);

            // 2. Botão de Fechar ("X") no canto superior esquerdo
            Guna.UI2.WinForms.Guna2Button btnFecharCard = new Guna.UI2.WinForms.Guna2Button();
            btnFecharCard.Text = "✕";
            btnFecharCard.Size = new System.Drawing.Size(30, 30);
            btnFecharCard.Location = new System.Drawing.Point(5, 5); // Canto superior esquerdo
            btnFecharCard.BorderRadius = 5;
            btnFecharCard.Font = new Font("Inter", 8, FontStyle.Regular);
            btnFecharCard.FillColor = Color.Transparent;
            btnFecharCard.BackColor = Color.Transparent;
            btnFecharCard.ForeColor = Color.White;
            btnFecharCard.Cursor = Cursors.Hand;

            // Ao clicar no X, remove o card da tela
            btnFecharCard.Click += (sender, e) =>
            {
                this.Controls.Remove(panelCard);
                panelCard.Dispose();
            };

            // 3. Criar o Guna2TextBox para digitar o nome da playlist
            Guna.UI2.WinForms.Guna2TextBox txtNomePlaylist = new Guna.UI2.WinForms.Guna2TextBox();
            txtNomePlaylist.PlaceholderText = "Nome da playlist";
            txtNomePlaylist.Size = new System.Drawing.Size(200, 36);
            txtNomePlaylist.BackColor = Color.Transparent;
            txtNomePlaylist.Location = new System.Drawing.Point(25, 35);
            txtNomePlaylist.BorderRadius = 10;// Ajustado para dar espaço ao botão em cima

            // 4. Criar o Guna2Button para confirmar a criação
            Guna.UI2.WinForms.Guna2Button btnSalvarPlaylist = new Guna.UI2.WinForms.Guna2Button();
            btnSalvarPlaylist.Text = "Criar";
            btnSalvarPlaylist.BackColor = Color.Transparent;
            btnSalvarPlaylist.Size = new System.Drawing.Size(180, 36);
            btnSalvarPlaylist.Location = new System.Drawing.Point(35, 75);
            btnSalvarPlaylist.BorderRadius = 10;// Ajustado para baixo

            // 5. Evento de clique do botão de criar
            btnSalvarPlaylist.Click += (sender, e) =>
            {
                string nomePlaylist = txtNomePlaylist.Text.Trim();

                if (string.IsNullOrEmpty(nomePlaylist))
                {
                    MessageBox.Show("Por favor, insira um nome para a playlist.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    // Usando o seu padrão de conexão Npgsql (visto nas funções anteriores)
                    using (NpgsqlConnection conn = conexao.Abrir())
                    {
                        conn.Open();

                        // Verificação se já existe uma playlist com esse nome para o mesmo usuário
                        string queryVerificacao = "SELECT COUNT(*) FROM playlist WHERE nome_playlist = @nome AND fk_id_user_playlist = @userId";
                        using (NpgsqlCommand cmdVerifica = new NpgsqlCommand(queryVerificacao, conn))
                        {
                            cmdVerifica.Parameters.AddWithValue("@nome", nomePlaylist);
                            cmdVerifica.Parameters.AddWithValue("@userId", User.id);

                            long count = (long)cmdVerifica.ExecuteScalar();

                            if (count > 0)
                            {
                                MessageBox.Show("Você já possui uma playlist com este nome!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }

                        // Se não existir, faz o INSERT com music_id = 0
                        string queryInsert = "INSERT INTO playlist (nome_playlist, music_playlist, fk_id_user_playlist) VALUES (@nome, 0, @userId)";
                        using (NpgsqlCommand cmdInsert = new NpgsqlCommand(queryInsert, conn))
                        {
                            cmdInsert.Parameters.AddWithValue("@nome", nomePlaylist);
                            cmdInsert.Parameters.AddWithValue("@userId", User.id);

                            cmdInsert.ExecuteNonQuery();
                            MessageBox.Show("Playlist criada com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Remove o mini card da tela após criar com sucesso
                            this.Controls.Remove(panelCard);
                            panelCard.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao criar playlist: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // Adiciona todos os controles dentro do painel
            panelCard.Controls.Add(btnFecharCard);
            panelCard.Controls.Add(txtNomePlaylist);
            panelCard.Controls.Add(btnSalvarPlaylist);

            // Adiciona o painel principal no formulário atual
            this.Controls.Add(panelCard);
            panelCard.BringToFront(); // Garante que fique visível na frente
        }
        private void guna2Button2_Click(object sender, EventArgs e)
        {
            CriarPlaylist();
        }
    }
}