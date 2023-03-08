using Microsoft.Extensions.Options;
using System.Text.Json;
using Data.Models;
using Data.Models.Interfaces;

namespace Data;
public class BlogApiJsonDirectAccess : IBlogApi
{
    private BlogApiJsonDirectAccessSetting _settings;

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
}