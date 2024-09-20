Title: Transbank Sniffer
Description: Allows the user to view Transbank communication.
Date: 2024-03-26
Author: Peter Waher
Master: /Master.md
JavaScript: /Events.js
JavaScript: /Sniffers/Sniffer.js
CSS: /Sniffers/Sniffer.css
UserVariable: User
Privilege: Admin.Communication.Sniffer
Privilege: Admin.Communication.Transbank
Login: /Login.md
Parameter: SnifferId

========================================================================

Transbank Communication
===========================

On this page, you can follow the Transbank API communication made from the machine to the 
Transbank back-end. The sniffer will automatically be terminated after some time to avoid 
performance degradation and leaks. Sniffers should only be used as a tool for 
troubleshooting.

{{
TAG.Payments.Transbank.TransbankServiceProvider.RegisterSniffer(SnifferId,Request,"User",["Admin.Communication.Sniffer","Admin.Communication.Transbank"])
}}
