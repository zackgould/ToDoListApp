using System;
using System.Collections.Generic;

namespace ToDo
{
    public class DeleteCommand : BaseCommand
    {
        public DeleteCommand()
        {
            CommandNames = new List<string> { "d", "delete", "del", "remove" };
        }

        public override string PrintYourself()
        {
            return "Delete Item - D";
        }

        public override void Execute(List todoList)
        {
            Console.Write("\nEnter Item Position: ");
            var itemPosition = int.Parse(Console.ReadLine());
            todoList.delete(itemPosition);
            Console.WriteLine("\nItem Successfully Deleted.");
        }
    }
}