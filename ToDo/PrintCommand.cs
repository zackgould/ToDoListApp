using System;
using System.Collections.Generic;

namespace ToDo
{
    public class PrintCommand : BaseCommand
    {
        public PrintCommand()
        {
            CommandNames = new List<string> { "p", "print", "pr", "pt" };
        }

        public override string PrintYourself()
        {
            return "Print Item - P";
        }

        public override void Execute(List todoList)
        {
            todoList.print();
        }
    }
}