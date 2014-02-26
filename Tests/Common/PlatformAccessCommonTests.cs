using System;
using NUnit.Framework;
using BuddySDK;

namespace DotNETTests
{
	[TestFixture]
	public class PlatformAccessCommonTests
	{
		[Category("User Settings")]
		[TestCase("testkey", "testvalue")]
		[TestCase("", "testvalue")]
		public void GetSet (string testKey, string testValue)
		{
			PlatformAccess.Current.ClearUserSetting (testKey);

			PlatformAccess.Current.SetUserSetting (testKey, testValue);

			var result = PlatformAccess.Current.GetUserSetting (testKey);

			Assert.AreEqual (testValue, result);
		}

		[Category("User Settings")]
		[TestCase((string)null, typeof(ArgumentNullException))]
		public void GetSetNull (string testKey, Type exceptionType)
		{
			Assert.Throws (exceptionType, () => PlatformAccess.Current.SetUserSetting (testKey, null));
		}

		[Category("User Settings")]
		[TestCase("testkey", "testvalue")]
		public void ExpiredFails(string testKey, string testValue)
		{
			PlatformAccess.Current.SetUserSetting (testKey, testValue, DateTime.UtcNow.AddDays(-1));

			var result = PlatformAccess.Current.GetUserSetting (testKey);

			Assert.IsNull (result);
		}

		[Category("User Settings")]
		[TestCase("testkey", "testvalue")]
		public void ExpiredSucceeds(string testKey, string testValue)
		{
			PlatformAccess.Current.SetUserSetting (testKey, testValue, DateTime.UtcNow.AddDays(1));

			var result = PlatformAccess.Current.GetUserSetting (testKey);

			Assert.AreEqual (testValue, result);
		}

		[Test]
		public void GetConfigSettings ()
		{
			var result = PlatformAccess.Current.GetConfigSetting ("test config setting name");

			Assert.AreEqual ("test config setting value", result);
		}
	}
}