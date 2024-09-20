using BLAZAM.Database.Models.User;
using BLAZAM.Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Database.Models.Chat
{
    public class UnreadChatMessage : AppDbSetBase
    {
        public ChatMessage ChatMessage { get; set; }
        //This  causes a cyclic FK loop
        //public ChatRoom ChatRoom { get; set; }
        //This  causes a cyclic FK loop
        //public AppUser User { get; set; }
        public int ChatRoomId { get; set; }
        public int ChatMessageId { get; set; }
        public int UserId { get; set; }
    }
}
