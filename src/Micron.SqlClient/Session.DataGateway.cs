namespace Micron.SqlClient
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal partial class Session 
    {
        public void Execute(CommandRequest command)
        {
            throw new NotImplementedException();
        }

        public Task ExecuteAsync(CommandRequest command, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public ReadRequest Read(ReadRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<IReadResult> ReadAsync(ReadRequest request, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public IReadMultipleResult ReadMultiple(ReadRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<IReadMultipleResult> ReadMultipleAsync(ReadRequest request, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public TValue Scalar<TValue>(ScalarRequest<TValue> request)
        {
            throw new NotImplementedException();
        }

        public Task<TValue> ScalarAsync<TValue>(ScalarRequest<TValue> request, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}