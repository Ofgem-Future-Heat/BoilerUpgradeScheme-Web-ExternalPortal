namespace Ofgem.Web.BUS.ExternalPortal.Domain.Exceptions;

/// <summary>
/// Captures details about a missing model field.
/// </summary>
public class ModelValidationException : Exception
{
    /// <summary>
    /// The field which has failed validation
    /// </summary>
    public string Field { get; private set; }

    /// <summary>
    /// Creates a new instance of <see cref="ModelValidationException"/>.
    /// </summary>
    /// <param name="message">A message describing the error.</param>
    /// <param name="field">The field which has failed validation</param>
    public ModelValidationException(string? message, string field) : base(message)
    {
        Field = field;
    }
}
