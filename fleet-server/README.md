# Fleet Server

Unity 2020.3 project that acts as the real-time relay between users and robots.
It receives raw hand landmarks over UDP, rebuilds the hand in a 3D scene,
computes finger joint angles, and streams those angles to the robot.

Originally published as
[jaximus808/HandRobotFleeetServer20212022](https://github.com/jaximus808/HandRobotFleeetServer20212022).

## What it does

| Script (`Assets/Scripts/`) | Role |
|---|---|
| `UDPServer.cs` | Listens on UDP port 8000. Routes packets to users or robots, runs the ping / timeout loop, registers with the master server over HTTP, marshals work back onto the Unity main thread. |
| `PacketHandler.cs` | Handlers for the four inbound packet IDs: new user, hand data, new robot, ping reply. |
| `Packet.cs` | Little-endian binary packet reader/writer shared (by convention) with the Python and Arduino clients. |
| `Client.cs` / `ArmClient.cs` | Per-connection state for a user and a robot respectively. |
| `HandRendererOne.cs` | Places 21 landmark spheres per hand, computes the bend angle at each knuckle with the law of cosines (wrist, MCP, PIP), and sends index / middle / ring angles to the paired robot every physics tick. |
| `WebCommunicator.cs` | Tiny HTTP client for talking to the master server. |

`Assets/Scenes/HandSimulationScene.unity` is the only scene. `Assets/GameObjects/`
holds the landmark prefab and hand prefab.

## Running

1. Open the folder in **Unity 2020.3.32f1** (see `ProjectSettings/ProjectVersion.txt`).
   Unity will regenerate `Library/`, the `.sln`, and `.csproj` files; they are
   intentionally not committed.
2. Copy `.env.example` to `.env` in the project root and set `FleetServerPass`
   to match the master server's `FLEET_SERVER_PASSWORD`. The file is read at
   startup by the `xyz.candycoded.env` package listed in `Packages/manifest.json`.
3. Select the object with the `UDPServer` component in the scene and set
   **Master Target Ip** to the master server's address (port 3000 is assumed).
   The ping timers are also exposed there.
4. Press Play. The console prints `Listening` and then
   `connected to master server!` once registration succeeds.

The fleet server must be reachable from both users and robots on UDP port 8000.
