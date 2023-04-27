using BLAZAM.Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Gui.UI.Chat
{
    public class ChatComponentBase:AppComponentBase
    {
        [Parameter]
        public List<ChatMessage> ChatMessages { get; set; }

        protected int UnreadMessages
        {
            get
            {
                return ChatMessages.Where(
                    cm => cm.User != CurrentUser.State.Preferences
                    && !cm.SeenBy.Contains(CurrentUser.State.Preferences
                    )).Count();
            }
        }
    }
}
