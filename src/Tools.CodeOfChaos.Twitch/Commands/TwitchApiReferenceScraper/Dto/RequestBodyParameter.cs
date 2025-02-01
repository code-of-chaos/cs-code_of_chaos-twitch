// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Xml.Serialization;

namespace Tools.CodeOfChaos.Twitch.Commands.TwitchApiReferenceScraper.Dto;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class RequestBodyParameter() {
    [XmlAttribute] public string Name { get; init; }
    [XmlAttribute] public string Type { get; init; }
    [XmlText] public string Description { get; init; }
}
