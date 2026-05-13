using Microsoft.Extensions.DependencyInjection;
using Tiferet.DependencyInjection;
using Tiferet.Interfaces;

namespace Tiferet.Tests.DependencyInjection;

// Test service interface and implementation.
public interface ICalculatorService : IService
{
    int Add(int a, int b);
}

public class CalculatorService : ICalculatorService
{
    public int Add(int a, int b) => a + b;
}

// A class with constructor injection for BuildFactory tests.
public class CalculatorConsumer
{
    public ICalculatorService Calculator { get; }

    public CalculatorConsumer(ICalculatorService calculator)
    {
        Calculator = calculator;
    }
}

public class DynamicServiceResolverTests
{
    [Fact]
    public void AddService_Instance_ThenResolve()
    {
        var resolver = new DynamicServiceResolver();
        var svc = new CalculatorService();
        resolver.AddService<ICalculatorService>(svc);

        var resolved = resolver.GetService<ICalculatorService>();
        Assert.NotNull(resolved);
        Assert.Same(svc, resolved);
    }

    [Fact]
    public void AddService_Factory_ThenResolve()
    {
        var resolver = new DynamicServiceResolver();
        resolver.AddService<ICalculatorService>(_ => new CalculatorService());

        var resolved = resolver.GetService<ICalculatorService>();
        Assert.NotNull(resolved);
        Assert.Equal(5, resolved!.Add(2, 3));
    }

    [Fact]
    public void AddService_ByType_ThenResolve()
    {
        var resolver = new DynamicServiceResolver();
        var svc = new CalculatorService();
        resolver.AddService(typeof(ICalculatorService), svc);

        var resolved = resolver.GetService(typeof(ICalculatorService));
        Assert.NotNull(resolved);
        Assert.Same(svc, resolved);
    }

    [Fact]
    public void GetService_ReturnsNull_WhenNotRegistered()
    {
        var resolver = new DynamicServiceResolver();
        Assert.Null(resolver.GetService<ICalculatorService>());
    }

    [Fact]
    public void GetService_ByType_ReturnsNull_WhenNotRegistered()
    {
        var resolver = new DynamicServiceResolver();
        Assert.Null(resolver.GetService(typeof(ICalculatorService)));
    }

    [Fact]
    public void RemoveService_RemovesRegistration()
    {
        var resolver = new DynamicServiceResolver();
        resolver.AddService<ICalculatorService>(new CalculatorService());
        Assert.NotNull(resolver.GetService<ICalculatorService>());

        resolver.RemoveService<ICalculatorService>();
        Assert.Null(resolver.GetService<ICalculatorService>());
    }

    [Fact]
    public void RemoveService_IsIdempotent()
    {
        var resolver = new DynamicServiceResolver();
        resolver.RemoveService<ICalculatorService>(); // no-op, should not throw
    }

    [Fact]
    public void BuildFactory_ConstructsWithInjectedDependencies()
    {
        var resolver = new DynamicServiceResolver();
        resolver.AddService<ICalculatorService>(new CalculatorService());

        var factory = resolver.BuildFactory<CalculatorConsumer>();
        var consumer = factory();

        Assert.NotNull(consumer);
        Assert.NotNull(consumer.Calculator);
        Assert.Equal(7, consumer.Calculator.Add(3, 4));
    }

    [Fact]
    public void BuildFactory_ReturnsNewInstanceEachCall()
    {
        var resolver = new DynamicServiceResolver();
        resolver.AddService<ICalculatorService>(new CalculatorService());

        var factory = resolver.BuildFactory<CalculatorConsumer>();
        var a = factory();
        var b = factory();

        Assert.NotSame(a, b);
    }
}

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddTiferet_RegistersIServiceResolver()
    {
        var services = new ServiceCollection();
        services.AddTiferet();

        var provider = services.BuildServiceProvider();
        var resolver = provider.GetService<IServiceResolver>();
        Assert.NotNull(resolver);
        Assert.IsType<DynamicServiceResolver>(resolver);
    }

    [Fact]
    public void AddTiferet_WithConfigure_InvokesCallback()
    {
        var services = new ServiceCollection();
        var configured = false;
        services.AddTiferet(resolver =>
        {
            configured = true;
            resolver.AddService<ICalculatorService>(new CalculatorService());
        });

        Assert.True(configured);

        var provider = services.BuildServiceProvider();
        var resolver = provider.GetRequiredService<IServiceResolver>();
        Assert.NotNull(resolver.GetService<ICalculatorService>());
    }

    [Fact]
    public void AddTiferet_ReturnsServiceCollection_ForChaining()
    {
        var services = new ServiceCollection();
        var result = services.AddTiferet();
        Assert.Same(services, result);
    }
}

public class IServiceResolverTests
{
    [Fact]
    public void IsInterface()
    {
        Assert.True(typeof(IServiceResolver).IsInterface);
    }

    [Fact]
    public void ExtendsIService()
    {
        Assert.True(typeof(IService).IsAssignableFrom(typeof(IServiceResolver)));
    }

    [Fact]
    public void DynamicServiceResolver_ImplementsIServiceResolver()
    {
        Assert.True(typeof(IServiceResolver).IsAssignableFrom(typeof(DynamicServiceResolver)));
    }
}
