using Microsoft.VisualStudio.TestTools.UnitTesting;
using PokeUtils.Models.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeUtils.Models.Functions.Tests
{
    [TestClass()]
    public class MetamonExpFunctionProviderTests
    {
        [TestMethod()]
        public void MetamonExpFunctionProviderTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetReplyMessageTest()
        {
            var provider = new MetamonExpFunctionProvider("a");
            var reply = provider.GetReplyMessage("さみしがりなメタモンがほしい");
            Assert.AreEqual(Personarities.Samisigari, provider.RequredPersonarity);

            reply = provider.GetReplyMessage("12345");
            Assert.AreEqual(12345, provider.CurrentExp);

            reply = provider.GetReplyMessage("-45");
            Assert.AreEqual(12345, provider.CurrentExp);

            reply = provider.GetReplyMessage("ようきなメタモンがほしい");
            Assert.AreEqual(Personarities.Youki, provider.RequredPersonarity);

            reply = provider.GetReplyMessage("へんなメタモンがほしい");
            Assert.AreEqual(Personarities.Youki, provider.RequredPersonarity);
        }

        [TestMethod()]
        public void GetNecessaryExpToRequiredPersonarityTest()
        {
            Assert.AreEqual(0, MetamonExpFunctionProvider.GetNecessaryExpToRequiredPersonarity(Personarities.Ganbaruya, 0));
            Assert.AreEqual(1, MetamonExpFunctionProvider.GetNecessaryExpToRequiredPersonarity(Personarities.Samisigari, 0));
            Assert.AreEqual(24, MetamonExpFunctionProvider.GetNecessaryExpToRequiredPersonarity(Personarities.Ganbaruya, 1));
            Assert.AreEqual(24, MetamonExpFunctionProvider.GetNecessaryExpToRequiredPersonarity(Personarities.Ganbaruya, 51));
        }
    }
}