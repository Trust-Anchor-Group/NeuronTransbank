namespace TAG.Networking.Transbank
{
	/// <summary>
	/// Payment type.
	/// </summary>
	public enum PaymentType
	{
		/// <summary>
		/// Debit Sale
		/// </summary>
		VD,

		/// <summary>
		/// Normal Sale
		/// </summary>
		BV,

		/// <summary>
		/// Sale in installments
		/// </summary>
		VC,

		/// <summary>
		/// 3 installments without interest
		/// </summary>
		YES,

		/// <summary>
		/// 2 installments without interest
		/// </summary>
		S2,

		/// <summary>
		/// N Installments without interest
		/// </summary>
		NC,

		/// <summary>
		/// Prepaid Sale
		/// </summary>
		PV,

		/// <summary>
		/// Unknown code
		/// </summary>
		Other
	}
}
