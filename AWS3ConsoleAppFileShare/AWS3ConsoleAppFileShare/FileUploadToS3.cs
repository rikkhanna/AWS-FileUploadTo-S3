using AWS3ConsoleAppFileShare.Signers;
using AWS3ConsoleAppFileShare.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;

namespace AWS3ConsoleAppFileShare
{
    class FileUploadToS3
    {
        static readonly string AWSAccessKey = ConfigurationManager.AppSettings["AWSAccessKey"];
        static readonly string AWSSecretKey = ConfigurationManager.AppSettings["AWSSecretKey"];



        private static string ObjectContent = "";


        /// <summary>
        /// Uploads content to an Amazon S3 object in a single call using Signature V4 authorization.
        /// </summary>
        public static void Run(string region, string bucketName, string objectKey, string filePath)
        {
            Console.WriteLine("PutS3ObjectSample");

            string keyname = $"{Path.GetFileNameWithoutExtension(filePath)}-{Guid.NewGuid()}{Path.GetExtension(filePath)}";

            objectKey = keyname;

            // Construct a virtual hosted style address with the bucket name part of the host address,
            // placing the region into the url if we're not using us-east-1.
            var regionUrlPart = string.Empty;
            if (!string.IsNullOrEmpty(region))
            {
                if (!region.Equals("us-east-1", StringComparison.OrdinalIgnoreCase))
                    regionUrlPart = string.Format("-{0}", region);
            }

            var endpointUri = string.Format("https://{0}.s3{1}.amazonaws.com/{2}",
                                               bucketName,
                                               regionUrlPart,
                                               objectKey);
            var uri = new Uri(endpointUri);

            using (var streamReader = new StreamReader(filePath, Encoding.UTF8))
            {
                ObjectContent = streamReader.ReadToEnd();
            }

            // precompute hash of the body content
            var contentHash = AWS4SignerBase.CanonicalRequestHashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(ObjectContent));
            var contentHashString = AWS4SignerBase.ToHexString(contentHash, true);

            var headers = new Dictionary<string, string>
            {
                {AWS4SignerBase.X_Amz_Content_SHA256, contentHashString},
                {"content-length",Encoding.UTF8.GetByteCount(ObjectContent).ToString()},
                {"content-type", "application/json"}

            };

            var signer = new AWS4SignerForAuthorizationHeader
            {
                EndpointUri = uri,
                HttpMethod = "PUT",
                Service = "s3",
                Region = "us-west-1"
            };

            var authorization = signer.ComputeSignature(headers,
                                                        "",   // no query parameters
                                                        contentHashString,
                                                        AWSAccessKey,
                                                        AWSSecretKey);

            // express authorization for this as a header
            headers.Add("Authorization", authorization);
            //headers.Add("SendChunked", "true");



            // make the call to Amazon S3
            HttpHelpers.InvokeHttpRequest(uri, "PUT", headers, ObjectContent);
        }
    }
}
