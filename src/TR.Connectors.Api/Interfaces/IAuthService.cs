using System.Threading;
using System.Threading.Tasks;

namespace TR.Connectors.Api.Interfaces
{
    public interface IAuthService
    {
        Task StartUpAsync(string connectionString, CancellationToken ct = default);
    }
}
