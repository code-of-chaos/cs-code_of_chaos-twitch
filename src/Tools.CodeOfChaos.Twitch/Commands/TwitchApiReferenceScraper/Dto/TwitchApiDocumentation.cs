// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace Tools.CodeOfChaos.Twitch.Commands.TwitchApiReferenceScraper.Dto;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class TwitchApiDocumentation {
    public string Name { get; init; } = string.Empty;
    public string RestCommand { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string[] AuthScopes { get; init; } = [];
    public bool RequiresOAuth { get; init; }
    public RequestQueryParameter[] QueryParameters { get; init; } = [];
    public RequestBodyParameter[] BodyParameters { get; init; } = [];
    public ResponseBodyParameter[] ResponseParameters { get; init; } = [];
    public string Description { get; init; } = string.Empty;
}
