using BLAZAM.Database.Models.User;
using BLAZAM.Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Database.Models.Chat
{
    public class ReadChatMessage:AppDbSetBase
    {
        public ChatRoom ChatRoom { get; set; }
        public ChatMessage ChatMessage { get; set; }
        public AppUser User { get; set; }
        public bool IsRead { get; set; }
    }
}
