using System.Threading;
using System.Threading.Tasks;
using Fsp.Lobby;

namespace Fsp.Backend
{
    public interface IPlayerProfileStore
    {
        Task<PlayerProfile> LoadAsync(string playerId, CancellationToken cancellationToken = default);
        Task SaveAsync(PlayerProfile profile, CancellationToken cancellationToken = default);
    }
}
