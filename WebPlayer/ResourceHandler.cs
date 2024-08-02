using Microsoft.AspNetCore.StaticFiles;
using TextAdventures.Quest;

namespace WebPlayer;

public static class ResourceHandler
{
    public static IResult GetResource(string name)
    {
        var result = PlayerHelper.GetResource(name);
        if (result != null)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(name, out var contentType);
            return Results.Content(result, contentType);
        }

        return Results.StatusCode(StatusCodes.Status404NotFound);
    }
}