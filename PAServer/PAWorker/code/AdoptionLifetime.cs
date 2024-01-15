namespace PolyArchitect.Worker {
    public class AdoptionLifetime : BackgroundService {
        private bool checkStatus;
        private Task? shutdownTask = null;
        public const float CHECK_PERIOD = 0.1f;
        public const float FIRST_CHECK_PERIOD = 3.0f;

        public void CheckUp() {
            checkStatus = true;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            checkStatus = false;
            await Task.Delay(TimeSpan.FromSeconds(FIRST_CHECK_PERIOD), stoppingToken);
            Console.WriteLine("First checkup obtained");

            while (checkStatus) {
                checkStatus = false;
                await Task.Delay(TimeSpan.FromSeconds(CHECK_PERIOD), stoppingToken);
            }
            Console.WriteLine($"Program was not checked up on by adopting connection in the last {CHECK_PERIOD} seconds.\nShutting down application...");
            shutdownTask = Program.StopAsync();
        }

        public override Task StopAsync(CancellationToken cancellationToken) {
            return base.StopAsync(cancellationToken);
        }
    }
}