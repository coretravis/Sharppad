using System.Text.Json;
using SharpPad.Shared.Models.Nuget;
using SharpPad.Client.Services.Caching;

namespace SharpPad.Client.Services.Integrations
{
    /// <summary>
    /// Service for searching NuGet packages.
    /// </summary>
    public class NuGetSearchService(IClientCache cache) : INuGetSearchService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly IClientCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        private const string NuGetApiUrl = "https://azuresearch-usnc.nuget.org/query";
        private const int CacheExpiry = 5;

        /// <summary>
        /// Searches for NuGet packages based on the specified search term.
        /// </summary>
        public async Task<IEnumerable<NugetPackage>> SearchPackagesAsync(
            string searchTerm,
            int skip = 0,
            int take = 20,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                throw new ArgumentException("Search term cannot be empty.", nameof(searchTerm));
            }

            var cacheKey = $"NugetSearch_{searchTerm}_{skip}_{take}";
            if (_cache.TryGetValue(cacheKey, out IEnumerable<NugetPackage> cachedPackages))
            {
                return cachedPackages;
            }

            var requestUrl = $"{NuGetApiUrl}?q={Uri.EscapeDataString(searchTerm)}&skip={skip}&take={take}";

            HttpResponseMessage responseMessage;
            try
            {
                responseMessage = await _httpClient.GetAsync(requestUrl, cancellationToken);
                responseMessage.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching data from NuGet API: {ex.Message}", ex);
            }

            var responseContent = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var packages = new List<NugetPackage>();

            try
            {
                using var document = JsonDocument.Parse(responseContent);
                if (document.RootElement.TryGetProperty("data", out JsonElement dataElement) &&
                    dataElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var packageElement in dataElement.EnumerateArray())
                    {
                        // Process authors which might be an array or a string.
                        List<string> authors = new();
                        if (packageElement.TryGetProperty("authors", out JsonElement authorsElement))
                        {
                            if (authorsElement.ValueKind == JsonValueKind.Array)
                            {
                                authors = authorsElement.EnumerateArray()
                                    .Select(a => a.GetString() ?? string.Empty)
                                    .Where(a => !string.IsNullOrEmpty(a))
                                    .ToList();
                            }
                            else if (authorsElement.ValueKind == JsonValueKind.String)
                            {
                                var authorString = authorsElement.GetString();
                                if (!string.IsNullOrEmpty(authorString))
                                {
                                    authors.Add(authorString);
                                }
                            }
                        }

                        packages.Add(new NugetPackage
                        {
                            Id = packageElement.GetProperty("id").GetString() ?? string.Empty,
                            Version = packageElement.GetProperty("version").GetString() ?? string.Empty,
                            Description = packageElement.GetProperty("description").GetString() ?? string.Empty,
                            Authors = authors,
                            Summary = packageElement.GetProperty("summary").GetString() ?? string.Empty,
                            Title = packageElement.TryGetProperty("title", out JsonElement titleElement) &&
                                    !string.IsNullOrWhiteSpace(titleElement.GetString())
                                    ? titleElement.GetString() ?? string.Empty
                                    : packageElement.GetProperty("id").GetString() ?? string.Empty,
                            TotalDownloads = packageElement.GetProperty("totalDownloads").GetInt64()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing NuGet API response: {ex.Message}", ex);
            }

            // Cache search results 
            _cache.Set(cacheKey, packages, TimeSpan.FromMinutes(CacheExpiry));
            return packages;
        }

        /// <summary>
        /// Retrieves all available versions for the specified package.
        /// </summary>
        public async Task<IEnumerable<string>> GetPackageVersionsAsync(
            string packageId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(packageId))
            {
                throw new ArgumentException("Package id cannot be empty.", nameof(packageId));
            }

            var normalizedPackageId = packageId.ToLowerInvariant();
            var cacheKey = $"NugetVersions_{normalizedPackageId}";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<string> cachedVersions))
            {
                return cachedVersions;
            }

            var requestUrl = $"https://api.nuget.org/v3-flatcontainer/{normalizedPackageId}/index.json";

            HttpResponseMessage responseMessage;
            try
            {
                responseMessage = await _httpClient.GetAsync(requestUrl, cancellationToken);
                responseMessage.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching package versions from NuGet API: {ex.Message}", ex);
            }

            var responseContent = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            List<string> versions = new();

            try
            {
                using var document = JsonDocument.Parse(responseContent);
                if (document.RootElement.TryGetProperty("versions", out JsonElement versionsElement) &&
                    versionsElement.ValueKind == JsonValueKind.Array)
                {
                    versions = versionsElement.EnumerateArray()
                        .Select(v => v.GetString() ?? string.Empty)
                        .Where(v => !string.IsNullOrEmpty(v))
                        .OrderByDescending(v => v)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing package versions for package '{packageId}': {ex.Message}", ex);
            }

            // Cache the versions result for 60 minutes.
            _cache.Set(cacheKey, versions, TimeSpan.FromMinutes(60));
            return versions;
        }
    }
}
