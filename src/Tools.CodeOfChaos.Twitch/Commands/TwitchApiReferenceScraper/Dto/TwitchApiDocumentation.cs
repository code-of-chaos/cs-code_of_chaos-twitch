// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Xml.Serialization;

namespace Tools.CodeOfChaos.Twitch.Commands.TwitchApiReferenceScraper.Dto;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class TwitchApiDocumentation() {
    public string Name { get; init; }
    public string RestCommand { get; init; }
    public string Url { get; init; }
    public string[] AuthScopes { get; init; }
    public bool RequiresOAuth { get; init; }
    public RequestQueryParameter[] QueryParameters { get; init; }
    public RequestBodyParameter[] BodyParameters { get; init; }
    public ResponseBodyParameter[] ResponseParameters { get; init; }
    public string Description { get; init; }
}
