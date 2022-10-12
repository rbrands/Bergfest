using System;
using System.Text;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using BlazorApp.Shared;

namespace BlazorApp.Shared
{
    public class InfoItem : CosmosDBEntity
    {
        [JsonPropertyName("orderId"), Range(-1000.0, 1000.0, ErrorMessage = "Ordnungszahl zur Steuerung der Reihenfolge nicht im gültigen Bereich."), Display(Name = "Ordnungszahl zur Steuerung der Reihenfolge.", Prompt = "Ordnungszahl zur Steuerung der Reihenfolge"), Required(ErrorMessage = "Ordnungszahl zur Steuerung der Reihenfolge eingeben.")]
        public int OrderId { get; set; } = 100;
        [JsonPropertyName("headerTitle"), Display(Name = "Kopf-Titel", Prompt = "Kopf-Titel der Info"), MaxLength(120, ErrorMessage = "Kopf-Titel zu lang."), Required(ErrorMessage = "Bitte Kopf-Titel eingeben.")]
        public string HeaderTitle { get; set; }
        [JsonPropertyName("title"), Display(Name = "Titel", Prompt = "Titel der Info"), MaxLength(120, ErrorMessage = "Titel zu lang.")]
        public string Title { get; set; }
        [JsonPropertyName("subTitle"), Display(Name = "Sub-Titel", Prompt = "Zweite Überschrift"), MaxLength(120, ErrorMessage = "Sub-Titel zu lang.")]
        public string SubTitle { get; set; }
        [JsonPropertyName("link"), MaxLength(250, ErrorMessage = "Link zu lang"), UIHint("url")]
        public string Link { get; set; }
        [JsonPropertyName("linkTitle"), MaxLength(60, ErrorMessage = "Link-Bezeichnung zu lang.")]
        public string LinkTitle { get; set; }
        [JsonPropertyName("linkImage"), MaxLength(512, ErrorMessage = "Link zu lang"), UIHint("url")]
        public string LinkImage { get; set; }
        [JsonPropertyName("infoContent"), Display(Name = "Inhalt", Prompt = "Inhalt der Info-Card"), MaxLength(5000, ErrorMessage = "Info zu lang.")]
        public string InfoContent { get; set; }
        [JsonPropertyName("infoLifeTimeInDays"), Range(0.0, 100.0, ErrorMessage = "Lebensdauer der Info nicht im gültigen Bereich."), Display(Name = "Lebensdauer der Info", Prompt = "Wie viel Tage soll die Info gespeichert werden? (0 für keine automatische Löschung."), Required(ErrorMessage = "Lebensdauer für die Info eingeben.")]
        public int InfoLifeTimeInDays { get; set; } = 0;
        [JsonPropertyName("challengeId"), MaxLength(128, ErrorMessage = "ChallengId zu lang")]
        public string ChallengeId { get; set; }
        public string DisplayTitle
        {
            get
            {
                string title = this.Title;
                if (String.IsNullOrEmpty(title))
                {
                    title = this.HeaderTitle;
                }
                if (String.IsNullOrEmpty(title))
                {
                    title = String.Empty;
                }
                return title;
            }
        }

    }
}
