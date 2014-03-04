using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuddySDK
{
   
    public class MessageCollection : BuddyCollectionBase<Message>
    {
        internal MessageCollection(BuddyClient client = null) : base(null, client) { }

        public Task<BuddyResult<Message>> SendMessageAsync(IEnumerable<string> recipients, string subject, string body, string thread = null)
        {

            return Task.Run<BuddyResult<Message>>(() =>
            {
                var c = new Message(null, this.Client)
                {
                    Recipients = recipients,
                    Subject = subject,
                    FromUserId = this.Client.User != null ? this.Client.User.ID : null,
                    Body = body,
                    ThreadId = thread
                };

                var t = c.SaveAsync();
                
                return t.Result.Convert(f => c);
            });
           
        }

        public Task<SearchResult<Message>> FindAsync(MessageType messageType, DateRange created = null, DateRange lastModified = null, BuddyGeoLocationRange locationRange = null, string pagingToken = null)
        {
            return base.FindAsync(
                userId: null,
                locationRange: locationRange,
                created: created,
                lastModified: lastModified,
                parameterCallback: (p) =>
                {
                    p["type"] = messageType;
                });
        }
    }
}
