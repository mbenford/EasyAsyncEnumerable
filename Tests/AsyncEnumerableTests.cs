using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace EasyAsyncEnumerable.Tests
{
    public class AsyncEnumerableTests
    {
        public class CreateMethod
        {
            [Theory]
            [InlineData("foo")]
            [InlineData(1)]
            [InlineData(true)]
            public void Creates_An_Async_Enumerator_Of<T>(T value)
            {
                // Arrange
                var enumerable = AsyncEnumerable.Create<T>((yield, token) => Task.WhenAll());

                // Act
                var enumerator = enumerable.GetEnumerator();

                // Assert
                using (new AssertionScope())
                {
                    enumerator.Should().BeAssignableTo<IAsyncEnumerator<T>>();
                    enumerator.Should().BeOfType<YieldableAsyncEnumerator<T>>();
                }
            }
        }

        public class EmptyMethod
        {
            [Theory]
            [InlineData("foo")]
            [InlineData(1)]
            [InlineData(true)]
            public async Task Creates_An_Empty_Async_Enumerable_Of<T>(T value)
            {
                var enumerable = AsyncEnumerable.Empty<T>();

                // Act
                var enumerator = enumerable.GetEnumerator();

                // Assert
                using (new AssertionScope())
                {
                    enumerator.Should().BeAssignableTo<IAsyncEnumerator<T>>();
                    var next = await enumerator.MoveNextAsync(CancellationToken.None);
                    next.Should().BeFalse();
                }
            }
        }

        public class ForEachMethod
        {
            [Theory]
            [InlineData("foo", "bar", "foobar")]
            [InlineData(1, 2, 3)]
            [InlineData(true, false, true)]
            public async Task Performs_the_specified_action_on_each_element_of_the_enumerable<T>(T value1, T value2, T value3)
            {
                // Arrange
                var enumerable = AsyncEnumerable.Create<T>(async (yield, token) => 
                {
                    yield.Return(value1);
                    yield.Return(value2);
                    yield.Return(value3);
                    yield.Break();
                    await Task.WhenAll();
                });

                // Act
                var values = new List<T>();
                await enumerable.ForEachAsync(element => values.Add(element));

                //Assert
                values.Should().BeEquivalentTo(value1, value2, value3);
            }

            [Theory]
            [InlineData("foo", "bar", "foobar")]
            [InlineData(1, 2, 3)]
            [InlineData(true, false, true)]
            public async Task Performs_the_specified_action_on_each_element_of_the_enumerable_asynchronously<T>(T value1, T value2, T value3)
            {
                // Arrange
                var enumerable = AsyncEnumerable.Create<T>(async (yield, token) => 
                {
                    yield.Return(value1);
                    yield.Return(value2);
                    yield.Return(value3);
                    yield.Break();
                    await Task.WhenAll();
                });

                // Act
                var values = new List<T>();
                await enumerable.ForEachAsync(async element =>
                {
                    values.Add(element);
                    await Task.WhenAll();
                });

                // Assert
                values.Should().BeEquivalentTo(value1, value2, value3);
            }

            [Fact]
            public async Task Passes_the_cancellation_token_to_the_underlying_enumerator()
            {
                // Arrange
                var producerToken = CancellationToken.None;
                var enumerable = AsyncEnumerable.Create<string>(async (yield, token) =>
                {
                    producerToken = token;
                    yield.Return("foo");
                    yield.Break();
                    await Task.WhenAll();
                });

                // Act
                var forEachToken = CancellationToken.None;
                var tokenSource = new CancellationTokenSource();
                await enumerable.ForEachAsync(tokenSource.Token, async (element, token) =>
                {
                    forEachToken = token;
                    await Task.WhenAll();
                });

                // Assert
                using (new AssertionScope())
                {
                    producerToken.Should().Be(tokenSource.Token);
                    forEachToken.Should().Be(tokenSource.Token);
                }
            }
        }
    }
}
