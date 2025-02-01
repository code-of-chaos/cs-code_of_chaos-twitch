// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Xml.Serialization;

namespace Tools.CodeOfChaos.Twitch.Commands.TwitchApiReferenceScraper.Dto;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class RequestQueryParameter {
    [XmlAttribute] public string Name { get; init; }
    [XmlAttribute] public string Type { get; init; }
    [XmlAttribute] public bool Required { get; init; }
    [XmlText] public string Description { get; init; }
}
