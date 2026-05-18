import os
from typing import List, Optional
from contextlib import contextmanager

import pymssql
from fastapi import FastAPI, HTTPException, Query
from pydantic import BaseModel
from sentence_transformers import SentenceTransformer

app = FastAPI(
    title="Image Catalog API",
    description="A FastAPI service that connects to the ImageDatabase on MSSQL.",
    version="0.1.0",
)


# ---------------------------------------------------------------------------
# Models
# ---------------------------------------------------------------------------

class Category(BaseModel):
    categoryId: int
    description: str


class RoomType(BaseModel):
    roomTypeId: int
    description: str


class Style(BaseModel):
    styleId: int
    description: str


class Image(BaseModel):
    id: str
    categoryId: int
    categoryDescription: Optional[str] = None
    roomTypeId: int
    roomTypeDescription: Optional[str] = None
    createdAt: Optional[str] = None
    styles: List[Style] = []


class EmbeddingRequest(BaseModel):
    text: str


class EmbeddingResponse(BaseModel):
    text: str
    embedding: List[float]
    dimensions: int


# ---------------------------------------------------------------------------
# Embedding model (loaded once at startup)
# ---------------------------------------------------------------------------

embedding_model = SentenceTransformer("all-MiniLM-L6-v2")


# ---------------------------------------------------------------------------
# Database helpers
# ---------------------------------------------------------------------------

def _get_conn_params() -> dict:
    return {
        "server": os.getenv("MSSQL_HOST", "mssql"),
        "port": os.getenv("MSSQL_PORT", "1433"),
        "user": os.getenv("MSSQL_USER", "sa"),
        "password": os.getenv("MSSQL_PASSWORD", "YourStrongPassword123!"),
        "database": os.getenv("MSSQL_DATABASE", "ImageDatabase"),
    }


@contextmanager
def get_connection():
    params = _get_conn_params()
    conn = pymssql.connect(**params)
    try:
        yield conn
    finally:
        conn.close()


def _row_to_dict(cursor, row) -> dict:
    """Convert a row tuple to a dict using cursor.description."""
    columns = [col[0] for col in cursor.description]
    return dict(zip(columns, row))


# ---------------------------------------------------------------------------
# Query functions
# ---------------------------------------------------------------------------
def query_images(limit: int, offset: int) -> List[dict]:
    with get_connection() as conn:
        with conn.cursor() as cur:
            cur.execute(
                """
                SELECT
                    i.id,
                    i.category_id,
                    c.description AS category_description,
                    i.room_type_id,
                    r.description AS room_type_description,
                    i.created_at
                FROM image i
                JOIN image_categories c ON c.category_id = i.category_id
                JOIN image_room_type r  ON r.room_type_id = i.room_type_id
                ORDER BY i.created_at DESC
                OFFSET %d ROWS FETCH NEXT %d ROWS ONLY
                """,
                (offset, limit),
            )
            images = [_row_to_dict(cur, row) for row in cur.fetchall()]

            # Attach styles to each image
            if images:
                image_ids = [img["id"] for img in images]
                placeholders = ",".join(["%s"] * len(image_ids))
                cur.execute(
                    f"""
                    SELECT iis.image_id, s.style_id, s.description
                    FROM image_image_style iis
                    JOIN image_style s ON s.style_id = iis.style_id
                    WHERE iis.image_id IN ({placeholders})
                    """,
                    tuple(str(iid) for iid in image_ids),
                )
                styles_by_image: dict = {}
                for row in cur.fetchall():
                    d = _row_to_dict(cur, row)
                    key = str(d["image_id"])
                    styles_by_image.setdefault(key, []).append(d)
                for img in images:
                    img["styles"] = styles_by_image.get(str(img["id"]), [])

            return images


def query_image(image_id: str) -> Optional[dict]:
    with get_connection() as conn:
        with conn.cursor() as cur:
            cur.execute(
                """
                SELECT
                    i.id,
                    i.category_id,
                    c.description AS category_description,
                    i.room_type_id,
                    r.description AS room_type_description,
                    i.created_at
                FROM image i
                JOIN image_categories c ON c.category_id = i.category_id
                JOIN image_room_type r  ON r.room_type_id = i.room_type_id
                WHERE i.id = %s
                """,
                (image_id,),
            )
            row = cur.fetchone()
            if row is None:
                return None
            image = _row_to_dict(cur, row)

            # Fetch styles
            cur.execute(
                """
                SELECT s.style_id, s.description
                FROM image_image_style iis
                JOIN image_style s ON s.style_id = iis.style_id
                WHERE iis.image_id = %s
                """,
                (image_id,),
            )
            image["styles"] = [_row_to_dict(cur, row) for row in cur.fetchall()]
            return image


# ---------------------------------------------------------------------------
# Normalization helpers
# ---------------------------------------------------------------------------

def normalize_image(row: dict) -> Image:
    styles = [
        Style(styleId=s["style_id"], description=s["description"])
        for s in row.get("styles", [])
    ]
    return Image(
        id=str(row["id"]),
        categoryId=row["category_id"],
        categoryDescription=row.get("category_description"),
        roomTypeId=row["room_type_id"],
        roomTypeDescription=row.get("room_type_description"),
        createdAt=str(row["created_at"]) if row.get("created_at") else None,
        styles=styles,
    )


# ---------------------------------------------------------------------------
# Endpoints
# ---------------------------------------------------------------------------

@app.get("/health")
async def health_check():
    """Check database connectivity."""
    try:
        with get_connection() as conn:
            with conn.cursor() as cur:
                cur.execute("SELECT 1")
                cur.fetchone()
        return {"status": "ok"}
    except Exception as exc:
        raise HTTPException(status_code=503, detail=str(exc))


@app.post("/embeddings", response_model=EmbeddingResponse)
async def generate_embedding(request: EmbeddingRequest):
    """Convert an input string into a 384-dimensional embedding vector."""
    try:
        vector = embedding_model.encode(request.text, show_progress_bar=False)
        embedding = [float(x) for x in vector]
        return EmbeddingResponse(
            text=request.text,
            embedding=embedding,
            dimensions=len(embedding),
        )
    except Exception as exc:
        raise HTTPException(status_code=500, detail=str(exc))


@app.get("/images", response_model=List[Image])
async def list_images(
    limit: int = Query(100, ge=1, le=1000),
    offset: int = Query(0, ge=0),
):
    """List images with joined category, room type, and styles."""
    rows = query_images(limit, offset)
    return [normalize_image(row) for row in rows]
