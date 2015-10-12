using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace EasyAsyncEnumerable.Tests
{
    public class YieldableAsyncEnumeratorTests
    {
        [Theory]
        [InlineData("foo", "bar", "foobar")]
        [InlineData(1, 2, 3)]
        [InlineData(true, false, true)]
        public async Task Enumerates_Over_All_Yielded_Values<T>(T value1, T value2, T value3)
        {
            // Arrange
            var sut = new YieldableAsyncEnumerator<T>(async (yield, token) =>
            {
                yield.Return(value1);
                yield.Return(value2);
                yield.Return(value3);
                yield.Break();
                await Task.WhenAll();
            });

            // Assert/Act
            using (new AssertionScope())
            {
                var next = await sut.MoveNextAsync(CancellationToken.None);
                next.Should().BeTrue();
                sut.Current.Should().Be(value1);

                next = await sut.MoveNextAsync(CancellationToken.None);
                next.Should().BeTrue();
                sut.Current.Should().Be(value2);

                next = await sut.MoveNextAsync(CancellationToken.None);
                next.Should().BeTrue();
                sut.Current.Should().Be(value3);

                next = await sut.MoveNextAsync(CancellationToken.None);
                next.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Stop_Enumerating_If_No_Value_Is_Yielded()
        {
            // Arrange
            var sut = new YieldableAsyncEnumerator<string>(async (yield, token) =>
            {
                await Task.WhenAll();
            });

            // Act/Assert
            var next = await sut.MoveNextAsync(CancellationToken.None);
            next.Should().BeFalse();
        }

        [Fact]
        public async Task Stop_Enumerating_If_Break_Is_Called()
        {
            // Arrange
            var sut = new YieldableAsyncEnumerator<string>(async (yield, token) =>
            {
                yield.Break();
                await Task.WhenAll();
            });

            // Act/Assert
            var next = await sut.MoveNextAsync(CancellationToken.None);
            next.Should().BeFalse();
        }

        [Fact]
        public async Task Stop_Enumerating_If_The_Cancellation_Token_Is_Set()
        {
            // Arrange
            var sut = new YieldableAsyncEnumerator<string>(async (yield, token) =>
            {
                yield.Return("foo");
                yield.Return("bar");
                await Task.WhenAll();
            });

            // Act/Assert
            using (new AssertionScope())
            {
                var tokenSource = new CancellationTokenSource();
                var next = await sut.MoveNextAsync(tokenSource.Token);
                next.Should().BeTrue();
                sut.Current.Should().Be("foo");

                tokenSource.Cancel();
                next = await sut.MoveNextAsync(tokenSource.Token);
                next.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Provides_The_Producer_Function_With_A_Cancellation_Token()
        {
            // Arrange
            var receivedToken = CancellationToken.None;
            var sut = new YieldableAsyncEnumerator<string>(async (yield, token) =>
            {
                receivedToken = token;
                await Task.WhenAll();
            });

            // Act/Assert
            var tokenSource = new CancellationTokenSource();
            await sut.MoveNextAsync(tokenSource.Token);
            receivedToken.Should().Be(tokenSource.Token);
        }
    }
}