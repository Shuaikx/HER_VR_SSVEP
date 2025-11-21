# VR_SSVEP Socket Server

A C# Console application that communicates with Unity's SocketControl.cs script.

## Features

- TCP Server listening on `127.0.0.1:4003`
- Handles authentication protocol (0x11)
- Receives and displays messages from Unity client
- Sends messages to Unity client via console input
- Supports both byte and string message types

## Communication Protocol

### Authentication Flow
1. Unity client connects to server
2. Unity sends authentication byte: `0x11`
3. Server responds with: `0x11`
4. Connection is authenticated

### Message Types
- **Byte messages**: Single byte values (0-255)
- **String messages**: UTF-8 encoded text
- **Quit message**: `0xFF` from Unity client

## Building and Running

### Prerequisites
- .NET 6.0 SDK or later

### Build
```powershell
cd C:\Shuaikx\Projects\UnityProjects\VR_SSVEP-main\SocketServer
dotnet build
```

### Run
```powershell
dotnet run
```

## Usage

1. Start the server first
2. Run Unity application with SocketControl
3. In Unity, press 'C' to connect to server
4. After authentication, you can:
   - Type byte values (0-255) in console to send single byte
   - Type text messages to send strings to Unity
   - Type 'Q' to quit server

## Example

```
=== VR_SSVEP Socket Server ===
Starting server on 127.0.0.1:4003...
Server started. Waiting for Unity client connection...
Press 'Q' to quit server

[SERVER] Client connected from: 127.0.0.1:xxxxx
[SERVER] Waiting for authentication...

[RECEIVED] Authentication request (0x11)
[SENT] Authentication response (0x11)
[SERVER] Client authenticated successfully!

You can now send messages:
  - Type byte value (0-255) to send single byte
  - Type text message to send as string
  - Type 'Q' to quit

> 17
[SENT] Byte: 0x11 (17)

> Hello Unity
[SENT] String: Hello Unity (11 bytes)
```

## Integration with Unity

The server is designed to work with `SocketControl.cs`:
- Server IP: `127.0.0.1`
- Server Port: `4003`
- Unity connects when 'C' key is pressed
- Unity quits when 'E' key is pressed (sends 0xFF)
