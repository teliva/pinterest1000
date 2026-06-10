namespace PinterestApi.Models;

public record EmbeddingRequest(string? text, int? categoryId, int? roomTypeId, int? styleId);
public record PythonEmbeddingResponse(string text, float[] embedding, int dimensions);
public record KeywordMatch(string keyword, float[] embedding);
public record PythonKeyWordsResponse(KeywordMatch[] keywords);
public record SimilarityMatch(int Id, double Score);
public record SpBestMatchResult(int? BestCategoryId, double? BestCategoryScore, int? BestRoomTypeId, double? BestRoomTypeScore, int? BestStyleId, double? BestStyleScore);
public record SearchResponse(IList<Image> Images, string? SearchText, int? CategoryId, double? CategoryScore, string? CategoryDescription, int? RoomTypeId, double? RoomTypeScore, string? RoomTypeDescription, int? StyleId, double? StyleScore, string? StyleDescription, string[] keywords);
