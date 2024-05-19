using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

using Newtonsoft.Json;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GetPost;

public class Function
{
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
    {
        string groupId = apigProxyEvent.QueryStringParameters["group-id"];
        string postId = apigProxyEvent.QueryStringParameters["post-id"];
        groupId = Uri.UnescapeDataString(groupId);
        var body = await QueryListAsync(groupId, postId);

        return new APIGatewayProxyResponse
        {
            Body = body,
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    private async Task<string> QueryListAsync(string groupId, string postId)
    {
        using var client = new AmazonDynamoDBClient(Amazon.RegionEndpoint.USEast1);

        var request = new QueryRequest()
        {
            TableName = "postList",
            KeyConditionExpression = "PK = :groupId and SK = :postId",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
            {
                {":groupId", new AttributeValue{S = groupId}},
                {":postId", new AttributeValue{S = postId}}
            }
        };


        var response = await client.QueryAsync(request);
        Console.WriteLine(response.Count);

        return JsonConvert.SerializeObject(response.Items);
    }
}