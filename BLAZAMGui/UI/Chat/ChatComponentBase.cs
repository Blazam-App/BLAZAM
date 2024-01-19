using BLAZAM.Database.Models.Chat;
using BLAZAM.Database.Models.User;
using BLAZAM.Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Gui.UI.Chat
{
    public class ChatComponentBase : AppComponentBase
    {
        /// <summary>
        /// Set's predefined page for this chat display
        /// </summary>

        [Parameter]
        public string ChatUri { get; set; }

        [Parameter]
        public ChatRoom? ChatRoom { get; set; }
        public ChatRoom? AppChatRoom { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            Chat.OnMessagePosted += async (message) =>
            {
                if ((ChatRoom != null && message.ChatRoomId.Equals(ChatRoom.Id))
                || (AppChatRoom != null && message.ChatRoomId.Equals(AppChatRoom.Id)))
                {
                    await Task.Delay(100);
                    await RefreshSelectedChatRooms();
                    await InvokeAsync(StateHasChanged);
                }

            };
            Chat.OnMessageRead += async (user) =>
            {
                if (CurrentUser.State.Id == user.Id)
                {
                    await Task.Delay(50);

                    await InvokeAsync(StateHasChanged);
                }
            };
        }

        private async Task RefreshSelectedChatRooms()
        {

            await Chat.GetChatRoom(ChatRoom);
        }

        protected int UnreadMessages
        {
            get
            {
                if (ChatRoom is null) return 0;
                if (CurrentUser.State == null || CurrentUser.State.Preferences == null) return 0;
                    return Chat.GetUnreadMessages(CurrentUser.State.Preferences).Count();
                
            }
        }
        protected async Task RefreshChatRooms()
        {
            var room = Chat.AppChatRoom;
            if (room is null && ChatUri != null)
            {
                Chat.CreateChatRoom(new()
                {
                    Name = "App Chat",
                    IsPublic = true,
                });

            }

            AppChatRoom = room;
        }
    }
}
