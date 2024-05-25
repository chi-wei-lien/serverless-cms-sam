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

namespace GetGroups;

public class Function
{
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
    {
        // context.Logger.LogLine("Get Request\n");
        var body = await QueryListAsync();

        return new APIGatewayProxyResponse
        {
            Body = body,
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    private async Task<string> QueryListAsync()
    {
        using var client = new AmazonDynamoDBClient(Amazon.RegionEndpoint.USEast1);

        // var request = new QueryRequest
        // {
        //     TableName = "Reply",
        //     KeyConditionExpression = "SK = :v_Id",
        //     ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
        //         {":v_Id", new AttributeValue { S =  "schema" }}}
        // };

        var request = new QueryRequest()
        {
            TableName = "postList",
            IndexName = "myGSI",
            KeyConditionExpression = "SK = :schema",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
            {
                {":schema", new AttributeValue("schema")}
            }
        };


        var response = await client.QueryAsync(request);

        Console.WriteLine(  )

        return JsonConvert.SerializeObject(response.Items);
    }


    
}