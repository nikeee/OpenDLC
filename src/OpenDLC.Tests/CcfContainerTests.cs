using System;
using Xunit;
using System.Linq;
using System.IO;

namespace OpenDLC.Tests
{
    public class CcfContainerTests
    {
        [Fact]
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

            Assert.NotNull(container);
            Assert.All(container, i => Assert.NotNull(i));
            Assert.Single(container);

            var package = container[0];

            Assert.NotNull(package);
            Assert.All(package, i => Assert.NotNull(i));
            Assert.Equal(4, package.Count);
            Assert.Equal(expectedPassword2, package.Password);
            // Assert.That(package.Password, Is.EqualTo(expectedPassword).Or.EqualTo(expectedPassword2));
            Assert.Equal(expectedTitle, package.Name);
            Assert.Equal(expectedService, package.Service);
            Assert.Equal(expectedPackageUrl, package.Url);
            Assert.Equal(expectedPackageComment, package.Comment);

            Assert.Equal(expectedLinks, package.Select(l => l.Url).ToArray());
            Assert.All(package.Select(f => f.FileSize), i => Assert.Equal(0, (long)i)); // Share-Links sets all links to size of zero.
        }

        [Fact]
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

            Assert.NotNull(container);
            Assert.All(container, i => Assert.NotNull(i));
            Assert.Single(container);

            var package = container[0];

            Assert.NotNull(package);
            Assert.All(package, i => Assert.NotNull(i));
            Assert.Equal(4, package.Count);
            // Assert.That(package.Password, Is.EqualTo(expectedPassword).Or.EqualTo(expectedPassword2));
            Assert.Equal(expectedTitle, package.Name);
            Assert.Equal(expectedService, package.Service);
            Assert.Equal(expectedPackageUrl, package.Url);
            Assert.Equal(expectedPackageComment, package.Comment);

            Assert.Equal(expectedLinks, package.Select(l => l.Url).ToArray());
            Assert.All(package.Select(f => f.FileSize), i => Assert.Equal(0, (long)i)); // Share-Links sets all links to size of zero.
        }

        [Fact]
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

                Assert.NotNull(actualContainer);
                Assert.All(actualContainer, i => Assert.NotNull(i));
                Assert.Single(actualContainer);

                var actualPackage = actualContainer[0];

                Assert.NotNull(actualPackage);
                Assert.All(actualPackage, i => Assert.NotNull(i));
                Assert.Equal(4, actualPackage.Count);
                // Assert.That(actualPackage.Password, Is.EqualTo(expectedPassword).Or.EqualTo(expectedPassword2));
                Assert.Equal(expectedTitle, actualPackage.Name);
                Assert.Equal(expectedService, actualPackage.Service);
                Assert.Equal(expectedPackageUrl, actualPackage.Url);
                Assert.Equal(expectedPackageComment, actualPackage.Comment);

                Assert.Equal(expectedLinks, actualPackage.Select(l => l.Url).ToArray());
                Assert.All(actualPackage.Select(f => f.FileSize), i => Assert.Equal(0, (long)i));
            }
        }

        [Fact]
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

                Assert.NotNull(actualContainer);
                Assert.All(actualContainer, i => Assert.NotNull(i));
                Assert.Single(actualContainer);

                var actualPackage = actualContainer[0];

                Assert.NotNull(actualPackage);
                Assert.All(actualPackage, i => Assert.NotNull(i));
                Assert.Equal(4, actualPackage.Count);
                // Assert.That(actualPackage.Password, Is.EqualTo(expectedPassword).Or.EqualTo(expectedPassword2));
                Assert.Equal(expectedTitle, actualPackage.Name);
                Assert.Equal(expectedService, actualPackage.Service);
                Assert.Equal(expectedPackageUrl, actualPackage.Url);
                Assert.Equal(expectedPackageComment, actualPackage.Comment);

                Assert.Equal(expectedLinks, actualPackage.Select(l => l.Url).ToArray());
                Assert.All(actualPackage.Select(f => f.FileSize), i => Assert.Equal(0, (long)i));
            }
        }

        [Fact]
        public void FromWithNullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => CcfContainer.FromBuffer(null));
            Assert.Throws<ArgumentNullException>(() => CcfContainer.FromFile(null));
            Assert.Throws<ArgumentNullException>(() => CcfContainer.FromFile(string.Empty));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await CcfContainer.FromFileAsync(null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await CcfContainer.FromFileAsync(string.Empty));
            Assert.Throws<ArgumentNullException>(() => CcfContainer.FromStream(null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await CcfContainer.FromStreamAsync(null));
        }

        [Fact]
        public void SaveWithNullArgument()
        {
            var c = new CcfContainer();
            Assert.Throws<ArgumentNullException>(() => c.SaveToFile(null));
            Assert.Throws<ArgumentNullException>(() => c.SaveToFile(string.Empty));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await c.SaveToFileAsync(null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await c.SaveToFileAsync(string.Empty));
            Assert.Throws<ArgumentNullException>(() => c.SaveToStream(null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await c.SaveToStreamAsync(null));
        }
    }
}
