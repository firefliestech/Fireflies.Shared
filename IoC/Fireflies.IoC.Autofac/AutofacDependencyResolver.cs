using Autofac;
using Fireflies.IoC.Abstractions;

namespace Fireflies.IoC.Autofac;

public class AutofacDependencyResolver : IDependencyResolver {
    private ILifetimeScope _rootContainer = null!;
    private readonly ILifetimeScopeBuilderExtender? _lifetimeScopeBuilderExtender;

    internal AutofacDependencyResolver() {
    }

    public AutofacDependencyResolver(ILifetimeScope rootContainer, ILifetimeScopeBuilderExtender? lifetimeScopeBuilderExtender = null) {
        _rootContainer = rootContainer;
        _lifetimeScopeBuilderExtender = lifetimeScopeBuilderExtender;
    }

    public IDependencyResolver BeginLifetimeScope(Action<ILifetimeScopeBuilder> builder) {
        var innerContainer = new AutofacDependencyResolver();

        var lifetimeScope = _rootContainer.BeginLifetimeScope(x => {
            x.RegisterInstance<IDependencyResolver>(innerContainer);
            builder(new LifetimeScopeBuilder(x, _lifetimeScopeBuilderExtender));
        });

        innerContainer._rootContainer = lifetimeScope;
        return innerContainer;
    }

    public T Resolve<T>() where T : class {
        return _rootContainer.Resolve<T>();
    }

    public object Resolve(Type type) {
        return _rootContainer.Resolve(type);
    }

    public bool TryResolve<T>(out T? instance) where T : class {
        return _rootContainer.TryResolve(out instance);
    }

    public void Dispose() {
        _rootContainer.Dispose();
    }
}