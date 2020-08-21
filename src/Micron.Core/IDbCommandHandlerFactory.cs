namespace Micron.Core
{
    public interface IDbCommandHandlerFactory
    {
        IDbCommandHandler Build();
    }
}
