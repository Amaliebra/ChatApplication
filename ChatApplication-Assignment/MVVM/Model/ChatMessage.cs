using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.MVVM.Model
{
    public class ChatMessage 
    {
        public string Text { get; set; }

        [JsonProperty("sender")]
        public string Sender { get; set; }

        [JsonProperty("recipient")]
        public string Recipient { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
