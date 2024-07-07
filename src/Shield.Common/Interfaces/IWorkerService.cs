using Shield.Common.Domain;

namespace Shield.Common.Interfaces
{
    public interface IWorkerService
    {
        void Execute();
        Task ExecuteAsync(CancellationToken cancellationToken = default);

        void ResetStatus();
        void Update(ServiceStatus newStatus);
    }
}
