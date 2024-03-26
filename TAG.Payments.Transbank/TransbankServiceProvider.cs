using Paiwise;
using System;
using System.IO;
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

		internal static TransbankClient CreateClient(ServiceConfiguration Configuration)
		{
			if (!Configuration.IsWellDefined)
				return null;

			if (xmlFileSniffer is null)
			{
				xmlFileSniffer = new XmlFileSniffer(Gateway.AppDataFolder + "Transbank" + Path.DirectorySeparatorChar +
					"Log %YEAR%-%MONTH%-%DAY%T%HOUR%.xml",
					Gateway.AppDataFolder + "Transforms" + Path.DirectorySeparatorChar + "SnifferXmlToHtml.xslt",
					7, BinaryPresentationMethod.Base64);
			}

			return new TransbankClient(Configuration.Production ? 
				TransbankClient.ProductionEnvironment : TransbankClient.IntegrationEnvironment,
				Configuration.MerchantId, Configuration.MerchantSecret, xmlFileSniffer, snifferProxy);
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
					new TransbankCardService(Config.Production, this)
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
				bool Production;

				if (ServiceId.EndsWith(".Test"))
					Production = false;
				else if (ServiceId.EndsWith(".Production"))
					Production = true;
				else
					return Task.FromResult<IBuyEDalerService>(null);

				ServiceId = ServiceId.Substring(0, ServiceId.Length - 5);

				Type T = Types.GetType(ServiceId);
				if (T is null)
					return Task.FromResult<IBuyEDalerService>(null);

				if (T.Assembly != typeof(TransbankServiceProvider).Assembly)
					return Task.FromResult<IBuyEDalerService>(null);

				if (!(Types.Instantiate(T, Production, this) is IBuyEDalerService Service))
					return Task.FromResult<IBuyEDalerService>(null);

				return Task.FromResult(Service);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return Task.FromResult<IBuyEDalerService>(null);
			}
		}

		#endregion
	}
}
