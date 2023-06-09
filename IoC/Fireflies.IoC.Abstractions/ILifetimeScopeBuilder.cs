﻿namespace Fireflies.IoC.Abstractions;

public interface ILifetimeScopeBuilder {
    TImplementation GetBuilder<TImplementation>() where TImplementation : class;

    void RegisterType<T>() where T : class;
    void RegisterType(Type type);
    void RegisterInstance<T>(T instance) where T : class;
    void RegisterTypeAsSingleInstance<T>() where T : class;
}