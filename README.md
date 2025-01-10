# NeuronTransbank

Provides an integration between the [TAG Neuron](https://lab.tagroot.io/Documentation/Index.md) and 
[Transbank](https://www.transbankdevelopers.cl/).

## Projects

The solution contains the following C# projects:

| Project                         | Framework         | Description |
|:--------------------------------|:------------------|:------------|
| `TAG.Networking.Transbank`      | .NET Standard 2.1 | Class library for communicating with Transbank via the [Transbank REST API](https://www.transbankdevelopers.cl/referencia/webpay?l=http). |
| `TAG.Networking.Transbank.Test` | .NET 8.0          | Unit tests for the `TAG.Networking.Transbank` library. |
| `TAG.Payments.Transbank`        | .NET Standard 2.1 | Service module for the [TAG Neuron](https://lab.tagroot.io/Documentation/Index.md), permitting users to buy eDaler using Transbank. |

## Nugets

The following nugets external are used. They faciliate common programming tasks, and
enables the libraries to be hosted on an [IoT Gateway](https://github.com/PeterWaher/IoTGateway).
This includes hosting the bridge on the [TAG Neuron](https://lab.tagroot.io/Documentation/Index.md).
They can also be used standalone.

| Nuget                                                                              | Description |
|:-----------------------------------------------------------------------------------|:------------|
| [Paiwise](https://www.nuget.org/packages/Paiwise)                                  | Contains services for integration of financial services into Neurons. |
| [Waher.Content](https://www.nuget.org/packages/Waher.Content/)                     | Pluggable architecture for accessing, encoding and decoding Internet Content. |
| [Waher.Content.Xml](https://www.nuget.org/packages/Waher.Content.Xml/)             | Content encoding and decoding of XML-related content, including ISO 8601 for date and times. |
| [Waher.Events](https://www.nuget.org/packages/Waher.Events/)                       | An extensible architecture for event logging in the application. |
| [Waher.IoTGateway](https://www.nuget.org/packages/Waher.IoTGateway/)               | Contains the [IoT Gateway](https://github.com/PeterWaher/IoTGateway) hosting environment. |
| [Waher.Networking](https://www.nuget.org/packages/Waher.Networking/)               | Tools for working with communication, including troubleshooting. |
| [Waher.Runtime.Inventory](https://www.nuget.org/packages/Waher.Runtime.Inventory/) | Maintains an inventory of type definitions in the runtime environment, and permits easy instantiation of suitable classes, and inversion of control (IoC). |
| [Waher.Runtime.Settings](https://www.nuget.org/packages/Waher.Runtime.Settings/)   | Provides easy access to persistent settings. |
| [Waher.Script](https://www.nuget.org/packages/Waher.Script/)                       | Contains an extensible script language. Includes support for easy late-bound .NET calls |

The Unit Tests further use the following libraries:

| Nuget                                                                                            | Description |
|:-------------------------------------------------------------------------------------------------|:------------|
| [Waher.Content.Html](https://www.nuget.org/packages/Waher.Content.Html/)                         | Contains encoders and decoders of HTML documents (web pages). |
| [Waher.Events.Console](https://www.nuget.org/packages/Waher.Events.Console/)                     | Outputs events logged to the console output. |
| [Waher.Persistence](https://www.nuget.org/packages/Waher.Persistence/)                           | Abstraction layer for object databases. |
| [Waher.Persistence.Files](https://www.nuget.org/packages/Waher.Persistence.Files/)               | An encrypted object database stored as local files. |
| [Waher.Runtime.Inventory.Loader](https://www.nuget.org/packages/Waher.Runtime.Inventory.Loader/) | Permits the inventory and seamless integration of classes defined in all available assemblies. |

## Installable Package

The `TAG.Payments.Transbank` project has been made into a package that can be downloaded and installed on any 
[TAG Neuron](https://lab.tagroot.io/Documentation/Index.md).
To create a package, that can be distributed or installed, you begin by creating a *manifest file*. The
`TAG.Payments.Transbank` project has a manifest file called `TAG.Payments.Transbank.manifest`. It defines the
assemblies and content files included in the package. You then use the `Waher.Utility.Install` and `Waher.Utility.Sign` command-line
tools in the [IoT Gateway](https://github.com/PeterWaher/IoTGateway) repository, to create a package file and cryptographically
sign it for secure distribution across the Neuron network.

The Transbank service is published as a package on TAG Neurons. If your neuron is connected to this network, you can install the
package using the following information:

| Package information ||
|:-----------------|:----|
| Package          | `TAG.Transbank.package`                                                                                        |
| Installation key | `Fyf4BSM1uCYXM5QSm++Zg+GUhClR4nMKxSUcpjkJqSfXrqnc/pyeGSYhu5W2V1BpFsqwCmy9JwIA5b9b381e434cf17f1edd56b47a7576ef` |
| More Information | TBD |

## Building, Compiling & Debugging

The repository assumes you have the [IoT Gateway](https://github.com/PeterWaher/IoTGateway) repository cloned in a folder called
`C:\My Projects\IoT Gateway`, and that this repository is placed in `C:\My Projects\NeuronTransbank`. You can place the
repositories in different folders, but you need to update the build events accordingly. To run the application, you select the
`TAG.Payments.Transbank` project as your startup project. It will execute the console version of the
[IoT Gateway](https://github.com/PeterWaher/IoTGateway), and make sure the compiled files of the `NeuronTransbank` solution
is run with it.

### Gateway.config

To simplify development, once the project is cloned, add a `FileFolder` reference
to your repository folder in your [gateway.config file](https://lab.tagroot.io/Documentation/IoTGateway/GatewayConfig.md). 
This allows you to test and run your changes to Markdown and Javascript immediately, 
without having to synchronize the folder contents with an external 
host, or recompile or go through the trouble of generating a distributable software 
package just for testing purposes. Changes you make in .NET can be applied in runtime
if you the *Hot Reload* permits, otherwise you need to recompile and re-run the
application again.

Example of how to point a web folder to your project folder:

```
<FileFolders>
  <FileFolder webFolder="/Transbank" folderPath="C:\My Projects\NeuronTransbank\TAG.Payments.Transbank\Root\Transbank"/>
</FileFolders>
```

**Note**: Once the file folder reference is added, you need to restart the IoT Gateway service for the change to take effect.

**Note 2**:  Once the gateway is restarted, the source for the files is in the new location. Any changes you make in the corresponding
`ProgramData` subfolder will have no effect on what you see via the browser.

**Note 3**: This file folder is only necessary on your developer machine, to give you real-time updates as you edit the files in your
developer folder. It is not necessary in a production environment, as the files are copied into the correct folders when the package 
is installed.

## Reference documentation

Reference documentation can be found on the following locations:

* [General information and overview](https://www.transbankdevelopers.cl/)
* [REST API documentation](https://www.transbankdevelopers.cl/referencia/webpay?l=http)
* [Test codes and cards](https://www.transbankdevelopers.cl/documentacion/como_empezar#codigos-de-comercio)