using System.Collections.Generic;

namespace ToDoWebsite.ToDo
{
    public class Person
    {
        public Person()
        {
            ToDoList = new List();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public List ToDoList { get; set; }
    }
}

