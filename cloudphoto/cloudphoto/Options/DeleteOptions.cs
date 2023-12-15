using CommandLine;

namespace cloudphoto;

[Verb("delete", HelpText = "Delete albums and photos in the cloud storage.")]
public class DeleteOptions : IBaseOptions
{
    [Option(shortName: 'a', longName: "album", Required = true, HelpText = "Album name")]
    public string? Album { get; set; }
    
    [Option(shortName: 'p', longName: "photo",  Required = false, HelpText = "Name of photo to delete")]
    public string? Photo { get; set; }
}