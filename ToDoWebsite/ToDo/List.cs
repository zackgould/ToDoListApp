using System.Collections.Generic;

namespace ToDoWebsite.ToDo
{
    public class List
    {
        public List<string> myList = new List<string>();

        public void add(string item)
        {
            myList.Add(item);
        }

        public void modify(string item, string task)
        {
            var index = myList.IndexOf(item);
            myList.Remove(item);
            myList.Insert(index, task);
        }
        public void delete(string item)
        {
            myList.Remove(item);
        }

        public void sort(string[] order)
        {
            myList.Clear();
            myList.AddRange(order);
        }

    }
}

