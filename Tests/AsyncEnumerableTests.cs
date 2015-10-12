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
    }
}
