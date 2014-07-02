using System;
using System.Collections.Generic;

namespace ToDo
{
    public class ModifyCommand : BaseCommand
    {
        public ModifyCommand()
        {
            CommandNames = new List<string> { "m", "mod", "modify", "modif" };
        }

        public override string PrintYourself()
        {
            return "Modify Item - M";
        }

        public override void Execute(List todoList)
        {
            Console.Write("\nEnter Item Position: ");
            int itemNumber = int.Parse(Console.ReadLine());
            Console.Write("\nModify To: ");
            string to = Console.ReadLine();
            todoList.modify(itemNumber, to);
            Console.WriteLine("\nItem Successfully Modified.");
        }
    }
}