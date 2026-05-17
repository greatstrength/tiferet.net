using Tiferet.Events;
using Tiferet.Testing;

namespace Tiferet.Tests.Events;

// Parameter record for async test events.
public record FetchDataParams(string Key, int Multiplier);

// A test async event that simulates an async operation.
public class FetchDataEvent : AsyncDomainEvent<FetchDataParams, string>
{
    public override async Task<string> ExecuteAsync(FetchDataParams p)
    {
        await Task.Delay(1); // Simulate async work.
        return $"{p.Key}:{p.Multiplier * 2}";
    }
}

// A test async event that uses Verify for domain rule enforcement.
public class GuardedAsyncEvent : AsyncDomainEvent<FetchDataParams, int>
{
    public override async Task<int> ExecuteAsync(FetchDataParams p)
    {
        Verify(p.Multiplier > 0, "INVALID_MULTIPLIER", "Multiplier must be positive.");
        await Task.Delay(1);
        return p.Multiplier * 10;
    }
}

public class AsyncDomainEventTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsTypedResult()
    {
        var evt = new FetchDataEvent();
        var result = await evt.ExecuteAsync(new FetchDataParams("abc", 5));
        Assert.Equal("abc:10", result);
    }

    [Fact]
    public async Task ExecuteAsync_DictionaryOverload_ConstructsParams()
    {
        var evt = new FetchDataEvent();
        var data = new Dictionary<string, object?> { ["Key"] = "xyz", ["Multiplier"] = 3 };
        var result = await evt.ExecuteAsync(data);
        Assert.Equal("xyz:6", result);
    }

    [Fact]
    public void SyncAdapter_Execute_ReturnsResult()
    {
        // The sync adapter should work without deadlocking.
        var evt = new FetchDataEvent();
        var data = new Dictionary<string, object?> { ["Key"] = "sync", ["Multiplier"] = 4 };
        var result = evt.Execute(data);
        Assert.Equal("sync:8", result);
    }

    [Fact]
    public async Task ExecuteAsync_Verify_ThrowsOnFailure()
    {
        var evt = new GuardedAsyncEvent();
        var ex = await Assert.ThrowsAsync<TiferetException>(
            () => evt.ExecuteAsync(new FetchDataParams("test", 0)));
        Assert.Equal("INVALID_MULTIPLIER", ex.ErrorCode);
    }

    [Fact]
    public async Task ExecuteAsync_Verify_PassesOnSuccess()
    {
        var evt = new GuardedAsyncEvent();
        var result = await evt.ExecuteAsync(new FetchDataParams("test", 5));
        Assert.Equal(50, result);
    }

    [Fact]
    public void SyncAdapter_PropagatesException()
    {
        var evt = new GuardedAsyncEvent();
        var data = new Dictionary<string, object?> { ["Key"] = "err", ["Multiplier"] = 0 };
        var ex = Assert.Throws<TiferetException>(() => evt.Execute(data));
        Assert.Equal("INVALID_MULTIPLIER", ex.ErrorCode);
    }

    [Fact]
    public void AsyncDomainEvent_InheritsFromDomainEvent()
    {
        Assert.True(typeof(DomainEvent).IsAssignableFrom(typeof(AsyncDomainEvent)));
        Assert.True(typeof(DomainEvent).IsAssignableFrom(typeof(AsyncDomainEvent<FetchDataParams, string>)));
    }

    [Fact]
    public async Task DomainEventHarness_ExecuteAsync_Works()
    {
        var result = await DomainEventHarness.ExecuteAsync<FetchDataEvent, FetchDataParams, string>(
            new FetchDataParams("harness", 7));
        Assert.Equal("harness:14", result);
    }

    [Fact]
    public async Task DomainEventHarness_AssertThrowsAsync_Works()
    {
        var ex = await DomainEventHarness.AssertThrowsAsync<GuardedAsyncEvent, FetchDataParams, int>(
            new FetchDataParams("test", 0), "INVALID_MULTIPLIER");
        Assert.Equal("INVALID_MULTIPLIER", ex.ErrorCode);
    }
}
