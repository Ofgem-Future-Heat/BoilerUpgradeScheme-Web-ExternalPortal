using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ofgem.Web.Bus.ExternalPortal.WebApp.UnitTests.Fakes;

public class FakeHttpSession : ISession
{
    private readonly Dictionary<string, object> _sessionValues = new Dictionary<string, object>();

    public bool IsAvailable => throw new NotImplementedException();

    public string Id => throw new NotImplementedException();

    public IEnumerable<string> Keys => _sessionValues.Keys;

    public void Clear()
    {
        _sessionValues.Clear();
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task LoadAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Remove(string key)
    {
        _sessionValues.Remove(key);
    }

    public void Set(string key, byte[] value)
    {
        _sessionValues[key] = Encoding.UTF8.GetString(value);
    }

    public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value)
    {
        if (_sessionValues.TryGetValue(key, out var dictionaryValue))
        {
            if (dictionaryValue == null)
            {
                value = null;
                return false;
            }

            value = Encoding.UTF8.GetBytes(dictionaryValue.ToString());
            return true;
        }

        value = null;
        return false;
    }
}
