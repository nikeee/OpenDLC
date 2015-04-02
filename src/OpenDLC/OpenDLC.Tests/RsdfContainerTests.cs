using System;
using NUnit.Framework;

namespace OpenDLC.Tests
{
    [TestFixture]
    public class RsdfContainerTests
    {
        [Test]
        public void FromString()
        {
            var container = RsdfContainer.FromString("696D6C5666374861645137307A6C466C56755341654E33716D3859367635734E476B5249415649744D4C713259723861726E4F5368673D3D0D0A");
            const string sampleLink = "http://foo.example.org/rsdftest.bar";

            Assert.That(container, Is.Not.Null);
            Assert.That(container, Is.All.Not.Null);
            Assert.AreEqual(1, container.Count);
            Assert.That(container[0].Url, Is.Not.Null);
            Assert.That(container[0].Url, Is.EqualTo(sampleLink));
        }

        [Test]
        public void SaveAsString()
        {
            var container = new RsdfContainer();
            const string destContainer = "696D6C5666374861645137307A6C466C56755341654E33716D3859367635734E476B5249415649744D4C713259723861726E4F5368673D3D0D0A";
            container.Add(new RsdfEntry("CCF: http://foo.example.org/rsdftest.bar"));
            var containerString = container.SaveAsString();

            Assert.That(containerString, Is.Not.Null);
            Assert.AreEqual(destContainer, containerString);
        }
    }
}
