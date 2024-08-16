using TextAdventures.Quest;

namespace WebPlayer.Components;

public class UrlGameDataProvider(string url) : IGameDataProvider
{
    public async Task<IGameData?> GetData()
    {
        var client = new HttpClient();
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        
        var stream = await response.Content.ReadAsStreamAsync();
        var filename = response.RequestMessage!.RequestUri!.Segments.Last();

        return new GameData(stream, filename);
    }

    public string Url => url;
}