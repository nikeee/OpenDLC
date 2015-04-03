using System;
using NUnit.Framework;
using System.Linq;
using System.IO;

namespace OpenDLC.Tests
{
    [TestFixture]
    public class CcfContainerTests
    {
        [Test]
        public void FromFile1()
        {
            // This is from Share-Links.biz
            string[] expectedLinks =
            {
                "http://example.eu",
                "http://example.com/example.pdf",
                "http://example.com/example.jpg",
                "https://example.us/example.jpg"
            };
            const string expectedTitle = "This is the title";
            const string expectedService = "";
            const string expectedPassword = null;
            const string expectedPassword2 = "";
            const string expectedPackageUrl = "Directlinks";
            const string expectedPackageComment = "Created on Share-Links.biz";

            var fileName = TestResources.GetResourcePath("sample-container-1.ccf");
            var container = CcfContainer.FromFile(fileName);

            Assert.That(container, Is.Not.Null);
            Assert.That(container, Is.All.Not.Null);
            Assert.That(container, Has.Count.EqualTo(1));

            var package = container[0];

            Assert.That(package, Is.Not.Null);
            Assert.That(package, Is.All.Not.Null);
            Assert.That(package, Has.Count.EqualTo(4));
            Assert.That(package.Password, Is.EqualTo(expectedPassword).Or.EqualTo(expectedPassword2));
            Assert.That(package.Name, Is.EqualTo(expectedTitle));
            Assert.That(package.Service, Is.EqualTo(expectedService));
            Assert.That(package.Url, Is.EqualTo(expectedPackageUrl));
            Assert.That(package.Comment, Is.EqualTo(expectedPackageComment));

            Assert.That(package.Select(l => l.Url).ToArray(), Is.EquivalentTo(expectedLinks));
            Assert.That(package.Select(f => f.FileSize), Has.All.EqualTo(0)); // Share-Links sets all links to size of zero.
        }
        [Test]
        public void FromFile2()
        {
            // This is from linkcrypt.ws
            string[] expectedLinks =
            {
                "http://example.eu",
                "http://example.com/example.pdf",
                "http://example.com/example.jpg",
                "https://example.us/example.jpg"
            };
            const string expectedTitle = "k4b98932quv7dm9";
            const string expectedService = "";
            const string expectedPassword = null;
            const string expectedPassword2 = "";
            const string expectedPackageUrl = "Directlinks";
            const string expectedPackageComment = "Erstellt von Linkcrypt.ws";

            var fileName = TestResources.GetResourcePath("sample-container-2.ccf");
            var container = CcfContainer.FromFile(fileName);

            Assert.That(container, Is.Not.Null);
            Assert.That(container, Is.All.Not.Null);
            Assert.That(container, Has.Count.EqualTo(1));

            var package = container[0];

            Assert.That(package, Is.Not.Null);
            Assert.That(package, Is.All.Not.Null);
            Assert.That(package, Has.Count.EqualTo(4));
            Assert.That(package.Password, Is.EqualTo(expectedPassword).Or.EqualTo(expectedPassword2));
            Assert.That(package.Name, Is.EqualTo(expectedTitle));
            Assert.That(package.Service, Is.EqualTo(expectedService));
            Assert.That(package.Url, Is.EqualTo(expectedPackageUrl));
            Assert.That(package.Comment, Is.EqualTo(expectedPackageComment));

            Assert.That(package.Select(l => l.Url).ToArray(), Is.EquivalentTo(expectedLinks));
            Assert.That(package.Select(f => f.FileSize), Has.All.EqualTo(0)); // Share-Links sets all links to size of zero.
        }

        [Test]
        public void SaveToStream1()
        {
            string[] expectedLinks =
            {
                "http://example.eu",
                "http://example.com/example.pdf",
                "http://example.com/example.jpg",
                "https://example.us/example.jpg"
            };
            const string expectedTitle = "This is the title";
            const string expectedService = "";
            const string expectedPassword = null;
            const string expectedPassword2 = "";
            const string expectedPackageUrl = "Directlinks";
            const string expectedPackageComment = "Created on Share-Links.biz";

            var container = new CcfContainer();
            var package = new CcfPackage();
            expectedLinks.ToList().ForEach(link => package.Add(new CcfEntry(link, 0, null)));
            package.Name = expectedTitle;
            package.Password = expectedPassword;
            package.Service = expectedService;
            package.Url = expectedPackageUrl;
            package.Comment = expectedPackageComment;
            container.Add(package);

            using (var ms = new MemoryStream())
            {
                container.SaveToStream(ms);

                var actualContainer = CcfContainer.FromStream(ms);

                Assert.That(actualContainer, Is.Not.Null);
                Assert.That(actualContainer, Is.All.Not.Null);
                Assert.That(actualContainer, Has.Count.EqualTo(1));

                var actualPackage = actualContainer[0];

                Assert.That(actualPackage, Is.Not.Null);
                Assert.That(actualPackage, Is.All.Not.Null);
                Assert.That(actualPackage, Has.Count.EqualTo(4));
                Assert.That(actualPackage.Password, Is.EqualTo(expectedPassword).Or.EqualTo(expectedPassword2));
                Assert.That(actualPackage.Name, Is.EqualTo(expectedTitle));
                Assert.That(actualPackage.Service, Is.EqualTo(expectedService));
                Assert.That(actualPackage.Url, Is.EqualTo(expectedPackageUrl));
                Assert.That(actualPackage.Comment, Is.EqualTo(expectedPackageComment));

                Assert.That(actualPackage.Select(l => l.Url).ToArray(), Is.EquivalentTo(expectedLinks));
                Assert.That(actualPackage.Select(f => f.FileSize), Has.All.EqualTo(0));
            }
        }

        [Test]
        public void SaveToStream2()
        {
            string[] expectedLinks =
            {
                "http://example.eu",
                "http://example.com/example.pdf",
                "http://example.com/example.jpg",
                "https://example.us/example.jpg"
            };
            const string expectedTitle = "k4b98932quv7dm9";
            const string expectedService = "";
            const string expectedPassword = null;
            const string expectedPassword2 = "";
            const string expectedPackageUrl = "Directlinks";
            const string expectedPackageComment = "Erstellt von Linkcrypt.ws";

            var container = new CcfContainer();
            var package = new CcfPackage();
            expectedLinks.ToList().ForEach(link => package.Add(new CcfEntry(link, 0, null)));
            package.Name = expectedTitle;
            package.Password = expectedPassword;
            package.Service = expectedService;
            package.Url = expectedPackageUrl;
            package.Comment = expectedPackageComment;
            container.Add(package);

            using (var ms = new MemoryStream())
            {
                container.SaveToStream(ms);

                var actualContainer = CcfContainer.FromStream(ms);

                Assert.That(actualContainer, Is.Not.Null);
                Assert.That(actualContainer, Is.All.Not.Null);
                Assert.That(actualContainer, Has.Count.EqualTo(1));

                var actualPackage = actualContainer[0];

                Assert.That(actualPackage, Is.Not.Null);
                Assert.That(actualPackage, Is.All.Not.Null);
                Assert.That(actualPackage, Has.Count.EqualTo(4));
                Assert.That(actualPackage.Password, Is.EqualTo(expectedPassword).Or.EqualTo(expectedPassword2));
                Assert.That(actualPackage.Name, Is.EqualTo(expectedTitle));
                Assert.That(actualPackage.Service, Is.EqualTo(expectedService));
                Assert.That(actualPackage.Url, Is.EqualTo(expectedPackageUrl));
                Assert.That(actualPackage.Comment, Is.EqualTo(expectedPackageComment));

                Assert.That(actualPackage.Select(l => l.Url).ToArray(), Is.EquivalentTo(expectedLinks));
                Assert.That(actualPackage.Select(f => f.FileSize), Has.All.EqualTo(0));
            }
        }

        [Test]
        public void FromWithNullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => CcfContainer.FromBuffer(null));
            Assert.Throws<ArgumentNullException>(() => CcfContainer.FromFile(null));
            Assert.Throws<ArgumentNullException>(() => CcfContainer.FromFile(string.Empty));
            Assert.Throws<ArgumentNullException>(async () => await CcfContainer.FromFileAsync(null));
            Assert.Throws<ArgumentNullException>(async () => await CcfContainer.FromFileAsync(string.Empty));
            Assert.Throws<ArgumentNullException>(() => CcfContainer.FromStream(null));
            Assert.Throws<ArgumentNullException>(async () => await CcfContainer.FromStreamAsync(null));
        }
    }
}
