using BuddySDK;
using NUnit.Framework;

namespace DotNetTests
{
    [TestFixture]
    public class PlatformAccessTests
    {
        [Test]
        [Category("PlatformAccess")]
        public void IsEmulator()
        {
            Assert.IsFalse(PlatformAccess.Current.IsEmulator);
        }
    }
}
