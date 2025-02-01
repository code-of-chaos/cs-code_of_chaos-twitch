// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Ansi;
using CodeOfChaos.CliArgsParser;
using CodeOfChaos.Extensions;
using Microsoft.Playwright;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Tools.CodeOfChaos.Twitch.Commands.TwitchApiReferenceScraper.Dto;

namespace Tools.CodeOfChaos.Twitch.Commands.TwitchApiReferenceScraper;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[CliArgsCommand("twitchapi-reference-scraper")]
[CliArgsDescription("scrapes the twitch api reference page and creates a file with the api documentation")]
public partial class TwitchApiReferenceScraperCommand : ICommand<TwitchApiReferenceScraperParameters> {

    [GeneratedRegex(@"^(?<Rest>GET|POST|PUT|PATCH|DELETE|HEAD|OPTIONS|TRACE|CONNECT)?\s*(?<Url>\S+)$")]
    private static partial Regex UrlAndRestRegex { get; }

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public async Task ExecuteAsync(TwitchApiReferenceScraperParameters parameters) {
        using IPlaywright playwright = await Playwright.CreateAsync();
        await using IBrowser browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
        IPage page = await browser.NewPageAsync();
        IResponse? response = await page.GotoAsync(parameters.ApiReferencePage);
        if (response == null) throw new Exception("no response");
        if (response.Status != 200) throw new Exception("response status != 200");

        var reference = new TwitchApiReference();
        var docs = new List<TwitchApiDocumentation>();
        var builder = new AnsiStringBuilder();

        await foreach (TwitchApiQuickLookup lookup in GetApiLookupsAsync(page)) {
            
            TwitchApiDocumentation doc = await GetApiDocumentationAsync(page, lookup);

            Console.WriteLine(builder
                .F.AppendWhite("Parsed Api endpoint : ")
                .F.AppendPalegreen(doc.Name)
                .ToStringAndClear()
            );
            
            docs.Add(doc);
        }

        reference.Documentation = docs.ToArray();
        
        await WriteToXml(reference, parameters);
    }

    private static async IAsyncEnumerable<TwitchApiQuickLookup> GetApiLookupsAsync(IPage page) {
        // Luckily for me, there is one table which has a direct ID reference to all the various API endpoints.
        //      We'll use this to establish which API endpoints to save
        ILocator tableBodyLocator = page.Locator("#twitch-api-reference + table > tbody");
        IReadOnlyList<ILocator> rows = await tableBodyLocator.Locator("tr").AllAsync();
        int count = rows.Count;
        if (count == 0) throw new Exception("no rows found");

        foreach (ILocator row in rows) {
            // Each column should contain three values, one for the section, one for the href, and one for the description
            //      I've decided to ignore the description because this is already the same when we try and actually gather the individual data.

            IReadOnlyList<ILocator> columns = await row.Locator("td").AllAsync();

            string? resource = await TaskHelper.FromTaskOrDefault(columns.ElementAtOrDefault(0)?.TextContentAsync());
            string? hrefId = await TaskHelper.FromTaskOrDefault(columns.ElementAtOrDefault(1)?.Locator("a").GetAttributeAsync("href"));
            // string? description = await TaskHelper.FromTaskOrDefault(columns.ElementAtOrDefault(2)?.TextContentAsync());

            yield return new TwitchApiQuickLookup(
                resource,
                hrefId
                // description
            );
        }
    }

    private static async Task<TwitchApiDocumentation> GetApiDocumentationAsync(IPage page, TwitchApiQuickLookup lookup) {
        if (lookup.ElementId.IsNullOrWhiteSpace()) throw new Exception("no element id");

        ILocator locator = page.Locator("section.left-docs", new PageLocatorOptions { Has = page.Locator(lookup.ElementId) });
        // Scroll and wait if we can, because some data might take some time to load.
        await locator.ScrollIntoViewIfNeededAsync();
        await locator.WaitForAsync();

        IReadOnlyList<ILocator> locators = await locator.AllAsync();
        if (locators.ElementAtOrDefault(0) is not {} docSection) throw new Exception($"no doc section for element id {lookup.ElementId}");

        // Url & REST extractor 
        string? urlTextContent = await docSection.Locator("h3:has-text('URL') + p code").TextContentAsync();

        if (urlTextContent.IsNullOrWhiteSpace()) throw new Exception($"no url text content for element id {lookup.ElementId}");

        if (UrlAndRestRegex.Match(urlTextContent) is not { Success: true } match) throw new Exception($"no match on url text content {urlTextContent}");

        string restCommand = match.Groups["Rest"].Value;
        if (restCommand.IsNullOrWhiteSpace()) restCommand = "GET";
        
        string url = match.Groups["Url"].Value;

        // Required Auth Scopes
        IReadOnlyList<ILocator> strongHighlights = await docSection.Locator("h3:has-text('Authorization') + p strong").AllAsync();
        string[] requiredAuthScopes = (await Task.WhenAll(strongHighlights.Select(async x => await x.TextContentAsync())))!;

        IReadOnlyList<ILocator> accessTokenLocators = await docSection.Locator("h3:has-text('Authorization') + p a[href=\"/docs/authentication#user-access-tokens\"]").AllAsync();
        bool requiresOAuth = accessTokenLocators.Count > 0;

        // Request Query parameters
        ILocator parametersTable = docSection.Locator("h3:has-text('Request Query Parameters') + table > tbody");
        IReadOnlyList<ILocator> queryRows = await parametersTable.Locator("tr").AllAsync();
        RequestQueryParameter[] requestQueryParameters = await Task.WhenAll(queryRows.Select(async static row => {
            IReadOnlyList<ILocator> columns = await row.Locator("td").AllAsync();

            string? name = await TaskHelper.FromTaskOrDefault(columns.ElementAtOrDefault(0)?.TextContentAsync());
            string? type = await TaskHelper.FromTaskOrDefault(columns.ElementAtOrDefault(1)?.TextContentAsync());
            string? required = await TaskHelper.FromTaskOrDefault(columns.ElementAtOrDefault(2)?.TextContentAsync());
            string? description = await TaskHelper.FromTaskOrDefault(columns.ElementAtOrDefault(3)?.TextContentAsync());

            bool isRequired = bool.TryParse(required ?? string.Empty, out bool result) && result;

            return new RequestQueryParameter {
                Name = name ?? string.Empty,
                Type = type ?? string.Empty,
                Required = isRequired,
                Description = description ?? string.Empty
            };
        }));

        // Request Body
        ILocator bodyTable = docSection.Locator("h3:has-text('Request Body') + table > tbody");
        IReadOnlyList<ILocator> bodyRows = await bodyTable.Locator("tr").AllAsync();
        RequestBodyParameter[] requestBodyParameters = await Task.WhenAll(bodyRows.Select(async static row => {
            IReadOnlyList<ILocator> columns = await row.Locator("td").AllAsync();

            string? name = await TaskHelper.FromTaskOrDefault(columns.ElementAtOrDefault(0)?.TextContentAsync());
            string? type = await TaskHelper.FromTaskOrDefault(columns.ElementAtOrDefault(1)?.TextContentAsync());
            string? description = await TaskHelper.FromTaskOrDefault(columns.ElementAtOrDefault(2)?.TextContentAsync());

            return new RequestBodyParameter {
                Name =name ?? string.Empty,
                Type =type ?? string.Empty,
                Description =description ?? string.Empty
            };
        }));

        // Response Body
        ILocator responseTable = docSection.Locator("h3:has-text('Response Body') + table > tbody");
        IReadOnlyList<ILocator> responseRows = await responseTable.Locator("tr").AllAsync();

        ResponseBodyParameter[] responseParameters = await Task.WhenAll(responseRows.Select(async static row => {
            IReadOnlyList<ILocator> columns = await row.Locator("td").AllAsync();

            string? name = await TaskHelper.FromTaskOrDefault(columns.ElementAtOrDefault(0)?.TextContentAsync());
            string? type = await TaskHelper.FromTaskOrDefault(columns.ElementAtOrDefault(1)?.TextContentAsync());
            string? description = await TaskHelper.FromTaskOrDefault(columns.ElementAtOrDefault(2)?.TextContentAsync());

            return new ResponseBodyParameter {
                Name = (name ?? string.Empty).Replace("\u00A0", " "),
                Type = type ?? string.Empty,
                Description = description ?? string.Empty
            };
        }));

        // Description
        IReadOnlyList<ILocator> textLocators = await docSection.Locator("p").AllAsync();
        string description = await TaskHelper.FromTaskOrDefault(textLocators.ElementAtOrDefault(0)?.TextContentAsync()) ?? string.Empty;

        // API endpoint name
        ILocator nameLocator = docSection.Locator(lookup.ElementId);
        string name = await nameLocator.TextContentAsync() ?? string.Empty;

        // Assemble and return the full documentation
        return new TwitchApiDocumentation {
            Name =name,
            RestCommand =restCommand,
            Url =url,
            AuthScopes = requiredAuthScopes,
            RequiresOAuth =requiresOAuth,
            QueryParameters = requestQueryParameters,
            BodyParameters = requestBodyParameters,
            ResponseParameters =responseParameters,
            Description =description
        };
    }

    private async Task WriteToXml(TwitchApiReference reference, TwitchApiReferenceScraperParameters parameters) {
        await using var stream = new FileStream(parameters.OutputFile, FileMode.Create, FileAccess.Write);
        var serializer = new XmlSerializer(typeof(TwitchApiReference));
        serializer.Serialize(stream, reference);
    }
}
