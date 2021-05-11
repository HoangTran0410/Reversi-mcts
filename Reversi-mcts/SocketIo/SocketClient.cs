using System;
using System.Threading;
using System.Threading.Tasks;
using SocketIOClient;

namespace Reversi_mcts.Client
{
    public class SocketClient
    {
        private const string ServerIp = "http://localhost:3000/";
        private const string ClientName = "newbitboard";

        private GameHandler _game;
        private SocketIO _client;
        private ManualResetEvent _manualResetEvent;

        public SocketClient()
        {
            _game = new GameHandler();
            _client = new SocketIO(ServerIp, new SocketIOOptions {EIO = 4});

            _client.OnConnected += OnConnected;
            _client.On("serverSendColor", OnServerSendColor);
            _client.On("serverSendWaitForMove", OnServerSendWaitForMove);
            _client.On("serverSendOpponentMove", OnServerSendOpponentMove);
            _client.On("serverSendResult", OnServerSendResult);
            _client.On("serverSendEndGame", OnServerSendEndGame);

            Console.WriteLine("connecting...");
            _client.ConnectAsync();

            _manualResetEvent = new ManualResetEvent(false);
            _manualResetEvent.WaitOne();
        }

        private void OnConnected(object sender, EventArgs e)
        {
            Console.WriteLine("Connected");
        }

        private async void OnServerSendColor(SocketIOResponse response)
        {
            var colorStr = response.GetValue<string>();
            _game.SetColor(colorStr.ToLower().Equals("black") ? Constant.Black : Constant.White);
            
            Console.WriteLine("You are " + colorStr.ToUpper());

            await _client.EmitAsync("clientSendReady", ClientName);
        }

        private async void OnServerSendWaitForMove(SocketIOResponse response)
        {
            await PerformAiMove();
        }

        private async void OnServerSendOpponentMove(SocketIOResponse response)
        {
            // nếu không có giá trị (undefined) thì nó trả về 0;
            var row = response.GetValue().GetProperty("row").GetInt32();
            var col = response.GetValue().GetProperty("col").GetInt32();
            var thinkingTime = response.GetValue().GetProperty("thinkingTime").GetDouble();
            var isPassed = response.GetValue().GetProperty("isPassed").GetBoolean();
            var requireNextMove = response.GetValue().GetProperty("requireNextMove").GetBoolean();

            if (isPassed)
            {
                Console.WriteLine("Opponent Passed.");
            }
            else
            {
                Console.WriteLine("Opponent move " + row + "," + col);
            }

            _game.MakeMove(row, col);

            if (requireNextMove)
            {
                await PerformAiMove();
            }
        }

        private void OnServerSendResult(SocketIOResponse response)
        {
            var result = response.GetValue<string>();
            Console.WriteLine(result + "\n\n");

            _game.Restart(); // reset game
        }

        private void OnServerSendEndGame(SocketIOResponse response)
        {
            Console.WriteLine("\nEnd Game.\n");
            _manualResetEvent.Set();
        }

        private async Task PerformAiMove()
        {
            Console.WriteLine(" ~ Thinking...");
            var isPassed = false;

            var startTime = DateTime.Now;
            var (row, col) = _game.PerformAiMove();
            var endTime = DateTime.Now;

            if (row == -1 || col == -1)
            {
                isPassed = true;
                Console.WriteLine("Me PASSED.");
            }
            else
            {
                Console.WriteLine("Me move at " + row + "," + col);
            }

            await _client.EmitAsync("clientSendMove", new
            {
                col,
                row,
                thinkingTime = (float) Math.Round((endTime - startTime).TotalSeconds, 3),
                isPassed,
                playoutCount = _game.GetLastPlayout()
            });

            Console.WriteLine(" ~ Waiting for opponent move...");
        }
    }
}