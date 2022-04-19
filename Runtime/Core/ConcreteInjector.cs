using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace Injector.Runtime.Core
{
    public class ConcreteInjector : IDependenciesInjector
    {
        private readonly Dictionary<Type, object> _instances = new();
        private readonly Dictionary<Type, Type> _implementations = new();
        private object[] _extras = Array.Empty<object>();

        public T Get<T>() where T : class
        {
            return Resolve(typeof(T)) as T;
        }

        public T Get<T>(object[] extraDependencies) where T : class
        {
            _extras = extraDependencies;
            var dependency = Get<T>();
            _extras = Array.Empty<object>();
            return dependency;
        }

        public T CreateSingle<T>() where T : class
        {
            return CreateSingle(typeof(T)) as T;
        }

        public T CreateSingle<T>(object[] extraDependencies) where T : class
        {
            _extras = extraDependencies;
            var dependency = CreateSingle<T>();
            _extras = Array.Empty<object>();
            return dependency;
        }


        public void Register<TInterface, TImplementation>() where TImplementation : class, TInterface
        {
            _implementations[typeof(TInterface)] = typeof(TImplementation);
        }

        public void Register<T>(T instance) where T : class
        {
            _implementations[typeof(T)] = typeof(T);
            _instances[typeof(T)] = instance;
        }

        public void Register<TClass>() where TClass : class
        {
            _implementations[typeof(TClass)] = typeof(TClass);
        }

        public void Clear<T>() where T : class
        {
            _implementations.Remove(typeof(T));
            _instances.Remove(typeof(T));
        }


        private bool TryGetExtraDep(Type type, out object instance)
        {
            var count = _extras.Length;
            for (var i = 0; i < count; i++)
            {
                var extraDependency = _extras[i];
                if (extraDependency.GetType() == type)
                {
                    instance = extraDependency;
                    return true;
                }
            }

            instance = null;
            return false;
        }

        #region Unity

        private static bool TryGetUnityReference(Type type, out object instance)
        {
            instance = default;
            if (!type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                return false;
            }

            var unityInstance = UnityEngine.Object.FindObjectOfType(type);
            if (!unityInstance)
            {
                return false;
            }

            instance = unityInstance;
            return true;
        }

        private static bool ValidateIfUnityInstance(object instance)
        {
            if (instance == null) return false;
            if (!instance.GetType().IsSubclassOf(typeof(UnityEngine.Object)))
            {
                return true;
            }

            //validates unity lifecycle
            return instance as UnityEngine.Object;
        }

        #endregion
        
        private bool TryGetCachedInstance(Type type, out object instance)
        {
            var hasValue = _instances.TryGetValue(type, out instance);
            return hasValue && ValidateIfUnityInstance(instance);
        }

        private bool TryGetInstance(Type type, out object instance)
        {
            return TryGetCachedInstance(type, out instance)
                   || TryGetExtraDep(type, out instance)
                   || TryGetUnityReference(type, out instance);
        }

        private object CreateSingle(Type type)
        {
            if (!IsRegistered(type))
            {
                if (type.IsInterface || type.IsAbstract)
                {
                    //cant be instantiated without a concrete class
                    return null;
                }

                //register the type as its own implementation
                _implementations[type] = type;
            }

            var constructor = GetBestConstructor(type);
            return constructor == null ? null : InvokeConstructor(constructor);
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
            _instances[type] = instance;
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
            if (!_implementations.TryGetValue(lowType, out var type))
            {
                type = lowType;
            }

            var constructors = type.GetConstructors();

            var maxParams = -1;
            ConstructorInfo maxConstructor = null;

            foreach (var constructor in constructors)
            {
                if (!CanInitialize(constructor)) continue;

                if (constructor.GetParameters().Length > maxParams)
                {
                    maxConstructor = constructor;
                    maxParams = constructor.GetParameters().Length;
                }
            }

            return maxConstructor;
        }

        private bool CanInitialize(MethodBase constructor)
        {
            return constructor.GetParameters().All(t => IsRegistered(t.ParameterType));
        }

        private bool IsRegistered(Type type)
        {
            return _implementations.ContainsKey(type);
        }
    }
}