namespace XRMFramework.Core
{
    public interface IEventHandler<TEventModel>
    {
        void Handle(TEventModel model);
    }

    public interface IEventHandler<TEventModel, TResponse>
    {
        TResponse Handle(TEventModel model);
    }
}