using AIGuiders.UI.Tokens;
using AIGuiders.UI.Web.HTMX.Extensions;
using AIGuiders.UI.Web.HTMX.Rendering;
using Anpm.View.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Anpm.View;

public static class AnpmViewExtensions
{
    public static IServiceCollection AddAnpmView(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddHumanUiRazor();
        services.AddControllersWithViews()
            .AddApplicationPart(typeof(AnpmViewExtensions).Assembly)
            .AddHumanUiWebHtmx();
        services.AddRazorPages()
            .AddApplicationPart(typeof(AnpmViewExtensions).Assembly)
            .AddHumanUiWebHtmx();
        return services;
    }

    public static WebApplication MapAnpmView(this WebApplication app, string routePrefix = "/view")
    {
        var prefix = NormalizePrefix(routePrefix);

        app.MapGet(prefix, () => Results.Redirect($"{prefix}/feed"));
        app.MapGet($"{prefix}/feed", RenderFeedOverview);
        app.MapGet($"{prefix}/pins", RenderPinMatrix);
        app.MapPost($"{prefix}/sync", PostSync);

        return app;

        async Task<IResult> RenderFeedOverview(
            IAnpmViewConfig config,
            HumanUiRazorRenderService razor,
            HttpContext http)
        {
            var model = AnpmViewPresenter.BuildOverview(config, prefix);
            var body = await razor.RenderViewAsync("/Pages/Feed/Overview", model, http);
            return Html(AnpmViewHtml.Page("ANPM feed", body));
        }

        async Task<IResult> RenderPinMatrix(
            IAnpmViewConfig config,
            HumanUiRazorRenderService razor,
            HttpContext http)
        {
            var model = AnpmViewPresenter.BuildPinMatrix(config, prefix);
            var body = await razor.RenderViewAsync("/Pages/Feed/Pins", model, http);
            return Html(AnpmViewHtml.Page("ANPM pins", body));
        }

        async Task<IResult> PostSync(
            IAnpmViewConfig config,
            HumanUiRazorRenderService razor,
            HttpContext http,
            [FromForm] bool dryRun = false,
            [FromForm] bool rebuildIndex = true)
        {
            var flash = AnpmViewPresenter.TrySync(config, dryRun, rebuildIndex);
            var model = AnpmViewPresenter.BuildOverview(config, prefix, flash);
            var body = await razor.RenderViewAsync("/Pages/Feed/Overview", model, http);
            return Html(AnpmViewHtml.Page("ANPM feed", body));
        }
    }

    private static IResult Html(string html) =>
        Results.Content(html, "text/html; charset=utf-8");

    private static string NormalizePrefix(string routePrefix)
    {
        var prefix = routePrefix.Trim().TrimEnd('/');
        return string.IsNullOrEmpty(prefix) ? "/view" : prefix;
    }
}

internal static class AnpmViewHtml
{
    internal static string Page(string title, string body)
    {
        var encodedTitle = System.Net.WebUtility.HtmlEncode(title);
        return "<!DOCTYPE html>\n"
            + "<html lang=\"en\">\n<head>\n"
            + "  <meta charset=\"utf-8\" />\n"
            + "  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />\n"
            + $"  <title>{encodedTitle}</title>\n"
            + $"  <style>{HumanUiTokensCss.Content}</style>\n"
            + "  <style>\n"
            + "    .anpm-shell { max-width: 960px; margin: 0 auto; padding: 1.5rem; }\n"
            + "    .anpm-nav a { margin-right: 1rem; }\n"
            + "    .anpm-table { width: 100%; border-collapse: collapse; margin-top: 1rem; }\n"
            + "    .anpm-table th, .anpm-table td { border: 1px solid #d0d7de; padding: 0.5rem 0.75rem; text-align: left; }\n"
            + "    .anpm-badge-present { color: #1a7f37; }\n"
            + "    .anpm-badge-missing { color: #cf222e; font-weight: 600; }\n"
            + "    .anpm-badge-extra { color: #9a6700; }\n"
            + "    .anpm-flash { padding: 0.75rem 1rem; margin: 1rem 0; border-radius: 6px; background: #f6f8fa; border: 1px solid #d0d7de; }\n"
            + "    .anpm-flash-error { background: #ffebe9; border-color: #ff8182; }\n"
            + "    .anpm-meta dt { font-weight: 600; margin-top: 0.75rem; }\n"
            + "    .anpm-meta dd { margin: 0.25rem 0 0; }\n"
            + "    .anpm-actions form { display: inline-block; margin-right: 0.5rem; }\n"
            + "  </style>\n</head>\n<body>\n"
            + "  <div class=\"anpm-shell\">\n"
            + body
            + "\n  </div>\n</body>\n</html>";
    }
}
