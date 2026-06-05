using KW_Calendar.Native;

namespace KW_Calendar.Tests.Native;

public class SingleInstanceTests
{
    private static string FreshScope() => Guid.NewGuid().ToString("N");

    [Fact]
    public void Acquire_FirstCall_IsFirstInstance()
    {
        var scope = FreshScope();

        using var first = SingleInstance.Acquire(scope);

        Assert.True(first.IsFirstInstance);
    }

    [Fact]
    public void Acquire_WhileHeld_SecondIsNotFirstInstance()
    {
        var scope = FreshScope();

        using var first = SingleInstance.Acquire(scope);
        using var second = SingleInstance.Acquire(scope);

        Assert.True(first.IsFirstInstance);
        Assert.False(second.IsFirstInstance);
    }

    [Fact]
    public void Acquire_AfterDispose_IsFirstInstanceAgain()
    {
        var scope = FreshScope();

        var first = SingleInstance.Acquire(scope);
        first.Dispose();

        using var revived = SingleInstance.Acquire(scope);

        Assert.True(revived.IsFirstInstance);
    }

    [Fact]
    public void StartListening_OnSecondInstance_Throws()
    {
        var scope = FreshScope();

        using var first = SingleInstance.Acquire(scope);
        using var second = SingleInstance.Acquire(scope);

        Assert.Throws<InvalidOperationException>(() => second.StartListening());
    }

    [Fact]
    public async Task NotifyExisting_RaisesOpenRequestedOnListener()
    {
        var scope = FreshScope();
        using var first = SingleInstance.Acquire(scope);

        var signal = new TaskCompletionSource();
        first.OpenRequested += (_, _) => signal.TrySetResult();
        first.StartListening();

        // listener 루프가 파이프 서버를 띄울 시간을 잠깐 줌
        await Task.Delay(50);

        SingleInstance.NotifyExisting(scope);

        var completed = await Task.WhenAny(signal.Task, Task.Delay(2000));
        Assert.Same(signal.Task, completed);
    }

    [Fact]
    public void NotifyExisting_WithoutListener_DoesNotThrow()
    {
        var scope = FreshScope();

        // listener 없음. 예외 없이 빠르게 리턴해야 함.
        SingleInstance.NotifyExisting(scope);
    }
}
