# Pinterest

## PostgreSQL Docker Setup

A PostgreSQL database container is configured in `docker-compose.yml`.

### Database details

- Database: `postes`
- User: `postgres`
- Password: `postgres`
- Port: `5432`

### Run the database container

```bash
docker compose up -d
```

### Stop the container

```bash
docker compose down
```

### Connect to the database

You can connect using any PostgreSQL client at `localhost:5432`.

## Content polling API container

A second container serves the local `content` folder at `http://localhost:8080`.

### API endpoints

- `GET /health` — service health check
- `GET /files` — list all files under `content`
- `GET /poll` — equivalent folder polling endpoint
- `GET /content/<path>` — return file contents

### Run both containers

```bash
docker compose up -d
```

### Access the content API

Open `http://localhost:8081/files` or `http://localhost:8081/poll`.

## Simple webpage container

A third container serves the static `src/app/index.html` webpage at `http://localhost:8082`.

### Access the webpage

Open `http://localhost:8082` in your browser.
