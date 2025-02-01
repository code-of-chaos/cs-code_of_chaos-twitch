// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Xml.Serialization;

namespace Tools.CodeOfChaos.Twitch.Commands.TwitchApiReferenceScraper.Dto;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[XmlRoot]
public class TwitchApiReference() {
    [XmlArray] public TwitchApiDocumentation[] Documentation { get; set; } = [];
    [XmlElement] public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");
}
