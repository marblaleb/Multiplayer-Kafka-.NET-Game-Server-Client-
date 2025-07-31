# 🎮 Multiplayer Game - .NET + Kafka

Un juego multijugador en tiempo real desarrollado con .NET y Apache Kafka, donde los jugadores pueden moverse y disparar en un mapa 2D compartido.


## ✨ Características

- 🏃‍♂️ **Movimiento en tiempo real** - Los jugadores se mueven instantáneamente
- 🔫 **Sistema de disparos direccionales** - Dispara en 4 direcciones (↑↓←→)
- 💥 **Detección de colisiones** - Las balas impactan y reducen vida
- ❤️ **Sistema de salud** - 100 HP por jugador, 25 de daño por disparo
- 👥 **Multijugador simultáneo** - Tantos jugadores como quieras
- 📺 **Interfaz visual en consola** - Mapa ASCII en tiempo real
- ⚡ **Comunicación asíncrona** - Usando Apache Kafka para mensajería



### Componentes Principales

- **GameServer**: Procesa lógica del juego, colisiones y estado global
- **GameClient**: Interfaz de usuario y manejo de input
- **Apache Kafka**: Message broker para comunicación en tiempo real
- **Docker Compose**: Orquestación de servicios (Kafka, Zookeeper, UI)

## 🚀 Inicio Rápido

### Prerrequisitos

- [Docker](https://docs.docker.com/get-docker/) y [Docker Compose](https://docs.docker.com/compose/install/)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Terminal/CMD

### 1. Clonar el repositorio

```bash
git clone https://github.com/tu-usuario/multiplayer-game-kafka.git
cd multiplayer-game-kafka
```

### 2. Iniciar servicios de Kafka

```bash
#  Docker Compose manual
docker-compose up -d
```

⏳ **Espera 15-20 segundos** para que Kafka esté completamente listo.

### 3. Ejecutar el juego

**Terminal 1 - Servidor del Juego:**
```bash
dotnet run
# Selecciona: 1 (Servidor)
```

**Terminal 2 - Jugador 1:**
```bash
dotnet run
# Selecciona: 2 (Cliente)
# Nombre: "Alice"
```

**Terminal 3 - Jugador 2:**
```bash
dotnet run
# Selecciona: 2 (Cliente)  
# Nombre: "Bob"
```

## 🎯 Controles del Juego

| Tecla | Acción |
|-------|--------|
| **W** | Mover arriba ⬆️ |
| **S** | Mover abajo ⬇️ |
| **A** | Mover izquierda ⬅️ |
| **D** | Mover derecha ➡️ |
| **I** | Disparar arriba ⬆️ |
| **K** | Disparar abajo ⬇️ |
| **J** | Disparar izquierda ⬅️ |
| **L** | Disparar derecha ➡️ |
| **Q** | Salir del juego |

## 🎮 Ejemplo de Gameplay

```
🎮 Jugador: Alice | Vida: 75
Controles: WASD=mover, IJKL=disparar, Q=salir
══════════════════════════════════════════════════
║                  ↑                             ║
║         #        ↑        @                    ║
║                  ↑                             ║
║                                                ║
║    ←                              →            ║
║                                                ║
║                                                ║
║                                                ║
══════════════════════════════════════════════════
👥 Jugadores conectados: 2
  🟢 Alice - Vida: 75 (TÚ)
  🟢 Bob - Vida: 100
```

**Leyenda:**
- `@` = Tu jugador
- `#` = Otros jugadores
- `↑↓←→` = Balas en movimiento
- `🟢` = Jugador vivo
- `💀` = Jugador eliminado


