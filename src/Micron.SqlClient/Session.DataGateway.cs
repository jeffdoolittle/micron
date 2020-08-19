namespace Micron.SqlClient
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal partial class Session
    {
        public void Execute(ICommandRequest command)
        {
            throw new NotImplementedException();
        }

        public Task ExecuteAsync(ICommandRequest command, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public IReadResult Read(IReadRequest request, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public IReadMultipleResult ReadMultiple(IReadRequest request, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public TValue Scalar<TValue>(IScalarRequest<TValue> request)
        {
            throw new NotImplementedException();
        }

        public Task<TValue> ScalarAsync<TValue>(IScalarRequest<TValue> request, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}