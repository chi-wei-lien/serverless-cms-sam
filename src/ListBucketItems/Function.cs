using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;

using Newtonsoft.Json;

using Amazon;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.S3;
using Amazon.S3.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ListBucketItems;

public class Function
{
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
    {
        const string bucketName = "serverless-cms-bucket";

        IAmazonS3 s3Client = new AmazonS3Client(RegionEndpoint.USEast1);

        var items = await ListingObjectsAsync(s3Client, bucketName);

        return new APIGatewayProxyResponse
        {
            Body = items,
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    public static async Task<string> ListingObjectsAsync(IAmazonS3 client, string bucketName)
    {
        var listObjectsV2Paginator = client.Paginators.ListObjectsV2(new ListObjectsV2Request
        {
            BucketName = bucketName,
        });

        List<Dictionary<string, string>> items = new List<Dictionary<string, string>>();

        await foreach (var response in listObjectsV2Paginator.Responses)
        {
            foreach (var entry in response.S3Objects)
            {
                var headRequest = new GetObjectMetadataRequest
                {
                    BucketName = bucketName,
                    Key = entry.Key
                };

                var headResponse = await client.GetObjectMetadataAsync(headRequest);

                items.Add(new Dictionary<string, string>
                {
                    { "key", entry.Key },
                    { "type", headResponse.Headers.ContentType },
                    { "size", entry.Size.ToString() },
                });
            }
        }

        return JsonConvert.SerializeObject(items);
    }
}