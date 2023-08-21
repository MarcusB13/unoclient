
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform.Compatibility;
using SocketIOClient;
using System.Text.Json;

namespace Uno_Spil__Real_
{
    public partial class MainPage : ContentPage
    {
        private string baseUrl = "http://10.130.66.233:3000";

        private string gameCode;
        private string playerName;
        private string color;
        private string turn;
        private string topCard;
        private string unoClicked = "false";
        private bool choosingColor;

        private SocketIO client;


        public MainPage()
        {
            InitializeComponent();
            client = new SocketIO(baseUrl);

            client.On("create-game", (res) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    JsonElement data = res.GetValue();
                    JsonElement cards = data.GetProperty("cards");
                    gameCode = data.GetProperty("code").ToString();

                    Label JoinCodeLabel = (Label)FindByName("joinCodeLabel");
                    JoinCodeLabel.Text = "Join code: " + gameCode;
                    await Clipboard.Default.SetTextAsync(gameCode);
                    if (gameCode == "") { return; }
                    playerName = "player1";
                    LoadPlayerCards(playerName, cards);
                    MainMenu.IsVisible = false;
                    StartGameLayout.IsVisible = true;
                });
            });

            client.On("other-join-game", (res) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    JsonElement data = res.GetValue();
                    JsonElement cards = data.GetProperty("cards");
                    string toAddPlayer = data.GetProperty("playerName").ToString();

                    Label JoinCodeLabel = (Label)FindByName("joinCodeLabel");
                    JoinCodeLabel.Text = "Join code: " + gameCode;

                    SetPlayersPosition();
                    LoadPlayerCards(toAddPlayer, cards);
                    MainMenu.IsVisible = false;
                });
            });

            client.On("join-game", (res) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    JsonElement data = res.GetValue();
                    JsonElement cards = data.GetProperty("cards");
                    playerName = data.GetProperty("playerName").ToString();


                    Label JoinCodeLabel = (Label)FindByName("joinCodeLabel");
                    JoinCodeLabel.Text = "Join code: " + gameCode;

                    SetPlayersPosition();
                    LoadPlayerCards(playerName, cards);
                    MainMenu.IsVisible = false;
                });
            });

            client.On("set-top-card", (res) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    JsonElement data = res.GetValue();
                    topCard = data.GetProperty("card").ToString();
                    SetTopCard(topCard);
                });
            });

            client.On("set-color", (res) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    JsonElement data = res.GetValue();
                    SetColor(data.GetProperty("color").ToString());
                    turn = data.GetProperty("nextPlayer").ToString();
                    SetTurn(turn);
                });
            });

            client.On("set-turn", (res) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    JsonElement data = res.GetValue();
                    turn = data.GetProperty("whosTurn").ToString();
                    SetTurn(turn);
                });
            });

            client.On("choose-color", (res) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    chooseColor.IsVisible = true;
                    choosingColor = true;
                    DecksLayout.IsVisible = false;
                });
            });


            client.On("start-game", (res) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    DecksLayout.IsVisible = true;
                    JsonElement data = res.GetValue();
                    string dataTurn = data.GetProperty("currentPlayersTurn").ToString();
                    string dataTopCard = data.GetProperty("topCard").ToString();

                    SetTurn(dataTurn);
                    SetColor(data.GetProperty("usedCardsColor").ToString());
                    SetTopCard(dataTopCard);
                });
            });

            client.On("LoadPlayerCards", (res) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    JsonElement data = res.GetValue();
                    JsonElement cards = data.GetProperty("cards");
                    string dataPlayerName = data.GetProperty("playerName").ToString();

                    LoadPlayerCards(dataPlayerName, cards);
                });
            });

            client.On("lay-card", (res) =>
            {
                JsonElement data = res.GetValue();
                JsonElement playersNames = data.GetProperty("playersNames");
                JsonElement players = data.GetProperty("players");
                string nextPlayer = data.GetProperty("nextPlayer").ToString();
                bool displayUno = data.GetProperty("uno").GetBoolean();
                topCard = data.GetProperty("topCard").ToString();

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    SetColor(data.GetProperty("color").ToString());
                    for (int i = 0; i < playersNames.GetArrayLength(); i++)
                    {
                        string iPlayerName = playersNames[i].ToString();
                        LoadPlayerCards(iPlayerName, players.GetProperty(iPlayerName));
                        SetTopCard(topCard);
                        SetTurn(nextPlayer);
                        if (nextPlayer == playerName)
                        {
                            if (displayUno) { DisplayUno(); }
                        }
                    }
                });
            });
        }


        public void DisplayUno()
        {
            Uno.IsVisible = true;
        }

        public void SetTopCard(string card)
        {
            topCard = card;
            usedCards.Source = baseUrl + "/public/" + topCard + ".png";
        }

        public void SetColor(string thisColor)
        {
            color = thisColor;
            switch (color)
            {
                case "red":
                    currentColor.Background = Brush.Red;
                    break;

                case "blue":
                    currentColor.Background = Brush.Blue;
                    break;

                case "green":
                    currentColor.Background = Brush.Green;
                    break;

                case "yellow":
                    currentColor.Background = Brush.Yellow;
                    break;
            }
        }

        public void SetTurn(string playersTurn)
        {
            turn = playersTurn;
            for(int i = 1; i<5; i++)
            {
                Microsoft.Maui.Controls.Shapes.Rectangle rect = this.FindByName<Microsoft.Maui.Controls.Shapes.Rectangle>("player" + i + "Turn");
                rect.IsVisible = false;
            }

            Microsoft.Maui.Controls.Shapes.Rectangle turnRect = this.FindByName<Microsoft.Maui.Controls.Shapes.Rectangle>(turn + "Turn");
            turnRect.IsVisible = true;
        }


        public void SetPlayersPosition()
        {
            if (playerName == "player1") return;

            HorizontalStackLayout playerLayout = this.FindByName<HorizontalStackLayout>(playerName);
            int currentRow = Grid.GetRow(playerLayout);
            int currentColumn = Grid.GetColumn(playerLayout);
            double currentRotation = playerLayout.Rotation;

            Microsoft.Maui.Controls.Shapes.Rectangle rect = this.FindByName<Microsoft.Maui.Controls.Shapes.Rectangle>(playerName+"Turn");
            int currentRowRect = Grid.GetRow(rect);
            int currentColumnRect = Grid.GetColumn(rect);
            if (currentRow == 5 && currentColumn == 1)
            {
                return;
            }

            HorizontalStackLayout playerToChangeWithLayout = this.FindByName<HorizontalStackLayout>("player1");
            int player1Row = Grid.GetRow(playerToChangeWithLayout);
            int player1Column = Grid.GetColumn(playerToChangeWithLayout);
            double player1Rotation = playerToChangeWithLayout.Rotation;

            Microsoft.Maui.Controls.Shapes.Rectangle rectplayer1 = this.FindByName<Microsoft.Maui.Controls.Shapes.Rectangle>("player1Turn");
            int currentRowRectplayer1 = Grid.GetRow(rectplayer1);
            int currentColumnRectplayer1 = Grid.GetColumn(rectplayer1);

            Grid.SetRow(playerLayout, player1Row);
            Grid.SetColumn(playerLayout, player1Column);
            playerLayout.Rotation = player1Rotation;
            Grid.SetRow(rect, currentColumnRectplayer1);
            Grid.SetColumn(rect, currentRowRectplayer1);

            Grid.SetRow(playerToChangeWithLayout, currentRow);
            Grid.SetColumn(playerToChangeWithLayout, currentColumn);
            playerToChangeWithLayout.Rotation = currentRotation;
            Grid.SetRow(rectplayer1, currentRowRect);
            Grid.SetColumn(rectplayer1, currentColumnRect);
        }


        private void LoadPlayerCards(string toAddPlayerName, JsonElement cards)
        {
            HorizontalStackLayout playerLayout = this.FindByName<HorizontalStackLayout>(toAddPlayerName);
            playerLayout.Children.Clear();

            for (int cardNumber = 0; cardNumber < cards.GetArrayLength(); cardNumber++)
            {
                string cardName = cards[cardNumber].ToString();
                if (toAddPlayerName != playerName)
                {
                    cardName = "back";
                }

                ImageButton img = new ImageButton
                {
                    Source = baseUrl + "/public/" + cardName + ".png",
                    IsVisible = true,
                    WidthRequest = 90,
                    HeightRequest = 150,
                    Aspect=Aspect.AspectFit,
                    ClassId = cardName
                };
                img.Clicked += (sender, e) =>
                {
                    if (turn != playerName || choosingColor) { return; }
                    string card = ((ImageButton)sender).ClassId;
                    string cardColor = "";
                    
                    if (card.Contains("blue")){ cardColor = "blue"; }
                    if (card.Contains("yellow")){ cardColor = "yellow"; }
                    if (card.Contains("red")){ cardColor = "red"; }
                    if (card.Contains("green")){ cardColor = "green"; }

                    string topCardName = topCard.Replace("green", "").Replace("red", "").Replace("blue", "").Replace("yellow", "");
                    string CardNamed = cardName.Replace("green", "").Replace("red", "").Replace("blue", "").Replace("yellow", "");

                    if (color != cardColor && CardNamed != topCardName && CardNamed != "wild" && CardNamed != "draw4")
                    {
                        return;
                    }

                    Dictionary<string, string> data = new Dictionary<string, string> {
                        { "player", playerName },
                        { "card",  cardName },
                        { "color", cardColor},
                        { "unoClicked", unoClicked}
                    };
                    client.EmitAsync("lay-card", data);
                };
                playerLayout.Add(img);
            }
        }


        private async void startTheGameClicked(object sender, EventArgs e)
        {
            if (playerName != "player1") return;

            StartGameLayout.IsVisible = false;
            await client.EmitAsync("start-game", gameCode);
        }

        public void drawCard(object sender, EventArgs e)
        {
            if (turn != playerName || choosingColor) { return; }
            client.EmitAsync("draw-card", playerName);
        }

        private async void CreateGameClicked(object sender, EventArgs e)
        {
            await client.ConnectAsync();
            await client.EmitAsync("create-game", "");
        }


        private async void JoinGameClicked(object sender, EventArgs e)
        {
            await client.ConnectAsync();
            gameCode = joinCode.Text;
            await client.EmitAsync("join-game", gameCode);
        }

        private void UnoClicked(object sender, EventArgs e)
        {
            Uno.IsVisible = false;
            unoClicked = "true";
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                Thread.Sleep(5000);
                unoClicked = "false";
            });
        }

        private void GreenClicked(object sender, EventArgs e)
        {
            colorClicked("green");
        }

        private void BlueClicked(object sender, EventArgs e)
        {
            colorClicked("blue");
        }

        private void YellowClicked(object sender, EventArgs e)
        {
            colorClicked("yellow");
        }

        private void RedClicked(object sender, EventArgs e)
        {
            colorClicked("red");
        }


        private void colorClicked(string color)
        {
            choosingColor = false;
            chooseColor.IsVisible = false;
            DecksLayout.IsVisible = true;
            Dictionary<string, string> data = new Dictionary<string, string> {
                { "player", playerName },
                { "color", color}
            };
            client.EmitAsync("choose-color", data);
        }
    }
}