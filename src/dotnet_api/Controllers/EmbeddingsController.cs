using Microsoft.AspNetCore.Mvc;
using PinterestApi.Models;

namespace PinterestApi.Controllers;

[ApiController]
[Route("api/embeddings")]
public class EmbeddingsController(IHttpClientFactory httpClientFactory) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Generate(EmbeddingRequest req)
    {
        var client = httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync("http://python_api:8000/embeddings", new { text = req.text });

        if (!response.IsSuccessStatusCode)
            return Problem("Failed to generate embedding from Python API.");

        var result = await response.Content.ReadFromJsonAsync<PythonEmbeddingResponse>();
        return Ok(result);
    }
}
