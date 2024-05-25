using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Text;

using Amazon;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.S3;
using Amazon.S3.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DeleteBucketItem;

public class Function
{
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
    {
        const string bucketName = "serverless-cms-bucket";
        string objectKey = apigProxyEvent.QueryStringParameters["objectKey"];
        objectKey = Uri.UnescapeDataString(objectKey);
        Dictionary<string, string> body;
        int statusCode;
        IAmazonS3 s3Client = new AmazonS3Client(RegionEndpoint.USEast1);

        try
        {
            await DeleteObjectNonVersionedBucketAsync(s3Client, bucketName, objectKey);
            body = new Dictionary<string, string>
            {
                { "message", "delete success" },
            };
            statusCode = 200;
        }
        catch (Exception e)
        {
            body = new Dictionary<string, string>
            {
                { "message", "delete failed" },
            };
            statusCode = 500;
        }

        return new APIGatewayProxyResponse
        {
            Body = JsonSerializer.Serialize(body),
            StatusCode = statusCode,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    public static async Task DeleteObjectNonVersionedBucketAsync(IAmazonS3 client, string bucketName, string keyName)
    {
        try
        {
            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = keyName,
            };

            await client.DeleteObjectAsync(deleteObjectRequest);
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine($"Error encountered on server. Message:'{ex.Message}' when deleting an object.");
        }
    }
}