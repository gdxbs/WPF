# Architecture Overview

## High-Level Design

The Chat Client is built using a layered architecture with clear separation of concerns:

```
┌─────────────────────────────────────────┐
│           UI Layer (WPF XAML)           │
│  - MainWindow (Multi-room interface)    │
│  - SettingsWindow (Preferences)         │
└──────────────┬──────────────────────────┘
               │
┌──────────────┴──────────────────────────┐
│         Business Logic Layer            │
│  - MainWindow.cs (Event handlers)       │
│  - SettingsWindow.cs (Settings logic)   │
└──────────────┬──────────────────────────┘
               │
┌──────────────┴──────────────────────────┐
│          Service Layer                  │
│  - NetworkService (TCP communication)   │
│  - MessageRouterService (Routing)       │
│  - PreferencesService (Data persistence)│
│  - SoundService (Notifications)         │
│  - ThemeManager (UI themes)             │
└──────────────┬──────────────────────────┘
               │
┌──────────────┴──────────────────────────┐
│           Data/Model Layer              │
│  - ChatRoom (Room state)                │
│  - UserPreferences (Settings model)     │
│  - Supabase (Cloud persistence)         │
│  - Python TCP Server (Chat protocol)    │
└─────────────────────────────────────────┘
```

## Component Breakdown

### 1. UI Layer

#### MainWindow.xaml / MainWindow.xaml.cs
- **Purpose**: Main application window with tabbed chat interface
- **Responsibilities**:
  - Display connection controls
  - Manage room tabs
  - Handle user input (join, create, send messages)
  - Display room users list
  - Show chat messages

**Key Features**:
- Dynamic tab creation for each joined room
- Real-time message display with auto-scroll
- Room management controls (left panel)
- User list display (right panel)
- Connection status indicator

#### SettingsWindow.xaml / SettingsWindow.xaml.cs
- **Purpose**: User preferences and configuration window
- **Responsibilities**:
  - Display preference options
  - Load current preferences
  - Save changes to Supabase
  - Apply theme changes in real-time

**Sections**:
- Appearance (theme, color, font size)
- Notifications (sound settings)
- Connection (auto-connect, last connection)
- Advanced (background image, transparency)

### 2. Service Layer

#### NetworkService.cs
- **Purpose**: Handle all TCP communication with Python server
- **Responsibilities**:
  - Establish/maintain TCP connection
  - Send commands and messages
  - Receive and buffer data
  - Parse null-terminated messages
  - Handle connection state

**Key Methods**:
- `ConnectAsync(host, port)`: Establish connection
- `SendCommandAsync(command)`: Send backtick-prefixed command
- `SendMessageAsync(message)`: Send regular chat message
- `ReceiveMessagesAsync()`: Background receive loop
- `Disconnect()`: Gracefully close connection

**Events**:
- `MessageReceived`: Fired when complete message received
- `ConnectionStatusChanged`: Connection state changes
- `ErrorOccurred`: Network errors

**Protocol Compliance**:
- Null-terminated messages: `message\0`
- Command prefix: `` `join ``, `` `start ``, etc.
- UTF-8 encoding for all data
- Handles partial messages and multiple messages in single read

#### MessageRouterService.cs
- **Purpose**: Route messages to appropriate room
- **Responsibilities**:
  - Maintain room registry
  - Extract room ID from messages
  - Route messages to correct ChatRoom
  - Manage room lifecycle

**Key Methods**:
- `RegisterRoom(roomId, room)`: Add room to routing
- `UnregisterRoom(roomId)`: Remove room
- `RouteMessage(message, roomId)`: Process and route message
- `GetRoom(roomId)`: Retrieve room by ID

#### PreferencesService.cs
- **Purpose**: Manage user preferences with Supabase persistence
- **Responsibilities**:
  - Load preferences from Supabase
  - Save preferences to Supabase
  - Maintain machine ID for identification
  - Provide default preferences

**Key Methods**:
- `InitializeAsync(url, key)`: Connect to Supabase
- `LoadPreferencesAsync()`: Fetch preferences
- `SavePreferencesAsync(prefs)`: Save preferences
- `GetCurrentPreferences()`: Get in-memory preferences

**Machine ID**:
- Stored in Windows Registry
- Uniquely identifies each app instance
- Used as key in Supabase

#### SoundService.cs
- **Purpose**: Play notification sounds
- **Responsibilities**:
  - Play system sounds for events
  - Control volume
  - Enable/disable sounds
  - Handle audio playback

**Key Methods**:
- `PlayNotificationAsync(eventType)`: Play sound
- `SetSoundEnabled(enabled)`: Toggle sounds
- `SetVolume(volume)`: Set volume level

**Event Types**:
- `message`: Chat message received
- `join_leave`: User join/leave event
- `error`: Error notification

#### ThemeManager.cs
- **Purpose**: Manage application theme and colors
- **Responsibilities**:
  - Apply theme (light/dark)
  - Update accent colors
  - Update dynamic resources

**Key Methods**:
- `ApplyTheme(themeName)`: Switch theme
- `ApplyAccentColor(colorHex)`: Change accent color
- `GetCurrentTheme()`: Get active theme
- `GetCurrentAccentColor()`: Get active color

### 3. Data Layer

#### ChatRoom.cs (Model)
- **Properties**:
  - `RoomId`: Unique room identifier
  - `RoomName`: Display name
  - `Messages`: Collection of chat messages
  - `Users`: Collection of room participants
  - `JoinedTime`: When user joined
  - `HasUnreadMessages`: Unread flag
  - `UnreadCount`: Number of unread messages

#### UserPreferences.cs (Model)
- **Properties**:
  - Theme settings (mode, accent color)
  - UI settings (font size, transparency, background)
  - Notification settings (enabled, volume, event types)
  - Connection settings (host, port, auto-connect)
  - Timestamps (created, updated)

#### Supabase Integration
- **Database**: PostgreSQL via Supabase
- **Tables**:
  - `user_preferences`: Store all preferences
  - `notification_sounds`: Store custom sound paths
- **Authentication**: Public access (no user auth required)
- **Sync**: Automatic on preference changes

### 4. Theme System

#### Theme Architecture
```
┌─ LightTheme.xaml ─────────────────┐
│  - BackgroundBrush: #FFFFFF       │
│  - ForegroundBrush: #1E1E1E       │
│  - AccentBrush: #0078D4           │
│  - And 8 other color resources    │
└───────────────────────────────────┘

┌─ DarkTheme.xaml ──────────────────┐
│  - BackgroundBrush: #1E1E1E       │
│  - ForegroundBrush: #E8E8E8       │
│  - AccentBrush: #0078D4           │
│  - And 8 other color resources    │
└───────────────────────────────────┘

        ↓ (Managed by)

     ThemeManager.cs

     ↓ (Updates)

  App.Resources.MergedDictionaries
```

#### Dynamic Accent Colors
- Accent color applied via `FindResource("AccentBrush")`
- Changed via `Application.Current.Resources["AccentBrush"]`
- Automatically applied to buttons, tabs, highlights

## Data Flow Diagrams

### Connection Flow
```
User clicks "Connect"
    ↓
MainWindow.ConnectButton_Click()
    ↓
NetworkService.ConnectAsync(host, port)
    ↓
TcpClient.ConnectAsync() → Success
    ↓
Start NetworkStream listener (Task.Run)
    ↓
ReceiveMessagesAsync() starts background loop
    ↓
ConnectionStatusChanged event
    ↓
UI updates status indicator & enables buttons
```

### Message Send Flow
```
User types message → clicks "Send"
    ↓
SendButton_Click() → gets current room
    ↓
NetworkService.SendMessageAsync(message)
    ↓
Format: "message\0"
    ↓
Encoding.UTF8.GetBytes()
    ↓
networkStream.WriteAsync()
    ↓
Server receives & processes
```

### Message Receive Flow
```
NetworkService.ReceiveMessagesAsync() loop
    ↓
networkStream.ReadAsync() → bytes received
    ↓
Encoding.UTF8.GetString() → decode
    ↓
Add to StringBuilder buffer
    ↓
Check for '\0' delimiter
    ↓
Split message at delimiter
    ↓
OnMessageReceived event
    ↓
MainWindow.ProcessReceivedMessage()
    ↓
Extract room ID from message
    ↓
MessageRouterService.RouteMessage()
    ↓
Add to ChatRoom.Messages
    ↓
UI auto-updates (ObservableCollection binding)
    ↓
PlayNotificationAsync() for sound
```

### Multi-Room Tab Creation
```
User clicks "Join" with room_id="general", username="Alice"
    ↓
JoinRoomButton_Click()
    ↓
NetworkService.SendCommandAsync("join general Alice")
    ↓
Server processes, responds: "You join @chatroom general"
    ↓
ProcessReceivedMessage() detects join message
    ↓
HandleRoomJoined() extracts room ID
    ↓
Create ChatRoom object
    ↓
AddRoomTab("general") creates:
  - TabItem with header "general"
  - TextBox for chat display (read-only)
  - StackPanel with message input + send button
    ↓
Register with MessageRouter
    ↓
Set as current room
    ↓
All future messages route to this room
```

### Preferences Save Flow
```
User changes setting in SettingsWindow
    ↓
Clicks "Save"
    ↓
SaveButton_Click()
    ↓
Update UserPreferences object
    ↓
PreferencesService.SavePreferencesAsync()
    ↓
If new preference:
  - Generate UUID
  - POST to Supabase user_preferences table
Else:
  - UPDATE existing row in Supabase
    ↓
Success response
    ↓
Update ThemeManager / SoundService with new values
    ↓
UI refreshes (theme/font changes apply immediately)
```

## Threading Model

### Main Thread (UI Thread)
- Runs WPF message pump
- All UI updates must happen here
- Blocks if long operations run here

### Background Threads
1. **Network Receive Loop** (Task.Run via ReceiveMessagesAsync)
   - Continuously reads from NetworkStream
   - Cannot update UI directly
   - Uses `Application.Current.Dispatcher.Invoke()` for UI updates

2. **Preference Loading** (On startup)
   - Async task to load from Supabase
   - Doesn't block UI

3. **Sound Playback** (Task.Run in PlayNotificationAsync)
   - Plays audio without blocking

### Thread-Safety Mechanisms
- **Dispatcher**: Routes UI updates from background threads
- **Async/Await**: Manages thread context automatically
- **ObservableCollection**: Thread-safe for UI binding
- **CancellationToken**: Graceful thread cancellation

## Protocol Details

### Commands
```
`join <room_id> <username>\0
  → Server joins user to room
  ← "You join @chatroom <room_id>"

`start <room_id> <room_name>\0
  → Server creates new room
  ← "Chatroom <room_id> started!"

`list\0
  → Server lists all rooms
  ← "room_name@room_id: user1@addr1 user2@addr2\n..."

`quit\0
  → Server removes user from current room
  ← "You quitted from chatroom ..."

`exit\0
  → Server disconnects user completely
  ← Connection closes
```

### Message Flow
- **Chat Message**: `"Hello world"\0` (no backtick)
- **Server Echoes**: `"username: Hello world"\0`
- **System Message**: `"Server: username joined @chatroom room_id.\0"`

## Error Handling

### Network Errors
- Connection refused → Show message box
- Connection lost → Update status, disable buttons
- Send failed → Log and allow retry

### Parse Errors
- Invalid UTF-8 → Skip message
- Malformed response → Log warning

### Database Errors
- Supabase unavailable → Use local defaults
- Save failed → Log error, allow retry

## Performance Considerations

1. **Message Buffering**
   - StringBuild accumulates partial messages
   - Prevents creating string objects for every byte

2. **Lazy Room Creation**
   - Tabs created only when user joins
   - Reduces memory usage for inactive rooms

3. **Event-Driven Architecture**
   - No polling loops
   - Events notify on data availability

4. **Async All The Way**
   - No blocking operations on UI thread
   - Network I/O doesn't freeze UI

## Security Considerations

1. **Local Storage**
   - Registry stores machine ID (non-sensitive)
   - No passwords stored locally

2. **Network**
   - TCP sends plain text (same as server expects)
   - No encryption on wire (application responsibility)

3. **Supabase**
   - Public read/write on preferences table
   - Machine ID used for isolation
   - No authentication required

## Extension Points

### Adding New Notifications
1. Add event type to `SoundService.GetSystemSoundPath()`
2. Add button in SettingsWindow
3. Call `_soundService.PlayNotificationAsync("new_type")` when needed

### Adding New Settings
1. Add property to `UserPreferences` class
2. Add UI control in SettingsWindow
3. Load/save in SettingsWindow code-behind
4. Apply effect in appropriate service

### Adding New Themes
1. Create `NewTheme.xaml` with color resources
2. Add option in SettingsWindow
3. Update `ThemeManager.ApplyTheme()` with new case

## Deployment

### Release Build
- Change Solution Configuration to "Release"
- Build produces optimized executable
- Self-contained executable can be shared

### System Requirements
- Windows 10/11 with .NET 6.0 runtime
- 100MB disk space
- Internet for Supabase sync

### Distribution
- Publish as self-contained: `dotnet publish -c Release -r win-x64 --self-contained`
- Produces standalone executable with all dependencies

## Integrated Server Launcher

The application includes an integrated Python-based chat server that is automatically managed by the main application. This design simplifies the user experience by eliminating the need for manual server setup.

### Server Launch and Management

1.  **Initiation**:
    The server is launched by the `StartServer` method, which is called during the application's startup sequence. This ensures that the server is running and ready to accept connections as soon as the main window appears.

2.  **Process Creation**:
    A new `System.Diagnostics.Process` is created to run the Python interpreter. The `ProcessStartInfo` is configured to execute the `is5.py` script, which contains the server's source code.

3.  **Silent Operation**:
    The server process is configured to run in the background without a visible window (`CreateNoWindow = true`). This provides a seamless user experience, as the server operates transparently.

4.  **Communication and Logging**:
    The application captures the server's standard output and error streams. This allows for real-time logging and debugging. All server output is redirected and logged to `server_log.txt`, making it easy to diagnose issues without interrupting the user.

5.  **Termination**:
    When the application is closed, the `Window_Closing` event handler ensures that the server process is terminated. This prevents the server from becoming a "zombie" process and ensures a clean shutdown.

This integrated approach provides a self-contained and user-friendly experience, as the application manages the entire lifecycle of the server.
