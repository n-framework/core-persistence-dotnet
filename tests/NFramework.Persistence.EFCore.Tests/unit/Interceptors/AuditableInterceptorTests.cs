using Microsoft.EntityFrameworkCore;
using Moq;
using NFramework.Persistence.EFCore.Interceptors;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Unit.Interceptors;

public class AuditableInterceptorTests
{
    [Fact]
    public void SavingChanges_WhenUpdateTimestampsFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var interceptor = new AuditableInterceptor();

        // We use reflection to call the private static method UpdateTimestamps with a context that will fail
        // Actually, we can just pass a mock context that throws when ChangeTracker is accessed.
        var mockContext = new Mock<DbContext>();
        mockContext.Setup(c => c.ChangeTracker).Throws(new Exception("Interception Failure"));

        var method = typeof(AuditableInterceptor).GetMethod(
            "UpdateTimestamps",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
        );

        // Act & Assert
        var exception = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
            method!.Invoke(null, [mockContext.Object])
        );

        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Contains(
            "An error occurred while updating auditable entity timestamps",
            exception.InnerException.Message,
            StringComparison.Ordinal
        );
    }
}
