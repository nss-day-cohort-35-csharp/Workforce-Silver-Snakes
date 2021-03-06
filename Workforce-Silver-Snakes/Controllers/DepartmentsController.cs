﻿using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Workforce_Silver_Snakes.Models;
using Workforce_Silver_Snakes.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Workforce_Silver_Snakes.Controllers
{
    public class DepartmentsController : Controller
    {
        private readonly IConfiguration _config;
        public DepartmentsController(IConfiguration config)
        {
            _config = config;
        }
        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }
        // GET: Departments
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT d.[Name], d.Budget, d.Id, COUNT(e.DepartmentId) AS EmployeeCount
                                        FROM Department d
                                        LEFT JOIN Employee e ON e.DepartmentId = d.Id
                                        GROUP BY d.[Name], d.Id, d.Budget 
                                        ORDER BY COUNT(e.DepartmentId)";
                    var reader = cmd.ExecuteReader();


                    var departments = new List<Department>();

                    while (reader.Read())
                    {
                        departments.Add(new Department
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Budget = reader.GetInt32(reader.GetOrdinal("Budget")),
                            EmployeeCount = reader.GetInt32(reader.GetOrdinal("EmployeeCount"))
                        });
                    }


                    reader.Close();
                    return View(departments);
                }
            }
        }

        // GET: Departments/Details/5
        public ActionResult Details(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT 
                                        d.[Name],
                                        d.Id,
                                        d.Budget,
                                        e.Id as EmployeeId, e.FirstName, e.LastName
                                        
                                        FROM Department d 
                                        LEFT JOIN Employee e ON e.DepartmentId = d.Id
                                        WHERE d.Id = @id";



                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    var reader = cmd.ExecuteReader();

                    var departments = new List<Department>();
                    var employees = new List<Employee>();

                    Department department = null;

                    while (reader.Read())
                    {
                        if (department == null)
                        {


                            department = new Department
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Budget = reader.GetInt32(reader.GetOrdinal("Budget")),

                                Employees = new List<Employee>()

                            };
                        }

                        var hasEmployee = !reader.IsDBNull(reader.GetOrdinal("EmployeeId"));

                        if (hasEmployee)
                        {
                            department.Employees.Add(new Employee()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),



                            });
                        }
                    }

                    reader.Close();

                    if (department == null)
                    {
                        return NotFound();
                    }
                    return View(department);
                }

            }

        }

        // GET: Departments/Create
        public ActionResult Create()
        {
            var departments = GetDepartment().Select(d => new SelectListItem
            {
                Text = d.Name,
                Value = d.Id.ToString()
            }).ToList();

            var viewModel = new DepartmentViewModel
            {
                Departments = departments
            };

            return View(viewModel);
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Department department)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Department (Name, Budget)
                                           VALUES (@Name, @Budget)";

                        cmd.Parameters.Add(new SqlParameter("@Name", department.Name));
                        cmd.Parameters.Add(new SqlParameter("@Budget", department.Budget));

                        cmd.ExecuteNonQuery();
                    }
                }
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return View();
            }
        }

        // GET: Departments/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Departments/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Departments/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        private List<Department> GetDepartment()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name, Budget FROM Department";

                    var reader = cmd.ExecuteReader();

                    var departments = new List<Department>();

                    while (reader.Read())
                    {
                        departments.Add(new Department
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Budget = reader.GetInt32(reader.GetOrdinal("Budget"))
                        });
                    }
                    reader.Close();
                    return departments;
                }
            }
        }

        private List<Employee> GetEmployees(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.Id AS EmployeeId, e.FirstName, e.LastName 
                                        FROM Employee e 
                                        WHERE e.DepartmentId = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    var reader = cmd.ExecuteReader();

                    var employees = new List<Employee>();

                    while (reader.Read())
                    {
                        employees.Add(new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName"))
                        });
                    }
                    reader.Close();
                    return employees;
                }
            }
        }


        //private List<Employee> GetEmployeeCount(int id)
        //{
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = @"COUNT 
        //                                e.Id AS EmployeeId, e.FirstName, e.LastName
        //                               FROM Employee e 
        //                                WHERE e.DepartmentId = @id";
        //            cmd.Parameters.Add(new SqlParameter("@id", id));

        //            var reader = cmd.ExecuteReader();

        //            while (reader.Read())
        //            {
        //                employees.Add(new Employee
        //                {
        //                    Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
        //                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
        //                    LastName = reader.GetString(reader.GetOrdinal("LastName"))

        //                });
        //            }
        //            reader.Close();
        //            return employees;
        //        }
        //    }
    }
}