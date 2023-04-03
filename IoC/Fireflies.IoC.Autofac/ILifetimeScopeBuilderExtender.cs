using Autofac.Builder;

namespace Fireflies.IoC.Autofac;

public interface ILifetimeScopeBuilderExtender {
    void RegisterType<T>(IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle> registrationExtender) where T : class;
}