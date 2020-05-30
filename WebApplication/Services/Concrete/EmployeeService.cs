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

        private WebApplicationContext db = new WebApplicationContext();

        public List<Employee> GetAll()
        {
            return db.Employees.ToList();
        }

        public Employee GetById(int? id)
        {
            Employee employee = db.Employees.Find(id);

            return employee;
        }

        public Employee Create(Employee employee)
        {
            db.Employees.Add(employee);
            db.SaveChanges();
            return employee;
        }

        public Employee Edit(Employee employee)
        {
            db.Entry(employee).State = EntityState.Modified;
            db.SaveChanges();
            return employee;
        }

        public void Delete(int? id)
        {
            Employee employee = db.Employees.Find(id);
            db.Employees.Remove(employee);
            db.SaveChanges();
        }

    }
}