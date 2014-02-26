using System;
using NUnit.Framework;
using BuddySDK;

namespace iOS
{
	[TestFixture]
	public class PlatformAccessTests
	{
		[Test]
		public void ConnectionType()
		{
			Assert.AreEqual (ConnectivityLevel.WiFi, PlatformAccess.Current.ConnectionType);
		}

        [Test]
        [Category("PlatformAccess")]
        public void IsEmulator()
        {
            Assert.IsTrue(PlatformAccess.Current.IsEmulator);
        }
    }
}