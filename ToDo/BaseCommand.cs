using System.Collections.Generic;

namespace ToDo
{
    public abstract class BaseCommand
    {
        protected List<string> CommandNames;

        public abstract string PrintYourself();

        public bool IsTheRightCommand(string userInput)
        {
            var lowerCased = userInput.ToLower();
            return CommandNames.Contains(lowerCased);
        }

        public abstract void Execute(List todoList);
    }
}