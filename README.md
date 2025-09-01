# TCP Client-Server Communication System

A robust C# implementation of a TCP/IP client-server communication system that enables real-time status monitoring and
message exchange between multiple clients and a centralized server. This is the server component of the project.

For UI client implementation, see https://github.com/Hari343/tcp-client

## 🚀 Features

### Server Application
- **Multi-client Support**: Handles multiple simultaneous client connections
- **Continuous Status Polling**: Automatically polls connected clients every 5 seconds
- **Real-time Message Display**: Shows all incoming messages with timestamps
- **Concurrent Operations**: Non-blocking async operations for optimal performance
- **Graceful Shutdown**: Clean disconnection of all clients on server shutdown

### Client Application
- **Interactive GUI**: Windows Forms interface with 10 status buttons
- **Protocol Compliance**: Implements the specified communication protocol:
    - Immediate acknowledgment of all server messages
    - Configurable delay before status replies
    - Busy state management during processing
- **Real-time Status Updates**: Visual button states reflect actual device status
- **Connection Management**: Connect/disconnect functionality with status indicators
- **Error Handling**: Comprehensive exception handling and user feedback

## 📋 Requirements

- **.NET 10.0** or higher
- **Windows 10 / 11**
 


## 🚦 Communication Protocol

The system implements a simple text-based protocol:

| Message Type      | Direction | Description                        |
|-------------------|-----------|------------------------------------|
| `REQUEST_STATUS`  | Server → Client | Request for current button status  |
| `ACK`             | Client → Server | Acknowledgment of received message |
| `CLIENT_BUSY`     | Client → Server | Client is busy                     |
| `STATUS_RESPONSE` | Client → Server | Current state of all 10 buttons    |



## 🔧 Installation & Setup

### 1. Clone the Repository

### 2. Build the Solution
From the root directory of the repository, run the following command:
```shell script
# Using .NET CLI
dotnet build --configuration Release
```
If you are using Windows 10 / 11, and have NET 10.0 or higher installed you don't have to build the project. You can directly run the the prebuilt binaries present
in the `TcpServer/bin/Release/net10.0` directory.

### 3. Run the Server

```shell script
cd TcpServer
dotnet run
```

**Created with ❤️ using C# and .NET**
