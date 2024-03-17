using System.Diagnostics;
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
		public async Task Test_01_CreateTransaction_CLP()
		{
			Assert.IsNotNull(client);

			await client.CreateTransactionCLP(Guid.NewGuid().ToString()[..26],
				Guid.NewGuid().ToString(), 1000, "https://example.org/");
		}

		[TestMethod]
		public async Task Test_02_GetTransaction_CLP()
		{
			Assert.IsNotNull(client);

			string BuyOrder = Guid.NewGuid().ToString()[..26];
			string SessionId = Guid.NewGuid().ToString();

			TransactionCreationResponse Transaction =
				await client.CreateTransactionCLP(BuyOrder, SessionId, 1000, "https://example.org/");

			await client.GetTransactionStatus(Transaction.Token);
		}

		[TestMethod]
		public async Task Test_03_Redirect_WaitForApproval_CLP()
		{
			Assert.IsNotNull(client);

			string BuyOrder = Guid.NewGuid().ToString()[..26];
			string SessionId = Guid.NewGuid().ToString();

			TransactionCreationResponse Transaction =
				await client.CreateTransactionCLP(BuyOrder, SessionId, 1000, "https://example.org/");

			ProcessStartInfo StartInfo = new()
			{
				FileName = Transaction.Url + "?token_ws=" + Transaction.Token,
				UseShellExecute = true
			};

			Process.Start(StartInfo);

			TransactionInformationResponse TransactionInfo = await client.WaitForConclusion(Transaction.Token, 15);

			if (TransactionInfo.AuthorizationResponseCode.HasValue)
				Assert.AreEqual(AuthorizationResponseCodeLevel1.Approved, TransactionInfo.AuthorizationResponseCode.Value, "Transaction not approved (1).");
			else
			{
				Assert.IsTrue(TransactionInfo.Vci.HasValue, "Transaction not completed in time.");

				TransactionInfo = await client.ConfirmTransaction(Transaction.Token);

				Assert.IsTrue(TransactionInfo.AuthorizationResponseCode.HasValue);
				Assert.AreEqual(AuthorizationResponseCodeLevel1.Approved, TransactionInfo.AuthorizationResponseCode.Value, "Transaction not approved (2).");
			}
		}

		[TestMethod]
		public async Task Test_04_CreateTransaction_USD()
		{
			Assert.IsNotNull(client);

			await client.CreateTransactionUSD(Guid.NewGuid().ToString()[..26],
				Guid.NewGuid().ToString(), 9.99M, "https://example.org/");
		}

		[TestMethod]
		public async Task Test_05_GetTransaction_USD()
		{
			Assert.IsNotNull(client);

			string BuyOrder = Guid.NewGuid().ToString()[..26];
			string SessionId = Guid.NewGuid().ToString();

			TransactionCreationResponse Transaction =
				await client.CreateTransactionUSD(BuyOrder, SessionId, 9.99M, "https://example.org/"); ;

			await client.GetTransactionStatus(Transaction.Token);
		}

		[TestMethod]
		public async Task Test_06_Redirect_WaitForApproval_USD()
		{
			Assert.IsNotNull(client);

			string BuyOrder = Guid.NewGuid().ToString()[..26];
			string SessionId = Guid.NewGuid().ToString();

			TransactionCreationResponse Transaction =
				await client.CreateTransactionUSD(BuyOrder, SessionId, 9.99M, "https://example.org/");

			ProcessStartInfo StartInfo = new()
			{
				FileName = Transaction.Url + "?token_ws=" + Transaction.Token,
				UseShellExecute = true
			};

			Process.Start(StartInfo);

			TransactionInformationResponse TransactionInfo = await client.WaitForConclusion(Transaction.Token, 15);

			if (TransactionInfo.AuthorizationResponseCode.HasValue)
				Assert.AreEqual(AuthorizationResponseCodeLevel1.Approved, TransactionInfo.AuthorizationResponseCode.Value, "Transaction not approved (1).");
			else
			{
				Assert.IsTrue(TransactionInfo.Vci.HasValue, "Transaction not completed in time.");

				TransactionInfo = await client.ConfirmTransaction(Transaction.Token);

				Assert.IsTrue(TransactionInfo.AuthorizationResponseCode.HasValue);
				Assert.AreEqual(AuthorizationResponseCodeLevel1.Approved, TransactionInfo.AuthorizationResponseCode.Value, "Transaction not approved (2).");
			}
		}

		// TODO: CLP/USD
		// TODO: Refunds
		// TODO: Capture
	}
}