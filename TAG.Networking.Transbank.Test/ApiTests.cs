using System.Text;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.Sniffers;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Runtime.Inventory;
using Waher.Runtime.Inventory.Loader;
using Waher.Runtime.Settings;

namespace TAG.Networking.Transbank.Test
{
	[TestClass]
	public class ApiTests
	{
		private static TransbankClient? client;

		[AssemblyInitialize]
		public static async Task AssemblyInitialize(TestContext _)
		{
			// Create inventory of available classes.
			TypesLoader.Initialize();

			// Register console event log
			Log.Register(new ConsoleEventSink(true, true));

			// Instantiate local encrypted object database.
			FilesProvider DB = await FilesProvider.CreateAsync(Path.Combine(Directory.GetCurrentDirectory(), "Data"), "Default",
				8192, 10000, 8192, Encoding.UTF8, 10000, true, false);

			await DB.RepairIfInproperShutdown(string.Empty);

			Database.Register(DB);

			// Start embedded modules (database lifecycle)

			await Types.StartAllModules(60000);
		}

		[AssemblyCleanup]
		public static async Task AssemblyCleanup()
		{
			Log.Terminate();
			await Types.StopAllModules();
		}

		[ClassInitialize]
		public static async Task ClassInitialize(TestContext _)
		{
			string MerchantID = await RuntimeSettings.GetAsync("Transbank.Merchant.Code", IntegrationMerchantCodes.WebpayPlus);
			string MerchantSecret = await RuntimeSettings.GetAsync("Transbank.Merchant.Secret", IntegrationMerchantCodes.SecretKey);

			client = new TransbankClient(TransbankClient.IntegrationEnvironment, MerchantID, MerchantSecret,
				new ConsoleOutSniffer(BinaryPresentationMethod.Base64, LineEnding.NewLine));
		}

		[ClassCleanup]
		public static void ClassCleanup()
		{
			client?.Dispose();
			client = null;
		}

		[TestMethod]
		public async Task Test_01_CreateTransaction()
		{
			Assert.IsNotNull(client);

			await client.CreateTransaction(Guid.NewGuid().ToString().Substring(0, 26), 
				Guid.NewGuid().ToString(), 10, "https://example.org/");
		}

	}
}