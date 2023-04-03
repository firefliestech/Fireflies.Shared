namespace Fireflies.Utility.Reflection.Fasterflect;

/// <summary>
///     A delegate to invoke an instance method or indexer of an object.
/// </summary>
/// <param name="obj">
///     The object whose method or indexer is to be invoked on.
///     Use <see langword="null" /> for static method.
/// </param>
/// <param name="parameters">
///     The properly-ordered parameter list of the method/indexer.
///     For indexer-set operation, the parameter array include parameters for the indexer plus
///     the value to be set to the indexer.
/// </param>
/// <returns>
///     The return value of the method or indexer.  Null is returned if the method has no
///     return type or if it's a indexer-set operation.
/// </returns>
public delegate object MethodInvoker(object obj, params object[] parameters);