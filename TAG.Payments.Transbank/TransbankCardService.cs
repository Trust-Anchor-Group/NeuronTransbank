using Paiwise;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using TAG.Networking.Transbank;
using Waher.IoTGateway;
using Waher.Persistence;
using Waher.Runtime.Inventory;

namespace TAG.Payments.Transbank
{
	/// <summary>
	/// Card payment service
	/// </summary>
	public class TransbankCardService : IBuyEDalerService, IPaymentService
	{
		private readonly TransbankServiceProvider provider;
		private readonly OperationMode mode;

		/// <summary>
		/// Transbank Card Service
		/// </summary>
		/// <param name="Mode">In what mode the service runs.</param>
		/// <param name="Provider">Reference to the service provider.</param>
		public TransbankCardService(OperationMode Mode, TransbankServiceProvider Provider)
		{
			this.mode = Mode;
			this.provider = Provider;
		}

		#region IServiceProvider

		/// <summary>
		/// ID of service
		/// </summary>
		public string Id => ServiceId + "." + this.mode.ToString();

		/// <summary>
		/// ID of service.
		/// </summary>
		public static string ServiceId = typeof(TransbankCardService).FullName;

		/// <summary>
		/// Name of service
		/// </summary>
		public string Name => "Webpay (transbank)";

		/// <summary>
		/// Icon URL
		/// </summary>
		public string IconUrl => Gateway.GetUrl("/Transbank/Images/1.Webpay_FB_800.svg");

		/// <summary>
		/// Width of icon, in pixels.
		/// </summary>
		public int IconWidth => 300;    // A lot of white-space to the sides.

		/// <summary>
		/// Height of icon, in pixels
		/// </summary>
		public int IconHeight => 270;

		#endregion

		#region IProcessingSupport<CaseInsensitiveString>

		/// <summary>
		/// How well a currency is supported
		/// </summary>
		/// <param name="Currency">Currency</param>
		/// <returns>Support</returns>
		public Grade Supports(CaseInsensitiveString Currency)
		{
			return Currency == "CLP" || Currency == "USD" ? Grade.Ok : Grade.NotAtAll;
		}

		#endregion

		#region IBuyEDalerService

		/// <summary>
		/// Contract ID of Template, for buying e-Daler
		/// </summary>
		public string BuyEDalerTemplateContractId => null;

		/// <summary>
		/// Reference to the service provider.
		/// </summary>
		public IBuyEDalerServiceProvider BuyEDalerServiceProvider => this.provider;

		/// <summary>
		/// If the service provider can be used to process a request to buy eDaler
		/// of a certain amount, for a given account.
		/// </summary>
		/// <param name="AccountName">Account Name</param>
		/// <returns>If service provider can be used.</returns>
		public Task<bool> CanBuyEDaler(CaseInsensitiveString AccountName)
		{
			return this.IsConfigured();
		}

		private async Task<bool> IsConfigured()
		{
			ServiceConfiguration Configuration = await ServiceConfiguration.GetCurrent();
			return Configuration.IsWellDefined;
		}

		/// <summary>
		/// Processes payment for buying eDaler.
		/// </summary>
		/// <param name="ContractParameters">Parameters available in the
		/// contract authorizing the payment.</param>
		/// <param name="IdentityProperties">Properties engraved into the
		/// legal identity signing the payment request.</param>
		/// <param name="Amount">Amount to be paid.</param>
		/// <param name="Currency">Currency</param>
		/// <param name="SuccessUrl">Optional Success URL the service provider can open on the client from a client web page, if payment has succeeded.</param>
		/// <param name="FailureUrl">Optional Failure URL the service provider can open on the client from a client web page, if payment has succeeded.</param>
		/// <param name="CancelUrl">Optional Cancel URL the service provider can open on the client from a client web page, if payment has succeeded.</param>
		/// <param name="ClientUrlCallback">Method to call if the payment service
		/// requests an URL to be displayed on the client.</param>
		/// <param name="State">State object to pass on the callback method.</param>
		/// <returns>Result of operation.</returns>
		public Task<PaymentResult> BuyEDaler(IDictionary<CaseInsensitiveString, object> ContractParameters,
			IDictionary<CaseInsensitiveString, CaseInsensitiveString> IdentityProperties,
			decimal Amount, string Currency, string SuccessUrl, string FailureUrl, string CancelUrl,
			ClientUrlEventHandler ClientUrlCallback, object State)
		{
			return this.Pay(Amount, Currency, string.Empty, SuccessUrl, FailureUrl, CancelUrl, ClientUrlCallback, State);
		}

		private async Task TryCancel(TransbankClient Client, string Token, decimal Amount, string Currency)
		{
			try
			{
				await Client.Information("Attempting to cancel or reverse transaction.");

				if (Currency == "CLP")
					await Client.RefundTransactionCLP(Token, (int)Amount);
				else
					await Client.RefundTransactionUSD(Token, Amount);
			}
			catch (Exception ex)
			{
				await Client.Error(ex.Message);
			}
		}

		private static PaymentResult ValidateResult(AuthorizationResponseCodeLevel1 ResponseCode, decimal Amount, string Currency)
		{
			string Msg = ValidateResult(ResponseCode);
			if (string.IsNullOrEmpty(Msg))
				return new PaymentResult(Amount, Currency);
			else
				return new PaymentResult(Msg);
		}

		private static string ValidateResult(AuthorizationResponseCodeLevel1 ResponseCode)
		{
			switch (ResponseCode)
			{
				case AuthorizationResponseCodeLevel1.Approved:
					return null;

				case AuthorizationResponseCodeLevel1.RejectionEntryError:
					return "Rejected due to invalid entry.";

				case AuthorizationResponseCodeLevel1.RejectionProcessingError:
					return "Unable to process transaction.";

				case AuthorizationResponseCodeLevel1.RejectionTransactionError:
					return "Error in transaction.";

				case AuthorizationResponseCodeLevel1.RejectionSender:
					return "Rejected by the sender.";

				case AuthorizationResponseCodeLevel1.Declined:
					return "Transaction was declined.";

				default:
					return "Transaction was rejected due to unknown causes.";
			}
		}

		/// <summary>
		/// Gets available payment options for buying eDaler.
		/// </summary>
		/// <param name="IdentityProperties">Properties engraved into the legal identity that will perform the request.</param>
		/// <param name="SuccessUrl">Optional Success URL the service provider can open on the client from a client web page, if getting options has succeeded.</param>
		/// <param name="FailureUrl">Optional Failure URL the service provider can open on the client from a client web page, if getting options has succeeded.</param>
		/// <param name="CancelUrl">Optional Cancel URL the service provider can open on the client from a client web page, if getting options has succeeded.</param>
		/// <param name="ClientUrlCallback">Method to call if the payment service
		/// requests an URL to be displayed on the client.</param>
		/// <param name="State">State object to pass on the callback method.</param>
		/// <returns>Array of dictionaries, each dictionary representing a set of parameters that can be selected in the
		/// contract to sign.</returns>
		public Task<IDictionary<CaseInsensitiveString, object>[]> GetPaymentOptionsForBuyingEDaler(
			IDictionary<CaseInsensitiveString, CaseInsensitiveString> IdentityProperties,
			string SuccessUrl, string FailureUrl, string CancelUrl, ClientUrlEventHandler ClientUrlCallback, object State)
		{
			return Task.FromResult(new IDictionary<CaseInsensitiveString, object>[0]);
		}

		#endregion

		#region IPaymentService

		/// <summary>
		/// Reference to service provider.
		/// </summary>
		public IPaymentServiceProvider PaymentServiceProvider => this.provider;

		/// <summary>
		/// Processes a payment.
		/// </summary>
		/// <param name="Amount">Amount to be paid.</param>
		/// <param name="Currency">Currency</param>
		/// <param name="Description">Description of payment.</param>
		/// <param name="SuccessUrl">Optional Success URL the service provider can open on the client from a client web page, if payment has succeeded.</param>
		/// <param name="FailureUrl">Optional Failure URL the service provider can open on the client from a client web page, if payment has succeeded.</param>
		/// <param name="CancelUrl">Optional Cancel URL the service provider can open on the client from a client web page, if payment has succeeded.</param>
		/// <param name="ClientUrlCallback">Method to call if the payment service requests an URL to be displayed on the client.</param>
		/// <param name="State">State object to pass on the callback method.</param>
		/// <returns>Result of operation.</returns>
		public async Task<PaymentResult> Pay(decimal Amount, string Currency, string Description, string SuccessUrl, string FailureUrl, string CancelUrl,
			ClientUrlEventHandler ClientUrlCallback, object State)
		{
			ServiceConfiguration Configuration = await ServiceConfiguration.GetCurrent();
			if (!Configuration.IsWellDefined)
				return new PaymentResult("Service not configured properly.");

			TransbankClient Client = TransbankServiceProvider.CreateClient(Configuration, Currency);
			if (Client is null)
				return new PaymentResult("Service not configured properly.");

			if (ClientUrlCallback is null)
				return new PaymentResult("Client URL Callback required.");

			try
			{
				string BuyOrder = Convert.ToBase64String(Gateway.NextBytes(18));    // Generates 24 characters code.
				string SessionId = Guid.NewGuid().ToString();
				string ReturnUrl = Gateway.GetUrl("/Transbank/TransactionResult.md?Currency=" + Currency +
					"&Success=" + HttpUtility.UrlEncode(SuccessUrl) +
					"&Failure=" + HttpUtility.UrlEncode(FailureUrl) +
					"&Cancel=" + HttpUtility.UrlEncode(CancelUrl));
				TransactionCreationResponse Transaction;

				if (Currency == "CLP")
				{
					int IntAmount = (int)Amount;
					if (IntAmount != Amount)
						return new PaymentResult("Amount must be an integer number of CLP.");

					Transaction = await Client.CreateTransactionCLP(BuyOrder, SessionId, IntAmount, ReturnUrl);
				}
				else
					Transaction = await Client.CreateTransactionUSD(BuyOrder, SessionId, Amount, ReturnUrl);

				CancellationTokenSource CancelToken = new CancellationTokenSource();

				TransbankServiceProvider.Register(Transaction.Token, BuyOrder, SessionId, Currency, CancelToken);
				try
				{
					await ClientUrlCallback(this, new ClientUrlEventArgs(Transaction.Url + "?token_ws=" + Transaction.Token, null));

					TransactionInformationResponse TransactionInfo = await Client.WaitForConclusion(Transaction.Token,
						Configuration.PollingIntervalSeconds * 1000, Configuration.TimeoutMinutes, CancelToken);

					if (TransactionInfo.AuthorizationResponseCode.HasValue)
						return ValidateResult(TransactionInfo.AuthorizationResponseCode.Value, Amount, Currency);
					else if (CancelToken.IsCancellationRequested)
						return new PaymentResult("Transaction was cancelled.");
					else if (!TransactionInfo.Vci.HasValue)
						return new PaymentResult("Transaction not completed in time.");
					else
					{
						int NrAttemptsLeft = 10;
						bool TryAgain = true;

						while (TryAgain)
						{
							TryAgain = false;

							try
							{
								TransactionInfo = await Client.ConfirmTransaction(Transaction.Token);
							}
							catch (IOException ex)
							{
								// Sometimes happens. Attempt again.

								if (NrAttemptsLeft-- < 0)
									ExceptionDispatchInfo.Capture(ex).Throw();

								await Task.Delay(2000);
								TryAgain = true;
							}
						}

						if (TransactionInfo.AuthorizationResponseCode.HasValue)
							return ValidateResult(TransactionInfo.AuthorizationResponseCode.Value, Amount, Currency);
						else
						{
							await this.TryCancel(Client, Transaction.Token, Amount, Currency);
							return new PaymentResult("Unable to confirm transaction.");
						}
					}
				}
				finally
				{
					TransbankServiceProvider.Unregister(Transaction.Token, BuyOrder, SessionId, Currency, false);
					CancelToken.Dispose();
				}
			}
			catch (Exception ex)
			{
				return new PaymentResult(ex.Message);
			}
			finally
			{
				TransbankServiceProvider.Dispose(Client);
			}
		}

		#endregion
	}
}
