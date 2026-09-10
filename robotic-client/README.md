# Robotic Client

Firmware for the physical hand: an ESP8266 NodeMCU handles networking and an
Arduino Uno drives the servos.

Originally published as
[jaximus808/HandArduinoController](https://github.com/jaximus808/HandArduinoController).

## `nodemcu/nodemcu.ino` (ESP8266)

1. Joins Wi-Fi.
2. POSTs the robot's ID, password, local IP, and UDP port to the master
   server's `/api/arm/register` until it gets back a fleet server IP and port.
3. Sends a UDP registration packet to that fleet server every 5 seconds until
   acknowledged.
4. Answers pings, and on a joint-angle packet (`int 2, int, int, int`) writes
   `<j1,j2,j3,>` to `Serial1` for the Uno.

Libraries: `ESP8266WiFi`, `ESP8266HTTPClient`, `WiFiUdp`, `ArduinoJson`.

Before flashing, edit the constants at the top of the sketch: `ssid`,
`password`, `server` (master server URL), `id`, `armPassword`, and `nodePass`
(must match the fleet server's expected node password).

## `uno/uno.ino` (Arduino Uno)

Reads `<a,b,c,>` frames from serial at 115200 baud, clamps each value to
0–360, halves it to fit the servo's 0–180 range, and writes it out:

| Servo | Pin |
|---|---|
| Index (pointer) | 10 |
| Middle | 9 |
| Ring | 11 |

Wire the NodeMCU's `Serial1` TX to the Uno's RX (with level shifting as
appropriate for 3.3 V ↔ 5 V).
