using BLAZAM.Database.Models.Chat;
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
        /// <summary>
        /// Set's predefined page for this chat display
        /// </summary>

        [Parameter]
        public string ChatUri { get; set; }

        [Parameter]
        public ChatRoom? ChatRoom { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            Chat.OnMessagePosted += async (message) =>
            {
                await Task.Delay(100);
                await RefreshChatRoom();
                await InvokeAsync(StateHasChanged);

            };
            Chat.OnMessageRead += async (user) =>
            {
                if (CurrentUser.State.Id == user.Id)
                {

                    await RefreshChatRoom();
                    await InvokeAsync(StateHasChanged);
                }
            };
        }
        protected int UnreadMessages
        {
            get
            {
                if (ChatRoom is null) return 0;
                //TODO  Pull from DB
                return ChatRoom.Messages.Count(m => !m.ReadByUsers.Contains(CurrentUser.State.Preferences));
            
            }
        }
        protected async Task RefreshChatRoom()
        {
            var room = (await Chat.GetChatRoomsAsync()).Where(cr => cr.Name.Equals(ChatUri)).FirstOrDefault();
            if (room is null && ChatUri!=null)
            {
                Chat.CreateChatRoom(new()
                {
                    Name = ChatUri,
                    IsPublic = true,
                });

            }

            ChatRoom = room;
        }
    }
}
