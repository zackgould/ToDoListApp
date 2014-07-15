using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using FubuMVC.Core;
using FubuMVC.Core.Continuations;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Linq;

namespace ToDoWebsite.ToDo
{
    public class ActivityEndpoint
    {
        public static DocumentStore Store = new DocumentStore {ConnectionStringName = "RavenDB"};
        public static HttpCookieCollection Collection = new HttpCookieCollection();

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

        internal static string EncryptUsername(string txtPassword)
        {
            byte[] passBytes = Encoding.Unicode.GetBytes(txtPassword);
            string encryptPassword = Convert.ToBase64String(passBytes);
            
            return encryptPassword;
        }

        internal static string DecryptUsername(string encryptedPassword)
        {
            byte[] passByteData = Convert.FromBase64String(encryptedPassword);
            string originalPassword = Encoding.Unicode.GetString(passByteData);
            
            return originalPassword;
        }

        public class LoginEndpoint
        {
            public LoginViewModel get_Login(LoginInputModel inputModel)
            {
                Collection.Clear();

                return new LoginViewModel();
            }

            public FubuContinuation post_Login(LoginPostInputModel inputModel)
            {
                Store.Initialize();
                IDocumentSession session = Store.OpenSession();
                var name = inputModel.Username;

                if (name == "")
                {
                    return FubuContinuation.RedirectTo<LoginInputModel>();
                }

                var password = GetStringSha1Hash(inputModel.Password);
                var user = session.Query<Person>().Where(u => (u.Id == name || u.Name == name) && u.Password == password).ToList();

                if (user.Count == 0)
                {
                    return FubuContinuation.RedirectTo<LoginInputModel>();
                }

                Collection.Add(new HttpCookie(name) {Expires = DateTime.Now.AddDays(1)});
        
                return FubuContinuation.RedirectTo(new ToDoListInputModel {Username = EncryptUsername(name)});
            }

            public ResetPasswordViewModel get_Emessage(ResetPasswordInputModel inputModel)
            {
                return new ResetPasswordViewModel();
            }

            public FubuContinuation post_Emessage(ResetPasswordPostInputModel inputModel)
            {
                Store.Initialize();
                IDocumentSession session = Store.OpenSession();
                var what = inputModel.Internet;

                if (what == "")
                {
                    return FubuContinuation.RedirectTo<ResetPasswordInputModel>();
                }

                var person = session.Load<Person>(what);
                var usersName = person.Name;

                RandomNumberGenerator rng = new RNGCryptoServiceProvider();
                var bytes = new Byte[8];
                rng.GetBytes(bytes);
                String sendThisInEmailAndStoreInDb = Convert.ToBase64String(bytes);
                sendThisInEmailAndStoreInDb = GetStringSha1Hash(sendThisInEmailAndStoreInDb);

                var msg = new MailMessage {From = new MailAddress("goulrz13@wfu.edu")};
                msg.To.Add(what);
                msg.Body = "<p>Hi there, " + usersName + "!</p> " +
                "<p>We're sorry your password needed to be reset. " +
                "In the meantime, use this temporary password to complete the reset process: " + sendThisInEmailAndStoreInDb + "</p>" +
                "<p> Thanks for being such a great user!</p> <p>Sincerely, </p> The To Do List Web Application Team";
                msg.IsBodyHtml = true;
                msg.Subject = "Your To Do List Password Reset";
                var smt = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("goulrz13@wfu.edu", "Cyclocross1#"),
                    EnableSsl = true
                };
                smt.Send(msg);

                person.Password = sendThisInEmailAndStoreInDb;
                session.SaveChanges();

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
                IDocumentSession session = Store.OpenSession();

                var person = session.Load<Person>(user);

                if (person.Password == temp)
                {
                    newPass = GetStringSha1Hash(newPass);
                    person.Password = newPass;
                    session.SaveChanges();
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
                IDocumentSession session = Store.OpenSession();

                var user = inputModel.Name;
                var email = inputModel.Email;
                var password = GetStringSha1Hash(inputModel.Password);

                if (user == "" || email == "" || password == "")
                {
                    return FubuContinuation.RedirectTo<RegistrationInputModel>();
                }

                var person = session.Load<Person>("People");
                
                if (person == null)
                {
                    person = new Person {Id = email, Name = user, Password = password};
                    session.Store(person);
                    session.SaveChanges();
                }
                return FubuContinuation.RedirectTo<LoginInputModel>();
            }
        }

        public class ToDoEndpoint
        {
            public FubuContinuation get_To(ToDoListInputModel inputModel)
            {
                Store.Initialize();
                IDocumentSession session = Store.OpenSession();

                var user = DecryptUsername(inputModel.Username);

                if (Collection.AllKeys.Contains(user) )
                {
                    var person = session.Load<Person>(user);
                    string encodedEmail = EncryptUsername(user);
                    
                    var viewModel = new ToDoListViewModel
                    {
                        ToDoList = person.myList,
                        Username = encodedEmail
                    };

                    return FubuContinuation.TransferTo(viewModel);
                }

                return FubuContinuation.RedirectTo<LoginInputModel>();
            }

            public FubuContinuation post_To(ToDoListPostInputModel inputModel)
            {
                Store.Initialize();
                IDocumentSession session = Store.OpenSession();

                var item = inputModel.AddItem;
                var cuurentRequest = HttpContext.Current.Request.Url.ToString();
                var estring = cuurentRequest.Substring(cuurentRequest.LastIndexOf('=') + 1);
                var user = DecryptUsername(estring);
                var toDoList = session.Load<Person>(user);

                if (!toDoList.myList.Contains(item) && item != "") 
                { 
                toDoList.add(item);
                session.Store(toDoList);
                session.SaveChanges();
                 }

                return FubuContinuation.RedirectTo(new ToDoListInputModel {Username = EncryptUsername(user)});
            }

            public FubuContinuation post_Removed(ToDoListDeletePostInputModel inputModel)
            {
                Store.Initialize();
                IDocumentSession session = Store.OpenSession();

                var left = inputModel.DeleteItem;
                var cuurentRequest = HttpContext.Current.Request.Url.ToString();
                var estring = cuurentRequest.Substring(cuurentRequest.LastIndexOf('=') + 1);
                var user = DecryptUsername(estring);
                var toDoList = session.Load<Person>(user);

                toDoList.delete(left);
                session.Store(toDoList);
                session.SaveChanges();

                return FubuContinuation.RedirectTo(new ToDoListInputModel {Username = EncryptUsername(user)});
            }

            public FubuContinuation post_Edited(ToDoListEditPostInputModel inputModel)
            {
                Store.Initialize();
                IDocumentSession session = Store.OpenSession();

                var rightleft = inputModel.CompareItem;
                var right = inputModel.ModItem;
                var cuurentRequest = HttpContext.Current.Request.Url.ToString();
                var estring = cuurentRequest.Substring(cuurentRequest.LastIndexOf('=') + 1);
                var user = DecryptUsername(estring);
                var toDoList = session.Load<Person>(user);
                
                if (!toDoList.myList.Contains(right) && right != "") 
                {
                toDoList.modify(rightleft, right);
                session.Store(toDoList);
                session.SaveChanges();
                }

                return FubuContinuation.RedirectTo(new ToDoListInputModel {Username = EncryptUsername(user)});
            }

            public FubuContinuation post_SortList(JsonResult inputModel)
            {
                Store.Initialize();
                IDocumentSession session = Store.OpenSession();

                var order = inputModel.ArrayToDo;
                var cuurentRequest = HttpContext.Current.Request.Url.ToString();
                var estring = cuurentRequest.Substring(cuurentRequest.LastIndexOf('=') + 1);
                var user = DecryptUsername(estring);
                var toDoList = session.Load<Person>(user);

                toDoList.sort(order);
                session.Store(toDoList);
                session.SaveChanges();

                return FubuContinuation.RedirectTo(new ToDoListInputModel {Username = EncryptUsername(user)});
            }
        }
    }

    public class ToDoListViewModel
    {
        public List<string> ToDoList { get; set; }
        public string Username { get; set; }
    }

    public class ToDoListInputModel 
    {
        [QueryString]
        public string Username { get; set; }
    }

    public class LoginViewModel
    {
    }

    public class LoginInputModel
    {
    }

    public class RegistrationViewModel 
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

    public class JsonResult 
    {
        public string[] ArrayToDo { get; set; }
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