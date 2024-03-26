using System.Threading.Tasks;
using Waher.Runtime.Settings;

namespace TAG.Payments.Transbank
{
	/// <summary>
	/// Contains the service configuration.
	/// </summary>
	public class ServiceConfiguration
	{
		private static ServiceConfiguration current = null;

		/// <summary>
		/// Contains the service configuration.
		/// </summary>
		public ServiceConfiguration()
		{
		}

		/// <summary>
		/// Merchant ID (CLP)
		/// </summary>
		public string MerchantIdClp
		{
			get;
			private set;
		}

		/// <summary>
		/// Merchant Secret (CLP)
		/// </summary>
		public string MerchantSecretClp
		{
			get;
			private set;
		}

		/// <summary>
		/// Merchant ID (USD)
		/// </summary>
		public string MerchantIdUsd
		{
			get;
			private set;
		}

		/// <summary>
		/// Merchant Secret (USD)
		/// </summary>
		public string MerchantSecretUsd
		{
			get;
			private set;
		}

		/// <summary>
		/// Number of seconds between status polling requests.
		/// </summary>
		public int PollingIntervalSeconds
		{
			get;
			private set;
		}

		/// <summary>
		/// Number of minutes to wait before failing attempt.
		/// </summary>
		public int TimeoutMinutes
		{
			get;
			private set;
		}

		/// <summary>
		/// If the API is in production mode or test mode.
		/// </summary>
		public OperationMode Mode
		{
			get;
			private set;
		}

		/// <summary>
		/// If configuration is well-defined.
		/// </summary>
		public bool IsWellDefined
		{
			get
			{
				if (this.PollingIntervalSeconds <= 0 || this.TimeoutMinutes <= 0)
					return false;

				if (string.IsNullOrEmpty(this.MerchantIdClp) ^
					string.IsNullOrEmpty(this.MerchantSecretClp))
				{
					return false;
				}

				if (string.IsNullOrEmpty(this.MerchantIdUsd) ^
					string.IsNullOrEmpty(this.MerchantSecretUsd))
				{
					return false;
				}

				bool BothClp =
					!string.IsNullOrEmpty(this.MerchantIdClp) &&
					!string.IsNullOrEmpty(this.MerchantSecretClp);

				bool BothUsd =
					!string.IsNullOrEmpty(this.MerchantIdUsd) &&
					!string.IsNullOrEmpty(this.MerchantSecretUsd);

				return BothClp || BothUsd;
			}
		}

		/// <summary>
		/// Loads configuration settings.
		/// </summary>
		/// <returns>Configuration settings.</returns>
		public static async Task<ServiceConfiguration> Load()
		{
			ServiceConfiguration Result = new ServiceConfiguration();
			string Prefix = TransbankServiceProvider.ServiceId;

			Result.MerchantIdClp = await RuntimeSettings.GetAsync(Prefix + ".MerchantId.CLP", string.Empty);
			Result.MerchantSecretClp = await RuntimeSettings.GetAsync(Prefix + ".MerchantSecret.CLP", string.Empty);
			Result.MerchantIdUsd = await RuntimeSettings.GetAsync(Prefix + ".MerchantId.USD", string.Empty);
			Result.MerchantSecretUsd = await RuntimeSettings.GetAsync(Prefix + ".MerchantSecret.USD", string.Empty);
			Result.PollingIntervalSeconds = (int)await RuntimeSettings.GetAsync(Prefix + ".PollingIntervalSeconds", 0.0);
			Result.TimeoutMinutes = (int)await RuntimeSettings.GetAsync(Prefix + ".TimeoutMinutes", 0.0);
			Result.Mode = (OperationMode)await RuntimeSettings.GetAsync(Prefix + ".Mode", OperationMode.Test);

			return Result;
		}

		/// <summary>
		/// Gets the current configuration.
		/// </summary>
		/// <returns>Configuration</returns>
		public static async Task<ServiceConfiguration> GetCurrent()
		{
			if (current is null)
				current = await Load();

			return current;
		}

		/// <summary>
		/// Invalidates the current configuration, triggering a reload of the
		/// configuration for the next operation.
		/// </summary>
		public static void InvalidateCurrent()
		{
			current = null;
		}
	}
}
