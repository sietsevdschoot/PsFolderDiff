using Xunit;

namespace PsFolderDiff.FileHashLookupLib.UnitTests;

public class InlineSampleTests
{
    [Fact]
    public void CanInvoke_Sample2()
    {
        var sample = new InlineSampleService2();

        // Arrange
        Action<string>? myAction = str => Console.WriteLine($"Hoiii {str}");
        sample.Execute(progress: null);
        sample.Execute(progress: myAction);

        // Act

        // Assert
    }
}