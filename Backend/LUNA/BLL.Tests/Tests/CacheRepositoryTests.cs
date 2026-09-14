using System;
using System.Text.Json;
using System.Threading.Tasks;
using BLL.Caching;
using BLL.Interfaces;
using BLL.Services.Repository;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace BLL.Tests.Caching
{
    public class CacheRepositoryTests
    {
      
        [Fact]
        public async Task GetAsync_DelegatesToRepository_AndReturnsItsResult()
        {
            var mockRepo = new Mock<ICacheRepository>();
            mockRepo.Setup(r => r.GetAsync("intents:all"))
                    .ReturnsAsync("[\"Calm\",\"Focus\"]");

            var sut = new RedisCacheService(mockRepo.Object);

            var result = await sut.GetAsync("intents:all");

            Assert.Equal("[\"Calm\",\"Focus\"]", result);
        }

        [Fact]
        public async Task SetCacheValueAsync_ForwardsKeyValueAndDuration_ToRepository()
        {
            var mockRepo = new Mock<ICacheRepository>();
            var payload = new[] { "Calm", "Focus" };
            var duration = TimeSpan.FromHours(6);

            var sut = new RedisCacheService(mockRepo.Object);

            await sut.SetCacheValueAsync("intents:all", payload, duration);

            mockRepo.Verify(r => r.SetAsync("intents:all", payload, duration), Times.Once);
        }

        [Fact]
        public async Task RemoveAsync_ForwardsTheKey_ToRepository()
        {
            // This test fails against the buggy version (cacheRepository.RemoveAsync()
            // with no key) and passes once you fix it to RemoveAsync(key).
            var mockRepo = new Mock<ICacheRepository>();

            var sut = new RedisCacheService(mockRepo.Object);

            await sut.RemoveAsync("intents:all");

            mockRepo.Verify(r => r.RemoveAsync("intents:all"), Times.Once);
        }
    }
}