using System;
using Xunit;
using System.Linq;

namespace OpenDLC.Tests
{
    public class RsdfContainerTests
    {
        [Fact(Skip=".NET Core does not support CFB mode that is needed for RSDF")]
        public void FromString()
        {
            var container = RsdfContainer.FromString("696D6C5666374861645137307A6C466C56755341654E33716D3859367635734E476B5249415649744D4C713259723861726E4F5368673D3D0D0A");
            const string sampleLink = "http://foo.example.org/rsdftest.bar";

            Assert.NotNull(container);
            Assert.All(container, i => Assert.NotNull(i));
            Assert.Single(container);
            Assert.NotNull(container[0].Url);
            Assert.Equal(sampleLink, container[0].Url);
        }

        [Fact(Skip=".NET Core does not support CFB mode that is needed for RSDF")]
        public void FromFile1()
        {
            // This is from Share-Links.biz
            string[] expectedLinks =
            {
                "http://example.eu",
                "http://example.com/example.pdf",
                "http://example.com/example.jpg",
                "https://example.us/example.jpg",
            };

            var fileName = TestResources.GetResourcePath("sample-container-1.rsdf");
            var container = RsdfContainer.FromFile(fileName);

            Assert.NotNull(container);
            Assert.All(container, i => Assert.NotNull(i));
            Assert.Equal(4, container.Count);
            Assert.Equal(expectedLinks, container.Select(l => l.Url).ToArray());
        }

        [Fact(Skip=".NET Core does not support CFB mode that is needed for RSDF")]
        public void FromFile2()
        {
            // This is from linkcrypt.ws
            string[] expectedLinks =
            {
                "http://example.eu",
                "http://example.com/example.pdf",
                "http://example.com/example.jpg",
                "https://example.us/example.jpg",
            };

            var fileName = TestResources.GetResourcePath("sample-container-2.rsdf");
            var container = RsdfContainer.FromFile(fileName);

            Assert.NotNull(container);
            Assert.All(container, i => Assert.NotNull(i));
            Assert.Equal(4, container.Count);
            Assert.Equal(expectedLinks, container.Select(l => l.Url).ToArray());
        }

        [Fact(Skip=".NET Core does not support CFB mode that is needed for RSDF")]
        public void SaveAsString()
        {
            var container = new RsdfContainer();
            const string destContainer = "696D6C5666374861645137307A6C466C56755341654E33716D3859367635734E476B5249415649744D4C713259723861726E4F5368673D3D0D0A";
            container.Add(new RsdfEntry("CCF: http://foo.example.org/rsdftest.bar"));
            var containerString = container.SaveAsString();

            Assert.NotNull(containerString);
            Assert.Equal(destContainer, containerString);
        }

        [Fact(Skip=".NET Core does not support CFB mode that is needed for RSDF")]
        public void FromWithNullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => RsdfContainer.FromString(null));
            Assert.Throws<ArgumentNullException>(() => RsdfContainer.FromString(string.Empty));
            Assert.Throws<ArgumentNullException>(() => RsdfContainer.FromFile(null));
            Assert.Throws<ArgumentNullException>(() => RsdfContainer.FromFile(string.Empty));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await RsdfContainer.FromFileAsync(null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await RsdfContainer.FromFileAsync(string.Empty));
            Assert.Throws<ArgumentNullException>(() => RsdfContainer.FromStream(null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await RsdfContainer.FromStreamAsync(null));
        }

        [Fact(Skip=".NET Core does not support CFB mode that is needed for RSDF")]
        public void SaveWithNullArgument()
        {
            var c = new RsdfContainer();
            Assert.Throws<ArgumentNullException>(() => c.SaveToFile(null));
            Assert.Throws<ArgumentNullException>(() => c.SaveToFile(string.Empty));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await c.SaveToFileAsync(null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await c.SaveToFileAsync(string.Empty));
            Assert.Throws<ArgumentNullException>(() => c.SaveToStream(null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await c.SaveToStreamAsync(null));
        }
    }
}
