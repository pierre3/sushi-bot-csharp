using Line.Messaging;
using Line.Messaging.Webhooks;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Cognitive.LUIS;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SushiBotCSharp
{

    /// <summary>
    /// Handles LINE requests. WebhookApplication inheritance prvides the handling points for you.
    /// Implement each method to handle requests.
    /// </summary>
    public class LineBotApp : WebhookApplication
    {
        private LineMessagingClient messagingClient;
        private TaklContext taklContext;
        private LuisClient luis;
        private TraceWriter log;

        public LineBotApp(LineMessagingClient messagingClient, TaklContext talkContext, LuisClient luis, TraceWriter log)
        {
            this.messagingClient = messagingClient;
            this.taklContext = talkContext;
            this.log = log;
            this.luis = luis;
        }
 
        protected override async Task OnMessageAsync(MessageEvent ev)
        {
            log.WriteInfo($"SourceType:{ev.Source.Type}, SourceId:{ev.Source.Id}, MessageType:{ev.Message.Type}");

            switch (ev.Message.Type)
            {
                case EventMessageType.Text:
                    await HandleTextAsync(ev.ReplyToken, ((TextEventMessage)ev.Message).Text, ev.Source.UserId);
                    break;
                case EventMessageType.Location:
                    var location = ((LocationEventMessage)ev.Message);
                    await HandleLocationAsync(ev.ReplyToken, location, ev.Source.Id);
                    break;
            }
        }

        protected override async Task OnFollowAsync(FollowEvent ev)
        {
            log.WriteInfo($"SourceType:{ev.Source.Type}, SourceId:{ev.Source.Id}");

            var userName = "";
            if (!string.IsNullOrEmpty(ev.Source.Id))
            {
                var userProfile = await messagingClient.GetUserProfileAsync(ev.Source.Id);
                userName = userProfile?.DisplayName ?? "";
            }

            await messagingClient.ReplyMessageAsync(ev.ReplyToken, $"Hello {userName}! Thank you for following !");
        }

        protected override async Task OnJoinAsync(JoinEvent ev)
        {
            log.WriteInfo($"SourceType:{ev.Source.Type}, SourceId:{ev.Source.Id}");

            await messagingClient.ReplyMessageAsync(ev.ReplyToken,
             $"Thank you for letting me join your {ev.Source.Type.ToString().ToLower()}!");
        }
 
        /// <summary>
        /// Handle text and returns sample messages depennds on what user typed.
        /// </summary>
        private async Task HandleTextAsync(string replyToken, string userMessage, string userId)
        {
            var luisResult = await luis.Predict(userMessage);
            var nlpResult = new NlpResult()
            {
                Intent = luisResult.TopScoringIntent.Name,
                Entities = luisResult.GetAllEntities().ToDictionary(x => x.Name, x => x.Value)
            };

            if (false == await taklContext.ProcessAsync(messagingClient, nlpResult, userId, replyToken))
            {
                await messagingClient.ReplyMessageAsync(replyToken, "へい！いらっしゃい！寿司出前BOTだよ！");
            }
        }

        /// <summary>
        /// Reply the location user send.
        /// </summary>
        private async Task HandleLocationAsync(string replyToken, LocationEventMessage location, string userId)
        {
            var nlpResult = new NlpResult()
            {
                Intent = "出前注文",
                Entities = new Dictionary<string,string>() {
                    ["address"] = location.Address
                }
            };
            await taklContext.ProcessAsync(messagingClient, nlpResult, userId, replyToken);
        }

    }
}
