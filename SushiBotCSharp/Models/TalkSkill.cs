using Line.Messaging;
using System.Collections.Generic;

namespace SushiBotCSharp
{
    public abstract class TalkSkill
    {
        public string Name { get; set; }
        public IList<ISendMessage> ReplyMessages { get; }
        public IReadOnlyDictionary<string, TalkParameter> RequierdParameters { get; protected set; }
        
        public TalkSkill()
        {
            ReplyMessages = new List<ISendMessage>();
        }
        public abstract void Start(TalkStatus status);
        public abstract void Finish(TalkStatus status);
    }
}