using Autofac;
using Fireflies.IoC.Abstractions;

namespace Fireflies.IoC.Autofac;

public class LifetimeScopeBuilder : ILifetimeScopeBuilder {
    private readonly ContainerBuilder _containerBuilder;
    private readonly ILifetimeScopeBuilderExtender? _lifetimeScopeBuilderExtender;

    public LifetimeScopeBuilder(ContainerBuilder containerBuilder, ILifetimeScopeBuilderExtender? lifetimeScopeBuilderExtender = null) {
        _containerBuilder = containerBuilder;
        _lifetimeScopeBuilderExtender = lifetimeScopeBuilderExtender;
    }

    public TImplementation GetBuilder<TImplementation>() where TImplementation : class {
        return (TImplementation)(object)_containerBuilder;
    }

    public void RegisterType<T>() where T : class {
        var builder = _containerBuilder.RegisterType<T>();
        _lifetimeScopeBuilderExtender?.RegisterType(builder);
    }

    public void RegisterType(Type type) {
        var builder = _containerBuilder.RegisterType(type);
        _lifetimeScopeBuilderExtender?.RegisterType(builder);
    }

    public void RegisterInstance<T>(T instance) where T : class {
        _containerBuilder.RegisterInstance(instance);
    }

    public void RegisterTypeAsSingleInstance<T>() where T : class {
        _containerBuilder.RegisterType<T>().SingleInstance();
    }
}