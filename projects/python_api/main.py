from typing import List
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from sentence_transformers import SentenceTransformer
from keybert import KeyBERT


my_database_vocabulary = [
    "Meeting & Collaboration", "Social Culture & Support", "Primary Workspaces", "Healthcare",
    "Wellness & Recreation", "Breakout Area / Open Meeting", "Conference / Boardroom",
    "Meeting Room (Small) / Huddle", "Brainstorm / Project Room", "Pantry / Café / Kitchen",
    "Reception Area / Waiting", "Work Lounge / Staff Lounge", "Library / Quiet Zone",
    "Open-Plan Office / Benching", "Private Office / Exec Suite", "Touch Down / Hot Desk",
    "Focus Room / Phone Booth", "Home / Garden Office", "Open Space",
    "Minimalist", "Mid-Century", "Modern", "Industrial", "Luxury/Exec", "Biophilic",
]

app = FastAPI(
    title="Image Catalog API",
    description="A FastAPI service that generates text embeddings.",
    version="0.1.1",
)

class EmbeddingRequest(BaseModel):
    text: str

class EmbeddingResponse(BaseModel):
    text: str
    embedding: List[float]
    dimensions: int

class KeywordRequest(BaseModel):
    text: str

class KeywordMatch(BaseModel):
    keyword: str
    embedding: List[float]

class KeywordReponse(BaseModel):
    keywords: List[KeywordMatch]

embedding_model = SentenceTransformer("all-MiniLM-L6-v2")
kw_model = KeyBERT(model=embedding_model)

@app.get("/health")
async def health_check():
    return {"status": "ok"}

@app.post("/embeddings", response_model=EmbeddingResponse)
async def generate_embedding(request: EmbeddingRequest):
    """Convert an input string into a 384-dimensional embedding vector."""
    try:
        vector = embedding_model.encode(request.text, show_progress_bar=False)
        embedding = [float(x) for x in vector]
        return EmbeddingResponse(
            text=request.text,
            embedding=embedding,
            dimensions=len(embedding)
        )
    except Exception as exc:
        raise HTTPException(status_code=500, detail=str(exc))

@app.post("/keywords", response_model=KeywordReponse)
async def find_keywords(request: KeywordRequest):
    """Generate list of keywords from a text query."""
    try:
        keywords = kw_model.extract_keywords(
            request.text, 
            seed_keywords=my_database_vocabulary,
            keyphrase_ngram_range=(1, 2), 
            stop_words='english',
            use_mmr=True,
            diversity=0.5,
            top_n=5
        )
        kw_strings = [kw for kw, _ in keywords]
        kw_vectors = embedding_model.encode(kw_strings, show_progress_bar=False)
        return KeywordReponse(
            keywords=[
                KeywordMatch(keyword=kw, embedding=[float(x) for x in vec])
                for kw, vec in zip(kw_strings, kw_vectors)
            ]
        )
    except Exception as exc:
        raise HTTPException(status_code=500, detail=str(exc))
