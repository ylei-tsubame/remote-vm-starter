$ExecutableName = "remote_vm_starter_client.exe"

csc.exe `
    /nologo `
    /define:TRACE `
    /target:exe `
    /out:"$ExecutableName" `
    /appconfig:".\${ExecutableName}.config" `
    .\src\client\RemoteVMStarterClientMain.cs `
    .\src\client\RemoteVMStarterClient.cs `
    .\src\common\RemoteVMStarterConstants.cs
