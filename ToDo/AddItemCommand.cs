using System;
using System.Collections.Generic;

namespace ToDo
{
    public class AddItemCommand : BaseCommand
    {
        public AddItemCommand()
        {
            CommandNames = new List<string> { "a", "add" };
        }

        public override string PrintYourself()
        {
            return "Add Item - A";
        }

        public override void Execute(List todoList)
        {
            Console.Write("\nEnter Item: ");
            var userInput = Console.ReadLine();
            todoList.add(userInput);
            Console.WriteLine("\nItem Successfully Added.");
        }
    }
}