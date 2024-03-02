using Mango.Services.RewardAPI.Data;
using Mango.Services.RewardAPI.Models;
using Mango.Services.RewardAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.RewardAPI.Services
{
    public class RewardService : IRewardService
    {
        private DbContextOptions<AppDbContext> _dbOptions;

        public RewardService(DbContextOptions<AppDbContext> dbOptions)
        {
            _dbOptions = dbOptions;
        }

        public async Task UpdateReward(RewardDto rewardDto)
        {
            try
            {
                Reward reward = new()
                {
                    OrderId = rewardDto.OrderId,
                    RewardActivity = rewardDto.RewardActivity,
                    UserId = rewardDto.UserId,
                    RewardDate = DateTime.Now
                };

                await using var _db = new AppDbContext(_dbOptions);
                await _db.Rewards.AddAsync(reward);
                await _db.SaveChangesAsync();
            }
            catch
            {
            }
        }
    }
}
