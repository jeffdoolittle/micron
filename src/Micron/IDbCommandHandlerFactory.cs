namespace Micron
{
    public interface IDbCommandHandlerFactory
    {
        IDbCommandHandler Build();
    }
}
