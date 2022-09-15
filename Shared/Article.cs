using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Shared
{
    public class Article : CosmosDBEntity
    {
        [JsonPropertyName("articleKey")]
        public string ArticleKey { get; set; }
        [JsonPropertyName("title"), MaxLength(120, ErrorMessage = "Titel zu lang.")]
        public string Title { get; set; }
        [JsonPropertyName("articleText"), Display(Name = "Text"), MaxLength(10000, ErrorMessage = "Der Text ist zu lang.")]
        public string ArticleText { get; set; }
    }
}
