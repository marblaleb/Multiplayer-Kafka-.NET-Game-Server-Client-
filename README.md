🎮 Multiplayer Game - .NET + Kafka
Un juego multijugador en tiempo real desarrollado con .NET y Apache Kafka, donde los jugadores pueden moverse y disparar en un mapa 2D compartido.

✨ Características

🏃‍♂️ Movimiento en tiempo real - Los jugadores se mueven instantáneamente
🔫 Sistema de disparos direccionales - Dispara en 4 direcciones (↑↓←→)
💥 Detección de colisiones - Las balas impactan y reducen vida
❤️ Sistema de salud - 100 HP por jugador, 25 de daño por disparo
👥 Multijugador simultáneo - Tantos jugadores como quieras
📺 Interfaz visual en consola - Mapa ASCII en tiempo real
⚡ Comunicación asíncrona - Usando Apache Kafka para mensajería

🚀 Inicio Rápido
Prerrequisitos

Docker y Docker Compose
.NET 8.0 SDK
Terminal/CMD

1. Clonar el repositorio
bashgit clone https://github.com/tu-usuario/multiplayer-game-kafka.git
cd multiplayer-game-kafka
2. Iniciar servicios de Kafka
docker-compose up -d
⏳ Espera 15-20 segundos para que Kafka esté completamente listo.
3. Ejecutar el juego
Terminal 1 - Servidor del Juego:
bashdotnet run
# Selecciona: 1 (Servidor)
Terminal 2 - Jugador 1:
bashdotnet run
# Selecciona: 2 (Cliente)
# Nombre: "Alice"
Terminal 3 - Jugador 2:
bashdotnet run
# Selecciona: 2 (Cliente)  
# Nombre: "Bob"
