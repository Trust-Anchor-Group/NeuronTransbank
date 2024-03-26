using Paiwise;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TAG.Networking.Transbank;
using Waher.Events;
using Waher.IoTGateway;
using Waher.Networking.HTTP;
using Waher.Networking.Sniffers;
using Waher.Persistence;
using Waher.Runtime.Inventory;

namespace TAG.Payments.Transbank
{
	/// <summary>
	/// Transbank service provider
	/// </summary>
	public class TransbankServiceProvider : IConfigurableModule, IBuyEDalerServiceProvider
	{
		/// <summary>
		/// Reference to client sniffer for Transbank communication.
		/// </summary>
		private static XmlFileSniffer xmlFileSniffer = null;

		/// <summary>
		/// Sniffable object that can be sniffed on dynamically.
		/// </summary>
		private static readonly Sniffable sniffable = new Sniffable();

		/// <summary>
		/// Sniffer proxy, forwarding sniffer events to <see cref="sniffable"/>.
		/// </summary>
		private static readonly SnifferProxy snifferProxy = new SnifferProxy(sniffable);

		/// <summary>
		/// Users are required to have this privilege in order to show and sign payments using this service.
		/// </summary>
		internal const string RequiredPrivilege = "Admin.Payments.Paiwise.Transbank";

		/// <summary>
		/// Transbank service provider
		/// </summary>
		public TransbankServiceProvider()
		{
		}

		#region IModule

		/// <summary>
		/// Starts the service.
		/// </summary>
		public Task Start()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Stops the service.
		/// </summary>
		public Task Stop()
		{
			xmlFileSniffer?.Dispose();
			xmlFileSniffer = null;

			return Task.CompletedTask;
		}

		#endregion

		#region IConfigurableModule interface

		/// <summary>
		/// Gets an array of configurable pages for the module.
		/// </summary>
		/// <returns>Configurable pages</returns>
		public Task<IConfigurablePage[]> GetConfigurablePages()
		{
			return Task.FromResult(new IConfigurablePage[]
			{
				new ConfigurablePage("Transbank", "/Transbank/Settings.md", "Admin.Payments.Paiwise.Transbank")
			});
		}

		#endregion

		#region IServiceProvider

		/// <summary>
		/// ID of service
		/// </summary>
		public string Id => ServiceId;

		/// <summary>
		/// ID of service.
		/// </summary>
		public static readonly string ServiceId = typeof(TransbankServiceProvider).Namespace;

		/// <summary>
		/// Name of service
		/// </summary>
		public string Name => "Transbank";

		/// <summary>
		/// Icon URL
		/// </summary>
		public string IconUrl => Gateway.GetUrl("/Transbank/Images/1.Webpay_FB_800.svg");

		/// <summary>
		/// Width of icon, in pixels.
		/// </summary>
		public int IconWidth => 800;

		/// <summary>
		/// Height of icon, in pixels
		/// </summary>
		public int IconHeight => 270;

		#endregion

		#region Transbank interface

		internal static TransbankClient CreateClient(ServiceConfiguration Configuration, string Currency)
		{
			if (!Configuration.IsWellDefined)
				return null;

			string MerchantId;
			string MerchantSecret;
			string Endpoint;

			switch (Currency.ToUpper())
			{
				case "CLP":
					MerchantId = Configuration.MerchantIdClp;
					MerchantSecret = Configuration.MerchantSecretClp;
					break;

				case "USD":
					MerchantId = Configuration.MerchantIdUsd;
					MerchantSecret = Configuration.MerchantSecretUsd;
					break;

				default:
					return null;
			}

			switch (Configuration.Mode)
			{
				case OperationMode.Production:
					Endpoint = TransbankClient.ProductionEnvironment;
					break;

				case OperationMode.Test:
					Endpoint = TransbankClient.IntegrationEnvironment;
					break;

				default:
					return null;
			}

			if (xmlFileSniffer is null)
			{
				xmlFileSniffer = new XmlFileSniffer(Gateway.AppDataFolder + "Transbank" + Path.DirectorySeparatorChar +
					"Log %YEAR%-%MONTH%-%DAY%T%HOUR%.xml",
					Gateway.AppDataFolder + "Transforms" + Path.DirectorySeparatorChar + "SnifferXmlToHtml.xslt",
					7, BinaryPresentationMethod.Base64);
			}

			return new TransbankClient(Endpoint, MerchantId, MerchantSecret, xmlFileSniffer, snifferProxy);
		}

		internal static void Dispose(TransbankClient Client)
		{
			Client?.Remove(xmlFileSniffer);
			Client?.Remove(snifferProxy);
			Client?.Dispose();
		}

		/// <summary>
		/// Registers a web sniffer on the Transbank client.
		/// </summary>
		/// <param name="SnifferId">Sniffer ID</param>
		/// <param name="Request">HTTP Request for sniffer page.</param>
		/// <param name="UserVariable">Name of user variable.</param>
		/// <param name="Privileges">Privileges required to view content.</param>
		/// <returns>Code to embed into page.</returns>
		public static string RegisterSniffer(string SnifferId, HttpRequest Request,
			string UserVariable, params string[] Privileges)
		{
			return Gateway.AddWebSniffer(SnifferId, Request, sniffable, UserVariable, Privileges);
		}

		#endregion

		#region IBuyEDalerServiceProvider

		/// <summary>
		/// Gets available payment services.
		/// </summary>
		/// <param name="Currency">Currency to use.</param>
		/// <param name="Country">Country where service is to be used.</param>
		/// <returns>Available payment services.</returns>
		public async Task<IBuyEDalerService[]> GetServicesForBuyingEDaler(CaseInsensitiveString Currency, CaseInsensitiveString Country)
		{
			ServiceConfiguration Config = await ServiceConfiguration.GetCurrent();

			if (Config.IsWellDefined && (Currency == "CLP" || Currency == "USD"))
			{
				return new IBuyEDalerService[]
				{
					new TransbankCardService(Config.Mode, this)
				};
			}
			else
				return new IBuyEDalerService[0];
		}

		/// <summary>
		/// Gets a payment service.
		/// </summary>
		/// <param name="ServiceId">Service ID</param>
		/// <param name="Currency">Currency to use.</param>
		/// <param name="Country">Country where service is to be used.</param>
		/// <returns>Service, if found, null otherwise.</returns>
		public Task<IBuyEDalerService> GetServiceForBuyingEDaler(string ServiceId, CaseInsensitiveString Currency, CaseInsensitiveString Country)
		{
			try
			{
				int i = ServiceId.LastIndexOf('.');
				if (i < 0 || !Enum.TryParse(ServiceId.Substring(i + 1), out OperationMode Mode))
					return Task.FromResult<IBuyEDalerService>(null);

				ServiceId = ServiceId.Substring(0, i);

				Type T = Types.GetType(ServiceId);
				if (T is null)
					return Task.FromResult<IBuyEDalerService>(null);

				if (T.Assembly != typeof(TransbankServiceProvider).Assembly)
					return Task.FromResult<IBuyEDalerService>(null);

				if (!(Types.Instantiate(T, Mode, this) is IBuyEDalerService Service))
					return Task.FromResult<IBuyEDalerService>(null);

				return Task.FromResult(Service);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return Task.FromResult<IBuyEDalerService>(null);
			}
		}

		/// <summary>
		/// Gets the result of a transaction.
		/// </summary>
		/// <param name="TransactionToken">Transaction token.</param>
		/// <param name="Currency">Currency of transaction.</param>
		/// <returns>Result of transaction.</returns>
		public static async Task<AuthorizationResponseCodeLevel1> GetTransactionResult(string TransactionToken, string Currency)
		{
			ServiceConfiguration Configuration = await ServiceConfiguration.GetCurrent();
			if (!Configuration.IsWellDefined)
				throw new Exception("Service not configured properly.");

			TransbankClient Client = CreateClient(Configuration, Currency)
				?? throw new Exception("Service not configured properly.");

			TransactionInformationResponse TransactionInfo = await Client.GetTransactionStatus(TransactionToken);

			return TransactionInfo.AuthorizationResponseCode ?? AuthorizationResponseCodeLevel1.Other;
		}

		/// <summary>
		/// Is called if the user cancels the transaction.
		/// </summary>
		/// <param name="TransactionToken">Transaction Token</param>
		/// <param name="BuyOrder">Buy Order</param>
		/// <param name="SessionId">Session ID</param>
		/// <param name="Currency">Currency of transaction.</param>
		public static void CancelProcess(string TransactionToken, string BuyOrder, string SessionId, string Currency)
		{
			Unregister(TransactionToken, BuyOrder, SessionId, Currency, true);
		}

		internal static void Register(string TransactionToken, string BuyOrder, string SessionId, string Currency, CancellationTokenSource CancellationToken)
		{
			lock (transactions)
			{
				transactions[TransactionToken] = new TransactionInfo()
				{
					BuyOrder = BuyOrder,
					SessionId = SessionId,
					Currency = Currency,
					CancellationToken = CancellationToken
				};
			}
		}

		internal static bool Unregister(string TransactionToken, string BuyOrder, string SessionId, string Currency, bool CancelIfFound)
		{
			lock (transactions)
			{
				if (!transactions.TryGetValue(TransactionToken, out TransactionInfo Info) ||
					Info.BuyOrder != BuyOrder ||
					Info.SessionId != SessionId ||
					Info.Currency != Currency)
				{
					return false;
				}

				transactions.Remove(TransactionToken);

				if (CancelIfFound)
				{
					try
					{
						Info.CancellationToken.Cancel();
					}
					catch (Exception)
					{
						// Ignore
					}
				}

				return true;
			}
		}

		private class TransactionInfo
		{
			public CancellationTokenSource CancellationToken;
			public string BuyOrder;
			public string SessionId;
			public string Currency;
		}

		private readonly static Dictionary<string, TransactionInfo> transactions = new Dictionary<string, TransactionInfo>();

		#endregion
	}
}
