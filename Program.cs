using KafkaGame.Clients;
using KafkaGame.Server;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("🎮 JUEGO MULTIJUGADOR");
        Console.WriteLine("1. Servidor");
        Console.WriteLine("2. Cliente");
        Console.Write("Selecciona una opción: ");

        var option = Console.ReadLine();

        switch (option)
        {
            case "1":
                var server = new GameServer();
                await server.StartAsync();
                break;
            case "2":
                Console.Write("Ingresa tu nombre: ");
                var playerName = Console.ReadLine() ?? "Jugador";
                var client = new GameClient(playerName);
                await client.StartAsync();
                break;
            default:
                Console.WriteLine("❌ Opción inválida");
                break;
        }
    }
}