using System;
using System.Collections.Generic;
using ChatClient.Models;

namespace ChatClient.Services
{
    public class MessageRouterService
    {
        private Dictionary<string, ChatRoom> _rooms;
        public event EventHandler<(string RoomId, string Message)> MessageRouted;

        public MessageRouterService()
        {
            _rooms = new Dictionary<string, ChatRoom>();
        }

        public void RegisterRoom(string roomId, ChatRoom room)
        {
            if (!_rooms.ContainsKey(roomId))
            {
                _rooms.Add(roomId, room);
            }
        }

        public void UnregisterRoom(string roomId)
        {
            _rooms.Remove(roomId);
        }

        public void RouteMessage(string message, string currentRoomId)
        {
            if (string.IsNullOrEmpty(message))
                return;

            string targetRoomId = ExtractRoomIdFromMessage(message);

            if (string.IsNullOrEmpty(targetRoomId))
                targetRoomId = currentRoomId;

            if (_rooms.TryGetValue(targetRoomId, out var room))
            {
                room.AddMessage(message);
                MessageRouted?.Invoke(this, (targetRoomId, message));
            }
        }

        public ChatRoom GetRoom(string roomId)
        {
            _rooms.TryGetValue(roomId, out var room);
            return room;
        }

        public IEnumerable<string> GetRoomIds()
        {
            return _rooms.Keys;
        }

        private string ExtractRoomIdFromMessage(string message)
        {
            if (message.Contains("@chatroom"))
            {
                var parts = message.Split(new[] { "@chatroom" }, StringSplitOptions.None);
                if (parts.Length > 1)
                {
                    var roomPart = parts[1].Trim().Split(new[] { ' ', '.' }, StringSplitOptions.RemoveEmptyEntries);
                    if (roomPart.Length > 0)
                    {
                        return roomPart[0];
                    }
                }
            }

            return null;
        }

        public void ClearAllRooms()
        {
            _rooms.Clear();
        }
    }
}
