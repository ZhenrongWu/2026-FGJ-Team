using NUnit.Framework;
using UnityEditor;

namespace FGJ.Tests.EditMode.Flow
{
    public class ProductSettingsTests
    {
        private const string GameName = "Deep Sea Gambler";

        [Test]
        public void ProductName_IsGameName()
        {
            Assert.AreEqual(GameName, PlayerSettings.productName);
        }
    }
}
