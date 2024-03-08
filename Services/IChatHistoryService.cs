
namespace AiAPI.Services
{
    public interface IChatHistoryService
    {
        void AddMessage(string role, string content);
        List<ChatHistoryService.Message> GetHistory();
    }
}