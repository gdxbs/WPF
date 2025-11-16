# Testing Guide

## Pre-Testing Setup

### 1. Start Python Server
```bash
python is5 (1).py
```
Server will listen on `localhost:9999`

### 2. Build WPF Application
- Open `ChatClient.csproj` in Visual Studio
- Press `F5` to build and run

## Unit Testing Scenarios

### Connection Tests

#### Test 1.1: Successful Connection
1. Run application
2. Enter host: `localhost`, port: `9999`
3. Click "Connect"
4. **Expected**: Status shows "Connected" in green
5. **Verify**: "Disconnect" button is enabled

#### Test 1.2: Failed Connection - Wrong Port
1. Run application
2. Enter host: `localhost`, port: `9998`
3. Click "Connect"
4. **Expected**: Error message shown, status remains "Disconnected"
5. **Verify**: Connect button remains enabled

#### Test 1.3: Failed Connection - Invalid Host
1. Run application
2. Enter host: `invalid-host-xyz`, port: `9999`
3. Click "Connect"
4. **Expected**: Error message shown after timeout
5. **Verify**: Status shows "Disconnected"

#### Test 1.4: Connection Persistence
1. Connect to server (Test 1.1)
2. Wait 30 seconds
3. **Expected**: Connection remains active
4. **Verify**: Status still shows "Connected"

#### Test 1.5: Graceful Disconnect
1. Connect to server (Test 1.1)
2. Click "Disconnect"
3. **Expected**: Status shows "Disconnected"
4. **Verify**: Connect button re-enabled, tabs cleared

### Room Management Tests

#### Test 2.1: Create Room
1. Connect to server
2. In left panel, enter Room ID: `test-room-1`
3. Enter Room Name: `Test Room 1`
4. Click "Create"
5. **Expected**: Server responds with confirmation
6. **Verify**: "List Rooms" shows the new room

#### Test 2.2: Create Room with Duplicate ID
1. Create room "test-room-1" (from Test 2.1)
2. Try creating another room with same ID
3. **Expected**: Server shows "Chatroom test-room-1 exists!" message
4. **Verify**: No new tab created

#### Test 2.3: Join Room (Single)
1. Create room "general" (Test 2.1)
2. In left panel, enter Room ID: `general`
3. Enter Username: `alice`
4. Click "Join"
5. **Expected**: New tab appears labeled "general"
6. **Verify**: Tab is active and empty (no messages yet)

#### Test 2.4: Join Room with Duplicate Username
1. Join room "general" as "alice" (Test 2.3)
2. Try joining again with same username
3. **Expected**: Server error: "Username 'alice' is already taken"
4. **Verify**: No new tab created

#### Test 2.5: Leave Room
1. Join room "general" as "alice" (Test 2.3)
2. Click "Leave Room"
3. **Expected**: Tab closes, message shows "You quitted from chatroom..."
4. **Verify**: "Leave Room" button becomes disabled

#### Test 2.6: Join Non-Existent Room
1. In left panel, enter Room ID: `nonexistent`
2. Enter Username: `bob`
3. Click "Join"
4. **Expected**: Server error: "Chatroom nonexistent NOT exist!"
5. **Verify**: No new tab created

### Multi-Room Tests

#### Test 3.1: Join Multiple Rooms
1. Create rooms: `room1`, `room2`, `room3`
2. Join `room1` as `user1`
3. Join `room2` as `user2`
4. Join `room3` as `user3`
5. **Expected**: Three tabs visible
6. **Verify**: Can click between tabs

#### Test 3.2: Switch Rooms
1. Complete Test 3.1
2. Click on `room1` tab
3. Send message: `Hello from room 1`
4. Click on `room2` tab
5. Send message: `Hello from room 2`
6. **Expected**: Each room shows only its message
7. **Verify**: Switch back and forth, messages persist

#### Test 3.3: Independent User Lists
1. Complete Test 3.1
2. Join `room1` again as different user
3. Click `room2` tab
4. **Expected**: Right panel shows only room2 users
5. **Verify**: Switch to room1, shows only room1 users

#### Test 3.4: Leave One Room, Stay in Others
1. Complete Test 3.1
2. While in `room2` tab, click "Leave Room"
3. **Expected**: `room2` tab closes
4. **Verify**: `room1` and `room3` tabs remain

### Message Tests

#### Test 4.1: Send Simple Message
1. Join room as user (Test 2.3)
2. In message input, type: `Hello everyone!`
3. Click "Send"
4. **Expected**: Message appears in chat area
5. **Verify**: Format: `alice: Hello everyone!`

#### Test 4.2: Send Empty Message
1. Join room (Test 2.3)
2. Leave message input empty
3. Click "Send"
4. **Expected**: Nothing happens, no error
5. **Verify**: No message sent to chat

#### Test 4.3: Send Long Message
1. Join room (Test 2.3)
2. Type 500+ character message
3. Click "Send"
4. **Expected**: Message wraps in chat area
5. **Verify**: Full message is readable

#### Test 4.4: Special Characters
1. Join room (Test 2.3)
2. Send message with symbols: `!@#$%^&*()`
3. **Expected**: Message displays correctly
4. **Verify**: Characters not corrupted

#### Test 4.5: Rapid Messages
1. Join room (Test 2.3)
2. Send 10 messages rapidly
3. **Expected**: All messages appear in order
4. **Verify**: No messages lost or duplicate

#### Test 4.6: Message Persistence in Tab
1. Join room, send message (Test 4.1)
2. Switch to another room
3. Switch back
4. **Expected**: Original message still visible
5. **Verify**: Scroll shows full message history

### System Message Tests

#### Test 5.1: User Join Notification
1. In one window, join room as `alice`
2. In another terminal, run Python client and join same room as `bob`
3. **Expected**: Chat shows `Server: bob joined us @chatroom`
4. **Verify**: Sound plays if enabled

#### Test 5.2: User Leave Notification
1. Complete Test 5.1
2. Exit Python client with `quit`
3. **Expected**: Chat shows `Server: bob quitted`
4. **Verify**: WPF user list updates

#### Test 5.3: Room Created Notification
1. While in room, admin creates new room via server console
2. Click "List Rooms"
3. **Expected**: New room appears in list
4. **Verify**: Can join it

### Settings Tests

#### Test 6.1: Change Theme - Light to Dark
1. Click ⚙ icon
2. Select "Appearance" tab
3. Click "Dark" radio button
4. **Expected**: UI changes to dark colors immediately
5. Click "Save"
6. **Expected**: Message "Settings saved successfully"
7. Close and reopen app
8. **Expected**: Dark theme persists

#### Test 6.2: Change Accent Color
1. Click ⚙ icon
2. Select "Appearance" tab
3. Click "Green" button
4. **Expected**: Accent color changes to green
5. Click "Save"
6. **Expected**: Settings saved

#### Test 6.3: Adjust Font Size
1. Click ⚙ icon
2. Select "Appearance" tab
3. Move "Font Size" slider to 16
4. **Expected**: Text in preview grows larger
5. Click "Save"
6. **Expected**: App text enlarged

#### Test 6.4: Sound Notifications
1. Click ⚙ icon
2. Select "Notifications" tab
3. Check "Enable Sound Notifications"
4. Set volume to 100%
5. Click "Save"
6. Send/receive message in room
7. **Expected**: Sound plays at maximum volume

#### Test 6.5: Disable Notifications
1. Complete Test 6.4
2. Click ⚙ icon
3. Uncheck "Message Notifications"
4. Click "Save"
5. Send message in room
6. **Expected**: No sound plays

#### Test 6.6: Auto-Connect
1. Click ⚙ icon
2. Select "Connection" tab
3. Verify "Last Connection" shows: `localhost` : `9999`
4. Check "Auto-connect on startup"
5. Click "Save"
6. Close application
7. Reopen application
8. **Expected**: Automatically connects to localhost:9999
9. **Verify**: Status shows "Connected" immediately

#### Test 6.7: Custom Connection Details
1. Click ⚙ icon
2. Select "Connection" tab
3. Change host to `127.0.0.1`, port to `9999`
4. Click "Save"
5. Disconnect and reconnect using new details
6. **Expected**: Connection successful
7. **Verify**: Next startup remembers these details

### List Rooms Tests

#### Test 7.1: List Empty
1. Connect to server
2. Click "List Rooms"
3. **Expected**: Shows "No room exists!"

#### Test 7.2: List Rooms
1. Create rooms: `room1`, `room2`
2. Join both rooms
3. Click "List Rooms"
4. **Expected**: Shows both rooms with their users

#### Test 7.3: List After User Joins
1. Join `room1`
2. From another app instance, join `room1`
3. Click "List Rooms"
4. **Expected**: `room1` shows both users

### Integration Tests

#### Test 8.1: Full Chat Session
1. Start Python server
2. Launch WPF app
3. Connect to server
4. Create room `general`
5. Join as `alice`
6. Send message `Hi everyone`
7. Launch second WPF instance
8. Connect to server
9. Join `general` as `bob`
10. Verify `alice` sees `bob joined` notification
11. `bob` sends `Hello alice`
12. Verify both see each other's messages
13. `alice` switches to another room
14. Create and join `team-chat`
15. Send message in `team-chat`
16. Switch back to `general`
17. Verify messages in correct rooms
18. Leave `general`
19. Verify tab closes, `team-chat` remains
20. **Expected**: All operations succeed as described

#### Test 8.2: Connection Recovery
1. Connect to server
2. Join room
3. Stop Python server
4. **Expected**: App shows connection lost after timeout
5. Restart Python server
6. Click "Connect"
7. **Expected**: Reconnection successful

#### Test 8.3: Multiple Simultaneous Connections
1. Launch 3 WPF app instances
2. Each connects to server
3. Create room `chat`
4. Each joins as different user
5. Each sends messages
6. **Expected**: All receive messages from all others
7. **Verify**: No interference between connections

### Edge Case Tests

#### Test 9.1: Rapid Connect/Disconnect
1. Connect to server
2. Immediately click Disconnect
3. **Expected**: No errors, clean state
4. **Verify**: Can reconnect

#### Test 9.2: Connect While List Shows
1. Join room
2. Click "List Rooms"
3. Immediately disconnect
4. **Expected**: List disappears, status updates
5. **Verify**: No orphaned UI elements

#### Test 9.3: Close App During Message Send
1. Connect and join room
2. Start typing long message
3. Close app while message in flight
4. **Expected**: App sends exit command before closing
5. **Verify**: Server shows user left

#### Test 9.4: Very Large Room
1. (Simulator test: modify server code temporarily)
2. Create room with 100+ users
3. Join room
4. **Expected**: App doesn't crash, shows users
5. **Verify**: Scrolling through user list works

## Performance Testing

### Test 10.1: Message Throughput
1. Join room
2. Rapidly send 100 messages
3. **Expected**: All messages appear
4. **Verify**: No lag in UI

### Test 10.2: Multiple Rooms Load
1. Join 10 rooms
2. Send message in each
3. **Expected**: App remains responsive
4. **Verify**: Switching tabs is smooth

### Test 10.3: Memory Stability
1. Leave app running for 1 hour
2. Monitor Task Manager memory usage
3. **Expected**: Memory stable (no constant growth)
4. **Verify**: No UI freezing

## Regression Test Checklist

After any code changes, verify:

- [ ] Connect button works
- [ ] Disconnect button works
- [ ] Create room button works
- [ ] Join room button works
- [ ] List rooms button works
- [ ] Leave room button works
- [ ] Send message button works
- [ ] All tabs switch correctly
- [ ] Settings window opens
- [ ] Settings apply immediately
- [ ] Settings save to database
- [ ] Sound notifications play
- [ ] Reconnect works
- [ ] No error messages on startup
- [ ] App closes without errors

## Bug Reporting

When reporting issues, provide:
1. Steps to reproduce
2. Expected behavior
3. Actual behavior
4. Screenshots if applicable
5. Server logs (if relevant)
6. App error messages or exceptions
7. Windows version
8. Network setup (localhost vs remote)
