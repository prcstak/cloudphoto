namespace cloudphoto;

public record Config(string? Bucket, 
    string? Endpoint, 
    string? Region,
    string? AccessKey,
    string? SecretKey);