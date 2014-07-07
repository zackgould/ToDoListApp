using System;
using System.Linq;
using FubuMVC.Core.Continuations;
using System.IO;
using FubuMVC.Core.Runtime;

namespace ToDoWebsite.ToDo
{

    public class ToDoEndpoint
    {
        private readonly IFubuRequest _fubuRequest;
        private const string path = @"C:\Users\zackgoul\Desktop\ExtendHealth\C#\ToDo\ToDo.txt";

        public ToDoEndpoint(IFubuRequest fubuRequest)
        {
            _fubuRequest = fubuRequest;
        }

        public ToDoListViewModel get_To(ToDoListInputModel inputModel)
        {
            //Create new file unless one already exists
            if (!File.Exists(path))
            {
                // Create a file to write to. 
                string createText = String.Empty;
                File.WriteAllText(path, createText);
            }
            var toDoLines = File.ReadAllLines(path);

            // this...
            //var viewModel = new ToDoListViewModel();
            //viewModel.ToDoList = fileText;

            // is the same as this...
            var viewModel = new ToDoListViewModel { ToDoList = toDoLines };

            return viewModel;
        }

        public FubuContinuation post_To(ToDoListPostInputModel inputModel)
        {
            var item = inputModel.DeleteItem;
            var linesToAdd = File.ReadAllLines(path).ToList();
            
            if (!linesToAdd.Contains(item))
            {
                // This text is always added, making the file longer over time 
                string appendText = item + Environment.NewLine;
                File.AppendAllText(path, appendText);
            }
            return FubuContinuation.RedirectTo<ToDoListInputModel>();
        }

        public FubuContinuation post_Removed(ToDoListPostInputModel inputModel)
        {
                var left = inputModel.DeleteItem;
           
                var tempFile = Path.GetTempFileName();

                var linesToKeep = File.ReadLines(path).Where(l => l != left);

                File.WriteAllLines(tempFile, linesToKeep);

                File.Delete(path);
                File.Move(tempFile, path);
            
           

            return FubuContinuation.RedirectTo<ToDoListInputModel>();
        }

        public FubuContinuation post_Edited(ToDoListPostInputModel inputModel)
        {
            var rightleft = inputModel.CompareItem;
            var right = inputModel.ModItem;
            var linesToReplace = File.ReadAllLines(path).ToList();

            if (!linesToReplace.Contains(right))
            {
               
                var tempFile = Path.GetTempFileName();
                var index = linesToReplace.IndexOf(rightleft);
                linesToReplace.RemoveAt(index);
                linesToReplace.Insert(index, right);
                File.WriteAllLines(tempFile, linesToReplace);
                File.Delete(path);
                File.Move(tempFile, path);
            }

            return FubuContinuation.RedirectTo<ToDoListInputModel>();
        }

        public FubuContinuation post_SortList(JsonResult inputModel)
        {
            var tempFile = Path.GetTempFileName();
           var listItems = inputModel.arrayToDo;
           // Persist to file
           File.WriteAllLines(tempFile, listItems);

           File.Delete(path);
           File.Move(tempFile, path);

           return FubuContinuation.RedirectTo<ToDoListInputModel>();
        }

    }

    public class ToDoListViewModel
    {
        public string[] ToDoList { get; set; }
    }

    public class ToDoListInputModel // this class represents the "/to" URL
    {
    }

     public class ToDoListPostInputModel
    {
        public string DeleteItem { get; set; }
        public string CompareItem { get; set; }
        public string ModItem { get; set; }

    }

     public class JsonResult  // this class represents the "/to" URL
     {
         public string[] arrayToDo { get; set; }
     }

}