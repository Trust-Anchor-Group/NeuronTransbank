namespace TAG.Networking.Transbank
{
	/// <summary>
	/// A decimal number with a fixed number of decimals.
	/// </summary>
	public class DecimalFixedDecimals
	{
		/// <summary>
		/// A decimal number with a fixed number of decimals.
		/// </summary>
		/// <param name="Amount">Amount</param>
		/// <param name="NrDecimals">Number of decimals</param>
		public DecimalFixedDecimals(decimal Amount, byte NrDecimals)
		{
			this.Amount = Amount;
			this.NrDecimals = NrDecimals;
		}

		/// <summary>
		/// Decimal amount.
		/// </summary>
		public decimal Amount { get; }

		/// <summary>
		/// Number of decimals to encode.
		/// </summary>
		public byte NrDecimals { get; }
	}
}
