namespace Fireflies.Utility.Reflection.Fasterflect;

/// <summary>
///     A delegate to retrieve an element of an array.
/// </summary>
/// <param name="array">The array whose element is to be retrieved</param>
/// <param name="index">The index of the element to be retrieved</param>
/// <returns>The element at <paramref name="index" /></returns>
public delegate object ArrayElementGetter(object array, int index);