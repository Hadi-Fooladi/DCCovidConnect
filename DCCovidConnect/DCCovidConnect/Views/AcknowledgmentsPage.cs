using System.Net;
using System.Linq;

namespace DCCovidConnect.Views
{
    internal class AcknowledgmentsPage : WebViewPage
    {
        static AcknowledgmentsPage()
        {
            Body = string.Concat(Persons.Select(ToTag));

            static string ToTag(Person p) => $"<p>{WebUtility.HtmlEncode(p.ToString())}</p>";
        }

        public AcknowledgmentsPage()
        {
            SetHtmlBody(Body);
        }

        private static readonly string Body;

        private static readonly Person[] Persons =
        {
            new Person("Anha Telluri", "atelluri@gwmail.gwu.edu"),
            new Person("Jennifer Chapman", "jchapman@childrensnational.org"),
            new Person("Savita Potarazu", "savitapotarazu@gwmail.gwu.edu"),
            new Person("Alex Lin", "alexlin247@gmail.com"),
            new Person("Kevin Cleary", "kcleary@childrensnational.org"),
            new Person("Hadi Fooladi-Talari", "hfooladit@childrensnational.org"),
            new Person("Alexandra Rucker", "arucker@childrensnational.org"),
            new Person("Harleen Marwah", "hmarwah@gwmail.gwu.edu"),
            new Person("Samer Metri", "smetri@gwmail.gwu.edu"),
            new Person("Adam Munday", "amunday@gwmail.gwu.edu")
        };

        private class Person
        {
            private readonly string Name, Email;

            public Person(string name, string email)
            {
                Name = name;
                Email = email;
            }

            public override string ToString() => $"{Name} <{Email}>";
        }
    }
}
