using Microsoft.JSInterop;

namespace BlazorApp.Client.Utils
{
    /// <summary>
    /// Service to copy text into the clipboard. 
    /// Follows the approach described in https://www.puresourcecode.com/dotnet/blazor/copy-to-clipboard-component-for-blazor/
    /// and https://github.com/erossini/BlazorCopyToClipboard
    /// Add builder.Services.AddScoped<ClipboardService>(); in Program.cs
    /// </summary>
    public class ClipboardService
    {
        private readonly IJSRuntime _jsRuntime;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClipboardService"/> class.
        /// </summary>
        /// <param name="jsRuntime">The js runtime.</param>
        public ClipboardService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public ValueTask WriteTextAsync(string text)
        {
            return _jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
        }
    }
}
