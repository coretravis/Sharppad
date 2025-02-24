using Microsoft.JSInterop;

namespace SharpPad.Client.Services.Storage;

public class LocalStorageService(IJSRuntime jsRuntime) : ISimpleStorage
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;

    public async Task<string> GetAsync(string key)
    {
        return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
    }

    public async Task SetAsync(string key, string value)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
    }

    public async Task RemoveAsync(string key)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }

    public string Get(string key)
    {
        throw new NotImplementedException();
    }

    public void Set(string key, string value)
    {
        throw new NotImplementedException();
    }

    public void Remove(string key)
    {
        throw new NotImplementedException();
    }
}
