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

namespace AddGroupToPostList;

public class Function
{
    public static AmazonDynamoDBClient client = new AmazonDynamoDBClient();
    public static string tableName = "postList";

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
    {
        var requestBody = apigProxyEvent.Body;
        Dictionary<string, string> requestBodyDict;
        string groupId;
        string fieldDictString;
        string groupName;

        try
        {
            string requestBodyString = string.Join(Environment.NewLine, requestBody);
            Console.WriteLine("requestBodyString: " + requestBodyString);

            requestBodyDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(requestBody);
            
            groupId = requestBodyDict["groupId"];
            groupName = requestBodyDict["groupName"];

            // TODO:check if schema passed in is valid
            fieldDictString = requestBodyDict["fields"];
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            throw new Exception("request body parsing failed");
        }

        var body = new Dictionary<string, string>
        {
            { "message", "item putted in dynamodb" },
        };

        await PutItemAsync(groupId, groupName, fieldDictString);
    
        return new APIGatewayProxyResponse
        {
            Body = JsonConvert.SerializeObject(body),
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    public static async Task<bool> PutItemAsync(string groupId, string groupName, string fieldDictString)
    {
        PutItemResponse response;
        var item = new Dictionary<string, AttributeValue>
        {
            ["PK"] = new AttributeValue { S = groupId },
            ["SK"] = new AttributeValue { S = groupName },
            ["data"] = new AttributeValue { S = fieldDictString },
        };

        var request = new PutItemRequest
        {
            TableName = tableName,
            Item = item,
        };

        try
        {
            response = await client.PutItemAsync(request);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            throw new Exception("Item putting failed");
        }
        return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
    }
}