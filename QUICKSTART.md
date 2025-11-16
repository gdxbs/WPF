# Quick Start Guide

## 5-Minute Setup

### 1. Open in Visual Studio
- File → Open → Project/Solution
- Select `ChatClient.csproj`

### 2. Build & Run
- Press `F5` to build and run
- NuGet packages will auto-restore

### 3. Connect to Server
- Ensure Python server is running on `localhost:9999`
- Click "Connect" button in the app
- Status indicator turns green when connected

### 4. Create & Join a Room
- Left panel: Enter Room ID (e.g., "general")
- Enter Room Name (e.g., "General Chat")
- Click "Create" button
- Enter same Room ID and your Username
- Click "Join" button

### 5. Chat
- Type message in the input field at bottom of tab
- Click "Send" or press Enter
- Messages appear in the chat area

### 6. Multi-Room (Optional)
- Repeat step 4 with different Room ID to join another room
- Click tabs to switch between rooms
- Each room shows its own messages

## Common Tasks

### Change Theme
1. Click ⚙ icon (top-right)
2. Select "Appearance" tab
3. Click "Dark" or "Light"
4. Click "Save"

### Adjust Font Size
1. Click ⚙ icon
2. Select "Appearance" tab
3. Move "Font Size" slider
4. Click "Save"

### Enable Sound
1. Click ⚙ icon
2. Select "Notifications" tab
3. Check "Enable Sound Notifications"
4. Adjust volume slider
5. Click "Save"

### Change Accent Color
1. Click ⚙ icon
2. Select "Appearance" tab
3. Click color button (Blue, Green, Orange, Red)
4. Click "Save"

### Auto-Connect
1. Click ⚙ icon
2. Select "Connection" tab
3. Check "Auto-connect on startup"
4. Click "Save"

## Keyboard Shortcuts

| Action | Shortcut |
|--------|----------|
| Send Message | `Enter` (when in message input) |
| Connect | N/A - use button |
| Open Settings | Click ⚙ |
| Close Window | `Alt+F4` (sends `exit` command) |

## Protocol Reference

The app uses these commands with the Python server:

```
`join <room_id> <username>    - Join a room
`start <room_id> <room_name>  - Create a room
`list                         - List all rooms
`quit                         - Leave current room
`exit                         - Disconnect from server
```

Regular chat messages are sent without the backtick prefix.

## Supabase Database

Your preferences are stored in Supabase:
- **Table**: `user_preferences`
- **Identifier**: Unique Machine ID
- **Data**: Theme, colors, volume, connection details

The app handles all database operations automatically. No manual database work needed.

## Troubleshooting Quick Fixes

### "Connection refused"
- [ ] Is Python server running?
- [ ] Check host/port in connection panel
- [ ] Check Windows firewall

### "No servers in list"
- [ ] No one has created rooms yet
- [ ] You must create a room first with "Create" button
- [ ] Click "List Rooms" to refresh

### "Can't send messages"
- [ ] Are you connected? (check green status indicator)
- [ ] Have you joined a room? (should see tab with room name)
- [ ] Make sure message input field is not empty

### Settings not saving
- [ ] Click "Save" button (not just "X" to close)
- [ ] Check internet for Supabase sync
- [ ] Try Save again

## Next Steps

- Explore Settings to customize the UI
- Experiment with creating and joining multiple rooms
- Try different themes and colors
- Configure sound notifications
- Set up auto-connect for faster startup

For detailed information, see `SETUP.md`
