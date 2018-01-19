using System.Collections.Generic;

namespace SushiBotCSharp
{
    public class NlpResult
    {
        public string Intent{get;set;}
        public IDictionary<string,string> Entities{get;set;}
    }
}