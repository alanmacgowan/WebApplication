using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using WebApplication.Data;
using WebApplication.Models;
using WebApplication.Services.Abstract;

namespace WebApplication.Services.Concrete
{
    public class EmployeeService : IEmployeeService
    {

        private WebApplicationContext _dbContext;

        public EmployeeService(WebApplicationContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<Employee> GetAll()
        {
            return _dbContext.Employees.ToList();
        }

        public Employee GetById(int? id)
        {
            Employee employee = _dbContext.Employees.Find(id);

            return employee;
        }

        public Employee Create(Employee employee)
        {
            _dbContext.Employees.Add(employee);
            _dbContext.SaveChanges();
            return employee;
        }

        public Employee Edit(Employee employee)
        {
            _dbContext.Entry(employee).State = EntityState.Modified;
            _dbContext.SaveChanges();
            return employee;
        }

        public void Delete(int? id)
        {
            Employee employee = _dbContext.Employees.Find(id);
            _dbContext.Employees.Remove(employee);
            _dbContext.SaveChanges();
        }

    }
}