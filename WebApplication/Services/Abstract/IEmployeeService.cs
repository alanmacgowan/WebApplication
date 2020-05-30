using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication.Models;

namespace WebApplication.Services.Abstract
{
    public interface IEmployeeService
    {
        List<Employee> GetAll();
        Employee GetById(int? id);
        Employee Create(Employee employee);
        Employee Edit(Employee employee);
        void Delete(int? id);
    }
}