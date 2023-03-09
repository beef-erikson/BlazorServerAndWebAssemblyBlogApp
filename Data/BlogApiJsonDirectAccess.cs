using System.Diagnostics;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Data.Models;
using Data.Models.Interfaces;
using Data.Models.Models;

namespace Data;
public class BlogApiJsonDirectAccess : IBlogApi
{
    private readonly BlogApiJsonDirectAccessSetting _settings;

    private List<BlogPost>? _blogPosts;
    private List<Category>? _categories;
    private List<Tag>? _tags;

    /// <summary>
    /// Creates directory structure for Blog Posts, Categories and Tags.
    /// </summary>
    /// <param name="option"></param>
    public BlogApiJsonDirectAccess(IOptions<BlogApiJsonDirectAccessSetting> option)
    {
        _settings = option.Value;
        
        // Creates directories if not present
        if (!Directory.Exists((_settings.DataPath)))
        {
            Directory.CreateDirectory(_settings.DataPath);
        }

        if (!Directory.Exists($@"{_settings.DataPath}/{_settings.BlogPostsFolder}"))
        {
            Directory.CreateDirectory($@"{_settings.DataPath}/{_settings.BlogPostsFolder}");
        }

        if (!Directory.Exists($@"{_settings.DataPath}/{_settings.CategoriesFolder}"))
        {
            Directory.CreateDirectory($@"{_settings.DataPath}/{_settings.CategoriesFolder}");
        }

        if (!Directory.Exists($@"{_settings.DataPath}/{_settings.TagsFolder}"))
        {
            Directory.CreateDirectory($@"{_settings.DataPath}/{_settings.TagsFolder}");
        }
    }

    /// <summary>
    /// Writes to the referenced list cached file(s) from the provided folder
    /// </summary>
    /// <param name="list">List to add entries to.</param>
    /// <param name="folder">Folder to parse entries for.</param>
    /// <typeparam name="T">BlogPost, Tag, Category</typeparam>
    private void Load<T>(ref List<T>? list, string folder)
    {
        if (list != null) return;
        
        // Reads all files from folder and adds any data found to list
        list = new List<T>();
        var fullPath = $@"{_settings.DataPath}/{folder}";
        foreach (var file in Directory.GetFiles(fullPath))
        {
            var json = File.ReadAllText(file);
            var bp = JsonSerializer.Deserialize<T>(json);
            if (bp != null)
            {
                list.Add(bp);
            }
        }
    }

    /// <summary>
    /// Caches all blog posts to _blogPosts list.
    /// </summary>
    /// <returns>Completed task.</returns>
    private Task LoadBlogPostsAsync()
    {
        Load<BlogPost>(ref _blogPosts, _settings.BlogPostsFolder);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Caches all tags to _tags list.
    /// </summary>
    /// <returns>Completed task.</returns>
    private Task LoadTagsAsync()
    {
        Load<Tag>(ref _tags, _settings.TagsFolder);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Caches all categories to _categories list.
    /// </summary>
    /// <returns>Completed task.</returns>
    private Task LoadCategoriesAsync()
    {
        Load<Category>(ref _categories, _settings.CategoriesFolder);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Generic that checks list for item, adds if not present, writes to filename.
    /// </summary>
    /// <param name="list">List to parse.</param>
    /// <param name="folder">Folder to write into.</param>
    /// <param name="filename">Filename to write to.</param>
    /// <param name="item">Item to add if not present to list.</param>
    /// <typeparam name="T">Generic for various blog functions.</typeparam>
    private async Task SaveAsync<T>(List<T>? list, string folder, string filename, T item)
    {
        var filepath = $@"{_settings.DataPath}/{folder}/{filename}";

        await File.WriteAllTextAsync(filepath, JsonSerializer.Serialize<T>(item));

        list ??= new List<T>();

        if (!list.Contains(item))
        {
            list.Add(item);
        }
    }

    /// <summary>
    /// Deletes JSON file.
    /// </summary>
    /// <param name="list"></param>
    /// <param name="folder">Folder where json resides.</param>
    /// <param name="id">Filename to delete.</param>
    /// <typeparam name="T"></typeparam>
    private void DeleteAsync<T>(List<T>? list, string folder, string id)
    {
        var filepath = $@"{_settings.DataPath}/{folder}/{id}.json";

        try
        {
            File.Delete(filepath);
        }
        catch
        {
        }
    }

    /// <summary>
    /// Loads blog posts and returns list is not null or new blogpost list.
    /// </summary>
    /// <param name="numberOfPosts"></param>
    /// <param name="startIndex"></param>
    /// <returns></returns>
    public async Task<List<BlogPost>?> GetBlogPostsAsync(int numberOfPosts, int startIndex)
    {
        await LoadBlogPostsAsync();
        return _blogPosts ?? new List<BlogPost>();
    }

    /// <summary>
    /// Loads blog post list and returns BlogPost based on the string id, if found.
    /// </summary>
    /// <param name="id">id of blogpost to retrieve from list.</param>
    /// <returns>BlogPost</returns>
    /// <exception cref="Exception">_blogPost == null</exception>
    public async Task<BlogPost?> GetBlogPostsAsync(string id)
    {
        await LoadBlogPostsAsync();

        if (_blogPosts == null)
        {
            throw new Exception("Blog posts not found");
        }

        return _blogPosts.FirstOrDefault(b => b.Id == id);
    }

    /// <summary>
    /// Populates _blogPost list and returns count.
    /// </summary>
    /// <returns>Number of BlogPost members.</returns>
    public async Task<int> GetBlogPostCountAsync()
    {
        await LoadBlogPostsAsync();

        return _blogPosts?.Count ?? 0;
    }

    /// <summary>
    /// Loads categories in list and returns list if present; new list otherwise.
    /// </summary>
    /// <returns>Categories list.</returns>
    public async Task<List<Category>?> GetCategoriesAsync()
    {
        await LoadCategoriesAsync();
        return _categories ?? new List<Category>();
    }

    /// <summary>
    /// Loads categories into list and returns category Id if present  
    /// </summary>
    /// <param name="id">Id to review from list.</param>
    /// <returns>_categories.Id of id if found.</returns>
    /// <exception cref="Exception"></exception>
    public async Task<Category?> GetCategoryAsync(string id)
    {
        await LoadCategoriesAsync();

        if (_categories == null)
        {
            throw new Exception("Categories not found.");
        }

        return _categories.FirstOrDefault(b => b.Id == id);
    }
}