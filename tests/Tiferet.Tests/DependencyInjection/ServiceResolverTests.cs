using Microsoft.Extensions.DependencyInjection;
using Tiferet.Blueprints;
using Tiferet.DependencyInjection;
using Tiferet.Events;
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
    public void AddTiferet_WithConfigure_InvokesCallback()
    {
        var services = new ServiceCollection();
        var configured = false;

        // The callback is invoked, but BootstrapAppConfiguration throws
        // because there is no real config on disk — that's expected.
        Assert.ThrowsAny<Exception>(() =>
            services.AddTiferet(options =>
            {
                configured = true;
                options.InterfaceId = "test";
            }));

        Assert.True(configured);
    }

    [Fact]
    public void AddTiferet_ReturnsServiceCollection_ForChaining()
    {
        // Use a temp config to verify chaining.
        var configDir = Path.Combine(Path.GetTempPath(), $"tiferet_ext_{Guid.NewGuid():N}");
        Directory.CreateDirectory(configDir);
        try
        {
            File.WriteAllText(Path.Combine(configDir, "config.yml"),
                "interfaces:\n  test:\n    Name: Test\n    AssemblyName: Tiferet\n    TypeName: Tiferet.Contexts.AppInterfaceContext\n" +
                "features: {}\nerrors: {}\nservices: {}\nconst: {}\nlogging:\n  formatters: {}\n  handlers: {}\n  loggers: {}\n");

            var services = new ServiceCollection();
            var result = services.AddTiferet(o => { o.InterfaceId = "test"; o.ConfigDir = configDir; });
            Assert.Same(services, result);
        }
        finally
        {
            Directory.Delete(configDir, true);
        }
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
