using FubuMVC.Core;
using FubuMVC.Core.Continuations;

namespace ToDoWebsite.HelloWorld
{
    public class HelloWorldEndpoint
    {
        public HelloWorldViewModel get_Hello(HelloWorldInputModel inputModel)
        {
            var name = "World";

            if (inputModel.Name != null)
            {
                name = inputModel.Name;
            }

            return new HelloWorldViewModel { Name = name };
        }

        public FubuContinuation post_Hello(HelloWorldPostInputModel inputModel)
        {
            var age = inputModel.Age;

            return FubuContinuation.RedirectTo(new TestInputModel { Age = age });
        }

        public string get_TestOther_Route(TestInputModel inputModel)
        {
            return "This was just a test; your age is " + inputModel.Age;
        }
    }

    public class HelloWorldInputModel
    {
        public string Name { get; set; }
    }

    public class HelloWorldViewModel
    {
        public string Name { get; set; }
    }

    public class HelloWorldPostInputModel
    {
        public int Age { get; set; }
    }

    public class TestInputModel
    {
        [QueryString]
        public int Age { get; set; }
    }
}