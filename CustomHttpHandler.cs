﻿namespace AiAPI
{
    public class CustomHttpHandler : HttpClientHandler
    {
       /* protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri != null && request.RequestUri.Host.Equals("api.openai.com", StringComparison.OrdinalIgnoreCase))
            {
                request.RequestUri = new Uri($"http://localhost:1234/api/generate"); //chat");
            }

            return base.SendAsync(request, cancellationToken);
        }*/
    }

}
