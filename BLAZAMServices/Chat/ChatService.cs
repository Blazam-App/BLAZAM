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

namespace BLAZAM.Services.Chat
{
    public class ChatService : IChatService
    {
        public AppEvent<ChatMessage> OnMessagePosted { get; set; }
        public AppEvent<IApplicationUserState> OnMessageRead { get; set; }
        public List<ChatRoom> GetChatRooms() => null;//Context.ChatRooms.ToList();

        public async Task<List<ChatRoom>> GetChatRoomsAsync()
        {
            var context = Context;
            return null;
           // return await context.ChatRooms.ToListAsync();
        }

        private IAppDatabaseFactory _appDatabaseFactory { get; set; }

        public IDatabaseContext Context => _appDatabaseFactory.CreateDbContext();

        public ChatService(IAppDatabaseFactory appDatabaseFactory) => _appDatabaseFactory = appDatabaseFactory;

        public void CreateChatRoom(ChatRoom room)
        {
            var context = Context;
            //context.ChatRooms.Add(room);
            //context.SaveChanges();
        }

        public ChatRoom? GetPrivateTwoWayChat(List<AppUser> parties)
        {
            if (parties.Count != 2) throw new ApplicationException("GetPrivateTwoWayChat must only be supplied with two users");
            var context = Context;
            var localParties = new List<AppUser>();
            parties.ForEach(p =>
            {
                localParties.Add(context.UserSettings.Where(us => us.Id == p.Id).FirstOrDefault());
            });
            parties = localParties;

            return null;
            //var chat = context.ChatRooms.Where(cr => cr.IsPublic == false
            //&& cr.MembersHash == parties.GetMembersHash()).FirstOrDefault();

            //if (chat == null)
            //{
            //    chat = new ChatRoom() { Name = "Private Chat", IsPublic = false, Members = parties };

            //    context.ChatRooms.Add(chat);
            //    try
            //    {
            //        context.SaveChanges();
            //    }
            //    catch (Exception ex)
            //    {

            //    }
            //}
            //return chat;
        }
        public void PostMessage(ChatMessage message)
        {
            //var context = Context;
            //if (message.User != null)
            //{
            //    message.User = context.UserSettings.Where(us => us.Id == message.User.Id).FirstOrDefault();
            //}
           
            //context.ChatMessages.Add(message);
            //context.SaveChanges();
            OnMessagePosted?.Invoke(message);
        }
        public void MessageRead(ChatMessage message, IApplicationUserState user)
        {
            var context = Context;
           // var dbEntry = context.ReadChatMessages.Where(rm => rm.ChatMessageId == message.Id && rm.UserId == user.Id).FirstOrDefault();
           
           //if(dbEntry == null)
           // {
           //     dbEntry = new()
           //     {
           //         ChatMessageId = message.Id,
           //         UserId = user.Id
           //     };
           //     context.ReadChatMessages.Add(dbEntry);
           // }
           // context.SaveChanges();
          
            OnMessageRead?.Invoke(user);
        }

        public void DeleteAllChatRooms()
        {
            var context = Context;
            //var allChatRooms = context.ChatRooms.ToList();
            //context.ChatRooms.RemoveRange(allChatRooms.ToArray());
            //context.SaveChanges();
        }

        public async Task<ChatRoom?> GetChatRoom(ChatRoom? chatRoom)
        {
            var context = Context;
            return null;
            //chatRoom = await context.ChatRooms.Where(cr => cr.Equals(chatRoom)).FirstOrDefaultAsync();
            //return chatRoom;
        }
    }
}
