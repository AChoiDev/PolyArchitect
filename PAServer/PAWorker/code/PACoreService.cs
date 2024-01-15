namespace PolyArchitect.Worker {
    using PolyArchitect.Core;
    public class PACoreService : BackgroundService {
        public SceneRegistry Scenes {get; private set;} = new();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            while (stoppingToken.IsCancellationRequested == false) {
                await Task.Delay(TimeSpan.FromSeconds(1f), stoppingToken);
            }
        }

    }
}