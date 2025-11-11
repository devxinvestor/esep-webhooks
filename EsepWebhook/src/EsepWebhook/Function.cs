using System.Text;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook;

public class Function
{
    public APIGatewayProxyResponse FunctionHandler(object input, ILambdaContext context)
    {
        context.Logger.LogInformation($"FunctionHandler received: {input}");
        dynamic json = JsonConvert.DeserializeObject<dynamic>(input.ToString());
        
        context.Logger.LogInformation($"Body: {json.body}");
        dynamic body = JsonConvert.DeserializeObject<dynamic>(json.body.ToString());
        context.Logger.LogInformation($"Issue: {body.issue}");
        context.Logger.LogInformation($"Html: {body.issue.html_url}");
        string payload = $"{{'text':'Issue Created: {body.issue.html_url}'}}";
        
        var client = new HttpClient();
        var webRequest = new HttpRequestMessage(HttpMethod.Post, Environment.GetEnvironmentVariable("SLACK_URL"))
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
    
        var response = client.Send(webRequest);
        using var reader = new StreamReader(response.Content.ReadAsStream());
        var slackResponse = reader.ReadToEnd();
        
        context.Logger.LogInformation($"Slack response: {slackResponse}");
        
        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonConvert.SerializeObject(new { message = "Success" })
        };
    }
}