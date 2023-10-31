using BLAZAM.Database.Context;
using BLAZAM.Database.Models.Chat;
using BLAZAM.Database.Models.User;
using BLAZAM.Helpers;
using BLAZAM.Server.Data;
using BLAZAM.Session.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MudBlazor.CategoryTypes;

namespace BLAZAM.Services.Chat
{
    public class ChatService : IChatService
    {
        private List<ChatRoom> chatRooms;

        private List<ChatRoom> ChatRooms
        {
            get
            {
                if (chatRooms == null)
                {
                    var context = Context;
                    chatRooms = context.ChatRooms.ToList();
                }
                return chatRooms;
            }
            set => chatRooms = value;
        }
        public AppEvent<ChatMessage> OnMessagePosted { get; set; }
        public AppEvent<AppUser> OnMessageRead { get; set; }
        public AppEvent<ChatRoom> OnChatRoomCreated { get; set; }
        public List<ChatRoom> GetChatRooms() => Context.ChatRooms.ToList();

        public async Task<List<ChatRoom>> GetChatRoomsAsync()
        {
            if (ChatRooms is null)
            {
                var context = Context;
                ChatRooms = await context.ChatRooms.ToListAsync();
            }
            return ChatRooms;
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

        public ChatService(IAppDatabaseFactory appDatabaseFactory) => _appDatabaseFactory = appDatabaseFactory;

        public void CreateChatRoom(ChatRoom room)
        {
            var context = Context;
            context.ChatRooms.Add(room);
            context.SaveChanges();
            ChatRooms.Add(room);
            OnChatRoomCreated?.Invoke(room);

        }

        public ChatRoom? GetPrivateTwoWayChat(AppUser currentUser, AppUser otherUser)
        {
            if (currentUser == null && otherUser == null) throw new ApplicationException("GetPrivateTwoWayChat must only be supplied with two users");
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
                chat.Name = chat.Name;
                context.ChatRooms.Add(chat);
                try
                {
                    context.SaveChanges();
                    ChatRooms.Add(chat);
                    OnChatRoomCreated?.Invoke(chat);
                }
                catch (Exception ex)
                {

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
            ChatRooms = new();
        }

        public async Task<ChatRoom?> GetChatRoom(ChatRoom? chatRoom)
        {
            chatRoom = ChatRooms.Where(cr => cr.Equals(chatRoom)).FirstOrDefault();
            //var context = Context;
            // return null;
            //chatRoom = await context.ChatRooms.Where(cr => cr.Equals(chatRoom)).FirstOrDefaultAsync();
            return chatRoom;
        }
    }
}
