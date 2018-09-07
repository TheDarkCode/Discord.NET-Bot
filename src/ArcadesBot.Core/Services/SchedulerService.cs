using System;
using System.Linq;
using System.Threading.Tasks;
using ArcadesBot.Handlers;
using ArcadesBot.Models.Chess;
using FluentScheduler;

namespace ArcadesBot.Services
{
    public class SchedulerService
    {
        public SchedulerService(DatabaseHandler database)
        {
            _database = database;
        }

        private DatabaseHandler _database { get; }

        public Task Initialize()
        {
            JobManager.Initialize();
            JobManager.AddJob(async () => await DeleteExpiredChallenges(), x => x.ToRunEvery(1).Minutes());
            return Task.CompletedTask;
        }

        private Task DeleteExpiredChallenges()
        {
            var challenges = _database.Query<ChessChallengeModel>()
                .Where(x => x.Accepted == false && x.TimeoutDate.AddSeconds(20) < DateTime.Now);

            foreach (var challenge in challenges)
                _database.Delete<ChessChallengeModel>(challenge.Id);

            return Task.CompletedTask;
        }
    }
}