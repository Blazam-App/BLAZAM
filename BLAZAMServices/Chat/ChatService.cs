using BLAZAM.Database.Context;
using BLAZAM.Database.Models.Chat;
using BLAZAM.Server.Data;
using BLAZAM.Session.Interfaces;
using Microsoft.EntityFrameworkCore;
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
            var context = Context;
                message = context.ChatMessages.Where(cm => cm.Id == message.Id).FirstOrDefault();
            var userInDb = context.UserSettings.Where(us => us.Id == user.Id).FirstOrDefault();
            //userInDb.ReadChatMessages.Add(message);
            message.ReadByUsers.Add(userInDb);

            context.SaveChanges();
            OnMessageRead?.Invoke(user);
        }
    }
}
