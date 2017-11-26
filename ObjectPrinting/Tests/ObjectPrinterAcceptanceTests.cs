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
                Age = 19,
                Height = 1.95,
            };
        }

		[Test]
		public void Demo()
		{
            var printer = ObjectPrinter.For<Person>()
               //1.Исключить из сериализации свойства определенного типа++
               .Excluding<Guid>()
               //2. Указать альтернативный способ сериализации для определенного типа ++
               .Printing<int>()
                   .Using(p => "hide")
               //3. Для числовых типов указать культуру
               .Printing<double>()
                   .Using(CultureInfo.InvariantCulture)
               //4. Настроить сериализацию конкретного свойства   ++
               .Printing(p => p.Name)
                   .Using(p => p + " +as")
               //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
               .Printing(p => p.Name)
                 .TrimmedToLength(10)
               //6. Исключить из сериализации конкретного свойства +++
               .Excluding(p => p.Name)
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

        [Test]
        public void PrintToString_ObjectExtensionTesting1()
        {
            var result = ObjectPrinter.For<Person>()
                .PrintToString(person);
            var extensionResult = person.PrintToString();
            extensionResult.Should().Be(result);
        }

        [Test]
        public void PrintToString_ObjectExtensionTesting2()
        {
            var result = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Printing<string>()
                    .Using(x => x.ToUpper())
                .Printing<double>()
                    .Using(CultureInfo.InvariantCulture)
                .Printing(x => x.Name)
                    .Using(x => x + "asdasd")
                .Printing(x => x.Name)
                    .TrimmedToLength(3)
                .PrintToString(person);
            var extensionResult = person.PrintToString(conf => conf
                .Excluding<Guid>()
                .Printing<string>()
                    .Using(x => x.ToUpper())
                .Printing<double>()
                    .Using(CultureInfo.InvariantCulture)
                .Printing(x => x.Name)
                    .Using(x => x + "asdasd")
                .Printing(x => x.Name)
                    .TrimmedToLength(3));
            extensionResult.Should().Be(result);
        }
        [Test]
        public void PrintToString_WithDefaultPrinterConfiguration()
        {
            var result = person.PrintToString();
            var expected =
                "Person\r\n" +
                "\tId = 00000000-0000-0000-0000-000000000000\r\n" +
                "\tName = John\r\n" +
                "\tHeight = 1,95\r\n"+
                "\tAge = 19\r\n";
            result.Should().Be(expected);
        }
        [Test]
        public void PrintToString_ExcludeStringAndGuid()
        {
            var result = person.PrintToString(conf => conf
            .Excluding<string>()
            .Excluding<Guid>());
            var expected =
                "Person\r\n" +
                "\tHeight = 1,95\r\n" +
                "\tAge = 19\r\n";
            result.Should().Be(expected);
        }
        [Test]
        public void PrintToString_ExcludeAgeProperty()
        {
            var result = person.PrintToString(conf => conf.Excluding(x => x.Age));
            var expected =
                "Person\r\n" +
                "\tId = 00000000-0000-0000-0000-000000000000\r\n" +
                "\tName = John\r\n" +
                "\tHeight = 1,95\r\n";
            result.Should().Be(expected);
        }
        [Test]
        public void PrintToString_SetCustomPrintingConfigurationForType()
        {
            var result = person.PrintToString(conf => conf.Printing<Guid>().Using(x => "007"));
            var expected =
                "Person\r\n" +
                "\tId = 007\r\n" +
                "\tName = John\r\n" +
                "\tHeight = 1,95\r\n" +
                "\tAge = 19\r\n";
            result.Should().Be(expected);
        }
        [Test]
        public void PrintToString_SetCustomPrintingConfigurationForProperty()
        {
            var result = person.PrintToString(conf => conf.Printing( x=> x.Name).Using(x => x + " Smith"));
            var expected =
                "Person\r\n" +
                "\tId = 00000000-0000-0000-0000-000000000000\r\n" +
                "\tName = John Smith\r\n" +
                "\tHeight = 1,95\r\n" +
                "\tAge = 19\r\n";
            result.Should().Be(expected);
        }
        [Test]
        public void PrintToString_NullObject()
        {
            person = null;
            var result = person.PrintToString();
            var expected ="null";
            result.Should().Be(expected);
        }

        [Test]
        public void PrintToString_ObjectWithNullProperty()
        {
            person.Name = null;
            var result = person.PrintToString();
            var expected =
                "Person\r\n" +
                "\tId = 00000000-0000-0000-0000-000000000000\r\n" +
                "\tName = null\r\n" +
                "\tHeight = 1,95\r\n" +
                "\tAge = 19\r\n";
            result.Should().Be(expected);
        }

        [Test]
        public void PrintToString_SetDigitsCulture()
        {
            var result = person.PrintToString(conf => 
                    conf.Printing<double>()
                    .Using(CultureInfo.InvariantCulture));
            var expected =
                "Person\r\n" +
                "\tId = 00000000-0000-0000-0000-000000000000\r\n" +
                "\tName = John\r\n" +
                "\tHeight = 1.95\r\n" + // double теперь с точкой
                "\tAge = 19\r\n";
            result.Should().Be(expected);
        }
        [Test]
        public void PrintToString_TrimmedStringProperty()
        {
            var result = person.PrintToString(conf =>
                conf.Printing(p => p.Name)
                .TrimmedToLength(2));
            var expected =
                "Person\r\n" +
                "\tId = 00000000-0000-0000-0000-000000000000\r\n" +
                "\tName = Jo\r\n" +// длина теперь 2
                "\tHeight = 1,95\r\n" + 
                "\tAge = 19\r\n";
            result.Should().Be(expected);
        }
        [Test]
        public void PrintToString_TrimmedStringPropertyWithCustomPrinting()
        {
            var result = person.PrintToString(conf =>
                conf
                .Printing(p => p.Name)
                    .Using(p => p.ToUpper())
                .Printing(p => p.Name)
                    .TrimmedToLength(3));
            var expected =
                "Person\r\n" +
                "\tId = 00000000-0000-0000-0000-000000000000\r\n" +
                "\tName = JOH\r\n" +// Обрезание у кастомной печати
                "\tHeight = 1,95\r\n" +
                "\tAge = 19\r\n";
            result.Should().Be(expected);
        }
    }
}