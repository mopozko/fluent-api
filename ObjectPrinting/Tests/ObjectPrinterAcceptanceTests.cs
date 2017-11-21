using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
	[TestFixture]
	public class ObjectPrinterAcceptanceTests
    {
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person
            {
                Name = "John",
                Surname = "Smith",
                Age = 19,
                Height = 1.95,
                Parent = new Person()
                {
                    Age = 45,
                    Name = "Mary",
                    Surname = "Smith",
                    Height = 1.65
                }
            };
        }

		[Test]
		public void Demo()
		{
            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа   ++
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа ++
                .Printing<int>()
                    .Using(p => "hide")
                //3. Для числовых типов указать культуру
                .Printing<double>()
                    .Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства   ++
                .Printing(p => p.Name)
                    .Using(p => p + " +")
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Surname)
                  .TrimmedToLength(4)
                //6. Исключить из сериализации конкретного свойства +++
                .Excluding(p => p.Surname)
                ;

            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию ++
            string s2 = person.PrintToString();

            //8. ...с конфигурированием ++
            string s3 = person.PrintToString(s => s.Excluding(p => p.Age));
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
		}




    }
}