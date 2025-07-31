using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaGame.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Confluent.Kafka;

    // Modelos de datos
    public class Player
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int X { get; set; }
        public int Y { get; set; }
        public int Health { get; set; } = 100;
        public bool IsAlive => Health > 0;
    }

    public class Bullet
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PlayerId { get; set; } = string.Empty;
        public int X { get; set; }
        public int Y { get; set; }
        public string Direction { get; set; } = string.Empty; // "UP", "DOWN", "LEFT", "RIGHT"
    }

    public enum MessageType
    {
        PlayerJoin,
        PlayerMove,
        PlayerShoot,
        PlayerLeave,
        GameState,
        BulletHit
    }

    public class GameMessage
    {
        public MessageType Type { get; set; }
        public string PlayerId { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

}
