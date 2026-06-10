# Image Catalog API

A FastAPI-based Python API that connects to the MSSQL `ImageDatabase`.

## Build and run with Docker Compose

From the repository root:

```bash
docker-compose up --build python_api
```

Then open:

- `http://localhost:8084/health`
- `http://localhost:8084/images`
- `http://localhost:8084/images/{image_id}`
- `http://localhost:8084/categories`
- `http://localhost:8084/room-types`
- `http://localhost:8084/styles`
- `http://localhost:8084/docs` — interactive Swagger UI

## Environment

The service uses the following environment variables (with defaults):

| Variable | Default |
|---|---|
| `MSSQL_HOST` | `mssql` |
| `MSSQL_PORT` | `1433` |
| `MSSQL_USER` | `sa` |
| `MSSQL_PASSWORD` | `YourStrongPassword123!` |
| `MSSQL_DATABASE` | `ImageDatabase` |
