namespace TAG.Networking.Transbank
{
	/// <summary>
	/// Authorization Response Code
	/// 
	/// https://www.transbankdevelopers.cl/producto/webpay#codigos-de-respuesta-de-autorizacion
	/// </summary>
	public enum AuthorizationResponseCodeLevel1
	{
		/// <summary>
		/// Transaction approved
		/// </summary>
		Approved = 0,

		/// <summary>
		/// Rejection - Possible transaction data entry error
		/// </summary>
		RejectionEntryError = -1,

		/// <summary>
		/// Rejection - There was an error processing the transaction, this rejection message 
		/// is related to parameters of the card and/or its associated account
		/// </summary>
		RejectionProcessingError = -2,

		/// <summary>
		/// Rejection - Error in Transaction
		/// </summary>
		RejectionTransactionError = -3,

		/// <summary>
		/// Rejection - Rejected by the sender
		/// </summary>
		RejectionSender = -4,

		/// <summary>
		/// Decline - Transaction at risk of possible fraud
		/// </summary>
		Declined = -5,

		/// <summary>
		/// Unknown code
		/// </summary>
		Other
	}
}
