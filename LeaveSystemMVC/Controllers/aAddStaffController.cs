﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using System.Dynamic;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class aAddStaffController : Controller
    {
        // GET: aAddStaff
        [HttpGet]
        public ActionResult Index()
        {
            /*The employee model object that will be passed into the view*/
            sEmployeeModel EmptyEmployee = new sEmployeeModel();

            //Intermediary staff roles/types selection list

            //Get list of available roles
            var connectionString = 
                ConfigurationManager.ConnectionStrings["DefaultConnection"].
                ConnectionString;
            var queryString = "SELECT Role_ID, Role_Name FROM dbo.Role";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        EmptyEmployee.staffTypeSelectionOptions.Add((int)reader[0], (string)reader[1]);
                    }
                }
                connection.Close();
            }

            //We should have all role types from the database now
            //end get roles

 

            //Get all departments for selection
            queryString = "SELECT Department_ID, Department_Name FROM dbo.Department";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        EmptyEmployee.departmentList.Add((int)reader[0], (string)reader[1]);
                    }
                }
                connection.Close();
            }
            //End get departments

            //Get a list of names and ids of line manager employees
            //This will be used to select a secondary line manager for an employee
            queryString = "SELECT Employee.Employee_ID, First_Name, Last_Name " +
                              "FROM dbo.Employee " +
                              "FULL JOIN dbo.Employee_Role " +
                              "ON dbo.Employee.Employee_ID = dbo.Employee_Role.Employee_ID " +
                              "FULL JOIN dbo.Role " +
                              "ON dbo.Role.Role_ID = dbo.Employee_Role.Role_ID " +
                              "WHERE dbo.Role.Role_Name = 'LM'";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        string fullName = (string)reader[1] + " " + (string)reader[2];
                        EmptyEmployee.SecondLMSelectionOptions.Add((int)reader[0], fullName);
                    }
                }
                connection.Close();
            }

            
            //End get line mananger list

            return View(EmptyEmployee);
        }

        

        [HttpPost]
        public ActionResult Index(sEmployeeModel SE)
        {
            
            SE.password = RandomPassword.Generate(7, 7);
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "INSERT INTO dbo.Employee (Employee_ID, First_Name, " +
                        "Last_Name, User_Name, Password, Designation, Email, Gender, PH_No, " +
                        "Emp_Start_Date, Account_Status, Department_ID) VALUES('" + SE.staffID +
                        "', '" + SE.firstName + "', '" + SE.lastName + "', '" + SE.userName +
                        "', '" + SE.password + "', '" + SE.designation + "', '" + SE.email +
                        "', '" + SE.gender + "', '" + SE.phoneNo + "', '" + SE.empStartDate +
                        "', '" +  "True" + "', '" + SE.deptId + "')";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                    connection.Close();
            }
            //////////////////////////////
            string temp_email = SE.email;
            string temp_username = SE.userName;

            MailMessage message = new MailMessage();

            message.From = new MailAddress("project_ict333@murdochdubai.ac.ae", "GIMEL LMS");
            message.To.Add(new MailAddress(temp_email));
            message.Subject = "Your User Details";

            string body = "";
            body = body + "Hi, Your user details are: username: " + temp_username + " and your password is: " + LeaveSystemMVC.Models.RandomPassword.Generate(7, 7);
            message.Body = body;

            SmtpClient client = new SmtpClient();

            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("project_ict333@murdochdubai.ac.ae", "ict@333");
            client.Send(message);
            return Index();
        }

        /*Function referred to from @ http://nimblegecko.com/using-simple-drop-down-lists-in-ASP-NET-MVC/
         * Essentially encapsulates the creation of a list of SelectItem. Or a SelectList object.
         * Please note that this function is not being used because a better solution has replaced 
          it's specific use for creating dropdown lists out of dictionary objects. However, this might
          become useful later(possibly in other projects). This (especially the link) will be kept here
          essentially for future reference because it helped clear up a lot of confusions.*/
        private IEnumerable<SelectListItem> GetDropdownListItems(Dictionary<int, string> elements)
        {
            var selectList = new List<SelectListItem>();

            foreach(var element in elements)
            {
                selectList.Add(new SelectListItem
                {
                    Value = element.Key.ToString(),
                    Text = element.Value
                });
            }

            return selectList;
        }

        [AllowAnonymous]
        [HttpPost]
        public JsonResult doesUserNameExist(string userName)
        {
            
            /*
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "SELECT User_Name FROM dbo.Employee";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                bool toTerminate = false;
                using (var reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        string newText = (string)reader[0];
                        if(newText.Equals(userName))
                        {
                            user = (string)reader[0];
                            toTerminate = true;
                            break;
                        }

                    }
                }
                connection.Close();
                if(toTerminate)
                {

                }
            }*/
            if (userName == "hamza.rahimyr")
            {
                return Json("The username is already taken", JsonRequestBehavior.AllowGet);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

    }
}