using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

using Newtonsoft.Json;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace CreateTable;

public class Function
{
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
    {
        // var requestBody = apigProxyEvent.Body;
        // var bodyDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(requestBody);
        // var jsonTableSchema = bodyDict["jsonTableSchema"];

        var body = new Dictionary<string, string>
        {
            // { "jsonTableSchema", jsonTableSchema },
            { "jsonTableSchema", "hello" },
        };

        return new APIGatewayProxyResponse
        {
            Body = JsonConvert.SerializeObject(body),
            StatusCode = 200,
            Headers = new Dictionary<string, string> { 
                { "Content-Type", "application/json"},
                { "Access-Control-Allow-Headers", "Content-Type" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "GET" }

            }
        };
    }
}