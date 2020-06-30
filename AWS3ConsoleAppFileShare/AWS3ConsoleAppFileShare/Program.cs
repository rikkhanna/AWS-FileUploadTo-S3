using System;

namespace AWS3ConsoleAppFileShare
{
    class Program
    {
        private static string bucketName = "checkbuck333";
        private static string awsRegion = "us-west-1";
        private const string filePath = "C:/Users/Rishabh/Desktop/complete/orders.csv";
        static void Main(string[] args)
        {
            Console.WriteLine("************************************************");
            FileUploadToS3.Run(awsRegion,
                                  bucketName,
                                  "key-name", filePath);

        }
    }
}
