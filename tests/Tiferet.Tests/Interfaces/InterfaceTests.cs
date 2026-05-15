using Tiferet.Interfaces;
using Tiferet.Mappers;
using Tiferet.Mappers.Feature;
using Tiferet.Mappers.Cli;
using Tiferet.Mappers.App;
using Tiferet.Mappers.Error;

namespace Tiferet.Tests.Interfaces;

public class InterfaceTests
{
    [Fact]
    public void IService_IsMarkerInterface()
    {
        Assert.Empty(typeof(IService).GetMethods());
    }

    [Fact]
    public void IRepository_ExtendsIService()
    {
        Assert.True(typeof(IService).IsAssignableFrom(typeof(IRepository<object>)));
    }

    [Fact]
    public void IRepository_DefinesCrudMethods()
    {
        var methods = typeof(IRepository<object>).GetMethods();
        var methodNames = methods.Select(m => m.Name).ToHashSet();
        Assert.Contains("Exists", methodNames);
        Assert.Contains("Get", methodNames);
        Assert.Contains("List", methodNames);
        Assert.Contains("Save", methodNames);
        Assert.Contains("Delete", methodNames);
    }

    [Fact]
    public void IAppService_ExtendsIRepository()
    {
        Assert.True(typeof(IService).IsAssignableFrom(typeof(IAppService)));
        Assert.True(typeof(IRepository<AppInterfaceAggregate>).IsAssignableFrom(typeof(IAppService)));
    }

    [Fact]
    public void IFeatureService_ExtendsIRepository()
    {
        Assert.True(typeof(IRepository<FeatureAggregate>).IsAssignableFrom(typeof(IFeatureService)));
    }

    [Fact]
    public void IErrorService_ExtendsIRepository()
    {
        Assert.True(typeof(IRepository<ErrorAggregate>).IsAssignableFrom(typeof(IErrorService)));
    }

    [Fact]
    public void ICliService_ExtendsIRepository()
    {
        Assert.True(typeof(IRepository<CliCommandAggregate>).IsAssignableFrom(typeof(ICliService)));
    }

    [Fact]
    public void IDIService_ExtendsIService()
    {
        Assert.True(typeof(IService).IsAssignableFrom(typeof(IDIService)));
    }

    [Fact]
    public void IFileService_ExtendsIDisposable()
    {
        Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(IFileService)));
    }

    [Fact]
    public void ISqliteService_ExtendsIDisposable()
    {
        Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(ISqliteService)));
    }

    [Fact]
    public void IConfigurationService_ExtendsIService()
    {
        Assert.True(typeof(IService).IsAssignableFrom(typeof(IConfigurationService)));
    }

    [Fact]
    public void ILoggingService_ExtendsIService()
    {
        Assert.True(typeof(IService).IsAssignableFrom(typeof(ILoggingService)));
    }
}
