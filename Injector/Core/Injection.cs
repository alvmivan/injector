namespace Injector.Core
{
    public static class Injection
    {
        private static readonly ConcreteInjector Injector = new ConcreteInjector();
        
        public static T Get<T>() where T : class
        {
            return Injector.Get<T>();
        }

        public static void Register<TInterface, TImplementation>() where TImplementation : class, TInterface
        {
            Injector.Register<TInterface, TImplementation>();
        }
        
        public static void Register<T>(T instance) where T : class
        {
            Injector.Register(instance);
        }

        public static void Register<TClass>() where TClass : class
        {
            Injector.Register<TClass>();
        }

        public static void Clear<T>() where T: class
        {
            Injector.Clear<T>();
        }
        
    }
}