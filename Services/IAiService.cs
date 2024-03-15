using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AiAPI.Services
{
    public interface IAiService
    {
        Task<OpenAIChatMessageContent?> PromptAsync(string prompt);
        IAsyncEnumerable<StreamingChatMessageContent> PromptStreamAsync(string prompt);
    }
}