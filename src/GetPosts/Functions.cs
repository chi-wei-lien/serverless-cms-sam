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

namespace GetPosts;

public class Function
{
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
    {
        string groupId = apigProxyEvent.QueryStringParameters["group-id"];
        groupId = Uri.UnescapeDataString(groupId);
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
            KeyConditionExpression = "PK = :groupId",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
            {
                {":groupId", new AttributeValue{S = groupId}},
            }
        };


        var response = await client.QueryAsync(request);
        // var posts = [];
        List<Dictionary<string, AttributeValue>> posts = new List<Dictionary<string, AttributeValue>>();

        for (int i = 0; i < response.Items.Count; ++i) {
            // comparing sorting key with "schema". If not equal add to result
            if (String.Compare(response.Items[0]["SK"].S, "schema") != 0) {
                posts.Add(response.Items[i]);
            }
            // Console.WriteLine(response.Iems[i]);
        }
        Console.WriteLine(response.Count);

        return JsonConvert.SerializeObject(posts);
    }
}