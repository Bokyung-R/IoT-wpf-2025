using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MovieFinder2025.Models
{
    public class MovieSearchResponse
    {
        [JsonPropertyName("results")]
        public List<MovieItem> Results { get; set; }
    }
}
