using Ninject;
using Ninject.Web.Common;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using WebApplication.Data;
using WebApplication.Services.Abstract;
using WebApplication.Services.Concrete;

public class NinjectDependencyResolver : IDependencyResolver
{
    private IKernel kernel;

    public NinjectDependencyResolver()
    {
        // Initialize kernel and add bindings
        kernel = new StandardKernel();
        AddBindings();
    }

    public object GetService(Type serviceType)
    {
        return kernel.TryGet(serviceType);
    }

    public IEnumerable<object> GetServices(Type serviceType)
    {
        return kernel.GetAll(serviceType);
    }

    private void AddBindings()
    {
        kernel.Bind<WebApplicationContext>().To<WebApplicationContext>().InRequestScope();
        kernel.Bind<IEmployeeService>().To<EmployeeService>().InRequestScope();

    }
}