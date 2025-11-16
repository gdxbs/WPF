using System;
using System.Collections.ObjectModel;

namespace ChatClient.Models
{
    public class ChatRoom
    {
        public string RoomId { get; set; }
        public string RoomName { get; set; }
        public ObservableCollection<string> Messages { get; set; }
        public ObservableCollection<string> Users { get; set; }
        public DateTime JoinedTime { get; set; }
        public bool HasUnreadMessages { get; set; }
        public int UnreadCount { get; set; }

        public ChatRoom()
        {
            Messages = new ObservableCollection<string>();
            Users = new ObservableCollection<string>();
            HasUnreadMessages = false;
            UnreadCount = 0;
        }

        public void AddMessage(string message)
        {
            Messages.Add(message);
        }

        public void ClearMessages()
        {
            Messages.Clear();
        }
    }
}
