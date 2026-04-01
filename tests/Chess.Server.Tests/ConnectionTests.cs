using Chess.Server.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Http.Connections;
using Xunit;

namespace Chess.Server.Tests;

public sealed class ConnectionTests : IClassFixture<WebApplicationFactory<Program>>
{
    // Keep waits short so connection regressions fail quickly in CI.
    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(10);
    private readonly WebApplicationFactory<Program> _factory;

    public ConnectionTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SignalRClientLibraryRoute_IsServed()
    {
        // Sanity check that the server is up and static assets are being served.
        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/signalr/signalr.min.js");
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task JoinQueue_WithSingleClient_SendsQueuedEvent()
    {
        await using var connection = BuildConnection();
        // A single player should stay queued until an opponent appears.
        var queuedTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        connection.On("Queued", () => queuedTcs.TrySetResult());
        await connection.StartAsync();

        await connection.InvokeAsync("JoinQueue");
        await WaitFor(queuedTcs.Task, TestTimeout);
    }

    [Fact]
    public async Task JoinQueue_WithTwoClients_StartsGameForBoth()
    {
        await using var first = BuildConnection();
        await using var second = BuildConnection();

        // Each client gets start metadata and initial board state.
        var firstStartedTcs = new TaskCompletionSource<GameStartedDto>(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondStartedTcs = new TaskCompletionSource<GameStartedDto>(TaskCreationOptions.RunContinuationsAsynchronously);
        var firstStateTcs = new TaskCompletionSource<GameStateDto>(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondStateTcs = new TaskCompletionSource<GameStateDto>(TaskCreationOptions.RunContinuationsAsynchronously);

        first.On<GameStartedDto>("GameStarted", info => firstStartedTcs.TrySetResult(info));
        second.On<GameStartedDto>("GameStarted", info => secondStartedTcs.TrySetResult(info));
        first.On<GameStateDto>("MoveApplied", state => firstStateTcs.TrySetResult(state));
        second.On<GameStateDto>("MoveApplied", state => secondStateTcs.TrySetResult(state));

        await first.StartAsync();
        await second.StartAsync();

        await first.InvokeAsync("JoinQueue");
        await second.InvokeAsync("JoinQueue");

        var firstStarted = await WaitFor(firstStartedTcs.Task, TestTimeout);
        var secondStarted = await WaitFor(secondStartedTcs.Task, TestTimeout);
        var firstState = await WaitFor(firstStateTcs.Task, TestTimeout);
        var secondState = await WaitFor(secondStateTcs.Task, TestTimeout);

        Assert.Equal(firstStarted.GameId, secondStarted.GameId);
        Assert.Equal(firstStarted.GameId, firstState.GameId);
        Assert.Equal(secondStarted.GameId, secondState.GameId);
        Assert.NotEqual(firstStarted.YouAre, secondStarted.YouAre);
    }

    private HubConnection BuildConnection()
    {
        // Use LongPolling because TestServer does not provide real WebSocket transport.
        return new HubConnectionBuilder()
            .WithUrl(
                new Uri(_factory.Server.BaseAddress!, "/hubs/chess"),
                options =>
                {
                    options.Transports = HttpTransportType.LongPolling;
                    options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                })
            .Build();
    }

    private static async Task WaitFor(Task task, TimeSpan timeout)
    {
        await WaitFor(Wrap(task), timeout);
    }

    private static async Task<T> WaitFor<T>(Task<T> task, TimeSpan timeout)
    {
        // Wrap with a timeout to prevent hanging.
        using var cts = new CancellationTokenSource(timeout);
        var delayTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
        var completed = await Task.WhenAny(task, delayTask);
        if (completed != task)
        {
            throw new TimeoutException($"Timed out waiting after {timeout.TotalSeconds:0} seconds.");
        }

        cts.Cancel();
        return await task;
    }

    private static async Task<object?> Wrap(Task task)
    {
        await task;
        return null;
    }
}
