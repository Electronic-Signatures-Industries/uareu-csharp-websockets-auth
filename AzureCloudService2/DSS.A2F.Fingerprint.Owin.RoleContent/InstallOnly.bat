@echo off

set TEMP=%DPUareUInstall%
md "%TEMP%\log"
REM setup.exe /s /v"/qn /l*v %temp%\ururte_install.log"

REM Uncomment the line below to install without Authentication Services
REM
setup.exe /s /v"REBOOT=ReallySuppress ADDLOCAL=AlwaysInstalled,DotNet,Java,JavaPOS,OPOS /qn /l*v %TEMP%\log\ururte_install.log"

REM Uncomment the line below to disable reboot after installation
REM
REM		setup.exe /s /v"REBOOT=ReallySuppress /qn /l*v %temp%\ururte_install.log"


@pause

REM this batch file will perform a silent install