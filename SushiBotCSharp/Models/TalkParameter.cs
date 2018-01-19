using Line.Messaging;
using System;
using System.Collections.Generic;

namespace SushiBotCSharp
{
    public class TalkParameter
    {
        protected ISendMessage messageToConfirm;
        protected Func<IList<ISendMessage>, TalkStatus, TalkReactionResult> reaction;
        
        public TalkParameter(ISendMessage messageToConfirm, Func<IList<ISendMessage>, TalkStatus, TalkReactionResult> reaction)
        {
            this.messageToConfirm = messageToConfirm;
            this.reaction = reaction;
        }

        public TalkReactionResult Reaction(IList<ISendMessage> replyMessages, TalkStatus status)
            => (reaction?.Invoke(replyMessages, status) ?? TalkReactionResult.Resolve);


        public void Confirm(IList<ISendMessage> replyMessages)
            => replyMessages.Add(messageToConfirm);
    }
}