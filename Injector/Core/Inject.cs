using System;

namespace Injector.Core
{
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method)]
    public class Inject : Attribute
    {
        
    }
    
    
}