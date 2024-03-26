Title: Transbank settings
Description: Configures integration with the Transbank backend payment service.
Date: 2024-03-26
Author: Peter Waher
Master: /Master.md
Cache-Control: max-age=0, no-cache, no-store
JavaScript: /Sniffers/Sniffer.js
UserVariable: User
Privilege: Admin.Payments.Paiwise.Transbank
Login: /Login.md

========================================================================

<form action="Settings.md" method="post">
<fieldset>
<legend>Transbank settings</legend>

The following settings are required by the integration of the Neuron(R) with the Transbank payments service back-end. 
By providing such an integration, card payments can be performed on the neuron, allowing end-users to buy eDaler(R).

{{
ModeEnum:=TAG.Payments.Transbank.OperationMode;

if exists(Posted) then
(
	SetSetting("TAG.Payments.Transbank.MerchantId.CLP",Posted.MerchantIdClp);
	SetSetting("TAG.Payments.Transbank.MerchantSecret.CLP",Posted.MerchantSecretClp);
	SetSetting("TAG.Payments.Transbank.MerchantId.USD",Posted.MerchantIdUsd);
	SetSetting("TAG.Payments.Transbank.MerchantSecret.USD",Posted.MerchantSecretUsd);
	SetSetting("TAG.Payments.Transbank.PollingIntervalSeconds",Num(Posted.PollingIntervalSeconds));
	SetSetting("TAG.Payments.Transbank.TimeoutMinutes",Num(Posted.TimeoutMinutes));
	SetSetting("TAG.Payments.Transbank.Mode",System.Enum.Parse(ModeEnum,Posted.Mode));

	TAG.Payments.Transbank.ServiceConfiguration.InvalidateCurrent();

	SeeOther("Settings.md");
);
}}

<fieldset>
<legend>Chilean Peso (CLP)</legend>
<p>
<label for="MerchantIdClp">Merchant ID:</label>  
<input type="text" id="MerchantIdClp" name="MerchantIdClp" value='{{GetSetting("TAG.Payments.Transbank.MerchantId.CLP","")}}' autofocus required title="Merchant ID identifying the Trust Provider in the Transbank backend for CLP transactions."/>
</p>

<p>
<label for="MerchantSecretClp">Merchant Secret:</label>  
<input type="password" id="MerchantSecretClp" name="MerchantSecretClp" value='{{GetSetting("TAG.Payments.Transbank.MerchantSecret.CLP","")}}' title="Merchant Secret for CLP transactions."/>
</p>
</fieldset>

<fieldset>
<legend>Chilean Peso (USD)</legend>
<p>
<label for="MerchantIdUsd">Merchant ID:</label>  
<input type="text" id="MerchantIdUsd" name="MerchantIdUsd" value='{{GetSetting("TAG.Payments.Transbank.MerchantId.USD","")}}' autofocus required title="Merchant ID identifying the Trust Provider in the Transbank backend for USD transactions."/>
</p>

<p>
<label for="MerchantSecretUsd">Merchant Secret:</label>  
<input type="password" id="MerchantSecretUsd" name="MerchantSecretUsd" value='{{GetSetting("TAG.Payments.Transbank.MerchantSecret.USD","")}}' title="Merchant Secret for USD transactions."/>
</p>
</fieldset>

<p>
<label for="PollingIntervalSeconds">Polling interval: (seconds)</label>  
<input type="number" id="PollingIntervalSeconds" name="PollingIntervalSeconds" min="1" max="60" step="1" value='{{GetSetting("TAG.Payments.Transbank.PollingIntervalSeconds",2)}}' required title="Interval (in seconds) with which to check the status of an ongoing request."/>
</p>

<p>
<label for="TimeoutMinutes">Timeout: (minutes)</label>  
<input type="number" id="TimeoutMinutes" name="TimeoutMinutes" min="5" max="60" value='{{GetSetting("TAG.Payments.Transbank.TimeoutMinutes",5)}}' required title="Maximum amount of time to wait (in minutes) before cancelling an open banking request."/>
</p>

<p>
<label for="Mode">Mode:</label>  
<select name="Mode" id="Mode" required>
<option value="Test"{{Mode:=GetSetting("TAG.Payments.Transbank.Mode",ModeEnum.Test); Mode=ModeEnum.Test?" selected" : ""}}>Test</option>
<option value="Production"{{Mode=ModeEnum.Production?" selected" : ""}}>Production</option>
</select>
</p>

<button type="submit" class="posButton">Apply</button>
</fieldset>

<fieldset>
<legend>Tools</legend>
<button type="button" class="posButton"{{
if User.HasPrivilege("Admin.Communication.Transbank") and User.HasPrivilege("Admin.Communication.Sniffer") then
	" onclick=\"OpenSniffer('Sniffer.md')\""
else
	" disabled"
}}>Sniffer</button>
</fieldset>
</form>