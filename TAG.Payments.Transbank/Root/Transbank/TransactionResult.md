Title: Transaction result
Description: Processes the result of a transaction.
Date: 2024-03-26
Author: Peter Waher
Master: /Master.md
Cache-Control: max-age=0, no-cache, no-store
AllowScriptTag: 1
Parameter: Success
Parameter: Failure
Parameter: Cancel
Parameter: token_ws
Parameter: Currency

========================================================================

{{
ResultEnum:=TAG.Networking.Transbank.AuthorizationResponseCodeLevel1;
Result:=TAG.Payments.Transbank.TransbankServiceProvider.GetTransactionResult(token_ws, Currency) ??? (LogCritical(Exception);0);
URL:="";

if Result=ResultEnum.Approved then 
(
	URL:=Success;
	]]

Success
==========

Transaction executed successfully.

[[
)
else if Result=ResultEnum.RejectionEntryError then 
(
	URL:=Failure;
	]]

Failure
==========

Transaction failed due to errors in data provided.

[[
)
else if Result=ResultEnum.RejectionProcessingError then 
(
	URL:=Failure;
	]]

Failure
==========

Transaction failed due to processing errors.

[[
)
else if Result=ResultEnum.RejectionTransactionError then 
(
	URL:=Failure;
	]]

Failure
==========

Transaction failed due to errors in transaction.

[[
)
else if Result=ResultEnum.RejectionSender then 
(
	URL:=Cancel;
	]]

Failure
==========

Transaction was cancelled.

[[
)
else if Result=ResultEnum.Declined then 
(
	URL:=Failure;
	]]

Failure
==========

Transaction was declined.

[[
)
else if Result=ResultEnum.Other then 
(
	URL:=Failure;
	]]

Failure
==========

An unrecognized error occurred.

[[
)
else
(
	URL:=Failure;
	]]

Failure
==========

Transaction was not found.

[[
);

if !empty(URL) then TemporaryRedirect(URL);

]]
You can close this page if it does not close automatically.

<script>window.close();</script>

[[
}}