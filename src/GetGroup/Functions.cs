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

namespace GetGroup;

public class Function
{
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
    {
        string groupId = apigProxyEvent.QueryStringParameters["group-id"];
        groupId = Uri.UnescapeDataString(groupId);
        // Console.WriteLine("groupId: " + groupId);
        // context.Logger.LogLine("Get Request\n");
        var body = await QueryListAsync(groupId);

        return new APIGatewayProxyResponse
        {
            Body = body,
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    private async Task<string> QueryListAsync(string groupId)
    {
        using var client = new AmazonDynamoDBClient(Amazon.RegionEndpoint.USEast1);

        var request = new QueryRequest()
        {
            TableName = "postList",
            KeyConditionExpression = "PK = :groupId and SK = :schema",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
            {
                {":groupId", new AttributeValue{S = groupId}},
                {":schema", new AttributeValue{S = "schema"}}
            }
        };


        var response = await client.QueryAsync(request);
        Console.WriteLine(response.Count);

        return JsonConvert.SerializeObject(response.Items);
    }
}