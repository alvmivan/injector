namespace Injector
{
    public interface IDependenciesInjector
    {
        T Get<T>() where T : class;
        void Register<TInterface, TImplementation>() where TImplementation : class, TInterface;
        void Register<T>(T instance) where T : class;
        void Register<TClass>() where TClass : class;
        void Clear<T>() where T: class;
    }
}