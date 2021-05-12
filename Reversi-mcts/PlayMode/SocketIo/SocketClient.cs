using System;
using System.Threading;
using System.Threading.Tasks;
using SocketIOClient;

namespace Reversi_mcts.PlayMode.SocketIo
{
    public class SocketClient
    {
        private const string ServerIp = "http://localhost:3000/";
        
        private readonly GameHandler _game;
        private readonly SocketIO _client;
        private readonly ManualResetEvent _manualResetEvent;

        private readonly string _clientName = "reversi-mcts";

        public SocketClient(int timeout = 100)
        {
            _clientName += (timeout*1.0/1000) + "s";
            
            _game = new GameHandler(timeout);
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
            Console.WriteLine("You are " + colorStr.ToUpper());
            
            //_game.SetColor(colorStr.ToLower().Equals("black") ? Constant.Black : Constant.White); // we dont need this

            await _client.EmitAsync("clientSendReady", _clientName);
        }

        private async void OnServerSendWaitForMove(SocketIOResponse response)
        {
            await PerformAiMove();
        }

        private async void OnServerSendOpponentMove(SocketIOResponse response)
        {
            // nếu không có giá trị (undefined) thì nó trả về 0;
            var thinkingTime = response.GetValue().GetProperty("thinkingTime").GetDouble();
            var isPassed = response.GetValue().GetProperty("isPassed").GetBoolean();
            var requireNextMove = response.GetValue().GetProperty("requireNextMove").GetBoolean();

            // default value
            var row = -1; 
            var col = -1;

            if (!isPassed)
            {
                row = response.GetValue().GetProperty("row").GetInt32();
                col = response.GetValue().GetProperty("col").GetInt32();
                //Console.WriteLine("Opponent move " + row + "," + col);
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
            // Console.WriteLine(" ~ Thinking...");
            var isPassed = false;

            var startTime = DateTime.Now;
            var (row, col) = _game.PerformAiMove();
            var endTime = DateTime.Now;

            if (row == -1 || col == -1)
            {
                isPassed = true;
                // Console.WriteLine("Me PASSED.");
            }
            else
            {
                // Console.WriteLine("Me move at " + row + "," + col);
            }

            await _client.EmitAsync("clientSendMove", new
            {
                col,
                row,
                thinkingTime = (float) Math.Round((endTime - startTime).TotalSeconds, 3),
                isPassed,
                playoutCount = _game.GetLastPlayout()
            });

            // Console.WriteLine(" ~ Waiting for opponent move...");
        }
    }
}