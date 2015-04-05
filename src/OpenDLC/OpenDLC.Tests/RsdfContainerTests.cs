using System;
using NUnit.Framework;
using System.Linq;

namespace OpenDLC.Tests
{
    [TestFixture]
    public class RsdfContainerTests
    {
        private static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }
        private const string MonoMessage = "Running on mono. RSDF is not supported on mono. Skipping test.";

        [Test]
        public void FromString()
        {
            if (IsRunningOnMono())
            {
                Console.WriteLine(MonoMessage);
                return;
            }

            var container = RsdfContainer.FromString("696D6C5666374861645137307A6C466C56755341654E33716D3859367635734E476B5249415649744D4C713259723861726E4F5368673D3D0D0A");
            const string sampleLink = "http://foo.example.org/rsdftest.bar";

            Assert.That(container, Is.Not.Null);
            Assert.That(container, Is.All.Not.Null);
            Assert.AreEqual(1, container.Count);
            Assert.That(container[0].Url, Is.Not.Null);
            Assert.That(container[0].Url, Is.EqualTo(sampleLink));
        }

        [Test]
        public void FromFile1()
        {
            if (IsRunningOnMono())
            {
                Console.WriteLine(MonoMessage);
                return;
            }

            // This is from Share-Links.biz
            string[] expectedLinks =
            {
                "http://example.eu",
                "http://example.com/example.pdf",
                "http://example.com/example.jpg",
                "https://example.us/example.jpg"
            };

            var fileName = TestResources.GetResourcePath("sample-container-1.rsdf");
            var container = RsdfContainer.FromFile(fileName);

            Assert.That(container, Is.Not.Null);
            Assert.That(container, Is.All.Not.Null);
            Assert.That(container, Has.Count.EqualTo(4));
            Assert.That(container.Select(l => l.Url).ToArray(), Is.EquivalentTo(expectedLinks));
        }

        [Test]
        public void FromFile2()
        {
            if (IsRunningOnMono())
            {
                Console.WriteLine(MonoMessage);
                return;
            }

            // This is from linkcrypt.ws
            string[] expectedLinks =
            {
                "http://example.eu",
                "http://example.com/example.pdf",
                "http://example.com/example.jpg",
                "https://example.us/example.jpg"
            };

            var fileName = TestResources.GetResourcePath("sample-container-2.rsdf");
            var container = RsdfContainer.FromFile(fileName);

            Assert.That(container, Is.Not.Null);
            Assert.That(container, Is.All.Not.Null);
            Assert.That(container, Has.Count.EqualTo(4));
            Assert.That(container.Select(l => l.Url).ToArray(), Is.EquivalentTo(expectedLinks));
        }

        [Test]
        public void SaveAsString()
        {
            if (IsRunningOnMono())
            {
                Console.WriteLine(MonoMessage);
                return;
            }

            var container = new RsdfContainer();
            const string destContainer = "696D6C5666374861645137307A6C466C56755341654E33716D3859367635734E476B5249415649744D4C713259723861726E4F5368673D3D0D0A";
            container.Add(new RsdfEntry("CCF: http://foo.example.org/rsdftest.bar"));
            var containerString = container.SaveAsString();

            Assert.That(containerString, Is.Not.Null);
            Assert.AreEqual(destContainer, containerString);
        }

        [Test]
        public void FromWithNullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => RsdfContainer.FromString(null));
            Assert.Throws<ArgumentNullException>(() => RsdfContainer.FromString(string.Empty));
            Assert.Throws<ArgumentNullException>(() => RsdfContainer.FromFile(null));
            Assert.Throws<ArgumentNullException>(() => RsdfContainer.FromFile(string.Empty));
            Assert.Throws<ArgumentNullException>(async () => await RsdfContainer.FromFileAsync(null));
            Assert.Throws<ArgumentNullException>(async () => await RsdfContainer.FromFileAsync(string.Empty));
            Assert.Throws<ArgumentNullException>(() => RsdfContainer.FromStream(null));
            Assert.Throws<ArgumentNullException>(async () => await RsdfContainer.FromStreamAsync(null));
        }

        [Test]
        public void SaveWithNullArgument()
        {
            var c = new RsdfContainer();
            Assert.Throws<ArgumentNullException>(() => c.SaveToFile(null));
            Assert.Throws<ArgumentNullException>(() => c.SaveToFile(string.Empty));
            Assert.Throws<ArgumentNullException>(async () => await c.SaveToFileAsync(null));
            Assert.Throws<ArgumentNullException>(async () => await c.SaveToFileAsync(string.Empty));
            Assert.Throws<ArgumentNullException>(() => c.SaveToStream(null));
            Assert.Throws<ArgumentNullException>(async () => await c.SaveToStreamAsync(null));
        }
    }
}
