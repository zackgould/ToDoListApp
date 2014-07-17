using System;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using FubuMVC.Core.Continuations;
using Raven.Client;

namespace ToDoWebsite.ToDo
{
    public class ResetPasswordEndpoint
    {
         private readonly IDocumentSession _session;
        private readonly IEncryptionService _encryptionService;

        public ResetPasswordEndpoint(IDocumentSession session, IEncryptionService encryptionService)
        {
            _session = session;
            _encryptionService = encryptionService;
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
            sendThisInEmailAndStoreInDb = _encryptionService.GetStringSha1Hash(sendThisInEmailAndStoreInDb);

            var msg = new MailMessage { From = new MailAddress("teamtodolistapp@gmail.com") };
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

            return FubuContinuation.RedirectTo<NewPasswordInputModel>();
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
                newPass = _encryptionService.GetStringSha1Hash(newPass);
                person.Password = newPass;

                return FubuContinuation.RedirectTo<LoginInputModel>();
            }

            return FubuContinuation.RedirectTo<NewPasswordInputModel>();
        }
    }

    public class ResetPasswordViewModel
    {
    }

    public class ResetPasswordInputModel
    {
    }

    public class ResetPasswordPostInputModel
    {
        public string Internet { get; set; }
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