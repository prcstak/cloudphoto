using CommandLine;

namespace cloudphoto;

[Verb("list", HelpText = "Displays a list of albums and photos in the cloud storage.")]
public class ListOptions : IBaseOptions
{
    [Option(shortName: 'a', longName: "album", Required = false, HelpText = "Album name")]
    public string? Album { get; set; }
}