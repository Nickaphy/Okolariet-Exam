# Local Docker deployment
All commands here must be run in the repository root ofc.
Keep in mind that Aspire is our development, debugging and monitoring "launcher".
While the compose is our exam requirement and the way we are showing off.
The difference is that Aspire runs seperate SQL instance in Docker but the API locally.
Compose runs both in docker (requirement).

## Local configuration
Keep the existing `JwtSettings__Secret` in the ignored `.env` file.
Add `MSSQL_SA_PASSWORD` with a password

Compose reads `.env` and passes only the configured variables to each container.
The .env file is excluded from the image by the .dockerignore.
Compose supplied ConnectionStrings__okolariet directly.

Aspire is a seperate launcher, and uses a different SQL container and volume.
Aspire is for our ease of development and not for actual exam stuff, we can tho talk about how 
we used it in development and monitoring.

## Build and start
```
docker compose up --build -d
```

The API currently has missing outbound adapters which leads to it failing to start.
This I assume will fix itself as the infrastructure is being set up.

## Portmapping and networking
API http://localhost:5080 -> Listens on port `8080` 
SQL Server localhost:14330 -> Listens on port `1433`
Prism, optional http://localhost:4010 -> Listens on port `4010`

## Stop and preserve data
```bash
docker compose down
```
