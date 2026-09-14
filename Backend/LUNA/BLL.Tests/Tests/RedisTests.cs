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
    public class RedisCacheServiceTests
    {
        [Fact]
        public async Task GetAsync_KeyMissing_ReturnsNull()
        {
            var mockDb = new Mock<IDatabase>();
            mockDb
                .Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(RedisValue.Null); // simulate "no such key"

            var mockConnection = new Mock<IConnectionMultiplexer>();
            mockConnection.Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                          .Returns(mockDb.Object);

            var sut = new CacheRepository(mockConnection.Object);

            var result = await sut.GetAsync("intents:all");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsync_KeyExists_ReturnsStoredString()
        {
            var mockDb = new Mock<IDatabase>();
            mockDb
                .Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(new RedisValue("[\"Calm\",\"Focus\"]"));

            var mockConnection = new Mock<IConnectionMultiplexer>();
            mockConnection.Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                          .Returns(mockDb.Object);

            var sut = new CacheRepository(mockConnection.Object);

            var result = await sut.GetAsync("intents:all");

            Assert.Equal("[\"Calm\",\"Focus\"]", result);
        }

        [Fact]
        public async Task SetAsync_SerializesValue_BeforeStoringInRedis()
        {
            var mockDb = new Mock<IDatabase>();
            var capturedValue = RedisValue.Null;

            mockDb
                .Setup(d => d.StringSetAsync(
                    It.IsAny<RedisKey>(), It.IsAny<RedisValue>(),
                    It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .Callback<RedisKey, RedisValue, TimeSpan?, bool, When, CommandFlags>(
                    (_, value, _, _, _, _) => capturedValue = value)
                .ReturnsAsync(true);

            var mockConnection = new Mock<IConnectionMultiplexer>();
            mockConnection.Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                          .Returns(mockDb.Object);

            var sut = new CacheRepository(mockConnection.Object);
            var payload = new[] { "Calm", "Focus" };

            await sut.SetAsync("intents:all", payload, TimeSpan.FromHours(1));

            // Prove it actually JSON-serialized the object, not ToString()'d it
            var expectedJson = JsonSerializer.Serialize(payload);
            Assert.Equal(expectedJson, capturedValue.ToString());
        }

        [Fact]
        public async Task RemoveAsync_CallsKeyDelete_WithTheGivenKey()
        {
            var mockDb = new Mock<IDatabase>();
            mockDb
                .Setup(d => d.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            var mockConnection = new Mock<IConnectionMultiplexer>();
            mockConnection.Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                          .Returns(mockDb.Object);

            var sut = new CacheRepository(mockConnection.Object);

            await sut.RemoveAsync("intents:all");

            mockDb.Verify(d => d.KeyDeleteAsync(
                It.Is<RedisKey>(k => k == "intents:all"),
                It.IsAny<CommandFlags>()),
                Times.Once);
        }

    }
}