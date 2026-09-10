# Master Server

Node.js + Express service that owns accounts and matchmaking for the robotic
hand system. It knows every registered robot, every live fleet server, and which
fleet server each robot is attached to.

Originally published as
[jaximus808/MasterRoboticHandServer](https://github.com/jaximus808/MasterRoboticHandServer).

## What it does

- **User accounts** — register / login with bcrypt-hashed passwords, returns a
  JWT (`routes/UserAuth.js`, `routes/validation.js`, `routes/models/UserObject.js`).
- **Robot registry** — a NodeMCU registers with a robot ID and password. The
  master stores it in MongoDB and assigns it the least-loaded fleet server
  (`routes/models/ArmObject.js`).
- **Fleet server registry** — fleet servers announce themselves on boot. Their
  IP and UDP port are kept in a local `fleetServers.json` (via `node-json-db`).
- **Matchmaking** — a logged-in user asks for a robot by ID and password and
  gets back the IP and port of the fleet server that robot is on.

Middleware in `routes/` gates each route: `userTokenVerify.js` (JWT),
`armPassVerify.js` (shared robot secret), `serverFleetVerify.js` (shared fleet
secret). The full route table is in the [root README](../README.md#master-server-http-api).

## Running

Requires Node.js and a MongoDB instance.

```sh
npm install
cp .env.example .env   # then fill in the values
npm run dev            # nodemon index.js, listens on port 3000
```

`.env` keys:

| Key | Purpose |
|---|---|
| `DB_CONNECT` | MongoDB connection string |
| `TOKEN_SECRET` | JWT signing secret |
| `ARM_PASSWORD` | Shared secret robots present when registering |
| `FLEET_SERVER_PASSWORD` | Shared secret fleet servers present on `/server/auth/*` |

`fleetServers.json` is created on first run and is git-ignored.
