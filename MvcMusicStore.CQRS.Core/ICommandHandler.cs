namespace MvcMusicStore.CQRS.Core
{
    public interface ICommandHandler<T> where T : ICommand
    {
        void Execute(T command);
    }
}