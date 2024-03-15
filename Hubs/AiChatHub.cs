using AiAPI.Controllers;
using AiAPI.Services;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Threading;
using static AiAPI.Services.ChatHistoryService;

namespace SignalRWebpack.Hubs;

public class AiChatHub : Hub
{
    private readonly IConfiguration Configuration;
    private IChatHistoryService ChatHistoryService;
    private IAiService AiService;
    private AiController controller;

    public AiChatHub(
        IConfiguration configuration, 
        IChatHistoryService chatHistoryService,
        IAiService aiService
    )
    {
        Configuration = configuration;
        ChatHistoryService = chatHistoryService;
        AiService = aiService;

        controller = new AiController(Configuration, ChatHistoryService, AiService);
    }

    public async Task SendAiMessage(List<Message> message, string? model)
    {
        var aiMessage = controller.PostOllamaStreamChatAsync(message, model); //, default);

        if (aiMessage != null) {
            await foreach (ChatResponse? response in aiMessage)
            {
                System.Diagnostics.Debug.WriteLine(response);
                await Clients.All.SendAsync("ReceiveAiMessage", response);
            }
        }
    }
}