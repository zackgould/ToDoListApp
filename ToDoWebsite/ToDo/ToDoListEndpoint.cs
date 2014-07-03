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

        public FubuContinuation post_Removed(ToDoListPostInputModel inputModel)
        {
                var left = inputModel.Item;
           
                var tempFile = Path.GetTempFileName();

                var linesToKeep = File.ReadLines(path).Where(l => l != left);

                File.WriteAllLines(tempFile, linesToKeep);

                File.Delete(path);
                File.Move(tempFile, path);
            
           

            return FubuContinuation.RedirectTo<ToDoListInputModel>();
        }

        public FubuContinuation post_Edited(ToDoListPostInputModel inputModel)
        {
            var left = inputModel.Item;
            var right = inputModel.Mod;

            var tempFile = Path.GetTempFileName();
            if (left != right)
            {

                var linesToReplace = File.ReadAllLines(path).ToList();
                var index = linesToReplace.IndexOf(left);
                linesToReplace.Remove(left);
                linesToReplace.Insert(index, right);
                File.WriteAllLines(tempFile, linesToReplace);

                File.Delete(path);
                File.Move(tempFile, path);
            }
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
        public string Mod { get; set; }

    }

}