﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using System.Web.Configuration;
using FubuMVC.Core;
using FubuMVC.Core.Continuations;
using FubuMVC.Core.Runtime;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Linq;
using Raven.Database.Server.Responders;

namespace ToDoWebsite.ToDo
{
    public class ActivityEndpoint
    {
        private readonly IDocumentSession _session;
        public static DocumentStore Store = new DocumentStore {ConnectionStringName = "RavenDB"};

        internal static string GetStringSha1Hash(string text)
        {
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            using (var sha1 = new SHA1Managed())
            {
                byte[] textData = Encoding.UTF8.GetBytes(text);

                byte[] hash = sha1.ComputeHash(textData);

                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }

        public ActivityEndpoint(IDocumentSession session)
        {
            _session = session;
        }

        public class LoginEndpoint
        {
            public LoginViewModel get_Login(LoginInputModel inputModel)
            {
                return new LoginViewModel();
            }

            public FubuContinuation post_Login(LoginPostInputModel inputModel)
            {
                Store.Initialize();
                IDocumentSession _session = Store.OpenSession();

                var name = inputModel.Username;

                if (name == "")
                {
                    return FubuContinuation.RedirectTo<LoginInputModel>();

                }

                var password = GetStringSha1Hash(inputModel.Password);

                var user = _session.Query<Person>().Where(u => (u.Id == name || u.Name == name) && u.Password == password).ToList();

                // using (IDocumentSession session = Store.OpenSession())
                //{

                if (user.Count == 0)
                {
                    return FubuContinuation.RedirectTo<LoginInputModel>();
                }

               // var person = _session.Load<Person>(name);
               // var dbname = person.Name;
                
                return FubuContinuation.RedirectTo(new ToDoListInputModel {Username = name});
            }

            public ResetPasswordViewModel get_Emessage(ResetPasswordInputModel inputModel)
            {

                return new ResetPasswordViewModel();
            }

            public FubuContinuation post_Emessage(ResetPasswordPostInputModel inputModel)
            {
                Store.Initialize();
                IDocumentSession _session = Store.OpenSession();
                var what = inputModel.Internet;

                if (what == "")
                {
                    return FubuContinuation.RedirectTo<ResetPasswordInputModel>();
                }

                var person = _session.Load<Person>(what);
                var usersName = person.Name;

                RandomNumberGenerator rng = new RNGCryptoServiceProvider();
                Byte[] bytes = new Byte[8];
                rng.GetBytes(bytes);
                String sendThisInEmailAndStoreInDB = Convert.ToBase64String(bytes);
                sendThisInEmailAndStoreInDB = GetStringSha1Hash(sendThisInEmailAndStoreInDB);

                MailMessage msg = new MailMessage();
                msg.From = new MailAddress("goulrz13@wfu.edu");
                msg.To.Add(what);
                msg.Body = "<p>Hi there, " + usersName + "!</p> " +
                "<p>We're sorry your password needed to be reset. " +
                "In the meantime, use this temporary password to complete the reset process: " + sendThisInEmailAndStoreInDB + "</p>" +
                "<p> Thanks for being such a great user!</p> <p>Sincerely, </p> The To Do List Web Application Team";
                msg.IsBodyHtml = true;
                msg.Subject = "Your To Do List Password Reset";
                SmtpClient smt = new SmtpClient("smtp.gmail.com");
                smt.Port = 587;
                smt.Credentials = new NetworkCredential("goulrz13@wfu.edu", "Cyclocross1#");
                smt.EnableSsl = true;
                smt.Send(msg);

                person.Password = sendThisInEmailAndStoreInDB;
                _session.SaveChanges();

                //string script = "<script>alert('Mail Sent Successfully');self.close();</script>";
                // this.ClientScript.RegisterClientScriptBlock(this.GetType(), "sendMail", script);



                return FubuContinuation.RedirectTo(new NewPasswordInputModel());
            }

            public NewPasswordViewModel get_NewPass(NewPasswordInputModel inputModel)
            {

                return new NewPasswordViewModel();
            }

            public FubuContinuation post_NewPass(NewPasswordPostInputModel inputModel)
            {
                var user = inputModel.User;
                var temp = inputModel.TempPassword;
                var newPass = inputModel.NewPassword;

                if (user == "" || temp == "" || newPass == "")
                {
                    return FubuContinuation.RedirectTo<NewPasswordInputModel>();
                }

                Store.Initialize();
                IDocumentSession _session = Store.OpenSession();

                var person = _session.Load<Person>(user);

                if (person.Password == temp)
                {
                    newPass = GetStringSha1Hash(newPass);
                    person.Password = newPass;
                    _session.SaveChanges();
                    return FubuContinuation.RedirectTo<LoginInputModel>();
                }

                return FubuContinuation.RedirectTo<NewPasswordInputModel>();
            }

        }

        public class RegistrationEndpoint
        {

            public RegistrationViewModel get_Register(RegistrationInputModel inputModel)
            {
                return new RegistrationViewModel();
            }

            public FubuContinuation post_Register(RegisterPostInputModel inputModel)
            {
                Store.Initialize();
                IDocumentSession _session = Store.OpenSession();

                var user = inputModel.Name;
                var email = inputModel.Email;


                var password = GetStringSha1Hash(inputModel.Password);


                if (user == "" || email == "" || password == "")
                {
                    return FubuContinuation.RedirectTo<RegistrationInputModel>();
                }

                // using (IDocumentSession session = Store.OpenSession())
                //  {

                var person = _session.Load<Person>("People");
                if (person == null)
                {

                    person = new Person {Id = email, Name = user, Password = password};

                    _session.Store(person);
                    _session.SaveChanges();
                }
                //  }
                return FubuContinuation.RedirectTo<LoginInputModel>();
            }

        }

        public class ToDoEndpoint
        {
            public ToDoListViewModel get_To(ToDoListInputModel inputModel)
            {

                Store.Initialize();
                IDocumentSession _session = Store.OpenSession();

                var user = inputModel.Username;
                var person = _session.Load<Person>(user);
                string encodedEmail = Uri.EscapeDataString(user);
                //var person = _session.Load<Person>(user);
              //  var dbname = person.Name;

              //  var toDoList = _session.Load<Person>(user);

                //using (IDocumentSession session = Store.OpenSession())
                //{

                //  var toDoList = session.Load<Person>(email);
                // if (toDoList == null)
                //  {
                //    toDoList = new Person {Id = email};
                //  session.Store(toDoList);
                // session.SaveChanges();
                //   }
                var viewModel = new ToDoListViewModel
                {
                    ToDoList = person.myList,
                    Username = encodedEmail
                };
                return viewModel;
            }

            public FubuContinuation post_To(ToDoListPostInputModel inputModel)
            {
                Store.Initialize();
                IDocumentSession _session = Store.OpenSession();

                var item = inputModel.AddItem;

                var cuurentRequest = HttpContext.Current.Request.Url.ToString();

                var estring = cuurentRequest.Substring(cuurentRequest.LastIndexOf('=') + 1);

                var user = Uri.UnescapeDataString(estring);

                //using (IDocumentSession session = Store.OpenSession())
                //{
                var toDoList = _session.Load<Person>(user);

                if (!toDoList.myList.Contains(item) && item != "") { 

                toDoList.add(item);
                _session.Store(toDoList);
                _session.SaveChanges();

                 }
                return FubuContinuation.RedirectTo(new ToDoListInputModel {Username = user});
            }

            public FubuContinuation post_Removed(ToDoListDeletePostInputModel inputModel)
            {
                Store.Initialize();
                IDocumentSession _session = Store.OpenSession();

                var left = inputModel.DeleteItem;
                //var email = inputModel.Email;

                var cuurentRequest = HttpContext.Current.Request.Url.ToString();

                var estring = cuurentRequest.Substring(cuurentRequest.LastIndexOf('=') + 1);

                var user = Uri.UnescapeDataString(estring);

                // using (IDocumentSession session = Store.OpenSession())
                // {
                var toDoList = _session.Load<Person>(user);

                toDoList.delete(left);
                _session.Store(toDoList);
                _session.SaveChanges();
                //  }

                return FubuContinuation.RedirectTo(new ToDoListInputModel {Username = user});
            }

            public FubuContinuation post_Edited(ToDoListEditPostInputModel inputModel)
            {
                Store.Initialize();
                IDocumentSession _session = Store.OpenSession();

                var rightleft = inputModel.CompareItem;
                var right = inputModel.ModItem;

                var cuurentRequest = HttpContext.Current.Request.Url.ToString();

                var estring = cuurentRequest.Substring(cuurentRequest.LastIndexOf('=') + 1);

                var user = Uri.UnescapeDataString(estring);

                //  using (IDocumentSession session = Store.OpenSession())
                // {
                var toDoList = _session.Load<Person>(user);
                
                if (!toDoList.myList.Contains(right) && right != "") {
                toDoList.modify(rightleft, right);
                _session.Store(toDoList);
                _session.SaveChanges();
                  }

                return FubuContinuation.RedirectTo(new ToDoListInputModel {Username = user});
            }

            public FubuContinuation post_SortList(JsonResult inputModel)
            {
                Store.Initialize();
                IDocumentSession _session = Store.OpenSession();

                var order = inputModel.arrayToDo;

                var cuurentRequest = HttpContext.Current.Request.Url.ToString();

                var estring = cuurentRequest.Substring(cuurentRequest.LastIndexOf('=') + 1);

                var user = Uri.UnescapeDataString(estring);

                //  using (IDocumentSession session = Store.OpenSession())
                //  {
                var toDoList = _session.Load<Person>(user);

                toDoList.sort(order);
                _session.Store(toDoList);
                _session.SaveChanges();
                // }

                return FubuContinuation.RedirectTo(new ToDoListInputModel {Username = user});
            }

        }
    }

    public class ToDoListViewModel
    {
        public List<string> ToDoList { get; set; }
        public string Username { get; set; }

    }

    public class ToDoListInputModel // this class represents the "/to" URL
    {
        [QueryString]
        public string Username { get; set; }


    }

    public class LoginViewModel // this class represents the "/to" URL
    {
    }

    public class LoginInputModel
    {
    }

    public class RegistrationViewModel // this class represents the "/to" URL
    {
    }

    public class RegistrationInputModel
    {
    }

    public class ToDoListPostInputModel
    {
        public string AddItem { get; set; }

    }

    public class LoginPostInputModel
    {
        public string Password { get; set; }
        public string Username { get; set; }
    }

    public class RegisterPostInputModel
    {
        public string Password { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class ToDoListDeletePostInputModel
    {
        public string DeleteItem { get; set; }

    }

    public class ToDoListEditPostInputModel
    {
        public string CompareItem { get; set; }
        public string ModItem { get; set; }
    }

    public class JsonResult // this class represents the "/to" URL
    {
        public string[] arrayToDo { get; set; }

    }

    public class ResetPasswordInputModel
    {
    }

    public class ResetPasswordViewModel
    {
    }

    public class ResetPasswordPostInputModel
    {
        public string Internet { get; set; }
        public string User { get; set; }
    }


    public class NewPasswordPostInputModel
    {
        public string User { get; set; }
        public string TempPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class NewPasswordInputModel
    {
    }

    public class NewPasswordViewModel
    {
    }

}