xcopy ..\BUILD\14\SharepointCommon.dll SharepointCommon\lib\net35\ /Y
xcopy ..\BUILD\14\SharepointCommon.xml SharepointCommon\lib\net35\ /Y
																 
xcopy ..\BUILD\15\SharepointCommon.dll SharepointCommon\lib\net45\ /Y
xcopy ..\BUILD\15\SharepointCommon.xml SharepointCommon\lib\net45\ /Y
																 
xcopy ..\BUILD\16\SharepointCommon.dll SharepointCommon\lib\net46\ /Y
xcopy ..\BUILD\16\SharepointCommon.xml SharepointCommon\lib\net46\ /Y


nuget pack SharepointCommon\SharepointCommon.nuspec

pause