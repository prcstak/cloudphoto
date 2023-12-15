using cloudphoto;
using CommandLine;

var result = Parser.Default
    .ParseArguments<UploadOptions,
        DownloadOptions,
        ListOptions,
        DeleteOptions,
        MakeSiteOptions,
        InitOptions>(args);

var command = result.Value switch
{
    (InitOptions _) => ConfigManager.Init(),
    _ => MapResult(result),
};

try
{
    return await command;
}
catch (Exception e)
{
    Console.WriteLine("Unexpected error");
    return 1;
}

async Task<int> MapResult(ParserResult<object> parserResult)
{
    Config config;
    try
    {
        config = await ConfigManager.GetConfig();
    }
    catch (Exception e)
    {
        Console.WriteLine("Config was not found. Try to init program");
        return 1;
    }
    
    if (ConfigManager.ValidateConfig(config))
    {
        var album = new AlbumManager(config);
        return await parserResult.MapResult(
            (UploadOptions options) => album.Upload(options),
            (DownloadOptions options) => album.Download(options),
            (ListOptions options) => album.List(options),
            (DeleteOptions options) => album.Delete(options),
            (MakeSiteOptions options) => album.MakeSite(),
            _ => Task.FromResult(1));
    }
    Console.WriteLine("Config not valid. Try to init program");
    return 1;
}

//  dotnet publish -r linux-x64 -p:PublishSingleFile=true --self-contained true

