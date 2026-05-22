namespace PinterestApi.Models;

public record EmbeddingRequest(string? text);
public record PythonEmbeddingResponse(string text, float[] embedding, int dimensions);
