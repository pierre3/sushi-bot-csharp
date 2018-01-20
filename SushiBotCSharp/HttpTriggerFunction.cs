using Line.Messaging;
using Line.Messaging.Webhooks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Cognitive.LUIS;

namespace SushiBotCSharp
{
    public static class HttpTriggerFunction
    {
        static LineMessagingClient lineMessagingClient;

        static HttpTriggerFunction()
        {
            lineMessagingClient = new LineMessagingClient(System.Configuration.ConfigurationManager.AppSettings["ChannelAccessToken"]);
            var sp = ServicePointManager.FindServicePoint(new Uri("https://api.line.me"));
            sp.ConnectionLeaseTimeout = 60 * 1000;
        }

        [FunctionName("LINEBOT")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                log.Info("C# HTTP trigger function processed a request.");
                var channelSecret = System.Configuration.ConfigurationManager.AppSettings["ChannelSecret"];
                var events = await req.GetWebhookEventsAsync(channelSecret);

                var connectionString = System.Configuration.ConfigurationManager.AppSettings["AzureWebJobsStorage"];
                var luisAppId = System.Configuration.ConfigurationManager.AppSettings["LuisAppId"];
                var luisSubscriptionKey = System.Configuration.ConfigurationManager.AppSettings["LuisSubscriptionKey"];

                var talkContext = await TaklContext.CreateAsync(connectionString);
                talkContext.RegisterTalkSkill(new SushiDeliveryTalkSkill());

                using (var luis = new LuisClient(luisAppId, luisSubscriptionKey))
                {
                    // Create the LineBotApp and run it.
                    var app = new LineBotApp(lineMessagingClient, talkContext, luis, log);
                    await app.RunAsync(events);
                }
            }
            catch (InvalidSignatureException e)
            {
                return req.CreateResponse(HttpStatusCode.Forbidden,e.Message);
            }
            catch (LineResponseException e)
            {
                log.Error(e.ToString());
                var debugUserId = System.Configuration.ConfigurationManager.AppSettings["DebugUser"];
                if (debugUserId != null)
                {
                    await lineMessagingClient.PushMessageAsync(debugUserId, $"{e.StatusCode}({(int)e.StatusCode}), {e.ResponseMessage?.ToString()??""}");
                }
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                var debugUserId = System.Configuration.ConfigurationManager.AppSettings["DebugUser"];
                if (debugUserId != null)
                {
                    await lineMessagingClient.PushMessageAsync(debugUserId, e.Message);
                }
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }

}
