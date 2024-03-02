
set "__VS=Visual Studio 16 2019"
set "__VSWhere=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
set "__VSDISPLAY="
set "__VSVER="
if exist "%__VSWhere%" (
    for /f "tokens=*" %%p in (
		'"%__VSWhere%" -latest -property catalog_productLineVersion'
	) do set __VSDISPLAY=%%p

	for /f "tokens=*" %%p in (
		'"%__VSWhere%" -latest -property catalog_productDisplayVersion'
	) do set __VSVER=%%p
	
)
if "%__VSVER%" neq "" (	
	set __VS=Visual Studio %__VSVER:~0,2% %__VSDisplay%
)

mkdir build32 & pushd build32
cmake -G "%__VS%" -A Win32 ..
popd
cmake --build build32 --config Release
md plugin_lua53\Plugins\x86
copy /Y build32\Release\xlua.dll plugin_lua53\Plugins\x86\xlua.dll
pause