using System.Diagnostics;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Data.Models;
using Data.Models.Interfaces;
using Data.Models.Models;

namespace Data;
public class BlogApiJsonDirectAccess : IBlogApi
{
    private BlogApiJsonDirectAccessSetting _settings;

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
}