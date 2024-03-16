namespace TAG.Networking.Transbank
{
	/// <summary>
	/// Response after creating a new transaction
	/// </summary>
	public class TransactionResponse
	{
		/// <summary>
		/// Response after creating a new transaction
		/// </summary>
		/// <param name="Token">Transaction token. Length: 64.</param>
		/// <param name="Url">Webpay payment form URL. Maximum length: 255.</param>
		public TransactionResponse(string Token, string Url)
		{
			this.Token = Token;
			this.Url = Url;
		}

		/// <summary>
		/// Transaction token. Length: 64.
		/// </summary>
		public string Token { get; }

		/// <summary>
		/// Webpay payment form URL. Maximum length: 255.
		/// </summary>
		public string Url { get; }
	}
}
