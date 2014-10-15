using System;
using arduino.net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ide.Backend.Tests
{
    [TestClass]
    public class ConfigSectionTests
    {
        [TestMethod]
        public void NoNameConfigSectionSerialization()
        {
            const string testFile = "TestSerial1.txt";
            var cs = new ConfigSection();
            cs["key1"] = "val1";
            cs["key3"] = "val3";
            cs.SaveToFile(testFile);

            var cs2 = ConfigSection.LoadFromFile(testFile);
            Assert.AreEqual(cs2["key1"], cs["key1"], false);
            Assert.AreEqual(cs2["key3"], cs["key3"], false);
        }

        [TestMethod]
        public void NamedConfigSectionSerialization()
        {
            const string testFile = "TestSerial1.txt";
            var cs = new ConfigSection() { Name = "base" };
            cs["key1"] = "val1";
            cs["key3"] = "val3";
            cs.SaveToFile(testFile);

            var cs2 = ConfigSection.LoadFromFile(testFile);
            Assert.AreEqual(cs2.GetSub("base")["key1"], cs["key1"], false);
            Assert.AreEqual(cs2.GetSub("base")["key3"], cs["key3"], false);
        }
    }
}
