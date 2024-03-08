using Azure.AI.OpenAI;
using System.Text.Json.Serialization;

namespace AiAPI.Services
{
    public class ChatHistoryService : IChatHistoryService
    {
        public class Message
        {
            public required string role { get; set; } //system, user or assistant
            public required string content { get; set; }

        }

        public class ChatResponse
        {
            public bool done { get; set; }
            public string model { get; set; }
            public Message message { get; set; }
            public int prompt_eval_count { get; set; }
            public int eval_count { get; set; }
        }

        private List<Message> history = new List<Message>();

        public void AddMessage(string role, string content)
        {
            history.Add(new Message { role = role, content = content });
            System.Diagnostics.Debug.WriteLine("### History ------------");
            foreach (Message msg in history)
            {
                System.Diagnostics.Debug.WriteLine(msg.role + ": " + msg.content);
            }
            System.Diagnostics.Debug.WriteLine("### ------------------");
        }

        public List<Message> GetHistory()
        {
            return history;
        }
    }
}
