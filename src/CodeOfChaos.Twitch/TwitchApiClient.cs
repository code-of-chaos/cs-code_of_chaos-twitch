// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using AterraEngine.Unions;
using RestSharp;
using System.Net;

namespace CodeOfChaos.Twitch;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class TwitchApiClient(string clientId, string clientSecret) {
    private readonly RestClient _client = new(new RestClientOptions("https://api.twitch.tv/helix/"));

    public async ValueTask<StartCommercialResult> StartCommercialAsync(string broadcasterId, int length, CancellationToken ct = default) {
        RestRequest request = new RestRequest("channels/commercial")
            .AddHeader("Client-ID", clientId)
            .AddHeader("Authorization", $"Bearer {clientSecret}");

        request.AddJsonBody(new Dictionary<string, string> {
            ["broadcaster_id"] = broadcasterId,
            ["length"] = length.ToString()
        });

        RestResponse<StartCommercialResponse> response = await _client.ExecuteAsync<StartCommercialResponse>(request, ct);
        return response switch {
            { IsSuccessful: true, Data: not null } => new Ok<StartCommercialResponse>(response.Data),
            { StatusCode : HttpStatusCode.Unauthorized } => new Unauthorized(),
            { StatusCode : HttpStatusCode.NotFound } => new NotFound(),
            { StatusCode : HttpStatusCode.TooManyRequests } => new TooManyRequests(),

            // Undocumented response
            _ => throw new NotImplementedException(response.Content)
        };
    }
}

public record StartCommercialResponse(int Length, string Message, int RetryAfter);

public record struct Ok<T>(T Value) : IValue<T>;

public record struct BadRequest;

public record struct Unauthorized;

public record struct Forbidden;

public record struct NotFound;

public record struct InternalServerError;

public record struct ServiceUnavailable;

public record struct GatewayTimeout;

public record struct TooManyRequests;

public readonly partial record struct StartCommercialResult() : IUnion<Ok<StartCommercialResponse>, BadRequest, Unauthorized, NotFound, TooManyRequests> {}
