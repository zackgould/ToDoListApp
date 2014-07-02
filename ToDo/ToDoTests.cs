using System.Collections.Generic;
using NUnit.Framework;

namespace ToDo
{
    [TestFixture]
    public class ToDoTests
    {
        [Test]
        public void AddNewItem()
        {
            List test2 = new List();

            string task = "";
            test2.add(task);
            var newCount = test2.myList.Count;

            var expectedCount = 1;

            Assert.AreEqual(expectedCount, newCount);
        }

        [Test]
        public void RemoveItem()
        {
            List test2 = new List();

            int item = 1;
            string task = "";
            test2.add(task);
            test2.delete(item);
            var newCount = test2.myList.Count;
            
            var expectedCount = 0;
            
            Assert.AreEqual(expectedCount, newCount);
        }

        [Test]
        public void ModifyItem()
        {
            List test2 = new List();

            int item = 3;
            string task1 = "1";
            string task2 = "2";
            string task3 = "3";
            string tasknew = "new";
            test2.add(task1);
            test2.add(task2);
            test2.add(task3);
            test2.modify(item, tasknew);
            var newCount = test2.myList.Count;

            var expectedCount = 3;

            Assert.AreEqual(expectedCount, newCount);
            Assert.AreNotSame(task3, tasknew);
           
        }

       /* [Test]
        public void NumberingTest()
        {
            List test2 = new List();

            int item = 3;
            string input = "Wake-up";
            test2.add(input);

           // Assert.AreEqual(input);
);

        }

        */
    }

    

}
