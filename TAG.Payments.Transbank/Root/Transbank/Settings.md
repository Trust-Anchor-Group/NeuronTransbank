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
if exists(Posted) then
(
	SetSetting("TAG.Payments.Transbank.MerchantId",Posted.MerchantId);
	SetSetting("TAG.Payments.Transbank.MerchantSecret",Posted.MerchantSecret);
	SetSetting("TAG.Payments.Transbank.PollingIntervalSeconds",Num(Posted.PollingIntervalSeconds));
	SetSetting("TAG.Payments.Transbank.TimeoutMinutes",Num(Posted.TimeoutMinutes));
	SetSetting("TAG.Payments.Transbank.Production",Boolean(Posted.Production));

	TAG.Payments.Transbank.ServiceConfiguration.InvalidateCurrent();

	SeeOther("Settings.md");
);
}}

<p>
<label for="MerchantId">Merchant ID:</label>  
<input type="text" id="MerchantId" name="MerchantId" value='{{GetSetting("TAG.Payments.Transbank.MerchantId","")}}' autofocus required title="Merchant ID identifying the Trust Provider in the Transbank backend."/>
</p>

<p>
<label for="MerchantSecret">Merchant Secret:</label>  
<input type="password" id="MerchantSecret" name="MerchantSecret" value='{{GetSetting("TAG.Payments.Transbank.MerchantSecret","")}}' title="Merchant Secret."/>
</p>

<p>
<label for="PollingIntervalSeconds">Polling interval: (seconds)</label>  
<input type="number" id="PollingIntervalSeconds" name="PollingIntervalSeconds" min="1" max="60" step="1" value='{{GetSetting("TAG.Payments.Transbank.PollingIntervalSeconds",2)}}' required title="Interval (in seconds) with which to check the status of an ongoing request."/>
</p>

<p>
<label for="TimeoutMinutes">Timeout: (minutes)</label>  
<input type="number" id="TimeoutMinutes" name="TimeoutMinutes" min="5" max="60" value='{{GetSetting("TAG.Payments.Transbank.TimeoutMinutes",5)}}' required title="Maximum amount of time to wait (in minutes) before cancelling an open banking request."/>
</p>

<p>
<input type="checkbox" id="Production" name="Production" title="Production mode if checked, test mode if not checked." {{GetSetting("TAG.Payments.Transbank.Production",false) ? "checked" : ""}}/>
<label for="Production">Production mode.</label>
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