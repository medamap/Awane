namespace AwaneCore;

// ライフサイクルインターフェース
public interface IAsyncStartable
{
    Task StartAsync(CancellationToken cancellationToken);
}

public interface ITickable
{
    void Tick();
}

public interface IFixedTickable
{
    void FixedTick();
}