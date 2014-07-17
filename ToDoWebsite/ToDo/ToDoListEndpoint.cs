using System.Collections.Generic;
using FubuMVC.Core.Continuations;
using Raven.Client;

namespace ToDoWebsite.ToDo
{
    public class ToDoEndpoint
    {
        private readonly IDocumentSession _session;

        public ToDoEndpoint(IDocumentSession session)
        {
            _session = session;
        }

        public ToDoListViewModel get_To(ToDoListInputModel inputModel)
        {
            var person = _session.Load<Person>(inputModel.Username);

            var viewModel = new ToDoListViewModel
            {
                ToDoList = person.ToDoList.myList
            };

            return viewModel;
        }

        public FubuContinuation post_To(ToDoListPostInputModel inputModel)
        {
            var item = inputModel.AddItem;
            var username = inputModel.Username;
            var person = _session.Load<Person>(username);
            var toDoList = person.ToDoList;

            if (!toDoList.myList.Contains(item) && item != "") 
            { 
                toDoList.add(item);
            }

            return FubuContinuation.RedirectTo<ToDoListInputModel>();
        }

        public FubuContinuation post_Removed(ToDoListDeletePostInputModel inputModel)
        {
            var left = inputModel.DeleteItem;
            var username = inputModel.Username;
            var person = _session.Load<Person>(username);
            var toDoList = person.ToDoList;

            toDoList.delete(left);

            return FubuContinuation.RedirectTo<ToDoListInputModel>();
        }

        public FubuContinuation post_Edited(ToDoListEditPostInputModel inputModel)
        {
            var rightleft = inputModel.CompareItem;
            var right = inputModel.ModItem;
            var username = inputModel.Username;
            var person = _session.Load<Person>(username);
            var toDoList = person.ToDoList;
                
            if (!toDoList.myList.Contains(right) && right != "") 
            {
                toDoList.modify(rightleft, right);
            }

            return FubuContinuation.RedirectTo<ToDoListInputModel>();
        }

        public FubuContinuation post_SortList(JsonResult inputModel)
        {
            var order = inputModel.ArrayToDo;
            var username = inputModel.Username;
            var person = _session.Load<Person>(username);
            var toDoList = person.ToDoList;

            toDoList.sort(order);

            return FubuContinuation.RedirectTo<ToDoListInputModel>();
        }
    }

    public class ToDoListViewModel
    {
        public List<string> ToDoList { get; set; }
    }

    public class ToDoListInputModel : IRequireAuthentication
    {
        public string Username { get; set; }
    }

    public class ToDoListPostInputModel : IRequireAuthentication
    {
        public string AddItem { get; set; }
        public string Username { get; set; }
    }

    public class ToDoListDeletePostInputModel : IRequireAuthentication
    {
        public string DeleteItem { get; set; }
        public string Username { get; set; }
    }

    public class ToDoListEditPostInputModel : IRequireAuthentication
    {
        public string CompareItem { get; set; }
        public string ModItem { get; set; }
        public string Username { get; set; }
    }

    public class JsonResult : IRequireAuthentication
    {
        public string[] ArrayToDo { get; set; }
        public string Username { get; set; }
    }
}