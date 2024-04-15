using System;

namespace TAG.Networking.Transbank
{
	/// <summary>
	/// Response containing information about a refund or cancellation of a transaction.
	/// </summary>
	public class TransactionRefundResponse
	{
		/// <summary>
		/// Refund type.
		/// </summary>
		public RefundType Type { get; internal set; }

		/// <summary>
		/// Cancellation authorization code.
		/// </summary>
		public string AuthorizationCode { get; internal set; }

		/// <summary>
		/// Date and time of authorization.
		/// </summary>
		public DateTime? AuthorizationDate { get; internal set; }

		/// <summary>
		/// Updated balance of the transaction (considers the sale less the canceled amount).
		/// </summary>
		public decimal? Balance { get; internal set; }

		/// <summary>
		/// Canceled amount.
		/// </summary>
		public decimal? NullifiedAmount { get; internal set; }

		/// <summary>
		/// Result code of the reversal/cancellation. If successful, it is 0, otherwise the reversal/undoing was not performed.
		/// </summary>
		public int? ResponseCode { get; internal set; }
	}
}
