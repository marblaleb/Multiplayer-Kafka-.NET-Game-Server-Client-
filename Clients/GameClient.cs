using Confluent.Kafka;
using KafkaGame.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KafkaGame.Clients
{
    public class GameClient
    {
        private readonly string _playerId;
        private readonly string _playerName;
        private readonly IProducer<string, string> _producer;
        private readonly IConsumer<string, string> _consumer;
        private Player? _currentPlayer;
        private List<Player> _allPlayers = new();
        private List<Bullet> _bullets = new();
        private int _mapWidth = 50;
        private int _mapHeight = 20;

        public GameClient(string playerName)
        {
            _playerId = Guid.NewGuid().ToString();
            _playerName = playerName;

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = "localhost:9092"
            };

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = $"player-{_playerId}",
                AutoOffsetReset = AutoOffsetReset.Latest
            };

            _producer = new ProducerBuilder<string, string>(producerConfig).Build();
            _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        }

        public async Task StartAsync()
        {
            _consumer.Subscribe("game-output");

            // Unirse al juego
            await JoinGame();

            // Iniciar tareas concurrentes
            var inputTask = Task.Run(HandleUserInput);
            var gameTask = Task.Run(HandleGameUpdates);

            await Task.WhenAny(inputTask, gameTask);
        }

        private async Task JoinGame()
        {
            var player = new Player
            {
                Id = _playerId,
                Name = _playerName
            };

            var message = new GameMessage
            {
                Type = MessageType.PlayerJoin,
                PlayerId = _playerId,
                Data = JsonSerializer.Serialize(player)
            };

            await _producer.ProduceAsync("game-input", new Message<string, string>
            {
                Key = _playerId,
                Value = JsonSerializer.Serialize(message)
            });

            Console.WriteLine($"🎮 ¡Bienvenido {_playerName}!");
            Console.WriteLine("Controles: WASD para moverse, IJKL para disparar, Q para salir");
        }

        private async Task HandleUserInput()
        {
            while (true)
            {
                var key = Console.ReadKey(true).Key;

                GameMessage? message = null;

                switch (key)
                {
                    case ConsoleKey.W:
                        message = CreateMoveMessage("UP");
                        break;
                    case ConsoleKey.S:
                        message = CreateMoveMessage("DOWN");
                        break;
                    case ConsoleKey.A:
                        message = CreateMoveMessage("LEFT");
                        break;
                    case ConsoleKey.D:
                        message = CreateMoveMessage("RIGHT");
                        break;
                    case ConsoleKey.I:
                        message = CreateShootMessage("UP");
                        break;
                    case ConsoleKey.K:
                        message = CreateShootMessage("DOWN");
                        break;
                    case ConsoleKey.J:
                        message = CreateShootMessage("LEFT");
                        break;
                    case ConsoleKey.L:
                        message = CreateShootMessage("RIGHT");
                        break;
                    case ConsoleKey.Q:
                        await LeaveGame();
                        return;
                }

                if (message != null)
                {
                    await _producer.ProduceAsync("game-input", new Message<string, string>
                    {
                        Key = _playerId,
                        Value = JsonSerializer.Serialize(message)
                    });
                }
            }
        }

        private async Task HandleGameUpdates()
        {
            while (true)
            {
                try
                {
                    var result = _consumer.Consume(TimeSpan.FromMilliseconds(100));
                    if (result?.Message != null)
                    {
                        var message = JsonSerializer.Deserialize<GameMessage>(result.Message.Value);
                        if (message?.Type == MessageType.GameState)
                        {
                            UpdateGameState(message.Data);
                            DrawGame();
                        }
                    }
                }
                catch (ConsumeException ex)
                {
                    Console.WriteLine($"❌ Error: {ex.Error.Reason}");
                }
            }
        }

        private void UpdateGameState(string gameStateJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(gameStateJson);
                var root = doc.RootElement;

                _allPlayers.Clear();
                if (root.TryGetProperty("Players", out var playersElement))
                {
                    foreach (var playerElement in playersElement.EnumerateArray())
                    {
                        var player = JsonSerializer.Deserialize<Player>(playerElement.GetRawText());
                        if (player != null)
                        {
                            _allPlayers.Add(player);
                            if (player.Id == _playerId)
                            {
                                _currentPlayer = player;
                            }
                        }
                    }
                }

                _bullets.Clear();
                if (root.TryGetProperty("Bullets", out var bulletsElement))
                {
                    foreach (var bulletElement in bulletsElement.EnumerateArray())
                    {
                        var bullet = JsonSerializer.Deserialize<Bullet>(bulletElement.GetRawText());
                        if (bullet != null)
                        {
                            _bullets.Add(bullet);
                        }
                    }
                }

                if (root.TryGetProperty("MapWidth", out var widthElement))
                    _mapWidth = widthElement.GetInt32();
                if (root.TryGetProperty("MapHeight", out var heightElement))
                    _mapHeight = heightElement.GetInt32();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error actualizando estado: {ex.Message}");
            }
        }

        private void DrawGame()
        {
            Console.Clear();
            Console.WriteLine($"🎮 Jugador: {_playerName} | Vida: {_currentPlayer?.Health ?? 0}");
            Console.WriteLine("Controles: WASD=mover, IJKL=disparar, Q=salir");
            Console.WriteLine(new string('═', _mapWidth + 2));

            for (int y = 0; y < _mapHeight; y++)
            {
                Console.Write("║");
                for (int x = 0; x < _mapWidth; x++)
                {
                    char cell = ' ';

                    // Verificar jugadores
                    var playerAtPos = _allPlayers.FirstOrDefault(p => p.X == x && p.Y == y && p.IsAlive);
                    if (playerAtPos != null)
                    {
                        cell = playerAtPos.Id == _playerId ? '@' : '#';
                    }

                    // Verificar balas
                    var bulletAtPos = _bullets.FirstOrDefault(b => b.X == x && b.Y == y);
                    if (bulletAtPos != null)
                    {
                        cell = bulletAtPos.Direction switch
                        {
                            "UP" => '↑',
                            "DOWN" => '↓',
                            "LEFT" => '←',
                            "RIGHT" => '→',
                            _ => '*'
                        };
                    }

                    Console.Write(cell);
                }
                Console.WriteLine("║");
            }

            Console.WriteLine(new string('═', _mapWidth + 2));

            // Mostrar jugadores conectados
            Console.WriteLine($"👥 Jugadores conectados: {_allPlayers.Count}");
            foreach (var player in _allPlayers)
            {
                var status = player.IsAlive ? "🟢" : "💀";
                var marker = player.Id == _playerId ? " (TÚ)" : "";
                Console.WriteLine($"  {status} {player.Name} - Vida: {player.Health}{marker}");
            }
        }

        private GameMessage CreateMoveMessage(string direction)
        {
            return new GameMessage
            {
                Type = MessageType.PlayerMove,
                PlayerId = _playerId,
                Data = direction
            };
        }

        private GameMessage CreateShootMessage(string direction)
        {
            return new GameMessage
            {
                Type = MessageType.PlayerShoot,
                PlayerId = _playerId,
                Data = direction
            };
        }

        private async Task LeaveGame()
        {
            var message = new GameMessage
            {
                Type = MessageType.PlayerLeave,
                PlayerId = _playerId,
                Data = ""
            };

            await _producer.ProduceAsync("game-input", new Message<string, string>
            {
                Key = _playerId,
                Value = JsonSerializer.Serialize(message)
            });

            Console.WriteLine("👋 ¡Hasta luego!");
            Environment.Exit(0);
        }
    }

}
