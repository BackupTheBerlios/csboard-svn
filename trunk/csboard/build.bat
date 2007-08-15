call compile
mkdir plugins
cd src\viewer\plugins\ecodb
call compile
copy EcoDbPlugin.dll ..\..\..\..\plugins
cd ..\..\..\..
cd src\viewer\plugins\gamedb
call compile
copy GameDbPlugin.dll ..\..\..\..\plugins
copy db4o.dll ..\..\..\..
cd ..\..\..\..