using Confluent.Kafka;
using KafkaGame.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KafkaGame.Server
{
    public class GameServer
    {
        private readonly Dictionary<string, Player> _players = new();
        private readonly List<Bullet> _bullets = new();
        private readonly IProducer<string, string> _producer;
        private readonly IConsumer<string, string> _consumer;
        private readonly Timer _gameTimer;
        private const int GAME_WIDTH = 50;
        private const int GAME_HEIGHT = 20;

        public GameServer()
        {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = "localhost:9092"
            };

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "game-server",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _producer = new ProducerBuilder<string, string>(producerConfig).Build();
            _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();

            // Timer para actualizar el juego cada 100ms
            _gameTimer = new Timer(UpdateGame, null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100));
        }

        public async Task StartAsync()
        {
            _consumer.Subscribe("game-input");

            Console.WriteLine("🎮 Servidor de juego iniciado");
            Console.WriteLine($"📏 Mapa: {GAME_WIDTH}x{GAME_HEIGHT}");

            while (true)
            {
                try
                {
                    var result = _consumer.Consume(TimeSpan.FromMilliseconds(100));
                    if (result?.Message != null)
                    {
                        await ProcessMessage(result.Message.Value);
                    }
                }
                catch (ConsumeException ex)
                {
                    Console.WriteLine($"❌ Error consuming: {ex.Error.Reason}");
                }
            }
        }

        private async Task ProcessMessage(string messageJson)
        {
            try
            {
                var message = JsonSerializer.Deserialize<GameMessage>(messageJson);
                if (message == null) return;

                switch (message.Type)
                {
                    case MessageType.PlayerJoin:
                        await HandlePlayerJoin(message);
                        break;
                    case MessageType.PlayerMove:
                        await HandlePlayerMove(message);
                        break;
                    case MessageType.PlayerShoot:
                        await HandlePlayerShoot(message);
                        break;
                    case MessageType.PlayerLeave:
                        await HandlePlayerLeave(message);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error processing message: {ex.Message}");
            }
        }

        private async Task HandlePlayerJoin(GameMessage message)
        {
            var player = JsonSerializer.Deserialize<Player>(message.Data);
            if (player == null) return;

            // Posición inicial aleatoria
            var random = new Random();
            player.X = random.Next(0, GAME_WIDTH);
            player.Y = random.Next(0, GAME_HEIGHT);
            player.Health = 100;

            _players[player.Id] = player;
            Console.WriteLine($"👤 Jugador {player.Name} se unió en ({player.X}, {player.Y})");

            await BroadcastGameState();
        }

        private async Task HandlePlayerMove(GameMessage message)
        {
            if (!_players.ContainsKey(message.PlayerId)) return;

            var direction = message.Data;
            var player = _players[message.PlayerId];

            switch (direction.ToUpper())
            {
                case "UP":
                    if (player.Y > 0) player.Y--;
                    break;
                case "DOWN":
                    if (player.Y < GAME_HEIGHT - 1) player.Y++;
                    break;
                case "LEFT":
                    if (player.X > 0) player.X--;
                    break;
                case "RIGHT":
                    if (player.X < GAME_WIDTH - 1) player.X++;
                    break;
            }

            await BroadcastGameState();
        }

        private async Task HandlePlayerShoot(GameMessage message)
        {
            if (!_players.ContainsKey(message.PlayerId)) return;

            var direction = message.Data;
            var player = _players[message.PlayerId];

            var bullet = new Bullet
            {
                PlayerId = player.Id,
                X = player.X,
                Y = player.Y,
                Direction = direction.ToUpper()
            };

            _bullets.Add(bullet);
            Console.WriteLine($"💥 {player.Name} disparó hacia {direction}");
        }

        private async Task HandlePlayerLeave(GameMessage message)
        {
            if (_players.ContainsKey(message.PlayerId))
            {
                var player = _players[message.PlayerId];
                _players.Remove(message.PlayerId);
                Console.WriteLine($"👋 Jugador {player.Name} se desconectó");
                await BroadcastGameState();
            }
        }

        private async void UpdateGame(object? state)
        {
            // Actualizar posiciones de balas
            for (int i = _bullets.Count - 1; i >= 0; i--)
            {
                var bullet = _bullets[i];

                switch (bullet.Direction)
                {
                    case "UP":
                        bullet.Y--;
                        break;
                    case "DOWN":
                        bullet.Y++;
                        break;
                    case "LEFT":
                        bullet.X--;
                        break;
                    case "RIGHT":
                        bullet.X++;
                        break;
                }

                // Remover balas fuera del mapa
                if (bullet.X < 0 || bullet.X >= GAME_WIDTH || bullet.Y < 0 || bullet.Y >= GAME_HEIGHT)
                {
                    _bullets.RemoveAt(i);
                    continue;
                }

                // Verificar colisiones con jugadores
                foreach (var player in _players.Values)
                {
                    if (player.Id != bullet.PlayerId && player.X == bullet.X && player.Y == bullet.Y && player.IsAlive)
                    {
                        player.Health -= 25;
                        _bullets.RemoveAt(i);

                        var shooterName = _players.ContainsKey(bullet.PlayerId) ? _players[bullet.PlayerId].Name : "Desconocido";
                        Console.WriteLine($"🎯 {shooterName} golpeó a {player.Name}! Vida: {player.Health}");

                        if (!player.IsAlive)
                        {
                            Console.WriteLine($"💀 {player.Name} fue eliminado!");
                        }
                        break;
                    }
                }
            }
        }

        private async Task BroadcastGameState()
        {
            var gameState = new
            {
                Players = _players.Values,
                Bullets = _bullets,
                MapWidth = GAME_WIDTH,
                MapHeight = GAME_HEIGHT
            };

            var message = new GameMessage
            {
                Type = MessageType.GameState,
                Data = JsonSerializer.Serialize(gameState)
            };

            await _producer.ProduceAsync("game-output", new Message<string, string>
            {
                Key = "gamestate",
                Value = JsonSerializer.Serialize(message)
            });
        }
    }
}
