using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ssomero.Api.Services;

public class InvitationEmailTemplateService
{
    private readonly ILogger<InvitationEmailTemplateService> _logger;
    private readonly string _templatesPath;

    public InvitationEmailTemplateService(ILogger<InvitationEmailTemplateService> logger)
    {
        _logger = logger;
        _templatesPath = Path.Combine(System.AppContext.BaseDirectory, "Templates");
    }

    public async Task<string> RenderTemplateAsync(string templateName, IDictionary<string, string?> tokens)
    {
        var file = Path.Combine(_templatesPath, templateName + ".html");
        if (!File.Exists(file))
        {
            _logger.LogError("Template not found: {File}", file);
            throw new FileNotFoundException("Template not found", file);
        }

        var content = await File.ReadAllTextAsync(file);

        // Replace tokens in the form {{TokenName}} safely (encode values)
        foreach (var kv in tokens)
        {
            var safe = System.Net.WebUtility.HtmlEncode(kv.Value ?? string.Empty);
            var pattern = Regex.Escape("{{") + Regex.Escape(kv.Key) + Regex.Escape("}}");
            content = Regex.Replace(content, pattern, safe);
        }

        return content;
    }
}
