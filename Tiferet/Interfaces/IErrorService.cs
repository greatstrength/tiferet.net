using Tiferet.Mappers.Error;

namespace Tiferet.Interfaces;

/// <summary>Service interface for managing error definitions.</summary>
public interface IErrorService : IRepository<ErrorAggregate> { }
