using System;
using System.Collections.Generic;
using System.Reflection;

namespace Injector.Core
{
    public class ConcreteInjector : DependenciesInjector
    {
        private static readonly object[] EmptyExtras = new object[0];
        private readonly Dictionary<Type, object> instances = new Dictionary<Type, object>();
        private readonly Dictionary<Type, Type> implementations = new Dictionary<Type, Type>();

        private object[] extras = EmptyExtras;

        public T Get<T>() where T : class
        {
            return Resolve(typeof(T)) as T;
        }

        public T ResolveWith<T>(params object[] extraDependencies) where T : class
        {
            extras = extraDependencies;
            var dependency = Resolve(typeof(T)) as T;
            extras = EmptyExtras;
            return dependency;
        }

        public void Register<TInterface, TImplementation>() where TImplementation : class, TInterface
        {
            implementations[typeof(TInterface)] = typeof(TImplementation);
        }

        public void Register<T>(T instance) where T : class
        {
            implementations[typeof(T)] = typeof(T);
            instances[typeof(T)] = instance;
        }

        public void Register<TClass>() where TClass : class
        {
            implementations[typeof(TClass)] = typeof(TClass);
        }

        public void Clear<T>() where T : class
        {
            implementations.Remove(typeof(T));
            instances.Remove(typeof(T));
        }

        private bool TryGetInstance(Type type, out object instance)
        {
            var hasInstance = instances.TryGetValue(type, out instance);
            if (hasInstance) return true;

            foreach (var extraDependency in extras)
            {
                if (extraDependency.GetType() == type)
                {
                    instance = extraDependency;
                    return true;
                }
            }

            return false;
        }

        private object Resolve(Type type)
        {
            if (TryGetInstance(type, out var instance))
            {
                return instance;
            }

            if (!IsRegistered(type))
            {
                return null;
            }

            var constructor = GetBestConstructor(type);
            if (constructor == null)
            {
                return null;
            }

            instance = InvokeConstructor(constructor);
            instances[type] = instance;
            return instance;
        }

        private object InvokeConstructor(ConstructorInfo constructor)
        {
            var parameters = constructor.GetParameters();
            var arguments = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                arguments[i] = Resolve(parameter.ParameterType);
            }

            return constructor.Invoke(arguments);
        }

        private ConstructorInfo GetBestConstructor(Type lowType)
        {
            if (!implementations.TryGetValue(lowType, out var type))
            {
                type = lowType;
            }

            var constructors = type.GetConstructors();

            var maxParams = -1;
            ConstructorInfo maxConstructor = null;

            foreach (var constructor in constructors)
            {
                if (!CanInitialize(constructor)) continue;

                if (HasAttribute<Inject>(constructor))
                {
                    return constructor;
                }

                if (constructor.GetParameters().Length > maxParams)
                {
                    maxConstructor = constructor;
                    maxParams = constructor.GetParameters().Length;
                }
            }

            return maxConstructor;
        }

        private bool CanInitialize(ConstructorInfo constructor)
        {
            foreach (var t in constructor.GetParameters())
            {
                if (!IsRegistered(t.ParameterType))
                    return false;
            }

            return true;
        }

        private bool IsRegistered(Type type)
        {
            return implementations.ContainsKey(type);
        }

        private bool HasAttribute<T>(MemberInfo info) where T : Attribute
        {
            return info.GetCustomAttribute<T>() != null;
        }
    }
}