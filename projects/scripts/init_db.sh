#!/bin/bash

for i in {1..30}; do
  /opt/mssql-tools/bin/sqlcmd -S mssql -U sa -P "$SA_PASSWORD" -i /scripts/create_img_tables.sql && \
  /opt/mssql-tools/bin/sqlcmd -S mssql -U sa -P "$SA_PASSWORD" -i /scripts/populate_img_data.sql && \
  exit 0
  echo 'Waiting for mssql...'
  sleep 5
 done

echo 'Failed to initialize database' >&2
exit 1
