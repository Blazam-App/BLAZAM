using BLAZAM.Common.Exceptions;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models.Chat;
using BLAZAM.Database.Models.User;
using BLAZAM.Helpers;
using BLAZAM.Logger;
using BLAZAM.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Services.Chat
{
    public class ChatService : IChatService
    {

        public IQueryable<ChatRoom> ChatRooms => Context.ChatRooms.AsQueryable();
        public AppEvent<ChatMessage> OnMessagePosted { get; set; }
        public AppEvent<AppUser> OnMessageRead { get; set; }
        public AppEvent<ChatRoom> OnChatRoomCreated { get; set; }

        public async Task<IQueryable<ChatRoom>> GetChatRoomsAsync()
        {
            return await Task.Run(() =>
            {
                return ChatRooms;


            });
        }
        public List<ChatRoom> GetPrivateChats(AppUser user)
        {
            return ChatRooms.Where(cr => cr.Members.Any(m => m.Id == user.Id)).ToList();
        }
        public List<ChatMessage> GetUnreadMessages(AppUser user)
        {
            var context = Context;
            return context.UnreadChatMessages.Where(rm => rm.UserId == user.Id).Select(rm => rm.ChatMessage).ToList();

        }
        public List<ChatMessage> GetUnreadMessages(AppUser user, ChatRoom room)
        {
            var context = Context;
            return context.UnreadChatMessages.Where(rm => rm.UserId == user.Id && rm.ChatRoomId == room.Id).Select(rm => rm.ChatMessage).ToList();

        }
        private IAppDatabaseFactory _appDatabaseFactory { get; set; }

        private IDatabaseContext Context => _appDatabaseFactory.CreateDbContext();

        public ChatRoom? AppChatRoom => ChatRooms.Where(cr => cr.Name.Equals("App Chat")).FirstOrDefault();

        public ChatService(IAppDatabaseFactory appDatabaseFactory) => _appDatabaseFactory = appDatabaseFactory;

        public void CreateChatRoom(ChatRoom room)
        {
            var context = Context;
            context.ChatRooms.Add(room);
            context.SaveChanges();
            //ChatRooms.Add(room);
            OnChatRoomCreated?.Invoke(room);

        }

        public ChatRoom? GetPrivateTwoWayChat(AppUser currentUser, AppUser otherUser)
        {
            if (currentUser == null && otherUser == null) throw new AppException("GetPrivateTwoWayChat must only be supplied with two users");
            var context = Context;
            var localParties = new List<AppUser>
            {
                context.UserSettings.Where(us => us.Id == currentUser.Id).FirstOrDefault(),
                context.UserSettings.Where(us => us.Id == otherUser.Id).FirstOrDefault()
            };



            var chat = ChatRooms.Where(cr => cr.IsPublic == false
            && cr.MembersHash == localParties.GetMembersHash()).FirstOrDefault();

            if (chat == null)
            {
                chat = new ChatRoom()
                {

                    IsPublic = false,
                    Members = localParties
                };
                context.ChatRooms.Add(chat);
                try
                {
                    context.SaveChanges();
                    //ChatRooms.Add(chat);
                    OnChatRoomCreated?.Invoke(chat);
                }
                catch (Exception ex)
                {
                    Loggers.DatabaseLogger.Error("Unable to creat private chat room {@Error}", ex);
                }
            }
            return chat;
        }
        public void PostMessage(ChatMessage message)
        {
            var context = Context;
            if (message.User != null)
            {
                message.User = context.UserSettings.Where(us => us.Id == message.User.Id).FirstOrDefault();
            }
            if (message.ChatRoom != null)
            {
                message.ChatRoom = context.ChatRooms.Where(cr => cr.Id == message.ChatRoom.Id).FirstOrDefault();
            }

            context.ChatMessages.Add(message);



            context.SaveChanges();



            if (message.ChatRoom.IsPublic)
            {
                foreach (var user in context.UserSettings)
                {
                    var dbEntry = new UnreadChatMessage()
                    {
                        ChatMessageId = message.Id,
                        UserId = user.Id,
                        ChatRoomId = message.ChatRoomId
                    };
                    context.UnreadChatMessages.Add(dbEntry);
                }
            }
            else
            {
                foreach (var member in message.ChatRoom.Members)
                {
                    var dbEntry = new UnreadChatMessage()
                    {
                        ChatMessageId = message.Id,
                        UserId = member.Id,
                        ChatRoomId = message.ChatRoomId
                    };
                    context.UnreadChatMessages.Add(dbEntry);
                }
            }
            context.SaveChanges();
            ChatRooms.Where(cr => cr.Id == message.ChatRoomId).First().Messages.Add(message);

            OnMessagePosted?.Invoke(message);
        }
        /// <summary>
        /// Marks a message as read in the database
        /// </summary>
        /// <param name="message"></param>
        /// <param name="user"></param>
        public void MessageRead(ChatMessage message, AppUser user)
        {
            var context = Context;
            var dbEntry = context.UnreadChatMessages.Where(rm => rm.ChatMessageId == message.Id && rm.UserId == user.Id).FirstOrDefault();

            if (dbEntry != null)
            {
                context.UnreadChatMessages.Remove(dbEntry);
                context.SaveChanges();
                OnMessageRead?.Invoke(user);
            }

        }

        public void DeleteAllChatRooms()
        {
            var context = Context;
            var allChatRooms = context.ChatRooms.ToList();
            context.ChatRooms.RemoveRange(allChatRooms.ToArray());
            context.SaveChanges();
            //ChatRooms = new();
        }

        public async Task<ChatRoom?> GetChatRoom(ChatRoom? chatRoom)
        {
            chatRoom = await ChatRooms.Where(cr => cr.Equals(chatRoom)).FirstOrDefaultAsync();
            //var context = Context;
            // return null;
            //chatRoom = await context.ChatRooms.Where(cr => cr.Equals(chatRoom)).FirstOrDefaultAsync();
            return chatRoom;
        }
    }
}
