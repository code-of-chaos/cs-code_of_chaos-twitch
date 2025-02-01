// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.CliArgsParser;

namespace Tools.CodeOfChaos.Twitch.Commands.TwitchApiReferenceScraper;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public partial struct TwitchApiReferenceScraperParameters : IParameters {
    [CliArgsParameter("api-reference-page", "arp")] [CliArgsDescription("the url to the reference page of the twitch api")]
    public string ApiReferencePage { get; init; } = "https://dev.twitch.tv/docs/api/reference/";

    [CliArgsParameter("output", "o")] [CliArgsDescription("the output file to write the xml to")]
    public required string OutputFile { get; init; }

}
