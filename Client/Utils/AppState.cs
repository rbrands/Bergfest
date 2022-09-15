using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using BlazorApp.Shared;

namespace BlazorApp.Client.Utils
{
    /// <summary>
    /// Use AppState pattern to hold state across all components
    /// </summary>
    public class AppState
    {
        public IDictionary<string, Article> ArticleCache { get; set; } = new Dictionary<string, Article>();

 
        public event Action? OnChange;
        public bool NotificationSubscriptionRequested { get; set; } = false;
        public void NotifyStateChanged() => OnChange?.Invoke();

        public AppState()
        {
        }
    }
}
