using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using SoftCircuits.IniFileParser;

namespace cloudphoto;

public static class ConfigManager
{
    private static readonly string? HomePath = Environment.GetEnvironmentVariable("HOME");

    private static readonly string ConfigDirPath =
        new StringBuilder().Append(HomePath).Append("/.config/cloudphoto").ToString();

    private static readonly string ConfigFilePath =
        new StringBuilder().Append(ConfigDirPath).Append("/cloudphotorc").ToString();

    public static async Task<int> Init()
    {
        var config = new IniFile();
        await CreateConfig(config);
        
        var bucketName = config.GetSetting("DEFAULT", "bucket");
        var endpoint = config.GetSetting("DEFAULT", "endpoint_url");
        var region = config.GetSetting("DEFAULT", "region");
        var accessKey = config.GetSetting("DEFAULT", "aws_access_key_id");
        var secretKey = config.GetSetting("DEFAULT", "aws_secret_access_key");
        
        var amazonS3Config = new AmazonS3Config
        {
            ServiceURL = endpoint,
            AuthenticationRegion = region
        };
        
        var s3Client = new AmazonS3Client(
            accessKey,
            secretKey,
            amazonS3Config
        );
        
        var response = await s3Client.ListBucketsAsync();
        
        var isBucketExist = response.Buckets.Any(bucket => bucket.BucketName == bucketName);
        
        if (!isBucketExist)
        {
            var request = new PutBucketRequest
            {
                BucketName = bucketName,
            };
            try
            {
                await s3Client.PutBucketAsync(request);
            }
            catch (BucketAlreadyExistsException e)
            {
                Console.WriteLine($"Bucket with name {bucketName} already exist");
            }
            Console.WriteLine($"Bucket {bucketName} was created!");
        }
        return 0;
    }

    public static async Task<Config> GetConfig()
    {
        var config = new IniFile();

        await config.LoadAsync(ConfigFilePath);
        
        var bucketName = config.GetSetting("DEFAULT", "bucket");
        var endpoint = config.GetSetting("DEFAULT", "endpoint_url");
        var region = config.GetSetting("DEFAULT", "region");
        var accessKey = config.GetSetting("DEFAULT", "aws_access_key_id");
        var secretKey = config.GetSetting("DEFAULT", "aws_secret_access_key");

        return new Config(bucketName, endpoint, region, accessKey, secretKey);
    }

    public static bool ValidateConfig(Config config)
    {
        if ((config.Bucket ?? config.Endpoint ?? config.Region ?? config.AccessKey ?? config.SecretKey) != null)
        {
            return true;
        }

        return false;
    }

    private static async Task CreateConfig(IniFile config)
    {
        Console.Write("Insert bucket name: ");
        var bucket = ReadConfigValue();

        Console.Write("Insert AWS access key: ");
        var access = ReadConfigValue();

        Console.Write("Insert AWS secret key: ");
        var secret = ReadConfigValue();

        config.SetSetting("DEFAULT", "bucket", bucket);
        config.SetSetting("DEFAULT", "aws_access_key_id", access);
        config.SetSetting("DEFAULT", "aws_secret_access_key", secret);
        config.SetSetting("DEFAULT", "region", "ru-central1");
        config.SetSetting("DEFAULT", "endpoint_url", "https://s3.yandexcloud.net");

        Directory.CreateDirectory(ConfigDirPath);

        await config.SaveAsync(ConfigFilePath);

        Console.WriteLine($"Config file was created in {ConfigFilePath}");
    }

    private static string ReadConfigValue()
    {
        string? value = null;
        while (string.IsNullOrEmpty(value))
        {
            value = Console.ReadLine();
        }

        return value;
    }
}