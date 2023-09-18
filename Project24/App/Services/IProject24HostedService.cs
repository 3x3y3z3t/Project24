/*  App/Services/IProject24HostedService.cs
 *  Version: v1.0 (2023.09.07)
 *  
 *  Author
 *      Arime-chan
 */

using System.Threading;
using System.Threading.Tasks;

namespace Project24.App.Services
{
    public interface IProject24HostedService
    {
        public Task StartAsync(CancellationToken _cancellationToken = default);

    }

}
