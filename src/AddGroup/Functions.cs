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

namespace AddGroup;

public class Function
{
    public static AmazonDynamoDBClient client = new AmazonDynamoDBClient();
    public static string tableName = "postList";

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
    {
        var requestBody = apigProxyEvent.Body;
        Dictionary<string, string> requestBodyDict;
        Dictionary<string, string> data = new Dictionary<string, string>();
        string groupId;
        // string fieldDictString;
        string groupName;
        string dataString;
        string createdOn;
        string fields;

        try
        {
            string requestBodyString = string.Join(Environment.NewLine, requestBody);
            Console.WriteLine("requestBodyString: " + requestBodyString);

            requestBodyDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(requestBody);
            
            groupId = requestBodyDict["groupId"];
            groupName = requestBodyDict["groupName"];
            createdOn = requestBodyDict["createdOn"];
            fields = requestBodyDict["fields"];

            data.Add("groupName", groupName);
            data.Add("createdOn", createdOn);
            data.Add("fields", fields);

            dataString = JsonConvert.SerializeObject(data);

            // TODO:check if schema passed in is valid
            // fieldDictString = requestBodyDict["fields"];
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

        await PutItemAsync(groupId, dataString);
    
        return new APIGatewayProxyResponse
        {
            Body = JsonConvert.SerializeObject(body),
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    public static async Task<bool> PutItemAsync(string groupId, string dataString)
    {
        PutItemResponse response;
        var item = new Dictionary<string, AttributeValue>
        {
            ["PK"] = new AttributeValue { S = groupId },
            ["SK"] = new AttributeValue { S = "schema" },
            ["data"] = new AttributeValue { S = dataString },
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