del "*.suo" /S/Q/F/A
del "*.csproj.user" /S/Q/F/A
rd ".vs" /S/Q

for /f "usebackq" %%f in (`"dir /ad/b/s obj"`) do rd "%%f" /S/Q
for /f "usebackq" %%f in (`"dir /ad/b/s bin"`) do rd "%%f" /S/Q
