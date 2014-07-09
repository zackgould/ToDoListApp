using System.Collections.Generic;
using FubuMVC.Core.Continuations;
using FubuMVC.Core.Runtime;

namespace ToDoWebsite.ToDo
{

    public class ToDoEndpoint
    {
        private readonly ISessionState _session;
        private const string path = @"C:\Users\zackgoul\Desktop\ExtendHealth\C#\ToDo\ToDo.txt";

        public ToDoEndpoint(ISessionState session)
        {
            _session = session;
        }

        public ToDoListViewModel get_To(ToDoListInputModel inputModel)
        {
            var toDoList = _session.Get<List>();

            if (toDoList == null)
            {
                toDoList = new List();
                _session.Set(toDoList);
            }

            var viewModel = new ToDoListViewModel { ToDoList = toDoList.myList };

            return viewModel;
        }

        public FubuContinuation post_To(ToDoListPostInputModel inputModel)
        {
            var item = inputModel.AddItem;

            var toDoList = _session.Get<List>();

            toDoList.add(item);

            return FubuContinuation.RedirectTo<ToDoListInputModel>();
        }

        public FubuContinuation post_Removed(ToDoListDeletePostInputModel inputModel)
        {
            var left = inputModel.DeleteItem;

            var toDoList = _session.Get<List>();
            toDoList.delete(left);

            return FubuContinuation.RedirectTo<ToDoListInputModel>();
        }

        public FubuContinuation post_Edited(ToDoListEditPostInputModel inputModel)
        {
            var rightleft = inputModel.CompareItem;
            var right = inputModel.ModItem;

            var toDoList = _session.Get<List>();
            toDoList.modify(rightleft, right);

            return FubuContinuation.RedirectTo<ToDoListInputModel>();
        }

        public FubuContinuation post_SortList(JsonResult inputModel)
        {
            var order = inputModel.arrayToDo;

            var toDoList = _session.Get<List>();
            toDoList.sort(order);

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
