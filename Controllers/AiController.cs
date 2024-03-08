using AiAPI.Services;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using System.Runtime.CompilerServices;
using System.Text;
using static AiAPI.Services.ChatHistoryService;

namespace AiAPI.Controllers
{
    public class OllamaGenerateRequest
    {
        public required string model { get; set; }
        public required string prompt { get; set; }
        public required bool stream { get; set; }

    }

    public class OllamaChatRequest
    {
        public required string model { get; set; }
        public required List<Message> messages { get; set; }
        public required bool stream { get; set; }

    }

    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AiController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        private IChatHistoryService ChatHistoryService;
        private string llmModel;
        private string llmHost;
        private RestClientOptions options;
        private HttpClient _client;

        public AiController(IConfiguration configuration, IChatHistoryService chatHistoryService)
        {
            Configuration = configuration;
            ChatHistoryService = chatHistoryService;

            llmModel = Configuration["llmModel"] ?? "gemma:2b";
            llmHost = Configuration["llmHost"] ?? "http://localhost:11434";
            System.Diagnostics.Debug.WriteLine(">>> " + llmModel);
            System.Diagnostics.Debug.WriteLine(">>> " + llmHost);

            options = new RestClientOptions(llmHost);
            options.MaxTimeout = 300000; // 5 mins

            _client = new HttpClient();
        }

        [HttpPost("postGenerate")]
        public async Task<string?> PostOllamaGenerateAsync(string prompt)
        {
            var client = new RestClient(options);

            var ollamaGenerateRequest = new OllamaGenerateRequest() { 
                model = llmModel,
                prompt = prompt,
                stream = false
            };
            var request = new RestRequest("api/generate").AddJsonBody(ollamaGenerateRequest);
            System.Diagnostics.Debug.WriteLine(">>> " + ollamaGenerateRequest.prompt);
            var response = await client.PostAsync(request);
            System.Diagnostics.Debug.WriteLine(response.StatusCode);
            System.Diagnostics.Debug.WriteLine(response.Content);
            return response.Content;
        }

        [HttpPost("postChat")]
        public async Task<ChatResponse?> PostOllamaChatAsync(string prompt, string? model)
        {
            var client = new RestClient(options);

            System.Diagnostics.Debug.WriteLine(">>> " + prompt);
            ChatHistoryService.AddMessage("user", prompt);
            var ollamaRequest = new OllamaChatRequest()
            {
                model = model ?? llmModel,
                messages = ChatHistoryService.GetHistory(),
                stream = false
            };
            var request = new RestRequest("api/chat").AddJsonBody(ollamaRequest);
            var response = await client.PostAsync(request);
            System.Diagnostics.Debug.WriteLine(response.StatusCode);
            System.Diagnostics.Debug.WriteLine(response.Content);

            // deserialize the response content into a ChatResponse object
            var chatResponse = JsonConvert.DeserializeObject<ChatResponse>(response.Content ?? "");
            if (chatResponse != null)
            {
                ChatHistoryService.AddMessage("assistant", chatResponse.message.content);
            }

            return chatResponse;
        }

        [HttpPost("postStreamChat")] // Task<IEnumerable<ChatResponse>>
        public async IAsyncEnumerable<ChatResponse?> PostOllamaStreamChatAsync(List<Message> history, string? model) //, CancellationToken cancellationToken = default)
        {
            System.Diagnostics.Debug.WriteLine(">>>> " + history[0].content);
            var chatRequest = new OllamaChatRequest()
            {
                messages = history,
                model = model ?? llmModel,
                stream = true
            };
            var request = new HttpRequestMessage(HttpMethod.Post, llmHost + "/api/chat")
            {
                Content = new StringContent(JsonConvert.SerializeObject(chatRequest), Encoding.UTF8, "application/json")
            };
            var completion = HttpCompletionOption.ResponseHeadersRead;

            //using 
            var response = await _client.SendAsync(request, completion); //, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(); // cancellationToken);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream) // && !cancellationToken.IsCancellationRequested)
            {
                string line = await reader.ReadLineAsync() ?? "";
                System.Diagnostics.Debug.WriteLine(line);

                var streamedResponse = JsonConvert.DeserializeObject<ChatResponse>(line);

                yield return streamedResponse;
            }
            //return await ProcessStreamedChatResponseAsync(response, cancellationToken);
        }

        private static async Task<IEnumerable<ChatResponse>> ProcessStreamedChatResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                string line = await reader.ReadLineAsync() ?? "";
                System.Diagnostics.Debug.WriteLine(line);

                var streamedResponse = JsonConvert.DeserializeObject<ChatResponse>(line);
                //responseContent = streamedResponse?.Message?.Content ?? "";

                if (streamedResponse?.done ?? false)
                {
                    List<ChatResponse> msgResponse = [streamedResponse];
                    //return streamedResponse;
                }
            }

            return Array.Empty<ChatResponse>(); //new ChatResponse();
        }
    }
}
