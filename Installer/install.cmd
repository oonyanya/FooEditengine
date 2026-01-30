setlocal

set IDE_PATH=C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE
set BUILD_PATH=C:\Program Files\Microsoft Visual Studio\18\Community\Msbuild\Current\Bin
set BUILD_TYPE=%1
set CPU_TYPE=Any CPU
set BATCH_FILE_FOLDER=%~dp0

if "%1"=="" set BUILD_TYPE=Release

md dist

pushd ..\UnitTest
"%BUILD_PATH%\msbuild" -t:clean;rebuild -p:Configuration=Debug
@dotnet test -c Debug
if errorlevel 1 (
  popd
  goto end
) else (
  popd
)

pushd ..\Windows\FooTextBox
"%BUILD_PATH%\msbuild" -t:pack -p:Configuration=%BUILD_TYPE%"
copy bin\%BUILD_TYPE%\*.nupkg "%BATCH_FILE_FOLDER%dist"
popd

pushd ..\DotNetTextStore
"%BUILD_PATH%\msbuild" -t:pack -p:Configuration=%BUILD_TYPE%"
copy bin\%BUILD_TYPE%\*.nupkg "%BATCH_FILE_FOLDER%dist"
popd

pushd ..\WPF\FooTextBox
"%BUILD_PATH%\msbuild" -t:pack -p:Configuration=%BUILD_TYPE%"
copy bin\%BUILD_TYPE%\*.nupkg "%BATCH_FILE_FOLDER%dist"
popd

pushd ..\UWP\FooTextBox
"%BUILD_PATH%\msbuild" -t:pack -p:Configuration=%BUILD_TYPE%"
copy bin\%BUILD_TYPE%\*.nupkg "%BATCH_FILE_FOLDER%dist"
popd

pushd ..\WinUI\FooTextBox
"%BUILD_PATH%\msbuild" -t:pack -p:Configuration=%BUILD_TYPE%"
copy bin\%BUILD_TYPE%\*.nupkg "%BATCH_FILE_FOLDER%dist"
copy bin\%BUILD_TYPE%\*.snupkg "%BATCH_FILE_FOLDER%dist"
popd

pushd ..\FooList\List
"%BUILD_PATH%\msbuild" -t:pack -p:Configuration=%BUILD_TYPE%"
copy bin\%BUILD_TYPE%\*.nupkg "%BATCH_FILE_FOLDER%dist"
copy bin\%BUILD_TYPE%\*.snupkg "%BATCH_FILE_FOLDER%dist"
popd

:end
endlocal
pause
