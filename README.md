# Remote VM Starter

## Overview

This project consists of a Windows service and a client that allows remote users to start Hyper-V virtual machines.

```
--------------------                   ------------------
|  Listener Host   |                   |  Client Host   |
|  --------------  |                   |  ------------  |
|  |  Listener  |== Port <=============|  |  Client  |  |
|  |  Service   |  |                   |  ------------  |
|  --------------  |                   ------------------
|     |            |
|     v            |
|  --------------  |
|  |  VM        |  |
|  --------------  |
--------------------
```

The process begins with the client. It needs 3 pieces of information to communicate with the listener service:

1. IPv4 address of the listener host,
2. the port on which the listener service is using, and
3. a string representation of a byte, i.e., `0` to `255`.

Upon execution with the appropriate parameters, the client will establish a connection to the listener service and deliver 1 byte. The listener will map the byte to a "dictionary" to find the corresponding VM(s) to start; depending on the configuration, the byte can be mapped to more than one VMs.

## Important Notes

- Information in this README is written based on Windows 10 version 2004 (build 19041.928); please adapt as necessary.
- It is intentional to build all components of this project with **only** built-in tools on the OS (except the IDE because Notepad heavily hindered productivity).
- One reason for writing this remote VM starter as opposed to using the (old) built-in SSH server is to avoid the excessive features.

## Build

### `csc.exe` (built-in C# compiler)

To enable calling `csc.exe` directly in PowerShell, add `C:\WINDOWS\Microsoft.NET\Framework64\v4.0.30319` to the **build user's** `Path` environmental variable. The reason for adding the directory to the `Path` of the user responsible for building is to avoid polluting the system's `Path` variable.

As noted in the compiler's help message, it only supports up to C# version 5; but the functionalities it offers are sufficient.

### Compile With PowerShell Scripts

There are several helper scripts prefixed with `compile_` for building the service and client executables. All of them uses `csc.exe` with the appropriate parameters to produce the executables.

All scripts are based on PowerShell version `5.1.19041.906`; check with the `Get-Host` command in a PowerShell terminal for comparison.

#### `compile_client`

Compiles the client with:

1. `/define:TRACE` to enable `System.Diagnostics.Trace`, and
2. `/appconfig:"*.config"` to add an **optional** config file for directing logs to the console by adding a `System.Diagnostics.ConsoleTraceListener`. `/appconfig:...` must be removed (or commented-out) to omit the config file.

#### `compile_listener_service`

Compiles the listener as a Windows service with:

1. `/define:TRACE` to enable `System.Diagnostics.Trace`,
2. `/reference:"...\System.Management.Automation.dll"` to allow PowerShell host creation within the listener, and
3. `/appconfig:"...config"` to add a **mandatory** config file for specifying the address and port, the "byte to VM name(s)" map, and trace listeners.

#### `compile_listener_test`

Compiles the test executable for testing the listener and client modules together.

### Config Templates

All config templates should end with `.config-template`; they must be renamed to end with `.config` **and** placed in the same directory as their corresponding executable in order to take effect.

An example directory structure of the listener service:

```
C:
|-- User
    |-- RemoteVMStarterService
        |-- listener_service.exe
        |-- listener_service.exe.config
```

## Listener Service

Both the install and uninstall process requires using `sc.exe` in an **elevated** Command Prompt and PowerShell instance.

### Install

Use the following command and provide the path to the listener service executable where indicted:

```ps1
sc.exe `
    create `
    RemoteVMStarterService `
    binPath= "[Insert absolute path to listener service executable]" `
    DisplayName= "Remote VM Starter Service"
```

Notes:

- The name of the service should match the `ServiceName` property which is set in the constructor of the service class.

#### **Remember To Add A Firewall Rule**

Before starting the service, it is necessary to add a firewall rule to allow inbound communication on the port that the listener is configured to use. It is also highly recommended to setup the rule with a restricted source, i.e., only allow connections from the same subnet as the listener host.

### Uninstall

```ps1
sc.exe `
    delete `
    RemoteVMStarterService
```

## Client

After compiling the client, the resulting executable can be placed on a machine that will be used to start VM(s) remotely.

It can be used from a PowerShell instance:

```
client.exe [Destination address] [Destination port] [VM key (1 byte)]
```

Notes:

- Failure to provide sufficient and valid parameters will result in no error messages if the client has no config file (see explanation in [its compile script](#compile_client)).

## Security Considerations

### There Is **No Encryption**

The service and client makes use of `System.Net.Sockets.TcpListener` and `System.Net.Sockets.TcpClient` in the .NET Framework. Since the data transmission is not encrypted, it is highly recommended to only use these tools within a well-protected network.

### Details Are Hidden From The Client

The client doesn't know which VM(s) are mapped to a byte because only the user needs to know that; this allows specifying the VM(s) without exposing their names. On the other side, the listener is designed to not send any data to the client to avoid exposing information on the system that is hosting the listener.
