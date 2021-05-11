$ExecutableName = "remote_vm_starter_listener_service.exe"

csc.exe `
    /nologo `
    /define:TRACE `
    /reference:"C:\Windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll" `
    /target:exe `
    /out:"$ExecutableName" `
    /appconfig:".\${ExecutableName}.config" `
    .\src\listener\RemoteVMStarterListenerService.cs `
    .\src\listener\RemoteVMStarterListener.cs `
    .\src\common\RemoteVMStarterConstants.cs
