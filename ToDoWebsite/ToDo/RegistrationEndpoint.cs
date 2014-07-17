using FubuMVC.Core.Continuations;
using Raven.Client;

namespace ToDoWebsite.ToDo
{
    public class RegistrationEndpoint
    {
        private readonly IDocumentSession _session;
        private readonly IEncryptionService _encryptionService;

        public RegistrationEndpoint(IDocumentSession session, IEncryptionService encryptionService)
        {
            _session = session;
            _encryptionService = encryptionService;
        }

        public RegistrationViewModel get_Register(RegistrationInputModel inputModel)
        {
            return new RegistrationViewModel();
        }

        public FubuContinuation post_Register(RegisterPostInputModel inputModel)
        {
            var user = inputModel.Name;
            var email = inputModel.Email;
            var password = _encryptionService.GetStringSha1Hash(inputModel.Password);

            if (user == "" || email == "" || password == "")
            {
                return FubuContinuation.RedirectTo<RegistrationInputModel>();
            }

            var person = new Person { Id = email, Name = user, Password = password };
            _session.Store(person);

            return FubuContinuation.RedirectTo<LoginInputModel>();
        }
    }

    public class RegistrationViewModel
    {
    }

    public class RegistrationInputModel
    {
    }

    public class RegisterPostInputModel
    {
        public string Password { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}