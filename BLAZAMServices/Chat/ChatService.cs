using BLAZAM.Database.Context;
using BLAZAM.Database.Models.Chat;
using BLAZAM.Database.Models.User;
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
        public List<ChatRoom> GetChatRooms() => Context.ChatRooms.ToList();

       public async Task<List<ChatRoom>> GetChatRoomsAsync()
        {
            using var context = await _appDatabaseFactory.CreateDbContextAsync();
            return await context.ChatRooms.ToListAsync();
        }

        private IAppDatabaseFactory _appDatabaseFactory { get; set; }

        public IDatabaseContext Context => _appDatabaseFactory.CreateDbContext();

        public ChatService(IAppDatabaseFactory appDatabaseFactory) => _appDatabaseFactory = appDatabaseFactory;

        public void CreateChatRoom(ChatRoom room)
        {
            var context = Context;
            context.ChatRooms.Add(room);
            context.SaveChanges();
        }

        public ChatRoom? GetPrivateTwoWayChat(List<AppUser> parties )
        {
            if (parties.Count != 2) throw new ApplicationException("GetPrivateTwoWayChat must only be supplied with two users");
            var context = Context;
            var localParties = new List<AppUser>();
            parties.ForEach(p => { 
                localParties.Add(context.UserSettings.Where(us=>us.Id== p.Id).FirstOrDefault());
            });
            parties = localParties;
            //ChatRoom matchingPrivateChat = null;
            //var chat= context.ChatRooms.Where(cr => cr.IsPublic == false
            //&& cr.Members.Count == 2).FirstOrDefault();
            var chat= context.ChatRooms.Where(cr => cr.IsPublic == false
            && cr.Members.Count == 2
            && cr.Members.Any(m=>m.Id==parties[0].Id)
            && cr.Members.Any(m => m.Id == parties[1].Id)).FirstOrDefault();
            
            var chat2= context.ChatRooms.Where(cr => cr.IsPublic == false
            && cr.Members.Count == 2
            && cr.Members.Any(m=>m.Equals(parties[0]))
            && cr.Members.Any(m => m.Equals(parties[1]))).FirstOrDefault();
            if (chat == null)
            {
                chat = new ChatRoom() { Name ="Private Chat", IsPublic = false, Members = parties };

                context.ChatRooms.Add(chat);
                try
                {
                    context.SaveChanges();
                }catch (Exception ex)
                {

                }
            }
            return chat;
        }
        public void PostMessage(ChatMessage message)
        {
            var context = Context;
            if(message.User != null) { 
            message.User=context.UserSettings.Where(us=>us.Id==message.User.Id).FirstOrDefault();
               }
            context.ChatMessages.Add(message);
            context.SaveChanges();
            OnMessagePosted?.Invoke(message);
        }
        public void MessageRead(ChatMessage message,IApplicationUserState user) {
            message.ReadByUsers.Add(user.Preferences);
            var context = Context;
                message = context.ChatMessages.Where(cm => cm.Id == message.Id).FirstOrDefault();
            var userInDb = context.UserSettings.Where(us => us.Id == user.Id).FirstOrDefault();
            //userInDb.ReadChatMessages.Add(message);
            message?.ReadByUsers.Add(userInDb);

            context.SaveChanges();
            OnMessageRead?.Invoke(user);
        }
    }
}
