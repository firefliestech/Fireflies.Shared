namespace Fireflies.Utility.Reflection.Fasterflect;

/// <summary>
///     A delegate to set multiple field/property values of an object.
/// </summary>
/// <param name="obj">
///     The object whose field's or property's value is to be set.
///     Use <see langword="null" /> if all fields and properties are static.
/// </param>
/// <param name="values">The value to be set to the field or property.</param>
public delegate void MultiSetter(object obj, params object[] values);