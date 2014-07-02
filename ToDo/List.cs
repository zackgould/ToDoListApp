using System;
using System.Collections.Generic;

namespace ToDo
{
    public class List
    {
      public  List<string> myList = new List<string>();

        public void print()
        {
            Console.WriteLine("\nThere are {0} items in your to do list.\n", myList.Count);

            for (int i = 0; i < myList.Count; i++)
            {
                Console.WriteLine(i+1 + ". " + myList[i]);
            }
        }

        public  void add(string item)
        {
            myList.Add(item);
        }

        public  void modify(int item, string task)
        {
             myList.RemoveAt(item - 1);
             myList.Insert(item - 1, task);
        }
        public  void delete(int item)
        {
            myList.RemoveAt(item-1);
        }
       
    }
}