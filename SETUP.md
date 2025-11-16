# Chat Client Setup Guide

## Prerequisites

- **Visual Studio 2022** (or later) with .NET 6.0 SDK installed
- **Python Chat Server** running on `localhost:9999` (or specified host/port)
- Internet connection for Supabase database integration

## Project Structure

```
ChatClient/
├── Models/
│   ├── ChatRoom.cs
│   └── UserPreferences.cs
├── Services/
│   ├── NetworkService.cs
│   ├── PreferencesService.cs
│   ├── SoundService.cs
│   └── MessageRouterService.cs
├── Themes/
│   ├── LightTheme.xaml
│   ├── DarkTheme.xaml
│   └── ThemeManager.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── SettingsWindow.xaml
├── SettingsWindow.xaml.cs
├── App.xaml
├── App.xaml.cs
└── ChatClient.csproj
```

## Installation Steps

1. **Open the Project**
   - Open Visual Studio 2022
   - Open the `ChatClient.csproj` file

2. **Restore NuGet Packages**
   - Visual Studio will automatically restore packages on opening
   - If needed, go to `Tools > NuGet Package Manager > Manage NuGet Packages for Solution`
   - Required packages:
     - `Supabase` (v4.0.0)
     - `Postgrest` (v3.0.0)

3. **Build the Project**
   - Press `Ctrl+Shift+B` or go to `Build > Build Solution`
   - Ensure no compilation errors

4. **Run the Application**
   - Press `F5` or go to `Debug > Start Debugging`
   - The main window will open

## Configuration

### Supabase Credentials

The application uses Supabase to store user preferences. The credentials are embedded in `MainWindow.xaml.cs`:

```csharp
await _preferencesService.InitializeAsync(
    "https://ndunxomhastqvcbtfrhp.supabase.co",
    "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im5kdW54b21oYXN0cXZjYnRmcmhwIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjMyMzYxNjgsImV4cCI6MjA3ODgxMjE2OH0.z1KObMN-GXbBX4KKZm90CT1D2I_Teecf6DkPKEaNn4s");
```

To change these credentials:
1. Replace the URL with your Supabase project URL
2. Replace the API key with your project's anonymous key

### Python Server Connection

1. **Default Settings**
   - Host: `localhost`
   - Port: `9999`

2. **Custom Connection**
   - Enter the desired host and port in the connection panel at the top
   - Click "Connect" button

3. **Save Connection**
   - The application saves your last successful connection details
   - These are automatically restored on the next startup

## Usage

### Connecting to Server

1. Enter the server host and port (defaults: localhost, 9999)
2. Click "Connect" button
3. Status indicator shows "Connected" when successful

### Creating a Room

1. In the left panel, enter:
   - Room ID: Unique identifier for the room
   - Room Name: Display name for the room
2. Click "Create" button
3. Server will confirm room creation

### Joining a Room

1. In the left panel, enter:
   - Room ID: ID of room to join
   - Username: Your chat name in this room
2. Click "Join" button
3. A new tab opens for the room

### Multi-Room Chat

1. **Switch Rooms**: Click on different tabs to switch between joined rooms
2. **Send Messages**: Type in the message input field at the bottom and click "Send"
3. **View Users**: Right panel shows participants in the current room
4. **Room History**: Each room maintains its own message history in its tab

### Managing Rooms

- **List Rooms**: Click "List Rooms" to see all active rooms and their participants
- **Leave Room**: Click "Leave Room" to exit the current room
- **Disconnect**: Click "Disconnect" to close connection and exit all rooms

### Settings

Click the ⚙ icon in the top-right to open Settings:

#### Appearance Tab
- **Theme**: Choose between Light and Dark mode
- **Accent Color**: Select from Blue, Green, Orange, or Red
- **Font Size**: Adjust text size (10-18pt)

#### Notifications Tab
- **Sound**: Enable/disable sound notifications
- **Volume**: Adjust notification volume (0-100%)
- **Event Types**: Toggle notifications for:
  - Messages received
  - User join/leave events
  - Error notifications

#### Connection Tab
- **Auto-connect**: Automatically connect to last server on startup
- **Last Connection**: View and edit saved connection details

#### Advanced Tab
- **Background Image**: Select custom background image for the window
- **Window Transparency**: Adjust window transparency (20-100%)

## Features

### Network Protocol

The client strictly adheres to the Python server's protocol:
- All messages are null-terminated (`\0`)
- Commands are prefixed with backtick (`` ` ``)
- Example commands: `` `join ``, `` `start ``, `` `list ``, `` `quit ``, `` `exit ``

### Multi-Room Support

- Join multiple rooms simultaneously
- Each room has its own tab with message history
- Switch between rooms without disconnecting
- Independent user lists for each room

### User Preferences

- **Local Storage**: Machine ID stored in Windows Registry
- **Cloud Storage**: All preferences synced to Supabase
- **Auto-save**: Settings saved immediately when changed
- **Persistence**: Preferences restored on next launch

### Sound Notifications

- System sounds play for different events
- Customizable volume and enable/disable per event
- Default sounds: Windows Media library

### Theme System

- **Dynamic Switching**: Themes apply immediately without restart
- **Accent Colors**: Customize UI highlight color
- **Dark Mode**: OLED-friendly colors for eye comfort
- **Persistence**: Theme preferences saved automatically

## Troubleshooting

### Connection Issues

**Problem**: Cannot connect to server
- **Solution**: Verify server is running and host/port are correct
- Check firewall settings allow connections to the port

**Problem**: "Connection lost" message
- **Solution**: Server may have crashed or network was interrupted
- Click "Reconnect" or close/reopen the application

### Message Issues

**Problem**: Messages appear in wrong room
- **Solution**: Click the correct room tab before sending
- Check the message arrives in the active room only

**Problem**: No sound notifications
- **Solution**: Enable sound in Settings > Notifications tab
- Verify Windows volume is not muted
- Check speaker/headphone connection

### Settings Issues

**Problem**: Settings not persisting
- **Solution**: Click "Save" button in Settings window
- Verify Supabase connection is working
- Check internet connectivity

## Known Limitations

1. **Direct Messages**: The application doesn't support direct messages; all communication is room-based
2. **Message Persistence**: Messages are only kept while connected; closing the app clears history
3. **Concurrent Connections**: Each instance of the application connects separately to the server
4. **Character Encoding**: Messages should be in UTF-8 format

## Performance Tips

1. **Large Rooms**: Many users in one room may impact UI responsiveness
2. **Multiple Rooms**: Joining too many rooms simultaneously may affect performance
3. **Message Volume**: High message frequency may require scrolling to see all messages

## Support

For issues with:
- **Python Server**: Refer to server documentation
- **Supabase**: Visit https://supabase.com/docs
- **C# WPF**: Refer to Microsoft documentation at https://learn.microsoft.com/en-us/dotnet/desktop/wpf/

## License

This is a client application for the Python chat server. See server documentation for license information.
