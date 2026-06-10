using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Ssomero.Api.Security;

public class ConfigurationKeyProvider : IKeyProvider
{
    private readonly Dictionary<string, byte[]> _keys = new();
    private readonly string _currentKeyId;

    public ConfigurationKeyProvider(IConfiguration config)
    {
        var section = config.GetSection("InvitationSecurity");
        _currentKeyId = section["CurrentKeyId"] ?? Guid.NewGuid().ToString();

        var current = section["CurrentKey"];
        if (!string.IsNullOrWhiteSpace(current))
        {
            try
            {
                var b = Convert.FromBase64String(current);
                _keys[_currentKeyId] = b;
            }
            catch
            {
                // ignore invalid key
            }
        }

        var prev = section.GetSection("PreviousKeys").GetChildren();
        foreach (var kid in prev)
        {
            var id = kid["KeyId"];
            var val = kid["Key"];
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(val)) continue;
            try
            {
                var b = Convert.FromBase64String(val);
                if (!_keys.ContainsKey(id)) _keys[id] = b;
            }
            catch { }
        }
    }

    public string GetCurrentKeyId() => _currentKeyId;

    public byte[] GetCurrentKey() => _keys[_currentKeyId];

    public bool TryGetKey(string keyId, out byte[]? keyBytes) => _keys.TryGetValue(keyId, out keyBytes);

    public IReadOnlyDictionary<string, byte[]> GetAllKeys() => _keys;
}
