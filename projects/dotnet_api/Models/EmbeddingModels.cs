namespace PinterestApi.Models;

public record EmbeddingRequest(string? text);
public record PythonEmbeddingResponse(string text, float[] embedding, int dimensions);
public record SimilarityMatch(int Id, double Score);
public record SearchResponse(IList<Image> Images, string? SearchText, int? CategoryId, double? CategoryScore, int? RoomTypeId, double? RoomTypeScore, int? StyleId, double? StyleScore);
