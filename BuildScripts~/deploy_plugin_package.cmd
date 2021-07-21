@echo off

cd..

rmdir /s /q .\Deploy

mkdir Deploy\package
cd Deploy\package

mkdir Documentation~
xcopy ..\..\Documentation~ Documentation~ /E

mkdir Editor
xcopy ..\..\Editor Editor /E

mkdir Runtime
xcopy ..\..\Runtime Runtime /E

mkdir Samples~
xcopy ..\..\Samples~ Samples~ /E

mkdir Tests
xcopy ..\..\Tests Tests /E

copy ..\..\*.meta .\
copy ..\..\*.md .\
copy ..\..\*.json .\

cd ..

del /s /q *.pdb

7z -ttar a dummy .\* -so | 7z -si -tgzip a com.faceplay.webrtc.tgz

copy com.faceplay.webrtc.tgz ..\com.faceplay.webrtc.tgz

cd ..

rmdir /s /q .\Deploy