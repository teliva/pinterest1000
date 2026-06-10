# Pinterest Starter
Project that will contaion 1000 images that can be used as feeder images to start the ideation process for KITS Assist.
You can filter categorized images hosted on the content server.

## Projects
### App
A web application for displaying search results and interating with the dotnet api: [here](http://localhost:8082) 

Application uses basic bootstrap and javascript - the UX is not finalized.

### .NET API
Gateway api to interact with the database and the python api.  This code will be moved to KITSWebApi.  Makes requests to the python api as well as the MSSQL database.

### Python API
Service for producing embeddings used for natural language search.

### MSSQL Database
MSSQL2025 DB that stores the UUID that is linked to the content server image and its category tags. Supports cosine similarity to support natural language search.

### Content server
A contains simulates our content server that will carry the images that can be referenced and sent to the client.

## Setup
Run the containers and use the web app to interact with the .net api
```
docker compose up
```
Docker will  automatically spin up and populate the DB with the appropriate data.