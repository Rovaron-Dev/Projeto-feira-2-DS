// =====================================================================================
// DOCUMENTAÇÃO: Form_Playlist.cs  (Projeto da Feira)
// -------------------------------------------------------------------------------------
// VISÃO GERAL
// Este Form (janela) exibe UMA playlist musical. Ele:
//   1. Recebe nome, id, lista de ids de músicas e imagem da playlist pelo construtor;
//   2. Pinta o painel de cabeçalho com a cor predominante da capa;
//   3. Consulta a API pública do Deezer para obter os dados de cada música;
//   4. Cria dinamicamente um "card" clicável por música dentro de um FlowLayoutPanel;
//   5. Avisa o Form pai (via eventos) quando o usuário clica em uma música ou no botão voltar.
//
// ESTRUTURA DO ARQUIVO
//   - Conexao (classe aninhada)  -> string de conexão com o PostgreSQL (Supabase)
//   - Deezer  (classe aninhada)  -> cliente HTTP para a API do Deezer
//   - Playlist / Cores (classes estáticas aninhadas) -> dados da playlist e paleta de cores
//   - Form_Playlist              -> lógica da tela
//
// ATENÇÃO (segurança): o código original tinha a senha do banco escrita no código-fonte.
// Nesta cópia ela foi substituída por um placeholder. Troque a senha no painel do Supabase
// e passe a lê-la de variável de ambiente ou arquivo de configuração fora do repositório.
// =====================================================================================

// --- IMPORTAÇÕES (using) ---
using Guna.UI2.WinForms;                 // Controles da biblioteca Guna UI2 (Guna2Panel, Guna2PictureBox, etc.)
using Newtonsoft.Json.Linq;              // JObject: permite ler o JSON retornado pela API como objeto navegável
using Npgsql;                            // Driver ADO.NET para PostgreSQL (NpgsqlConnection)
using System;                            // Tipos básicos: EventArgs, Uri, Exception, Action...
using System.Collections.Generic;        // List<T>
using System.ComponentModel;             // Suporte a componentes/designer do WinForms
using System.Data;                       // Tipos ADO.NET genéricos (não usado diretamente aqui)
using System.Drawing;                    // Color, Point, Font, Bitmap, FontStyle
using System.Linq;                       // Métodos LINQ (não usado diretamente aqui)
using System.Net.Http;                   // HttpClient: faz as requisições HTTP
using System.Security.Policy;            // (Não utilizado; pode ser removido)
using System.Text;                       // Utilidades de texto (não usado diretamente aqui)
using System.Threading.Tasks;            // Task / async / await
using System.Windows.Forms;              // Form, Label, TableLayoutPanel, FlowLayoutPanel, etc.
using static Projeto_da_feira.Form1;     // Importa membros estáticos de Form1 (não utilizado neste arquivo; pode ser removido)

// Namespace (agrupador lógico) do projeto.
namespace Projeto_da_feira
{
    // Classe parcial: a outra metade (Designer.cs) contém os controles criados no editor visual
    // (guna2PictureBox1, tableLayoutPanel1, flowLayoutPanel1, label_nomePlaylist, etc.).
    // Herda de Form, ou seja, é uma janela.
    public partial class Form_Playlist : Form
    {
        // =============================================================================
        // CLASSE Conexao — acesso ao banco PostgreSQL (Supabase)
        // OBS: nesta tela ela é instanciada, mas nenhum método do banco é chamado.
        // =============================================================================
        public class Conexao
        {
            // String de conexão. Cada parte separada por ";" é um parâmetro.
            private string connectionString =
                "Host=db.zjlnoxudmxjanibkptpf.supabase.co;Port=5432;" + // Servidor e porta padrão do PostgreSQL
                "Database=postgres;" +                                  // Nome do banco
                "Username=postgres;" +                                  // Usuário
                "Password=<SENHA-REDIGIDA>;" +                          // Senha (NUNCA deixe no código!)
                "SSL Mode=Require;" +                                   // Exige conexão criptografada (SSL)
                "Trust Server Certificate=true";                        // Aceita o certificado do servidor sem validar a cadeia


            // Cria (mas NÃO abre) uma conexão. Apesar do nome "Abrir", quem chama
            // precisa executar .Open() / .OpenAsync() depois.
            public NpgsqlConnection Abrir()
            {
                return new NpgsqlConnection(connectionString);
            }
        }

        // Instância do cliente Deezer usada por todo o Form (campo da classe).
        Deezer deezer = new Deezer();

        // =============================================================================
        // CLASSE Deezer — cliente da API pública https://api.deezer.com
        // Todos os métodos montam uma URL, fazem GET e devolvem o JSON como JObject.
        // =============================================================================
        public class Deezer
        {
            // HttpClient reutilizável para todas as requisições desta instância.
            private readonly HttpClient client;

            // Endereço base da API; constante, não muda em tempo de execução.
            private const string BaseUrl =
                "https://api.deezer.com";

            // Construtor: cria o HttpClient.
            public Deezer()
            {
                client = new HttpClient();
            }


            // Pesquisa músicas por texto livre. GET /search?q=texto
            public async Task<JObject> PesquisarMusicas(string pesquisa)
            {
                // EscapeDataString codifica caracteres especiais (espaço -> %20, & -> %26...)
                string query =
                    Uri.EscapeDataString(pesquisa);

                string url =
                    $"{BaseUrl}/search?q={query}";

                return await FazerRequisicao(url);
            }


            // Busca UMA música pelo id. GET /track/{id}
            // É o método usado por CarregarMusicas().
            public async Task<JObject> BuscarMusica(long id)
            {
                string url =
                    $"{BaseUrl}/track/{id}";

                return await FazerRequisicao(url);
            }


            // Pesquisa artistas por nome. GET /search/artist?q=texto
            public async Task<JObject> PesquisarArtistas(string pesquisa)
            {
                string query =
                    Uri.EscapeDataString(pesquisa);

                string url =
                    $"{BaseUrl}/search/artist?q={query}";

                return await FazerRequisicao(url);
            }


            // Busca dados de um artista pelo id. GET /artist/{id}
            public async Task<JObject> BuscarArtista(long id)
            {
                string url =
                    $"{BaseUrl}/artist/{id}";

                return await FazerRequisicao(url);
            }


            // Busca as músicas mais tocadas de um artista (padrão: 30). GET /artist/{id}/top?limit=N
            public async Task<JObject> BuscarTopArtista(
                long id,
                int limite = 30)
            {
                string url =
                    $"{BaseUrl}/artist/{id}/top?limit={limite}";

                return await FazerRequisicao(url);
            }


            // Lista os álbuns de um artista. GET /artist/{id}/albums
            public async Task<JObject> BuscarAlbunsArtista(long id)
            {
                string url =
                    $"{BaseUrl}/artist/{id}/albums";

                return await FazerRequisicao(url);
            }


            // Pesquisa álbuns por nome. GET /search/album?q=texto
            public async Task<JObject> PesquisarAlbuns(string pesquisa)
            {
                string query =
                    Uri.EscapeDataString(pesquisa);

                string url =
                    $"{BaseUrl}/search/album?q={query}";

                return await FazerRequisicao(url);
            }


            // Busca dados de um álbum pelo id. GET /album/{id}
            public async Task<JObject> BuscarAlbum(long id)
            {
                string url =
                    $"{BaseUrl}/album/{id}";

                return await FazerRequisicao(url);
            }


            // Lista as faixas de um álbum. GET /album/{id}/tracks
            public async Task<JObject> BuscarMusicasAlbum(long id)
            {
                string url =
                    $"{BaseUrl}/album/{id}/tracks";

                return await FazerRequisicao(url);
            }


            // Pesquisa playlists do Deezer por nome. GET /search/playlist?q=texto
            public async Task<JObject> PesquisarPlaylists(string pesquisa)
            {
                string query =
                    Uri.EscapeDataString(pesquisa);

                string url =
                    $"{BaseUrl}/search/playlist?q={query}";

                return await FazerRequisicao(url);
            }


            // Busca dados de uma playlist do Deezer pelo id. GET /playlist/{id}
            public async Task<JObject> BuscarPlaylist(long id)
            {
                string url =
                    $"{BaseUrl}/playlist/{id}";

                return await FazerRequisicao(url);
            }


            // Lista as faixas de uma playlist do Deezer. GET /playlist/{id}/tracks
            public async Task<JObject> BuscarMusicasPlaylist(long id)
            {
                string url =
                    $"{BaseUrl}/playlist/{id}/tracks";

                return await FazerRequisicao(url);
            }


            // Lista todos os gêneros musicais. GET /genre
            public async Task<JObject> BuscarGeneros()
            {
                string url =
                    $"{BaseUrl}/genre";

                return await FazerRequisicao(url);
            }


            // Busca um gênero específico pelo id. GET /genre/{id}
            public async Task<JObject> BuscarGenero(long id)
            {
                string url =
                    $"{BaseUrl}/genre/{id}";

                return await FazerRequisicao(url);
            }


            // Busca música pelo nome (mesmo endpoint de PesquisarMusicas; é redundante).
            public async Task<JObject> BuscarMusicaNome(string nome)
            {
                string pesquisaUrl =
                    Uri.EscapeDataString(nome);

                string url =
                    $"{BaseUrl}/search?q={pesquisaUrl}";

                return await FazerRequisicao(url);
            }


            // Top 30 músicas do chart geral (id 0 = todos os gêneros). GET /chart/0/tracks?limit=30
            public async Task<JObject> BuscarTop30Musicas()
            {
                string url =
                    $"{BaseUrl}/chart/0/tracks?limit=30";

                return await FazerRequisicao(url);
            }


            // Top 30 artistas do chart geral. GET /chart/0/artists?limit=30
            public async Task<JObject> BuscarTop30Artistas()
            {
                string url =
                    $"{BaseUrl}/chart/0/artists?limit=30";

                return await FazerRequisicao(url);
            }


            // Top 30 álbuns do chart geral. GET /chart/0/albums?limit=30
            public async Task<JObject> BuscarTop30Albuns()
            {
                string url =
                    $"{BaseUrl}/chart/0/albums?limit=30";

                return await FazerRequisicao(url);
            }


            // Top 30 músicas de um gênero específico. GET /chart/{idGenero}/tracks?limit=30
            public async Task<JObject> BuscarTop30Genero(long id)
            {
                string url =
                    $"{BaseUrl}/chart/{id}/tracks?limit=30";

                return await FazerRequisicao(url);
            }


            // Método central (privado) que TODOS os outros usam para chamar a API.
            private async Task<JObject> FazerRequisicao(string url)
            {
                // Faz o GET e lê o corpo da resposta como texto (sem travar a interface).
                string json =
                    await client.GetStringAsync(url);

                // Converte o texto JSON em JObject para acessar campos como obj["title"].
                return JObject.Parse(json);
            }
        }

        // Instância da classe de conexão com o banco (atualmente sem uso nesta tela).
        Conexao conexao = new Conexao();

        // =============================================================================
        // CONSTRUTOR — chamado pelo Form pai ao abrir a tela de uma playlist
        // Parâmetros: nome da playlist, id, lista de ids do Deezer das músicas e URL da capa.
        // =============================================================================
        public Form_Playlist(string nome, long id, List<long> musicas, string imagem)
        {
            // Cria todos os controles desenhados no editor visual (Designer).
            InitializeComponent();

            // Guarda os dados recebidos na classe estática Playlist,
            // acessível por qualquer método deste Form.
            Playlist.id = id;
            Playlist.musicas = musicas;
            Playlist.nome = nome;
            Playlist.imagem = imagem;

            // Configura o visual do cabeçalho.
            // Não usa await: o método roda de forma assíncrona e o construtor segue sem esperar.
            startGraph();

        }

        // =============================================================================
        // startGraph — configura a aparência do cabeçalho da playlist
        // =============================================================================
        private async Task startGraph()
        {
            // Posiciona a imagem usando o construtor Point(int dw), que interpreta o número
            // como coordenadas "empacotadas" (X = 16 bits baixos, Y = 16 bits altos).
            // Na prática vira Point(Height/2, 0). Provável erro: era para ser Point(x, y).
            // Esta posição é sobrescrita logo abaixo

            // Carrega a capa da playlist a partir da URL (download automático pelo controle).
            guna2PictureBox1.ImageLocation = Playlist.imagem;
            guna2PictureBox1.SizeMode = PictureBoxSizeMode.StretchImage; // Estica a imagem para preencher o controle
            tableLayoutPanel2.Margin = new Padding(50, 0, 0, 0);

            // Mostra o nome da playlist no rótulo do cabeçalho.
            label_nomePlaylist.Text = Playlist.nome;

            // Só calcula a cor de fundo se existir uma imagem.
            if (Playlist.imagem != null)
            {
                // Aguarda (sem travar a tela) o cálculo da cor média da capa e a aplica
                // como cor de preenchimento do painel em degradê. (Havia ";;" duplicado.)
                guna2GradientPanel1.FillColor = await ObterCorPredominanteAsync(Playlist.imagem);
            }

            // Reposiciona a imagem: X = 0 e Y = metade da largura do painel menos metade da
            // largura da imagem. Provavelmente os eixos estão trocados (o cálculo de
            // "centralizar" deveria ser no X). Dentro de um TableLayoutPanel a Location
            // costuma ser ignorada, pois o layout define a posição

            // Cor de fundo da janela: tom escuro secundário da paleta.
            BackColor = Cores.FundoSecundario;

            // Move a imagem da playlist para o centro do painel, considerando a altura do painel
            // e a altura da imagem: calcula um espaçamento superior igual a 1/16 da sobra de altura

            // Aplica esse espaçamento como margem superior da imagem (esquerda, topo, direita, ba

        }

        // =============================================================================
        // PROPRIEDADES PÚBLICAS — o Form pai lê estes valores ao receber o evento "tocarmusica"
        // =============================================================================

        // ID da música atualmente selecionada/ativa (por instância).
        public long idMusicaAtual { get; set; } = 0;

        // Título da música clicada.
        public string nome { get; set; } = "";

        // URL da capa da música clicada (apesar do nome "url", guarda a imagem).
        public string url { get; set; } = "";

        // =============================================================================
        // CLASSE ESTÁTICA Playlist — dados da playlist exibida
        // ATENÇÃO: por ser estática, é compartilhada por TODAS as instâncias do Form.
        // Se abrir duas playlists ao mesmo tempo, os dados se sobrescrevem.
        // =============================================================================
        public static class Playlist
        {
            public static string nome;          // Nome da playlist
            public static long id;              // ID da playlist (no seu banco)

            public static List<long> musicas;   // IDs das músicas (IDs do Deezer)

            public static string imagem;        // URL da capa da playlist

        }

        // =============================================================================
        // CLASSE ESTÁTICA Cores — paleta de cores do tema escuro do aplicativo
        // =============================================================================
        public static class Cores
        {
            // Fundo principal (quase preto). FromArgb(alfa, R, G, B); alfa 255 = opaco.
            public static Color Fundo =
                Color.FromArgb(255, 5, 5, 7);

            // Fundo secundário (cinza-azulado escuro), usado na janela e nos cards.
            public static Color FundoSecundario =
                Color.FromArgb(255, 19, 18, 26);

            // Cor de destaque (roxo).
            public static Color Roxo =
                Color.FromArgb(255, 101, 95, 188);
        }

        // =============================================================================
        // EVENTO LOAD — disparado quando a janela é carregada pela primeira vez
        // =============================================================================
        private void Form_Playlist_Load(object sender, EventArgs e)
        {
            // Inicia o carregamento das músicas da playlist.
            CarregarMusicas();
        }


        // =============================================================================
        // CarregarMusicas — busca cada música na API e cria seu card
        // "async void" é aceitável aqui por ser handler de evento, mas exceções não
        // tratadas podem derrubar o app (não há try/catch).
        // =============================================================================
        private async void CarregarMusicas()
        {
            // Percorre cada id de música da playlist, em ordem.
            foreach (long idMusica in Playlist.musicas)
            {
                // Consulta a API do Deezer (uma requisição por música, em sequência).
                JObject musica = await deezer.BuscarMusica(idMusica);

                // Extrai os campos do JSON:
                long id = (long)musica["id"];                                  // id da faixa
                string titulo = musica["title"].ToString();                    // título
                string artista = musica["artist"]["name"].ToString();          // nome do artista
                string album = musica["album"]["title"].ToString();            // título do álbum
                string imagem = musica["album"]["cover_medium"].ToString();    // URL da capa (tamanho médio)

                // Cria um novo controle de música e adiciona ao FlowLayoutPanel.
                CardMusica(id, titulo, artista, album, imagem);

            }
        }





        // =============================================================================
        // CardMusica — monta, por código, uma linha (card) de música na lista
        // Estrutura: Guna2Panel (moldura) > TableLayoutPanel (5 colunas) > controles.
        // O parâmetro "duracao" tem valor padrão fixo "0.29" (não vem da API; o Deezer
        // retorna o campo "duration" em segundos, que poderia ser formatado mm:ss).
        // =============================================================================
        private void CardMusica(long id, string titulo, string artista, string album, string imagem, string duracao = "0.29")
        {
            // Painel externo do card (fundo arredondado).
            Guna2Panel panel = new Guna2Panel();

            // Nome único do controle, útil para localizá-lo depois.
            panel.Name = "Xcardmusic" + id;

            // Usamos a largura do flowLayoutPanel1 descontando uma margem segura para evitar quebra de linha.
            panel.Width = flowLayoutPanel1.Width - 30;

            // Altura fixa de cada card.
            panel.Height = 85;

            // Espaço externo entre cards (esq, topo, dir, base).
            panel.Margin = new Padding(5, 15, 5, 0);

            // Espaço interno do painel (esq, topo, dir, base).
            panel.Padding = new Padding(0, 2, 50, 2);

            // Cor de fundo do card (tom secundário da paleta).
            panel.FillColor = Cores.FundoSecundario;

            // Cantos arredondados (8 px).
            panel.BorderRadius = 8;

            // Sem borda.
            panel.BorderThickness = 0;

            // Grade interna que organiza o conteúdo em colunas.
            TableLayoutPanel grid = new TableLayoutPanel();

            // Preenche todo o painel pai.
            grid.Dock = DockStyle.Fill;

            // 1 linha e 5 colunas.
            grid.RowCount = 1;
            grid.ColumnCount = 5;

            // Fundo transparente para mostrar a cor do card.
            grid.BackColor = Color.Transparent;

            // Sem margem externa.
            grid.Margin = new Padding(0);

            // Recuo interno de 50 px nas laterais.
            grid.Padding = new Padding(50, 0, 50, 0);

            // Definindo as colunas da lista horizontal
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 75F));  // 1. Capa (65 px fixos)
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));   // 2. Título (35% do espaço restante)
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));   // 3. Artista (25%)
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));   // 4. Álbum (30%)
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));  // 5. Duração (50 px fixos)

            // A única linha ocupa 100% da altura.
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // --- Capa do álbum ---
            Guna2PictureBox pictureBox = new Guna2PictureBox();
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage; // Estica a imagem para preencher o controle
            pictureBox.Dock = DockStyle.Fill;                      // Ocupa toda a célula
            pictureBox.Name = "pictureBox" + id;                   // Nome único
            pictureBox.ImageLocation = imagem;                     // URL da capa (baixada automaticamente)
            pictureBox.BorderRadius = 6;                           // Cantos arredondados
            pictureBox.Margin = new Padding(0);                    // Sem margem

            // --- Título da música ---
            Label labelNome = new Label();
            labelNome.Text = titulo;                               // Texto exibido
            labelNome.Font = new Font("Inter", 15, FontStyle.Bold);// Fonte Inter, 10 pt, negrito
            labelNome.Padding = new Padding(10, 0, 0, 0);                  // Recuo à esquerda de 10 px
            labelNome.ForeColor = Color.White;                     // Texto branco
            labelNome.Dock = DockStyle.Fill;                       // Ocupa a célula toda
            labelNome.TextAlign = ContentAlignment.MiddleLeft;     // Alinhado à esquerda, centralizado na vertical
            labelNome.AutoEllipsis = true;                         // Corta com "..." se o texto não couber

            // --- Nome do artista ---
            Label labelArtist = new Label();
            labelArtist.Text = artista;
            labelArtist.Font = new Font("Inter", 10, FontStyle.Regular);
            labelArtist.ForeColor = Color.DarkGray;                // Cinza: destaque menor que o título
            labelArtist.Dock = DockStyle.Fill;
            labelArtist.TextAlign = ContentAlignment.MiddleLeft;
            labelArtist.AutoEllipsis = true;

            // --- Nome do álbum ---
            Label labelAlbum = new Label();
            labelAlbum.Text = album;
            labelAlbum.Font = new Font("Inter", 10, FontStyle.Regular);
            labelAlbum.ForeColor = Color.DarkGray;
            labelAlbum.Dock = DockStyle.Fill;
            labelAlbum.TextAlign = ContentAlignment.MiddleLeft;
            labelAlbum.AutoEllipsis = true;

            // --- Duração ---
            Label labelDuracao = new Label();
            labelDuracao.Text = duracao;
            labelDuracao.Font = new Font("Inter", 10, FontStyle.Regular);
            labelDuracao.ForeColor = Color.DarkGray;
            labelDuracao.Dock = DockStyle.Fill;
            labelDuracao.TextAlign = ContentAlignment.MiddleRight; // Alinhada à direita

            // Função local (closure): captura id, titulo e imagem deste card específico.
            // Executada quando o usuário clica em qualquer parte do card.
            void CliqueCard(object sender, EventArgs e)
            {
                idMusicaAtual = id;                                // Registra qual música foi clicada
                nome = titulo;                                     // Registra o título
                url = imagem;                                      // Registra a capa
                tocarmusica?.Invoke(this, EventArgs.Empty);        // Avisa o Form pai (só se houver assinante)
            }

            // O clique precisa ser ligado em CADA controle, pois os filhos "cobrem" o painel
            // e recebem o clique no lugar dele.
            panel.Click += CliqueCard;
            grid.Click += CliqueCard;
            labelNome.Click += CliqueCard;
            labelArtist.Click += CliqueCard;
            labelAlbum.Click += CliqueCard;
            labelDuracao.Click += CliqueCard;
            pictureBox.Click += CliqueCard;

            // Cursor de "mãozinha" em todos os controles, indicando que são clicáveis.
            panel.Cursor = Cursors.Hand;
            grid.Cursor = Cursors.Hand;
            labelAlbum.Cursor = Cursors.Hand;
            labelNome.Cursor = Cursors.Hand;
            labelArtist.Cursor = Cursors.Hand;
            labelDuracao.Cursor = Cursors.Hand;
            pictureBox.Cursor = Cursors.Hand;

            // Insere cada controle na grade: Add(controle, coluna, linha).
            grid.Controls.Add(pictureBox, 0, 0);   // Coluna 0: capa
            grid.Controls.Add(labelNome, 1, 0);    // Coluna 1: título
            grid.Controls.Add(labelArtist, 2, 0);  // Coluna 2: artista
            grid.Controls.Add(labelAlbum, 3, 0);   // Coluna 3: álbum
            grid.Controls.Add(labelDuracao, 4, 0); // Coluna 4: duração

            // Coloca a grade dentro do painel do card.
            panel.Controls.Add(grid);

            // Retornando para o uso do seu flowLayoutPanel1 original: adiciona o card à lista visível.
            flowLayoutPanel1.Controls.Add(panel);
        }

        // HttpClient estático e compartilhado, exclusivo do cálculo de cor.
        // (Estático é a prática recomendada: evita esgotar sockets ao criar vários HttpClient.)
        private static readonly HttpClient httpClient = new HttpClient();

        // =============================================================================
        // ObterCorPredominanteAsync — calcula a COR MÉDIA de uma imagem da web
        // Retorna Color.Black se a URL for vazia ou se ocorrer erro.
        // =============================================================================
        public async Task<Color> ObterCorPredominanteAsync(string urlImagem)
        {
            // Se não há URL, devolve preto imediatamente.
            if (string.IsNullOrEmpty(urlImagem))
                return Color.Black;

            try
            {
                // 1. Baixa a imagem da web diretamente para a memória (Stream) sem bloquear o disco
                using (var stream = await httpClient.GetStreamAsync(urlImagem))
                // Cria um Bitmap a partir do stream (liberado automaticamente pelo using).
                using (Bitmap bitmap = new Bitmap(stream))
                {
                    // Acumuladores das somas de cada canal de cor (long evita estouro de int).
                    long rTotal = 0;
                    long gTotal = 0;
                    long bTotal = 0;

                    // Quantidade de pixels considerados na média.
                    long totalPixels = 0;

                    // Dica de ouro: para imagens maiores (ex: 250x250), pular de 10 em 10 acelera muito
                    // sem perder a precisão da cor predominante.
                    for (int x = 0; x < bitmap.Width; x += 10)
                    {
                        for (int y = 0; y < bitmap.Height; y += 10)
                        {
                            // Lê a cor do pixel na posição (x, y).
                            Color corPixel = bitmap.GetPixel(x, y);

                            // Ignora pixels quase transparentes (alfa <= 10).
                            if (corPixel.A > 10)
                            {
                                rTotal += corPixel.R;   // Soma o vermelho
                                gTotal += corPixel.G;   // Soma o verde
                                bTotal += corPixel.B;   // Soma o azul
                                totalPixels++;          // Conta o pixel
                            }
                        }
                    }

                    // Se todos os pixels eram transparentes, evita divisão por zero.
                    if (totalPixels == 0) return Color.Black;

                    // Média de cada canal = soma / quantidade de pixels.
                    int mediaR = (int)(rTotal / totalPixels);
                    int mediaG = (int)(gTotal / totalPixels);
                    int mediaB = (int)(bTotal / totalPixels);

                    // Monta e devolve a cor resultante.
                    return Color.FromArgb(mediaR, mediaG, mediaB);
                }
            }
            catch (Exception ex)
            {
                // Registra o erro na janela de saída de depuração (não aparece para o usuário).
                System.Diagnostics.Debug.WriteLine("Erro ao calcular cor da imagem: " + ex.Message);
                return Color.Black; // Cor padrão caso dê erro na web
            }
        }

        // Handler vazio (criado pelo designer): nenhuma lógica personalizada de desenho.
        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        // =============================================================================
        // EVENTOS PÚBLICOS — canal de comunicação com o Form pai
        // O pai assina (+=) estes eventos para reagir às ações desta tela.
        // =============================================================================

        // Disparado ao clicar no botão de voltar.
        public event EventHandler BotaoFoiClicado;

        // Disparado ao clicar em um card de música (o pai lê idMusicaAtual, nome e url).
        public event EventHandler tocarmusica;

        // Clique do botão guna2Button1 (voltar/fechar).
        private void guna2Button1_Click(object sender, EventArgs e)
        {
            // Avisa o pai (só se alguém estiver ouvindo o evento).
            BotaoFoiClicado?.Invoke(this, EventArgs.Empty);

            // Fecha esta janela.
            this.Close();
        }

        // Handlers abaixo: vazios, gerados pelo designer ao clicar duas vezes nos controles.
        // Podem ser removidos (junto com a ligação no Designer.cs) se não forem usados.
        private void guna2GradientPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label_nomePlaylist_Click(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Button1_Click_1(object sender, EventArgs e)
        {
            
        }
    }
}