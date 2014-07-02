using System;
using System.Collections.Generic;
using System.Linq;
using FubuMVC.Core.Continuations;
using System.IO;

namespace ToDoWebsite.ToDo
{
    public class ToDoEndpoint
    {
        private const string path = @"C:\Users\zackgoul\Desktop\ExtendHealth\C#\ToDo\ToDo.txt";

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
            var item = inputModel.Item;
            
            // This text is always added, making the file longer over time 
            string appendText = item + Environment.NewLine;
            File.AppendAllText(path, appendText);

            return FubuContinuation.RedirectTo<ToDoListInputModel>();
        }

        public FubuContinuation post_Completed(ToDoListPostInputModel inputModel)
        {
            var delete = inputModel.Item;
            
            var tempFile = Path.GetTempFileName();
            var linesToKeep = File.ReadLines(path).Where(l => l != delete);

            File.WriteAllLines(tempFile, linesToKeep);

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
        public string Item { get; set; }
    }

}