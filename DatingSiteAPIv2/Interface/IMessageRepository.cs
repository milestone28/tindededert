using DatingSiteAPIv2.DTO;
using DatingSiteAPIv2.Helpers;
using DatingSiteAPIv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingSiteAPIv2.Interface
{
   public interface IMessageRepository
    {
        void AddMessage(Message message);
        void DeleteMessage(Message message);

        Task<Message> GetMessage(int id);
        Task<PagedList<MessageDto>> GetMessageForUser(MessageParams messageParams);
        Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserId, string recipientId);
        Task<bool> SaveAllAsync();
    }
}
