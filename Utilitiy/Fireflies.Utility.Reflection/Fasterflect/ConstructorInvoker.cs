namespace Fireflies.Utility.Reflection.Fasterflect;

/// <summary>
///     A delegate to invoke the constructor of a type.
/// </summary>
/// <param name="parameters">The properly-ordered parameter list of the constructor.</param>
/// <returns>An instance of type whose constructor is invoked.</returns>
public delegate object ConstructorInvoker(params object[] parameters);