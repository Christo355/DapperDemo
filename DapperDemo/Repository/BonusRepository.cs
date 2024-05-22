﻿using Dapper;
using DapperDemo.Models;
using Humanizer;
using Microsoft.Data.SqlClient;
using System.ComponentModel.Design;
using System.Data;
using System.Runtime.Intrinsics.X86;
using System.Transactions;

namespace DapperDemo.Repository
{
    public class BonusRepository : IBonusRepository
    {
        private IDbConnection db;

        public BonusRepository(IConfiguration configuration)
        {
            this.db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

        public void AddTestCompanyWithEmployees(Company company)
        {
            var sql = "INSERT INTO Companies (Name, Address, City, State, PostalCode) VALUES (@Name, @Address, @City, @State, @PostalCode);" + "SELECT CAST(SCOPE_IDENTITY() as int);";
            var id = db.Query<int>(sql, new
            {
                @Name = company.Name,
                @Address = company.Address,
                @City = company.City,
                @State = company.State,
                @PostalCode = company.PostalCode
            }).Single();
            company.CompanyId = id;

            foreach (var employee in company.Employees)
            {
                employee.CompanyId = company.CompanyId;
                var sql1 = "INSERT INTO Employees (Name, Title, Email, Phone, CompanyId) VALUES (@Name, @Title, @Email, @Phone, @CompanyId);" + "SELECT CAST(SCOPE_IDENTITY() as int);";
                db.Query<int>(sql1, employee).Single();
            }
        }

        public List<Company> GetAllCompanyWithEmployees()
        {
            var sql = "SELECT C.*, E.* FROM Employees AS E INNER JOIN Companies AS C ON E.CompanyId = C.CompanyId";

            var companyDictionary = new Dictionary<int, Company>();

            var company = db.Query<Company, Employee, Company>(sql, (c, e) =>
            {
                if (!companyDictionary.TryGetValue(c.CompanyId, out var currentCompany))
                {
                    currentCompany = c;
                    companyDictionary.Add(currentCompany.CompanyId, currentCompany);
                }
                currentCompany.Employees.Add(e);
                return currentCompany;
            }, splitOn: "EmployeeId");

            return company.Distinct().ToList();
        }



        /// <summary>
        /// Note, CetCompanyWithAddresses is a typo, it should be GetCompanyWithEmployees
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Company GetCompanyWithAddresses(int id)
        {
            var p = new
            {
                CompanyId = id
            };

            var sql = "SELECT * FROM Companies WHERE CompanyId = @CompanyId;"
           + " SELECT * FROM Employees WHERE CompanyId = @CompanyId;";

            Company company;

            using (var lists = db.QueryMultiple(sql, p))
            {
                company = lists.Read<Company>().ToList().FirstOrDefault();
                company.Employees = lists.Read<Employee>().ToList();
            }
            return company;
        }

        public List<Employee> GetEmployeWithCompany(int id)
        {
            var sql = "SELECT E.*, C.* FROM Employees AS E INNER JOIN Companies AS C ON E.CompanyId = C.CompanyId";

            if (id != 0)
            {
                sql += " WHERE E.CompanyId = @Id";
            }

            var employee = db.Query<Employee, Company, Employee>(sql, (e, c) =>
            {
                e.Company = c;
                return e;
            }, 
            new { id }, 
            splitOn: "CompanyId");
            return employee.ToList();
        }

        public void RemoveRange(int[] companyId)
        {
            db.Query("DELETE FROM Companies WHERE CompanyId IN @companyId", new {companyId});
        }

        public List<Company> FilterCompanyByName(string name)
        {
            return db.Query<Company>("SELECT * FROM Companies WHERE Name LIKE @name", new { Name = $"%{name}%" }).ToList();
        }

        public void AddTestCompanyWithEmployeesWithTransation(Company company)
        {
            using (var transation = new TransactionScope())
            {
                try
                {
                    var sql = "INSERT INTO Companies (Name, Address, City, State, PostalCode) VALUES (@Name, @Address, @City, @State, @PostalCode);" + "SELECT CAST(SCOPE_IDENTITY() as int);";
                    var id = db.Query<int>(sql, new
                    {
                        @Name = company.Name,
                        @Address = company.Address,
                        @City = company.City,
                        @State = company.State,
                        @PostalCode = company.PostalCode
                    }).Single();
                    company.CompanyId = id;

                    foreach (var employee in company.Employees)
                    {
                        employee.CompanyId = company.CompanyId;
                        var sql1 = "INSERT INTO Employees (Name, Title, Email, Phone, CompanyId) VALUES (@Name, @Title, @Email, @Phone, @CompanyId);" + "SELECT CAST(SCOPE_IDENTITY() as int);";
                        db.Query<int>(sql1, employee).Single();
                    }
                    transation.Complete();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
    }
}