using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using FubuMVC.Core.Continuations;
using FubuMVC.Core.Runtime;
using Raven.Client;
using Raven.Client.Document;

namespace ToDoWebsite.ToDo
{
    public class ToDoEndpoint
    {
        public static DocumentStore Store = new DocumentStore { ConnectionStringName = "RavenDB" };
        private const string path = @"C:\Users\zackgoul\Desktop\ExtendHealth\C#\ToDo\ToDo.txt";

        public ToDoListViewModel get_To(ToDoListInputModel inputModel)
        {
            Store.Initialize();
            using (IDocumentSession session = Store.OpenSession())
            {
                var toDoList = session.Load<List>("1");
                if (toDoList == null)
                {
                    toDoList = new List{Id = "1"};
                    session.Store(toDoList);
                    session.SaveChanges();
                }
                var viewModel = new ToDoListViewModel {ToDoList = toDoList.myList};
                return viewModel;
            }
        }

        public FubuContinuation post_To(ToDoListPostInputModel inputModel)
        {
            var item = inputModel.AddItem;

            using (IDocumentSession session = Store.OpenSession())
            {
                var toDoList = session.Load<List>("1");

                toDoList.add(item);
                session.Store(toDoList);
                session.SaveChanges();
            }
            return FubuContinuation.RedirectTo<ToDoListInputModel>();
        }

        public FubuContinuation post_Removed(ToDoListDeletePostInputModel inputModel)
        {
            var left = inputModel.DeleteItem;

            using (IDocumentSession session = Store.OpenSession())
            {
                var toDoList = session.Load<List>("1");

                toDoList.delete(left);
                session.Store(toDoList);
                session.SaveChanges();
            }

            return FubuContinuation.RedirectTo<ToDoListInputModel>();
        }

        public FubuContinuation post_Edited(ToDoListEditPostInputModel inputModel)
        {
            var rightleft = inputModel.CompareItem;
            var right = inputModel.ModItem;

            using (IDocumentSession session = Store.OpenSession())
            {
                var toDoList = session.Load<List>("1");

                toDoList.modify(rightleft, right);
                session.Store(toDoList);
                session.SaveChanges();
            }

            return FubuContinuation.RedirectTo<ToDoListInputModel>();
        }

        public FubuContinuation post_SortList(JsonResult inputModel)
        {
            var order = inputModel.arrayToDo;

            using (IDocumentSession session = Store.OpenSession())
            {
                var toDoList = session.Load<List>("1");

                toDoList.sort(order);
                session.Store(toDoList);
                session.SaveChanges();
            }

            return FubuContinuation.RedirectTo<ToDoListInputModel>();
        }
    }

    public class ToDoListViewModel
    {
        public List<string> ToDoList { get; set; }
    }

    public class ToDoListInputModel // this class represents the "/to" URL
    {
    }

    public class ToDoListPostInputModel
    {
        public string AddItem { get; set; }
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

    public class JsonResult  // this class represents the "/to" URL
    {
        public string[] arrayToDo { get; set; }
    }

}
