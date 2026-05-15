using System.ComponentModel.DataAnnotations;

namespace Tiferet.Domain.Feature;

/// <summary>
/// A base step in a feature workflow.
/// </summary>
/// <param name="Name">The name of the feature step.</param>
public record FeatureStepConfiguration([Required] string Name) : DomainObject;
