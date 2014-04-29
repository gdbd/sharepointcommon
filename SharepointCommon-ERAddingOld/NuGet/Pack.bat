xcopy ..\BUILD\14\SharepointCommon.dll SharepointCommon\lib\net35\ /Y
xcopy ..\BUILD\14\SharepointCommon.xml SharepointCommon\lib\net35\ /Y

xcopy ..\BUILD\15\SharepointCommon.dll SharepointCommon\lib\net40\ /Y
xcopy ..\BUILD\15\SharepointCommon.xml SharepointCommon\lib\net40\ /Y

nuget pack SharepointCommon\SharepointCommon.nuspec

pause