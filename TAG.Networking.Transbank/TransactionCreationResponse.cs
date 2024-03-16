namespace TAG.Networking.Transbank
{
	/// <summary>
	/// Response after creating a new transaction
	/// </summary>
	public class TransactionCreationResponse
	{
		/// <summary>
		/// Transaction token. Length: 64.
		/// </summary>
		public string Token { get; internal set; }

		/// <summary>
		/// Webpay payment form URL. Maximum length: 255.
		/// </summary>
		public string Url { get; internal set; }
	}
}
