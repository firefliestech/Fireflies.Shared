namespace Fireflies.Utility.Reflection.Fasterflect;

/// <summary>
///     A delegate to set the value of an instance field or property of an object.
/// </summary>
/// <param name="obj">
///     The object whose field's or property's value is to be set.
///     Use <see langword="null" /> for static field or property.
/// </param>
/// <param name="value">The value to be set to the field or property.</param>
public delegate void MemberSetter(object obj, object value);