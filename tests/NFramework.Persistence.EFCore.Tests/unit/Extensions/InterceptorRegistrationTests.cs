using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NFramework.Persistence.EFCore.Extensions;
using NFramework.Persistence.EFCore.Interceptors;
using NFramework.Persistence.EFCore.Tests.Unit.Helpers;
using Shouldly;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Unit.Extensions;

public class InterceptorRegistrationTests
{
    [Fact]
    public void AddSoftDeleteInterceptor_AddsInterceptorToOptions()
    {
        // Arrange
        var builder = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());

        // Act
        builder.AddSoftDeleteInterceptor();

        // Assert
        var options = builder.Options;
        options
            .GetExtension<CoreOptionsExtension>()
            .Interceptors!.Any(i => i is SoftDeletionInterceptor)
            .ShouldBeTrue();
    }

    [Fact]
    public void AddAuditableInterceptor_AddsInterceptorToOptions()
    {
        // Arrange
        var builder = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());

        // Act
        builder.AddAuditableInterceptor();

        // Assert
        var options = builder.Options;
        options.GetExtension<CoreOptionsExtension>().Interceptors!.Any(i => i is AuditableInterceptor).ShouldBeTrue();
    }

    [Fact]
    public void AddAuditLoggerInterceptor_AddsInterceptorToOptions()
    {
        // Arrange
        var builder = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());

        // Act
        builder.AddAuditLoggerInterceptor();

        // Assert
        var options = builder.Options;
        options.GetExtension<CoreOptionsExtension>().Interceptors!.Any(i => i is AuditLoggerInterceptor).ShouldBeTrue();
    }

    [Fact]
    public void AddInterceptors_NonGenericBuilder_AddsInterceptors()
    {
        // Arrange
        var builder = new DbContextOptionsBuilder();

        // Act
        builder.AddSoftDeleteInterceptor().AddAuditableInterceptor().AddAuditLoggerInterceptor();

        // Assert
        var options = builder.Options;
        var interceptors = options.GetExtension<CoreOptionsExtension>().Interceptors;
        interceptors.ShouldNotBeNull();
        interceptors!.Any(i => i is SoftDeletionInterceptor).ShouldBeTrue();
        interceptors.Any(i => i is AuditableInterceptor).ShouldBeTrue();
        interceptors.Any(i => i is AuditLoggerInterceptor).ShouldBeTrue();
    }
}
