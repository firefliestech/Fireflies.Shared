namespace Fireflies.Utility.Reflection.Fasterflect;

/// <summary>
///     A delegate to set an element of an array.
/// </summary>
/// <param name="array">The array whose element is to be set.</param>
/// <param name="index">The index of the element to be set.</param>
/// <param name="value">The value to set to the element.</param>
public delegate void ArrayElementSetter(object array, int index, object value);