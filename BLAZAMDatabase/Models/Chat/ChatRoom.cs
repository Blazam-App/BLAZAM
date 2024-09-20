
using BLAZAM.Database.Models.User;
using BLAZAM.Helpers;
using BLAZAM.Server.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Database.Models.Chat
{
    public class ChatRoom : AppDbSetBase
    {
        private string _name;
        public string Name
        {
            get
            {
                if (!_name.IsNullOrEmpty()) return _name;
                return String.Join(", ", Members.OrderBy(m => m.Username).Select(m => m.Username).ToArray());
            }
            set => _name = value;
        }
        public List<ChatMessage> Messages { get; set; } = new();
        public List<AppUser> Members { get; set; } = new();
        [NotMapped]
        public int MemberCount { get => Members.Count; set { _ = value; } }
        public long MembersHash
        {
            get
            {
                long hash = 0;
                foreach (var member in Members)
                {
                    hash += member.Username.GetAppHashCode();
                }
                return hash;
            }
            set { _ = value; }
        }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }
        public bool IsPublic { get; set; }
        [NotMapped]
        public string ElementId => "ChatWindow" + Id + Name.GetAppHashCode();
    }
}
