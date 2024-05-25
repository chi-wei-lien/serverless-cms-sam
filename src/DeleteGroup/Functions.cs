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

namespace DeleteGroup;

public class Function
{
    public static AmazonDynamoDBClient client = new AmazonDynamoDBClient();
    public static string tableName = "postList";
    public static int statusCode = 200;
    public static Dictionary<string, string> body = new Dictionary<string, string>
    {
        { "message", "group deleted from dynamodb" },
    };


    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
    {

        string groupId = apigProxyEvent.QueryStringParameters["group-id"];
        groupId = Uri.UnescapeDataString(groupId);

        await DeleteItemAsync(groupId);        

        return new APIGatewayProxyResponse
        {
            Body = JsonConvert.SerializeObject(body),
            StatusCode = statusCode,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    public static async Task DeleteItemAsync(string groupId)
    {
        try {
            var key = new Dictionary<string, AttributeValue>
            {
                ["PK"] = new AttributeValue { S = groupId },
                ["SK"] = new AttributeValue { S = "schema" },
            };

            var request = new DeleteItemRequest
            {
                TableName = tableName,
                Key = key,
            };

            var response = await client.DeleteItemAsync(request);

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            body = new Dictionary<string, string>
            {
                { "message", "group deleted from dynamodb failed" },
            };
            statusCode = 500;
            throw new Exception("group deletion failed");
        }        
    }
}