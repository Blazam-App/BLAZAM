using BLAZAM.Database.Models.Chat;

namespace BLAZAM.Gui.UI.Chat
{
    public class ChatComponentBase : DatabaseComponentBase
    {
        /// <summary>
        /// Set's predefined page for this chat display
        /// </summary>

        [Parameter]
        public string ChatUri { get; set; }

        [Parameter]
        public ChatRoom? ChatRoom { get; set; }
        public ChatRoom? AppChatRoom { get; set; }



        protected int unreadAppChatMessages;
        protected int unreadChatMessages;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            Chat.OnMessagePosted += MessagePosted;
            Chat.OnMessageRead += MessageRead;
        }

        public override void Dispose()
        {
            base.Dispose();
            Chat.OnMessagePosted -= MessagePosted;
            Chat.OnMessageRead -= MessageRead;
        }

        private async void MessageRead(AppUser user)
        {
            if (CurrentUser.State.Id == user.Id)
            {
                await Task.Delay(50);

                await StateHasChangedAsync();
            }
        }

        private async void MessagePosted(ChatMessage message)
        {
            if ((ChatRoom != null && message.ChatRoomId.Equals(ChatRoom.Id))
                || (AppChatRoom != null && message.ChatRoomId.Equals(AppChatRoom.Id)))
            {
                await Task.Delay(100);
                await RefreshChatRooms();
                await StateHasChangedAsync();
            }
        }



        protected int LastUnreadMessages;

        protected int GetUnreadMessages()
        {

            if (ChatRoom is null)
            {
                return 0;
            }

            if (CurrentUser == null || CurrentUser.State.Preferences == null)
            {
                return 0;
            }

            return Chat.GetUnreadMessages(CurrentUser.State.Preferences).Count;


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

            if (ChatRoom is not null)
            {
                ChatRoom = await Chat.GetChatRoom(ChatRoom);

            }
            try
            {
                unreadAppChatMessages = Chat.GetUnreadMessages(CurrentUser.State.Preferences).Count(ur => ur.ChatRoomId == AppChatRoom.Id);
                unreadChatMessages = Chat.GetUnreadMessages(CurrentUser.State.Preferences).Count(ur => ur.ChatRoomId != AppChatRoom.Id);
            }
            catch (Exception ex)
            {
                Loggers.DatabaseLogger.Error(ex, "Error getting unread chat messages");
            }
        }
    }
}
