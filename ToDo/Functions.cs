using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace ToDo
{
    public class Functions
    {
       //Instance Creation
       static List list = new List();
       static List<BaseCommand> commands = new List<BaseCommand>
       {
           new AddItemCommand(),
           new DeleteCommand(),
           new ModifyCommand(),
           new PrintCommand()
       }; 
       
        //myChoice
        static string myChoice = "";

        public static void Main()
        {
            PrintInstructions();

            while (myChoice.ToLower() != "q")
            {
                myChoice = Console.ReadLine();

                if (myChoice.ToLower() == "o")
                {
                    Console.WriteLine();
                    foreach (var command in commands)
                    {
                        Console.WriteLine(command.PrintYourself());
                    }
                    Console.WriteLine("Waiting for your command:");
                }
                else if (myChoice.ToLower() != "q")
                {
                    try
                    {
                        var theRightCommand = commands.First(x => x.IsTheRightCommand(myChoice));
                        theRightCommand.Execute(list);
                        list.print();
                        Console.WriteLine("\nWhat would you like to do?");
                        Console.Write("\nType the letter O to be reminded of options: ");
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("\nInvalid Command.\n");
                        PrintInstructions();
                    }

                }


            }

        }

        private static void PrintInstructions()
        {
            Console.WriteLine("What would you like to do?");
            Console.Write("\nType the letter O for options: ");
        }

    
    }
}

