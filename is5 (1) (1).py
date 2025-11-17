import socketserver
import threading
import sys
import time

class ChatRoom:
    def __init__(self, room_id, room_name='Unnamed'):
        self.room_id = room_id
        self.room_name = room_name
        self.clients = {}
        self.lock = threading.Lock() 

    def add_client(self, username, client_handler):
        with self.lock:
            self.clients[username] = client_handler
        self.broadcast(f"{username}: joined us @chatroom {self.room_id}.", exclude_username=username)

    def remove_client(self, username, broadcast_message=None):
        with self.lock:
            if username in self.clients:
                del self.clients[username]
        if broadcast_message:
            self.broadcast(broadcast_message)
    
    def get_client_count(self):
        with self.lock:
            return len(self.clients)

    def get_chatters_list(self):
        with self.lock:
            return [f"{handler.username}@{handler.client_address}" for handler in self.clients.values()]

    def broadcast(self, message, exclude_username=None):
        with self.lock:
            for username, handler in list(self.clients.items()):
                if username == exclude_username:
                    continue
                try:
                    print(f"Send to {handler.client_address}: {message}")
                    handler.send(message)
                except Exception as e:
                    print(f"System: Error broadcasting to {username}: {e}")
                    del self.clients[username]

    def kick_client(self, username_to_kick, admin_message):
        with self.lock:
            if username_to_kick in self.clients:
                handler_to_kick = self.clients[username_to_kick]
                print(f"Send to {handler_to_kick.client_address}: {admin_message}")
                handler_to_kick.send(admin_message)
                handler_to_kick.room = None
                handler_to_kick.username = None
                del self.clients[username_to_kick]
                return True
            return False

    def close_room(self, admin_message):
        with self.lock:
            for username, handler in list(self.clients.items()):
                try:
                    print(f"Send to {handler.client_address}: {admin_message}")
                    handler.send(admin_message)
                    handler.room = None
                    handler.username = None
                except Exception as e:
                    print(f"System: Error closing room for {username}: {e}")
            self.clients.clear()


class ThreadedTCPServer(socketserver.ThreadingMixIn, socketserver.TCPServer):
    allow_reuse_address = True
    def __init__(self, server_address, RequestHandlerClass):
        super().__init__(server_address, RequestHandlerClass)
        self.rooms = {} 
        self.rooms_lock = threading.Lock() 


class ChatHandler(socketserver.BaseRequestHandler):
    def send(self, message):
        print(f"Send to {self.client_address}: {message}")
        self.request.sendall(f"{message}\0".encode())

    def handle(self):
        self.room = None
        self.username = None
        self.server_rooms = self.server.rooms
        self.server_rooms_lock = self.server.rooms_lock
        
        print(f"System: new connection from {self.client_address}")
        
        try:
            while True:
                data_raw = self.request.recv(1024)
                if not data_raw:
                    break 

                messages = data_raw.decode().split('\0')
                
                for data in messages:
                    if not data:
                        continue 
                    
                    data = data.strip()
                    print(f"Received message from {self.client_address}: {data}")

                    if data.startswith('`'):
                        self.handle_command(data)
                    else:
                        self.send_message(data)
        except Exception as e:
            pass 
        finally:
            print(f"System: Client {self.client_address} disconnected.")
            if self.room and self.username:
                quit_msg = f"Server: {self.username} quitted."
                self.room.remove_client(self.username, broadcast_message=quit_msg)

    def handle_command(self, command):
        parts = command.strip().split()
        cmd = parts[0][1:]

        if cmd == "start":
            if len(parts) >= 3:
                room_id, room_name = parts[1], " ".join(parts[2:])
                self.start_room(room_id, room_name)
            else:
                self.send("Usage: `start <id> <name>")
        
        elif cmd == "join":
            if len(parts) >= 3:
                room_id, username = parts[1], " ".join(parts[2:])
                self.join_room(room_id, username)
            else:
                self.send("Usage: `join <id> <username>")
        
        elif cmd == "list":
            self.list_rooms()
        
        elif cmd == "quit":
            self.quit_room()
        
        elif cmd == "exit":
            self.quit_room()

    def start_room(self, room_id, room_name):
        with self.server_rooms_lock:
            if room_id not in self.server_rooms:
                self.server_rooms[room_id] = ChatRoom(room_id, room_name)
                self.send(f"Chatroom {room_id} started!")
            else:
                self.send(f"Chatroom {room_id} exists!")

    def join_room(self, room_id, username):
        if self.room:
             self.send(f"You must quit your current chatroom {self.room.room_id} first.")
             return
        
        with self.server_rooms_lock:
            if room_id not in self.server_rooms:
                self.send(f"Chatroom {room_id} NOT exist!")
                return
            
            room_to_join = self.server_rooms[room_id]

        with room_to_join.lock:
            if username in room_to_join.clients:
                self.send(f"Server: Username '{username}' is already taken.")
                return

        self.room = room_to_join
        self.username = username
        self.room.add_client(self.username, self)
        self.send(f"You join @chatroom {self.room.room_id}")

    def list_rooms(self):
        with self.server_rooms_lock:
            if not self.server_rooms:
                self.send("No room exists!")
                return

            room_list = "\n"
            for room_id, room in self.server_rooms.items():
                room_list += f"{room.room_name}@{room_id}: "
                chatters = room.get_chatters_list()
                if not chatters:
                    room_list += "no chatters!\n"
                else:
                    room_list += " ".join(chatters) + "\n"
        
        self.send(room_list.strip())

    def quit_room(self):
        if self.room and self.username:
            room_name = self.room.room_name
            room_id = self.room.room_id
            quit_msg = f"Server: {self.username} quitted."
            self.room.remove_client(self.username, broadcast_message=quit_msg)
            self.send(f"You quitted from chatroom {room_name}@{room_id}")
            self.room = None
            self.username = None

    def send_message(self, message):
        if self.room and self.username:
            self.room.broadcast(f"{self.username}: {message}", exclude_username=self.username)
        else:
            self.send("Join a room first!")

def server_console(server):
    print("Chat administration console is running. Type '`help'.")
    while True:
        try:
            cmd_line = input("Chat administration:> ").strip()
            if not cmd_line:
                continue
            
            parts = cmd_line.split()
            cmd = parts[0]

            if cmd == "`help":
                print("\n`help`: Show usage of all commands.")
                print("`list`: List all chat rooms and the chatters in each.")
                print("`end <chatroom_id>`: Withdraw all chatters from the specified chat room and then end the room.")
                print("`start <chatroom_id> <chatroom_name>: Start a new chat room with the given ID and name.")
                print("`kick <chatter_name> <chatroom_id>`: Kick the specified chatter out of the given chat room.")
                print("`quit`:  Clean all chat rooms and end all chatter connections. Quit the server.\n")

            elif cmd == "`list":
                with server.rooms_lock:
                    if not server.rooms:
                        print("Administrator: No chatroom exists!")
                        continue
                    
                    print("\nAdministrator: Active Rooms:")
                    for room_id, room in server.rooms.items():
                        chatters = room.get_chatters_list()
                        if not chatters:
                            print(f"{room.room_name}@{room_id}: no chatters!")
                        else:
                            print(f"{room.room_name}@{room_id}: {' '.join(chatters)}")
                    print("")

            elif cmd == "`start":
                if len(parts) >= 3:
                    room_id, room_name = parts[1], " ".join(parts[2:])
                    with server.rooms_lock:
                        if room_id not in server.rooms:
                            server.rooms[room_id] = ChatRoom(room_id, room_name)
                            print(f"Administrator: Chatroom {room_id} started!")
                        else:
                            print(f"Administrator: Chatroom {room_id} exists!")
                else:
                    print("Usage: `start <id> <name>")
            
            elif cmd == "`kick":
                if len(parts) == 3:
                    chatter_name, room_id = parts[1], parts[2]
                    with server.rooms_lock:
                        if room_id not in server.rooms:
                            print("Administrator: Room ID not found.")
                            continue
                        room = server.rooms[room_id]
                    
                    kick_msg = f"You were kicked out from chatroom {room.room_name}@{room.room_id} by the Administrator!"
                    if room.kick_client(chatter_name, kick_msg):
                        print(f"Administrator: {chatter_name} was kicked from chatroom {room_id}.")
                        room.broadcast(f"Administrator: {chatter_name} was kicked out.")
                    else:
                        print(f"Administrator: {chatter_name} not found in room {room_id}.")
                else:
                    print("Usage: `kick <username> <room_id>")

            elif cmd == "`end":
                if len(parts) == 2:
                    room_id = parts[1]
                    with server.rooms_lock:
                        if room_id not in server.rooms:
                            print("Administrator: Room ID not found.")
                            continue
                        room = server.rooms.pop(room_id)
                    
                    end_msg = "Administrator: chatroom is being closed by the Administrator."
                    room.close_room(end_msg)
                    print(f"Administrator: chatroom {room_id} closed.")
                else:
                    print("Usage: `end <room_id>")

            elif cmd == "`quit":
                print("Administrator: notify clients about shutting down server...")
                with server.rooms_lock:
                    for room_id, room in list(server.rooms.items()):
                        room.close_room("Administrator: Chatserver is going to shutdown...")
                    server.rooms.clear()
                
                print("System: please terminate the console")
                print("Administrator: Shutting down server console...")
                server.shutdown()
                break
            
            else:
                print(f"Unknown commands.")

        except Exception as e:
            print(f"Administrator: Error in console: {e}")
        except KeyboardInterrupt:
            print("\nUse `quit to shut down the server.")


if __name__ == "__main__":
    HOST, PORT = "localhost", 9999
    
    try:
        server = ThreadedTCPServer((HOST, PORT), ChatHandler)
        
        server_thread = threading.Thread(target=server.serve_forever)
        server_thread.daemon = True
        server_thread.start()
        
        print(f"System: ChatServer is listening at ({HOST}, {PORT})...")

        server_console(server)

    except Exception as e:
        print(f"System: Failed to start server: {e}")
    except KeyboardInterrupt:
        print("\nSystem: Caught interrupt, shutting down.")
    finally:
        if 'server' in locals() and server_thread.is_alive():
            server.shutdown()
            server.server_close()
        print("\nSystem: Server shut down complete.")