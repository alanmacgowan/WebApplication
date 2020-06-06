using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace WebApplication.Tests.Acceptance.Base
{
    public static class UnityContainerFactory
    {
        private static IUnityContainer _unityContainer;

        static UnityContainerFactory()
        {
            _unityContainer = new UnityContainer();
        }

        public static IUnityContainer GetContainer()
        {
            return _unityContainer;
        }
    }
}
