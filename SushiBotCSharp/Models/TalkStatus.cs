using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SushiBotCSharp
{
    public class TalkStatus : TableEntity
    {
        [IgnoreProperty]
        public string UserId { get => PartitionKey; set => PartitionKey = value; }
        [IgnoreProperty]
        public string Context { get => RowKey; set => RowKey = value; }
        public string WaitingParameter { get; set; }
        public string ParametersAsJson
        {
            get
            {
                if (Parameters == null) { return ""; }
                return JsonConvert.SerializeObject(Parameters);
            }
            set
            {
                Parameters = JsonConvert.DeserializeObject<IDictionary<string,string>>(value)??new Dictionary<string,string>();
            }
        }


        [IgnoreProperty]
        public IDictionary<string, string> Parameters { get; protected set; } = new Dictionary<string, string>();

        public TalkStatus()
        {
            
        }

        public TalkStatus(string userId, string context, IDictionary<string,string> entities)
        {
            UserId = userId;
            Context = context;
            Parameters = entities;
            WaitingParameter = "";
        }
    }

}