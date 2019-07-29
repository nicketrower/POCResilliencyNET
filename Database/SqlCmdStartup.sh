time_w=15
echo Creating Database...
#run the setup script to create the DB and the schema in the DB
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Pass@word -d master -i SqlCmdScript.sql