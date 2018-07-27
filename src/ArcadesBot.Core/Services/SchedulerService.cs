using FluentScheduler;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadesBot
{
    public class SchedulerService
    {
        private DatabaseHandler Database { get; }

        public SchedulerService(DatabaseHandler database) 
            => Database = database;

        public Task Initialize()
        {
            JobManager.Initialize();
            JobManager.AddJob(async () => await DeleteExpiredChallenges(), x => x.ToRunEvery(1).Minutes());
            return Task.CompletedTask;
        }
        private Task DeleteExpiredChallenges()
        {
            var challenges = Database.Query<ChessChallengeModel>().Where(x => x.Accepted == false && x.TimeoutDate.AddSeconds(20) < DateTime.Now);

            foreach(var challenge in challenges)
                Database.Delete<ChessChallengeModel>(challenge.Id);

            return Task.CompletedTask;
        }
    }
}
