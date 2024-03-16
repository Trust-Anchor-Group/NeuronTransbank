namespace TAG.Networking.Transbank
{
	/// <summary>
	/// Transaction status
	/// </summary>
	public enum TranscationStatus
	{
		/// <summary>
		/// Initialized
		/// </summary>
		INITIALIZED, 

		/// <summary>
		/// Authorized
		/// </summary>
		AUTHORIZED, 

		/// <summary>
		/// Reversed
		/// </summary>
		REVERSED, 

		/// <summary>
		/// Failed
		/// </summary>
		FAILED, 

		/// <summary>
		/// Nullified
		/// </summary>
		NULLIFIED, 

		/// <summary>
		/// Partially Nullified
		/// </summary>
		PARTIALLY_NULLIFIED, 

		/// <summary>
		/// Captured
		/// </summary>
		CAPTURED,

		/// <summary>
		/// Unknown code
		/// </summary>
		Other
	}
}
