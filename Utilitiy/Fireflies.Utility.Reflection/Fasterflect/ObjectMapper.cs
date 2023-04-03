namespace Fireflies.Utility.Reflection.Fasterflect;

/// <summary>
///     A delegate to copy values of instance members (fields, properties, or both) from one object to another.
/// </summary>
/// <param name="source">The object whose instance members' values will be read.</param>
/// <param name="target">The object whose instance members' values will be written.</param>
public delegate void ObjectMapper(object source, object target);