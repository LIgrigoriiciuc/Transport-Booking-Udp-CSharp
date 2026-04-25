using Shared.Proto;

namespace Shared;

public interface IObserver
{
    void OnPushReceived(PushPayload push);
    
}