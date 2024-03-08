
namespace AiAPI.Services
{
    public interface IAiService
    {
        Task<string?> PromptAsync(string prompt);
    }
}