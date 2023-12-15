using CommandLine;

namespace cloudphoto;

[Verb("download", HelpText = "Download photos from the cloud storage.")]
public class DownloadOptions : IBaseOptions
{
    [Option(shortName: 'a', longName: "album", Required = true, HelpText = "Album name")]
    public string? Album { get; set; }
    
    [Option(shortName: 'p', longName: "path",  Required = false, HelpText = "Path to directory with photos")]
    public string? Path { get; set; }
}