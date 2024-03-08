using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Net.Http;

namespace AiAPI.Services
{
/*    public class AiService : IAiService
    {
        public Kernel kernel;
        public HttpClient httpClient;

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
        }

        public async Task<string?> PromptAsync(string prompt)
        {
            var response = await kernel.InvokePromptAsync(prompt);
            Console.WriteLine(response.GetValue<string>());
            return response.GetValue<string>();
        }

    }*/
}
