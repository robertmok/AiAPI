using Azure;
using Azure.AI.OpenAI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Net.Http;
using System.Text;
using System.Threading;
using static AiAPI.Services.ChatHistoryService;

namespace AiAPI.Services
{
    public class AiService : IAiService
    {
        public Kernel kernel;
        public HttpClient httpClient;
        public IChatCompletionService chat;
        public ChatHistory chatHistory;

        public AiService()
        {
            httpClient = new HttpClient(new CustomHttpHandler());

            kernel = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion(
                    "fake-model-name",
                    "fake-api-key",
                    httpClient: httpClient
                 )
                .Build();

            chat = kernel.GetRequiredService<IChatCompletionService>();
            chatHistory = new ChatHistory();
        }

        public async Task<OpenAIChatMessageContent?> PromptAsync(string prompt)
        {
            chatHistory.AddUserMessage(prompt);
            foreach (var msg in chatHistory)
            {
                System.Diagnostics.Debug.WriteLine(msg.Role + ": " + msg.Content);
            }
            var setting = new OpenAIPromptExecutionSettings
            {
                Temperature = 0.7,
                MaxTokens = 1000,
            };
            var response = (OpenAIChatMessageContent)await chat.GetChatMessageContentAsync(
                chatHistory,
                setting,
                kernel
            );
            //chatHistory.AddMessage(response.Role, response.Content ?? string.Empty);
            chatHistory.AddAssistantMessage(response.Content ?? string.Empty);
            System.Diagnostics.Debug.WriteLine(response);
            return response;
        }

        public async IAsyncEnumerable<StreamingChatMessageContent> PromptStreamAsync(string prompt)
        {
            chatHistory.AddUserMessage(prompt);
            foreach (var msg in chatHistory)
            {
                System.Diagnostics.Debug.WriteLine(msg.Role + ": " + msg.Content);
            }
            var setting = new OpenAIPromptExecutionSettings
            {
                Temperature = 0.7,
                MaxTokens = 1000,
            };

            var sb = new StringBuilder();
            await foreach (var update in chat.GetStreamingChatMessageContentsAsync(
                chatHistory,
                setting,
                kernel)
            )
            {
                if (update.Content is not null)
                {
                    Console.Write(update.Content);
                    sb.Append(update.Content);
                    yield return update;
                }
                //sb.Append(update.Content);
            }
            chatHistory.AddAssistantMessage(sb.ToString());
        }
    }
}
