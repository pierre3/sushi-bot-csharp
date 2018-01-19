using Line.Messaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SushiBotCSharp
{
    public class SushiBotTalkSkill : TalkSkill
    {
        public SushiBotTalkSkill()
        {
            Name = "出前注文";
            var requierdParams = new Dictionary<string, TalkParameter>()
            {
                ["menu"] = new TalkParameter(
                    messageToConfirm:
                        new TemplateMessage("「にぎり」と「ちらし」のどちらにしやすか？",
                            new ButtonsTemplate("「にぎり」と「ちらし」があるけど、どちらにしやすか？", null, "メニューを選択してください",
                                new[]
                                {
                                    new MessageTemplateAction("にぎり","にぎり"),
                                    new MessageTemplateAction("ちらし","ちらし")
                                })),
                    reaction: (replyMessages, status) =>
                     {
                         if (status.Parameters.TryGetValue("menu", out string menu) && !string.IsNullOrEmpty(menu))
                         {
                             replyMessages.Add(new TextMessage($"あいよ！「{menu}」ね！"));
                             return TalkReactionResult.Resolve;
                         }
                         else
                         {
                             replyMessages.Add(new TextMessage("ごめんよ！ウチは「にぎり」か「ちらし」しかやってねえんでさ！"));
                             return TalkReactionResult.Reject;
                         }
                     }
                ),
                ["grade"] = new TalkParameter(
                    messageToConfirm:
                        new TemplateMessage("「松」「竹」「梅」のどれにしやすか？",
                            new ButtonsTemplate("「松」「竹」「梅」があるけど、どれにしやすか？", null, "松・竹・梅から選んでください",
                                new[]
                                {
                                    new MessageTemplateAction("松","松"),
                                    new MessageTemplateAction("竹","竹"),
                                    new MessageTemplateAction("梅","梅")
                                })),
                    reaction: (replyMessages, status) =>
                     {
                         if (status.Parameters.TryGetValue("grade", out string grade) && !string.IsNullOrEmpty(grade))
                         {
                             replyMessages.Add(new TextMessage($"あいよ！「{grade}」ね！"));
                             return TalkReactionResult.Resolve;
                         }
                         else
                         {
                             replyMessages.Add(new TextMessage("「松」「竹」「梅」以外は扱ってないよ！"));
                             return TalkReactionResult.Reject;
                         }
                     }
                ),
                ["address"] = new TalkParameter(
                    messageToConfirm:
                        new TemplateMessage("お届け先を指定してください。",
                            new ButtonsTemplate("お届け先を指定してください。", null, "お届け先",
                            new[] { LineSchemeUrl.GetLocationUriTemplateAction("地図を開く") })),
                    reaction: (replyMessages, status) =>
                     {
                         if (status.Parameters.TryGetValue("address", out string address) && !string.IsNullOrEmpty(address))
                         {
                             replyMessages.Add(new TextMessage($"「{address}」だね！"));
                             return TalkReactionResult.Resolve;
                         }
                         else
                         {
                             replyMessages.Add(new TextMessage("地図から指定してくれよ！"));
                             return TalkReactionResult.Reject;
                         }
                     }
                )
            };

            var optionalParameters = new Dictionary<string, TalkParameter>()
            {
                ["option"] = new TalkParameter(messageToConfirm: null,
                    reaction: (replyMessage, status) =>
                    {
                        if (status.Parameters.TryGetValue("option", out string option) && !string.IsNullOrEmpty(option))
                        {
                            replyMessage.Add(new TextMessage($"「{option}」ね！了解！"));
                            return TalkReactionResult.Resolve;
                        }
                        return TalkReactionResult.Reject;
                    })

            };

            RequierdParameters = new ReadOnlyDictionary<string, TalkParameter>(requierdParams);
        }

        public override void Start(TalkStatus status)
        {
            if (!status.Parameters.TryGetValue("menu", out string menu)) { menu = ""; }
            if (!status.Parameters.TryGetValue("grade", out string grade)) { grade = ""; }
            if (!string.IsNullOrEmpty(menu) || !string.IsNullOrEmpty(grade))
            {
                ReplyMessages.Add(new TextMessage($"まいどあり！「{grade}{menu}」を出前ですね！"));
            }
            else
            {
                ReplyMessages.Add(new TextMessage($"まいどあり！出前のご注文ですね！"));
            }

        }

        public override void Finish(TalkStatus status)
        {
            if (!status.Parameters.TryGetValue("menu", out string menu)) { menu = ""; }
            if (!status.Parameters.TryGetValue("grade", out string grade)) { grade = ""; }
            if (!status.Parameters.TryGetValue("address", out string address)) { address = ""; }
            ReplyMessages.Add(new TextMessage($"承りやした！「{grade}{menu}」を「{address}」までお届けしやすね！まいどあり！"));
        }
    }
}