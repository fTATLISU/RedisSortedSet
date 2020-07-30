using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace RedisSortedSet
{
    class Program
    {
        private const string CACHE_KEY = "key";

        static async Task Main(string[] args)
        {
            IDatabase redis = await GetRedis();

            //Create data that will store in Redis
            var pastDateTimeTicks = DateTime.UtcNow.AddYears(-10).Ticks;
            SortedSetEntry sortedSetPastEntry = new SortedSetEntry("My past data", pastDateTimeTicks);

            var presentDateTimeTicks = DateTime.UtcNow.Ticks;
            SortedSetEntry sortedSetPresentEntry = new SortedSetEntry("My present data", presentDateTimeTicks);

            var futureDateTimeTicks = DateTime.UtcNow.AddYears(10).Ticks;
            SortedSetEntry sortedSetFutureEntry = new SortedSetEntry("My future data", futureDateTimeTicks);

            //Save to Redis
            await redis.SortedSetAddAsync(CACHE_KEY, new SortedSetEntry[] { sortedSetPastEntry, sortedSetPresentEntry, sortedSetFutureEntry } );

            //Get only past data with score
            var pastDateTicks = DateTime.UtcNow.AddDays(-1).Ticks;
            var pastData = await redis.SortedSetRangeByScoreAsync(CACHE_KEY, stop: pastDateTicks);

            //Get only present data with score
            var startDateTicks = DateTime.UtcNow.AddDays(-1).Ticks;
            var stoptDateTicks = DateTime.UtcNow.AddDays(1).Ticks;
            var presentData = await redis.SortedSetRangeByScoreAsync(CACHE_KEY, start: startDateTicks, stop: stoptDateTicks);

            //Get only future data with score
            var currentDateTicks = DateTime.UtcNow.AddHours(1).Ticks;
            var futureData = await redis.SortedSetRangeByScoreAsync(CACHE_KEY, start: currentDateTicks);

            //Remove past and present data
            var removeDateTicks = DateTime.UtcNow.AddYears(1).Ticks;
            await redis.SortedSetRemoveRangeByScoreAsync(CACHE_KEY, start: double.NegativeInfinity, stop: removeDateTicks);

        }

        private static async Task<IDatabase> GetRedis()
        {
            ConnectionMultiplexer multiplexer = await ConnectionMultiplexer.ConnectAsync("redis");
            return multiplexer.GetDatabase();
        }
    }
}
