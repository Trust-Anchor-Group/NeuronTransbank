using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Json;
using Waher.Content.Putters;
using Waher.Content.Xml;
using Waher.Networking.Sniffers;
using Waher.Script;

namespace TAG.Networking.Transbank
{
	/// <summary>
	/// Implements the Transbank REST API
	/// 
	/// https://www.transbankdevelopers.cl/referencia/webpay?l=http
	/// </summary>
	public class TransbankClient : Sniffable, IDisposable
	{
		/// <summary>
		/// URI to Production API
		/// </summary>
		public const string ProductionEnvironment = "https://webpay3g.transbank.cl/";

		/// <summary>
		/// URI to Integration API
		/// </summary>
		public const string IntegrationEnvironment = "https://webpay3gint.transbank.cl/";

		private readonly string apiEndpoint;
		private readonly string merchantID;
		private readonly string merchantSecret;

		/// <summary>
		/// Implements the Transbank REST API
		/// </summary>
		/// <param name="ApiEndpoint">API Endpoint</param>
		/// <param name="MerchantID">Merchant API ID</param>
		/// <param name="MerchantSecret">Merchant API Secret</param>
		/// <param name="Sniffers">Sniffers</param>
		public TransbankClient(string ApiEndpoint, string MerchantID, string MerchantSecret, params ISniffer[] Sniffers)
			: base(Sniffers)
		{
			this.apiEndpoint = ApiEndpoint;
			this.merchantID = MerchantID;
			this.merchantSecret = MerchantSecret;
		}

		private Task<Dictionary<string, object>> Post(string Resource, Dictionary<string, object> Request)
		{
			return this.Post(Resource, Request,
				new KeyValuePair<string, string>("Tbk-Api-Key-Id", this.merchantID),
				new KeyValuePair<string, string>("Tbk-Api-Key-Secret", this.merchantSecret));
		}

		private async Task<Dictionary<string, object>> Post(string Resource, Dictionary<string, object> Request,
			params KeyValuePair<string, string>[] CustomHeaders)
		{
			Uri Uri = new Uri(this.apiEndpoint + Resource);

			if (this.HasSniffers)
				await this.Written("POST", Uri, Request, CustomHeaders);

			object Obj = await InternetContent.PostAsync(Uri, Request, CustomHeaders);

			if (this.HasSniffers)
				this.Received(Obj);

			if (!(Obj is Dictionary<string, object> Response))
				throw new IOException("Unexpected response of type " + Obj.GetType().FullName + " received.");

			return Response;
		}

		private Task<Dictionary<string, object>> Put(string Resource)
		{
			return this.Put(Resource,
				new KeyValuePair<string, string>("Tbk-Api-Key-Id", this.merchantID),
				new KeyValuePair<string, string>("Tbk-Api-Key-Secret", this.merchantSecret));
		}

		private async Task<Dictionary<string, object>> Put(string Resource, params KeyValuePair<string, string>[] CustomHeaders)
		{
			Uri Uri = new Uri(this.apiEndpoint + Resource);

			if (this.HasSniffers)
				await this.Written("PUT", Uri, null, CustomHeaders);

			WebPutter Putter = new WebPutter();
			KeyValuePair<byte[], string> P = await Putter.PutAsync(Uri, new byte[0], JsonCodec.DefaultContentType, null, null, CustomHeaders);
			object Obj = await InternetContent.DecodeAsync(P.Value, P.Key, Uri);

			if (this.HasSniffers)
				this.Received(Obj);

			if (!(Obj is Dictionary<string, object> Response))
				throw new IOException("Unexpected response of type " + Obj.GetType().FullName + " received.");

			return Response;
		}

		private Task<Dictionary<string, object>> Get(string Resource)
		{
			return this.Get(Resource,
				new KeyValuePair<string, string>("Tbk-Api-Key-Id", this.merchantID),
				new KeyValuePair<string, string>("Tbk-Api-Key-Secret", this.merchantSecret));
		}

		private async Task<Dictionary<string, object>> Get(string Resource, params KeyValuePair<string, string>[] CustomHeaders)
		{
			Uri Uri = new Uri(this.apiEndpoint + Resource);

			if (this.HasSniffers)
				await this.Written("GET", Uri, null, CustomHeaders);

			object Obj = await InternetContent.GetAsync(Uri, CustomHeaders);

			if (this.HasSniffers)
				this.Received(Obj);

			if (!(Obj is Dictionary<string, object> Response))
				throw new IOException("Unexpected response of type " + Obj.GetType().FullName + " received.");

			return Response;
		}

		private void Received(object Response)
		{
			this.ReceiveText(JSON.Encode(Response, true));
		}

		private async Task Written(string Method, Uri Uri, object Data, params KeyValuePair<string, string>[] CustomHeaders)
		{
			StringBuilder sb = new StringBuilder();
			string s;

			sb.Append(Method);
			sb.Append('(');
			sb.Append(Uri.ToString());

			if (!(Data is null))
			{
				if (Data is Dictionary<string, object> Obj)
					s = JSON.Encode(Obj, true);
				else
				{
					KeyValuePair<byte[], string> P = await InternetContent.EncodeAsync(Data, Encoding.UTF8);
					s = Encoding.UTF8.GetString(P.Key);
				}

				sb.AppendLine(",");
				sb.Append(s);
			}

			if ((CustomHeaders?.Length ?? 0) > 0)
			{
				foreach (KeyValuePair<string, string> H in CustomHeaders)
				{
					sb.AppendLine(",");
					sb.Append(H.Key);
					sb.Append(':');
					sb.Append(H.Value);
				}
			}

			sb.Append(')');

			this.TransmitText(sb.ToString());
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
		}

		/// <summary>
		/// Creates a transaction using Chilean Peso
		/// </summary>
		/// <param name="BuyOrder">Store purchase order. This number must be unique for each transaction. Maximum length: 26. 
		/// The purchase order can have: Numbers, letters, upper and lower case letters, and the signs.</param>
		/// <param name="SessionId">Session identifier, internal business use, this value is returned at the end of the transaction.
		/// Maximum length: 61</param>
		/// <param name="Amount">Transaction amount. Maximum 2 decimal places for USD. Maximum length: 17</param>
		/// <param name="ReturnUrl">Merchant URL, to which Webpay will redirect after the authorization process. Maximum length: 256</param>
		/// <returns>Information about initiated transaction.</returns>
		/// <exception cref="ArgumentException">If any of the arguments do not comply with input requirements.</exception>
		public async Task<TransactionCreationResponse> CreateTransactionCLP(string BuyOrder, string SessionId, int Amount, string ReturnUrl)
		{
			if (string.IsNullOrEmpty(BuyOrder))
				throw new ArgumentException("Buy order empty.", nameof(BuyOrder));

			if (BuyOrder.Length > 26)
				throw new ArgumentException("Buy order too long.", nameof(BuyOrder));

			foreach (char ch in BuyOrder)
			{
				if (!char.IsNumber(ch) && !char.IsLetter(ch) && sign.IndexOf(ch) < 0)
					throw new ArgumentException("Buy order contains invalid character.", nameof(BuyOrder));
			}

			if (string.IsNullOrEmpty(SessionId))
				throw new ArgumentException("Session ID empty.", nameof(SessionId));

			if (SessionId.Length > 61)
				throw new ArgumentException("Session ID too long.", nameof(SessionId));

			if (Amount <= 0)
				throw new ArgumentException("Invalid amount.", nameof(Amount));

			if (string.IsNullOrEmpty(ReturnUrl))
				throw new ArgumentException("Return URL empty.", nameof(ReturnUrl));

			if (SessionId.Length > 256)
				throw new ArgumentException("Return URL too long.", nameof(ReturnUrl));

			Dictionary<string, object> Response = await this.Post("rswebpaytransaction/api/webpay/v1.2/transactions",
				new Dictionary<string, object>()
				{
					{ "buy_order", BuyOrder },
					{ "session_id", SessionId },
					{ "amount", Amount },
					{ "return_url", ReturnUrl }
				});

			if (!Response.TryGetValue("token", out object Obj) || !(Obj is string Token) ||
				!Response.TryGetValue("url", out Obj) || !(Obj is string Url))
			{
				throw new IOException("Unexpected response returned.");
			}

			return new TransactionCreationResponse()
			{
				Token = Token,
				Url = Url
			};
		}

		private const string sign = "|_=&%.,~:/?[+!@()>-";

		/// <summary>
		/// Confirms a transaction
		/// </summary>
		/// <param name="Token">Transaction token. Length: 64.</param>
		/// <returns>Information about initiated transaction.</returns>
		/// <exception cref="ArgumentException">If any of the arguments do not comply with input requirements.</exception>
		public async Task<TransactionInformationResponse> ConfirmTransaction(string Token)
		{
			if (string.IsNullOrEmpty(Token) || Token.Length != 64)
				throw new ArgumentException("Invalid token.", nameof(Token));

			Dictionary<string, object> Response = await this.Put("rswebpaytransaction/api/webpay/v1.2/transactions/" + Token);

			return GetTransactionInformationResponse(Response);
		}

		private static TransactionInformationResponse GetTransactionInformationResponse(Dictionary<string, object> Response)
		{
			if (!Response.TryGetValue("amount", out object AmountObj) ||
				!Response.TryGetValue("status", out object Obj) || !(Obj is string StatusStr) ||
				!Response.TryGetValue("buy_order", out Obj) || !(Obj is string BuyOrder) ||
				!Response.TryGetValue("session_id", out Obj) || !(Obj is string SessionId) ||
				!Response.TryGetValue("accounting_date", out Obj) || !(Obj is string AccountingDateStr) ||
				!Response.TryGetValue("transaction_date", out Obj) || !(Obj is string TransactionDateStr) ||
				!XML.TryParse(TransactionDateStr, out DateTime TransactionDate) ||
				!Response.TryGetValue("installments_number", out Obj) || !(Obj is int InstallmentsNumber))
			{
				throw new IOException("Unexpected response returned.");
			}


			AuthorizationResponseCodeLevel1? AuthorizationResponseCode;
			CardholderAuthenticationResult? Vci;
			PaymentType? PaymentTypeCode;
			int? ResponseCode;

			if (!Response.TryGetValue("card_detail", out Obj) ||
				!(Obj is Dictionary<string, object> CardDetail) ||
				!CardDetail.TryGetValue("card_number", out Obj) ||
				!(Obj is string CardNumber))
			{
				CardNumber = null;
			}

			if (!Response.TryGetValue("vci", out Obj) || !(Obj is string VciStr))
			{
				VciStr = null;
				Vci = null;
			}
			else
			{
				if (Enum.TryParse(VciStr, out CardholderAuthenticationResult Vci2))
					Vci = Vci2;
				else
					Vci = CardholderAuthenticationResult.Other;
			}

			if (!Enum.TryParse(StatusStr, out TranscationStatus Status))
				Status = TranscationStatus.Other;

			if (!Response.TryGetValue("authorization_code", out Obj) || !(Obj is string AuthorizationCode))
				AuthorizationCode = null;

			if (!Response.TryGetValue("payment_type_code", out Obj) || !(Obj is string PaymentTypeCodeStr))
			{
				PaymentTypeCodeStr = null;
				PaymentTypeCode = null;
			}
			else
			{
				if (Enum.TryParse(PaymentTypeCodeStr, out PaymentType PaymentTypeCode2))
					PaymentTypeCode = PaymentTypeCode2;
				else
					PaymentTypeCode = PaymentType.Other;
			}

			if (!Response.TryGetValue("response_code", out Obj) || !(Obj is int ResponseCode2))
			{
				ResponseCode = null;
				AuthorizationResponseCode = null;
			}
			else
			{
				ResponseCode = ResponseCode2;
				AuthorizationResponseCode = (AuthorizationResponseCodeLevel1)ResponseCode2;
			}

			int Amount = (int)Expression.ToDouble(AmountObj);
			int? InstallmentsAmount = null;
			int? Balance = null;

			if (Response.TryGetValue("installments_amount", out Obj))
				InstallmentsAmount = (int)Expression.ToDouble(Obj);

			if (Response.TryGetValue("balance", out Obj))
				Balance = (int)Expression.ToDouble(Obj);

			return new TransactionInformationResponse()
			{
				Vci = Vci,
				VciStr = VciStr,
				Amount = Amount,
				Status = Status,
				StatusStr = StatusStr,
				BuyOrder = BuyOrder,
				SessionId = SessionId,
				CardNumber = CardNumber,
				AccountingDate = AccountingDateStr,
				TransactionDate = TransactionDate,
				AuthorizationCode = AuthorizationCode,
				PaymentType = PaymentTypeCode,
				PaymentTypeStr = PaymentTypeCodeStr,
				AuthorizationResponseCode = AuthorizationResponseCode,
				AuthorizationResponseCodeInt = ResponseCode,
				InstallmentsNumber = InstallmentsNumber,
				InstallmentsAmount = InstallmentsAmount,
				Balance = Balance
			};
		}

		/// <summary>
		/// Gets the status of a transaction.
		/// </summary>
		/// <param name="Token">Transaction token. Length: 64.</param>
		/// <returns>Information about initiated transaction.</returns>
		/// <exception cref="ArgumentException">If any of the arguments do not comply with input requirements.</exception>
		public async Task<TransactionInformationResponse> GetTransactionStatus(string Token)
		{
			if (string.IsNullOrEmpty(Token) || Token.Length != 64)
				throw new ArgumentException("Invalid token.", nameof(Token));

			Dictionary<string, object> Response = await this.Get("rswebpaytransaction/api/webpay/v1.2/transactions/" + Token);

			return GetTransactionInformationResponse(Response);
		}

	}
}
