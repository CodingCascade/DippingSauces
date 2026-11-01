using System.Text.Json;
using DippingList.Models;

namespace DippingList.Services;

public sealed class JsonDippingListStore : IDippingListStore
{
    private readonly string _path;
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    public JsonDippingListStore(string path)
    {
        _path = Path.GetFullPath(path);
    }

    public IReadOnlyList<DippingItem> Load()
    {
        string json;
        try
        {
            json = File.ReadAllText(_path);
        }
        catch (FileNotFoundException)
        {
            return Array.Empty<DippingItem>();
        }
        catch (DirectoryNotFoundException)
        {
            return Array.Empty<DippingItem>();
        }

        List<DippingItem> items;
        try
        {
            items = JsonSerializer.Deserialize<List<DippingItem>>(
                json, _options)
                ?? throw new InvalidDataException(
                    "The saved list must be a JSON array.");
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException(
                "The saved list contains invalid JSON.", ex);
        }

        Validate(items);
        return items;
    }

    public void Save(IReadOnlyList<DippingItem> items)
    {
        Validate(items);
        string directory = Path.GetDirectoryName(_path)!;
        Directory.CreateDirectory(directory);
        string temporaryPath = Path.Combine(directory,
            $".{Guid.NewGuid():N}.tmp");

        try
        {
            string json = JsonSerializer.Serialize(items, _options);
            File.WriteAllText(temporaryPath, json);
            File.Move(temporaryPath, _path, overwrite: true);
        }
        finally
        {
            // Best-effort cleanup must not hide the save error.
            try
            {
                File.Delete(temporaryPath);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }

    private static void Validate(
        IReadOnlyList<DippingItem> items)
    {
        var ids = new HashSet<Guid>();
        foreach (var item in items)
        {
            if (item is null || item.Id == Guid.Empty ||
                !ids.Add(item.Id) ||
                string.IsNullOrWhiteSpace(item.Name) ||
                item.Name.Length > 80 ||
                item.Name != item.Name.Trim())
            {
                throw new InvalidDataException(
                    "The saved list contains an invalid item.");
            }
        }
    }
}
