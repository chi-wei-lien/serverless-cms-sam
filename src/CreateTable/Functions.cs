using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

using Newtonsoft.Json;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace CreateTable;

public class Function
{
    public static AmazonDynamoDBClient client = new AmazonDynamoDBClient();
    public static string tableName = "ExampleTable";
    
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
    {
        var requestBody = apigProxyEvent.Body;
        Dictionary<string, string> fieldDict;
        string[] fields;

        try
        {
            fieldDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(requestBody);
            // fieldDictAsString = string.Join(Environment.NewLine, fieldDict);
            fields = fieldDict.Keys.ToArray();
            // fieldsString = String.Join(",", fields);
            // var jsonTableSchema = bodyDict["content"];
            // Console.WriteLine("request body: " + fieldsString + "\n");
        }
        catch (Exception e)
        {
            throw new Exception("JSON parsing failed");
        }

        try
        {
            await CreateMovieTableAsync(fieldDict);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            throw new Exception("Table creation failed");
        }

        var body = new Dictionary<string, string>
        {
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
    public static async Task<bool> CreateMovieTableAsync(Dictionary<string, string> fieldDict)
    {
        List<AttributeDefinition> attributeDefinitions = new List<AttributeDefinition>();
        attributeDefinitions.Add(new AttributeDefinition
        {
            AttributeName = "id",
            AttributeType = "N"
        });

        foreach (KeyValuePair<string, string> field in fieldDict)
        {
            attributeDefinitions.Add(new AttributeDefinition
            {
                AttributeName = field.Key,
                AttributeType = field.Value
            });
        }

        var response = await client.CreateTableAsync(new CreateTableRequest
        {
            TableName = tableName,
            AttributeDefinitions = attributeDefinitions,
            KeySchema = new List<KeySchemaElement>()
            {
                new KeySchemaElement
                {
                    AttributeName = "id",
                    KeyType = KeyType.HASH,
                },
            },
            ProvisionedThroughput = new ProvisionedThroughput
            {
                ReadCapacityUnits = 5,
                WriteCapacityUnits = 5,
            },
        });

        // Wait until the table is ACTIVE and then report success.
        Console.Write("Waiting for table to become active...");

        var request = new DescribeTableRequest
        {
            TableName = response.TableDescription.TableName,
        };

        TableStatus status;

        int sleepDuration = 2000;

        do
        {
            System.Threading.Thread.Sleep(sleepDuration);

            var describeTableResponse = await client.DescribeTableAsync(request);
            status = describeTableResponse.Table.TableStatus;

            Console.Write(".");
        }
        while (status != "ACTIVE");

        return status == TableStatus.ACTIVE;
    }
}