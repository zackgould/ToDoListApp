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
using Raven.Client.Linq;

namespace ToDoWebsite.ToDo
{
    public class ActivityEndpoint
    {
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
            private readonly IDocumentSession _session;

            public LoginEndpoint(IDocumentSession session)
            {
                _session = session;
            }

            public LoginViewModel get_Login(LoginInputModel inputModel)
            {
                Collection.Clear();

                return new LoginViewModel();
            }

            public FubuContinuation post_Login(LoginPostInputModel inputModel)
            {
                var name = inputModel.Username;

                if (name == "")
                {
                    return FubuContinuation.RedirectTo<LoginInputModel>();
                }

                var password = GetStringSha1Hash(inputModel.Password);
                var user = _session.Query<Person>().Where(u => (u.Id == name || u.Name == name) && u.Password == password).ToList();

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
                var what = inputModel.Internet;

                if (what == "")
                {
                    return FubuContinuation.RedirectTo<ResetPasswordInputModel>();
                }

                var person = _session.Load<Person>(what);
                var usersName = person.Name;

                RandomNumberGenerator rng = new RNGCryptoServiceProvider();
                var bytes = new Byte[8];
                rng.GetBytes(bytes);
                String sendThisInEmailAndStoreInDb = Convert.ToBase64String(bytes);
                sendThisInEmailAndStoreInDb = GetStringSha1Hash(sendThisInEmailAndStoreInDb);

                var msg = new MailMessage {From = new MailAddress("teamtodolistapp@gmail.com")};
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
                    Credentials = new NetworkCredential("teamtodolistapp@gmail.com", "todolistapp"),
                    EnableSsl = true
                };
                smt.Send(msg);

                person.Password = sendThisInEmailAndStoreInDb;

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

                var person = _session.Load<Person>(user);

                if (person.Password == temp)
                {
                    newPass = GetStringSha1Hash(newPass);
                    person.Password = newPass;

                    return FubuContinuation.RedirectTo<LoginInputModel>();
                }

                return FubuContinuation.RedirectTo<NewPasswordInputModel>();
            }
        }

        public class RegistrationEndpoint
        {
            private readonly IDocumentSession _session;

            public RegistrationEndpoint(IDocumentSession session)
            {
                _session = session;
            }

            public RegistrationViewModel get_Register(RegistrationInputModel inputModel)
            {
                return new RegistrationViewModel();
            }

            public FubuContinuation post_Register(RegisterPostInputModel inputModel)
            {
                var user = inputModel.Name;
                var email = inputModel.Email;
                var password = GetStringSha1Hash(inputModel.Password);

                if (user == "" || email == "" || password == "")
                {
                    return FubuContinuation.RedirectTo<RegistrationInputModel>();
                }

                var person = new Person {Id = email, Name = user, Password = password};
                _session.Store(person);

                return FubuContinuation.RedirectTo<LoginInputModel>();
            }
        }

        public class ToDoEndpoint
        {
            private readonly IDocumentSession _session;

            public ToDoEndpoint(IDocumentSession session)
            {
                _session = session;
            }

            public FubuContinuation get_To(ToDoListInputModel inputModel)
            {
                var user = DecryptUsername(inputModel.Username);

                if (Collection.AllKeys.Contains(user) )
                {
                    var person = _session.Load<Person>(user);
                    string encodedEmail = EncryptUsername(user);
                    
                    var viewModel = new ToDoListViewModel
                    {
                        ToDoList = person.ToDoList.myList,
                        Username = encodedEmail
                    };

                    return FubuContinuation.TransferTo(viewModel);
                }

                return FubuContinuation.RedirectTo<LoginInputModel>();
            }

            public FubuContinuation post_To(ToDoListPostInputModel inputModel)
            {
                var item = inputModel.AddItem;
                var username = HttpContext.Current.Request.QueryString["Username"];
                var user = DecryptUsername(username);
                var person = _session.Load<Person>(user);
                var toDoList = person.ToDoList;

                if (!toDoList.myList.Contains(item) && item != "") 
                { 
                    toDoList.add(item);
                }

                return FubuContinuation.RedirectTo(new ToDoListInputModel {Username = EncryptUsername(user)});
            }

            public FubuContinuation post_Removed(ToDoListDeletePostInputModel inputModel)
            {
                var left = inputModel.DeleteItem;
                var username = HttpContext.Current.Request.QueryString["Username"];
                var user = DecryptUsername(username);
                var person = _session.Load<Person>(user);
                var toDoList = person.ToDoList;

                toDoList.delete(left);

                return FubuContinuation.RedirectTo(new ToDoListInputModel {Username = EncryptUsername(user)});
            }

            public FubuContinuation post_Edited(ToDoListEditPostInputModel inputModel)
            {
                var rightleft = inputModel.CompareItem;
                var right = inputModel.ModItem;
                var username = HttpContext.Current.Request.QueryString["Username"];
                var user = DecryptUsername(username);
                var person = _session.Load<Person>(user);
                var toDoList = person.ToDoList;
                
                if (!toDoList.myList.Contains(right) && right != "") 
                {
                    toDoList.modify(rightleft, right);
                }

                return FubuContinuation.RedirectTo(new ToDoListInputModel {Username = EncryptUsername(user)});
            }

            public FubuContinuation post_SortList(JsonResult inputModel)
            {
                var order = inputModel.ArrayToDo;
                var username = HttpContext.Current.Request.QueryString["Username"];
                var user = DecryptUsername(username);
                var person = _session.Load<Person>(user);
                var toDoList = person.ToDoList;

                toDoList.sort(order);

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