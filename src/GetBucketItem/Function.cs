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

namespace GetBucketItem;

public class Function
{
    public static string ConvertStreamToBase64(Stream inputStream)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            inputStream.CopyTo(memoryStream);
            byte[] byteArray = memoryStream.ToArray();
            string base64String = Convert.ToBase64String(byteArray);
            return base64String;
        }
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
    {
        const string bucketName = "serverless-cms-bucket";
        string objectKey = apigProxyEvent.QueryStringParameters["objectKey"];

        const double timeoutDuration = 12;
        AWSConfigsS3.UseSignatureVersion4 = true;

        IAmazonS3 s3Client = new AmazonS3Client(RegionEndpoint.USEast1);

        var file = await s3Client.GetObjectAsync(bucketName, objectKey);
        await using var responseStream = file.ResponseStream;

        string str = "";
        
        try
        {
            str = ConvertStreamToBase64(responseStream);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            throw new Exception("failed to convert stream to base 64 string");
        }

        return new APIGatewayProxyResponse
        {
            Body = str,
            StatusCode = 200,
            IsBase64Encoded = true,
            Headers = new Dictionary<string, string> { 
                { "Content-Type", file.Headers.ContentType } 
            }
        };
    }
}