using CommandLine;

namespace cloudphoto;

[Verb("upload", HelpText = "Sending photos to the cloud storage.")]
public class UploadOptions : IBaseOptions 
{
    [Option(shortName: 'a', longName: "album", Required = true, HelpText = "Album name")]
    public string? Album { get; set; }
    
    [Option(shortName: 'p', longName: "path",  Required = false, HelpText = "Path to directory where photos will be saved")]
    public string? Path { get; set; }
}