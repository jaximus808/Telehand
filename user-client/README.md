# User Client

Python desktop app that logs the user in, connects to a robot, and streams
webcam hand tracking to the fleet server.

Originally published as
[jaximus808/PythonHandController](https://github.com/jaximus808/PythonHandController).

## Files (`HandController/`)

| File | Role |
|---|---|
| `WindowRenderer.py` | **Entry point.** Tkinter UI with three pages: register/login, robot connect, and streaming. Talks to the master server over HTTP and stores the JWT in `localstorage.txt`. |
| `HandController.py` | Wraps MediaPipe Hands. Turns a webcam frame into the binary hand-data packet (21 landmarks × xyz floats per hand). |
| `UnityCommunicator.py` | UDP socket with a background receive thread. Handles the fleet server's ID assignment, pings, and shutdown packets. |
| `packet.py` | Minimal little-endian packet reader. |

`HandController.sln` / `HandController.pyproj` are the Visual Studio project
files the app was developed in; they are optional.

## Running

```sh
pip install opencv-python mediapipe requests
cd HandController
python WindowRenderer.py
```

Set `MasterServer` at the top of `WindowRenderer.py` to your master server's
address. The client binds UDP port 8001 locally and sends to whatever fleet
server the master server assigns.

Once a robot is connected, the OpenCV window shows the tracked hand with a
`connecting` / `connected` / `Fleet Server Closed` status overlay. Close the
window to stop streaming.

Note: `UnityCommunicator.py` catches `WindowsError`, so as written the receive
loop only runs on Windows.
