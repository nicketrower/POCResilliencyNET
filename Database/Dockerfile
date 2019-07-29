FROM microsoft/mssql-server-linux:2017-latest
ENV SA_PASSWORD=Pass@word
ENV ACCEPT_EULA=Y
COPY Database/entrypoint.sh entrypoint.sh
COPY Database/SqlCmdStartup.sh SqlCmdStartup.sh
COPY Database/SqlCmdScript.sql SqlCmdScript.sql
RUN chmod +x ./SqlCmdStartup.sh
CMD /bin/bash ./entrypoint.sh
