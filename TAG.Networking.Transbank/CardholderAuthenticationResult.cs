namespace TAG.Networking.Transbank
{
	/// <summary>
	/// Cardholder authentication result.
	/// </summary>
	/// <remarks>
	/// This field is additional information supplementary to responseCode but the merchant should 
	/// not validate this field because new authentication mechanisms are constantly being added 
	/// that translate into new values for this field that are not necessarily documented. (In the 
	/// case of international cards that do not provide 3D-Secure, the merchant's decision to 
	/// accept them or not is made at the merchant configuration level in Transbank and must be 
	/// discussed with the merchant's executive)
	/// </remarks>
	public enum CardholderAuthenticationResult
	{
		/// <summary>
		/// Successful Authentication
		/// </summary>
		TSY,

		/// <summary>
		/// Authentication Refused
		/// </summary>
		TSN,

		/// <summary>
		/// Do not Participate, without authentication
		/// </summary>
		NP,

		/// <summary>
		/// Connection failure, Authentication Rejected
		/// </summary>
		U3,

		/// <summary>
		/// Invalid Data
		/// </summary>
		INV,

		/// <summary>
		/// Tried
		/// </summary>
		A,

		/// <summary>
		/// Trade does not participate
		/// </summary>
		CNP1,

		/// <summary>
		/// Operational Error
		/// </summary>
		EOP,

		/// <summary>
		/// BIN not adhered
		/// </summary>
		BNA,

		/// <summary>
		/// Issuer not adhered
		/// </summary>
		ENA,

		// For foreign sale, these are some of the codes:

		/// <summary>
		/// Frictionless Authentication Successful. Authentication Result: Authentication Successful
		/// </summary>
		TSYS,

		/// <summary>
		/// Attempt, card not enrolled / issuer not available. Authentication Result: Successful Authentication
		/// </summary>
		TSAS,

		/// <summary>
		/// Failed, unauthenticated, denied / does not allow attempts. Authentication result: Authentication denied
		/// </summary>
		TSNS,

		/// <summary>
		/// Authentication Refused - Frictionless. Authentication Result: Authentication Refused
		/// </summary>
		TSRS,

		/// <summary>
		/// Authentication could not be performed due to technical problem or other reason. Authentication result: Authentication failed
		/// </summary>
		TSUS,

		/// <summary>
		/// Authentication with friction (Not accepted by the merchant). Authentication result: Authentication incomplete
		/// </summary>
		TSCF,

		/// <summary>
		/// Authentication successful with friction. Authentication result: Authentication successful
		/// </summary>
		TSYF,

		/// <summary>
		/// Not Authenticated. Transaction Frictionally Denied. Authentication Result: Authentication Denied
		/// </summary>
		TSNF,

		/// <summary>
		/// Authentication with friction could not be performed due to a technical or other problem. Authentication result: Authentication failed
		/// </summary>
		TSUF,

		/// <summary>
		/// Merchant does not participate. Authentication result: Merchant/BIN does not participate
		/// </summary>
		NPC,

		/// <summary>
		/// BIN does not participate. Authentication result: Merchant/BIN does not participate
		/// </summary>
		NPB,

		/// <summary>
		/// Merchant and BIN do not participate. Authentication result: Merchant/BIN does not participate
		/// </summary>
		NPCB,

		/// <summary>
		/// Merchant and BIN do participate. Authentication result: Authorization incomplete
		/// </summary>
		SPCB,

		/// <summary>
		/// Unknown code
		/// </summary>
		Other
	}
}
