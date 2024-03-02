
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


mkdir build64_54 & pushd build64_54
cmake -DLUA_VERSION=5.4.1 -G "%__VS%" -A x64  ..
popd
cmake --build build64_54 --config Release
md plugin_lua54\Plugins\x86_64
copy /Y build64_54\Release\xlua.dll plugin_lua54\Plugins\x86_64\xlua.dll

mkdir build32_54 & pushd build32_54
cmake -DLUA_VERSION=5.4.1 -G "%__VS%" -A Win32 ..
popd
cmake --build build32_54 --config Release
md plugin_lua54\Plugins\x86
copy /Y build32_54\Release\xlua.dll plugin_lua54\Plugins\x86\xlua.dll

pause