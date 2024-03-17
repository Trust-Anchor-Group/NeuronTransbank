using System;
using System.Text;
using Waher.Content;
using Waher.Content.Json;
using Waher.Runtime.Inventory;

namespace TAG.Networking.Transbank
{
	/// <summary>
	/// Encodes <see cref="DecimalFixedDecimals"/> to JSON.
	/// </summary>
	public class DecimalFixedDecimalsEncoder : IJsonEncoder
	{
		/// <summary>
		/// Encodes <see cref="DecimalFixedDecimals"/> to JSON.
		/// </summary>
		public DecimalFixedDecimalsEncoder()
		{
		}

		/// <summary>
		/// <see cref="IJsonEncoder.Encode(object, int?, StringBuilder)"/>
		/// </summary>
		public void Encode(object Object, int? Indent, StringBuilder Json)
		{
			if (Object is DecimalFixedDecimals Amount)
				Json.Append(CommonTypes.Encode(Amount.Amount, Amount.NrDecimals));
			else
			{
				throw new NotSupportedException("Can only encode objects of type " +
					typeof(DecimalFixedDecimals).FullName);
			}
		}

		/// <summary>
		/// <see cref="IProcessingSupport{T}.Supports(T)"/>
		/// </summary>
		public Grade Supports(Type Object)
		{
			return Object == typeof(DecimalFixedDecimals) ? Grade.Excellent : Grade.NotAtAll;
		}
	}
}
