using Injector.Runtime.Core;

namespace Injector
{
    public static class Injection
    {
        private static readonly ConcreteInjector Injector = new();

        #region Read

        public static T Get<T>() where T : class
        {
            return Injector.Get<T>();
        }

        public static T GetWithExtraDependencies<T>(params object[] extraDeps) where T : class
        {
            return Injector.Get<T>(extraDeps);
        }

        public static T Create<T>() where T : class
        {
            return Injector.CreateSingle<T>();
        }

        public static T CreateWithExtraDependencies<T>(params object[] extraDeps) where T : class
        {
            return Injector.CreateSingle<T>(extraDeps);
        }

        #endregion

        #region Write

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

        public static void Clear<T>() where T : class
        {
            Injector.Clear<T>();
        }

        #endregion
    }
}