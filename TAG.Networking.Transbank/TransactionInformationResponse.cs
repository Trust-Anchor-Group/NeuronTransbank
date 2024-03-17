using System;

namespace TAG.Networking.Transbank
{
	/// <summary>
	/// Response containing information about a transcation.
	/// </summary>
	public class TransactionInformationResponse
	{
		/// <summary>
		/// Cardholder authentication result.
		/// </summary>
		public CardholderAuthenticationResult? Vci { get; internal set; }

		/// <summary>
		/// Original string representation of <see cref="Vci"/>.
		/// </summary>
		public string VciStr { get; internal set; }

		/// <summary>
		/// Whole number format for transactions in pesos and decimal for transactions in dollars.
		/// </summary>
		public int Amount { get; internal set; }

		/// <summary>
		/// Transaction status
		/// </summary>
		public TranscationStatus Status { get; internal set; }

		/// <summary>
		/// Original string representation of <see cref="Status"/>.
		/// </summary>
		public string StatusStr { get; internal set; }

		/// <summary>
		/// Purchase order from the store indicated in Transaction.create(). Maximum length: 26
		/// </summary>
		public string BuyOrder { get; internal set; }

		/// <summary>
		/// Session identifier, the same one originally sent by the merchant in Transaction.create(). Maximum length: 61.
		/// </summary>
		public string SessionId { get; internal set; }

		/// <summary>
		/// 4 últimos números de la tarjeta de crédito del tarjetahabiente. Largo máximo: 19.
		/// </summary>
		public string CardNumber { get; internal set; }

		/// <summary>
		/// Authorization date. Length: 4, MMDD format
		/// </summary>
		public string AccountingDate { get; internal set; }

		/// <summary>
		/// Authorization date and time.
		/// </summary>
		public DateTime TransactionDate { get; internal set; }

		/// <summary>
		/// Transaction authorization code Maximum length: 6
		/// </summary>
		public string AuthorizationCode { get; internal set; }

		/// <summary>
		/// Payment type of the transaction.
		/// </summary>
		public PaymentType? PaymentType { get; internal set; }

		/// <summary>
		/// Original string representation of <see cref="PaymentType"/>.
		/// </summary>
		public string PaymentTypeStr { get; internal set; }

		/// <summary>
		/// Authorization response code.
		/// </summary>
		public AuthorizationResponseCodeLevel1? AuthorizationResponseCode { get; internal set; }

		/// <summary>
		/// Original integer representation of <see cref="AuthorizationResponseCode"/>.
		/// </summary>
		public int? AuthorizationResponseCodeInt { get; internal set; }

		/// <summary>
		/// Installments number
		/// </summary>
		public int InstallmentsNumber { get; internal set; }

		/// <summary>
		/// Amount of fees.
		/// </summary>
		public int? InstallmentsAmount { get; internal set; }

		/// <summary>
		/// Remaining amount for a canceled item.
		/// </summary>
		public int? Balance { get; internal set; }
	}
}
