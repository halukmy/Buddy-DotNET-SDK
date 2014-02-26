using System;
using NUnit.Framework;
using BuddySDK;

namespace AndroidTests
{
	[TestFixture]
	public class TestsSample
	{
		[Category("PlatformAccess User Settings")]
		[TestCase("testkey", "testvalue")]
		[TestCase("", "testvalue")]
		public void GetSet (string testKey, string testValue)
		{
			PlatformAccess.Current.ClearUserSetting (testKey);

			PlatformAccess.Current.SetUserSetting (testKey, testValue);

			var result = PlatformAccess.Current.GetUserSetting (testKey);

			Assert.AreEqual (testValue, result);
		}

		[Category("PlatformAccess User Settings")]
		[TestCase((string)null, typeof(ArgumentNullException))]
		public void GetSetNull (string testKey, Type exceptionType)
		{
			Assert.Throws (exceptionType, () => PlatformAccess.Current.SetUserSetting (testKey, null));
		}

		[Category("PlatformAccess User Settings")]
		[TestCase("testkey", "testvalue")]
		public void ExpiredFails(string testKey, string testValue)
		{
			PlatformAccess.Current.SetUserSetting (testKey, testValue, DateTime.UtcNow.AddDays(-1));

			var result = PlatformAccess.Current.GetUserSetting (testKey);

			Assert.IsNull (result);
		}

		[Test]
		[Category("PlatformAccess User Settings")]
		[TestCase("testkey", "testvalue")]
		public void ExpiredSucceeds(string testKey, string testValue)
		{
			PlatformAccess.Current.SetUserSetting (testKey, testValue, DateTime.UtcNow.AddDays(1));

			var result = PlatformAccess.Current.GetUserSetting (testKey);

			Assert.AreEqual (testValue, result);
		}

		[Test]
		[Category("PlatformAccess")]
		public void GetConfigSettings ()
		{
			var result = PlatformAccess.Current.GetConfigSetting ("test application meta-data name");

			Assert.AreEqual ("test application meta-data value", result);
		}

		[Test]
		[Category("PlatformAccess")]
		public void IsEmulator()
		{
			Assert.IsTrue (PlatformAccess.Current.IsEmulator);
		}

		[Test]
		[Category("PlatformAccess")]
		public void ConnectionType()
		{
			Assert.AreEqual (ConnectivityLevel.Carrier, PlatformAccess.Current.ConnectionType);
		}

		[Test]
		[Category("PlatformAccess")]
		public void AppVersion()
		{
			Assert.AreEqual ("1.0", PlatformAccess.Current.AppVersion);
		}

        [Test]
        [Category("PlatformAccess")]
        public void IsEmulator()
        {
            Assert.IsTrue(PlatformAccess.Current.IsEmulator);
        }
    }
}