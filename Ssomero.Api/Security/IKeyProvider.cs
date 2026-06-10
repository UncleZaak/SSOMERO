using System.Collections.Generic;

namespace Ssomero.Api.Security;

public interface IKeyProvider
{
    /// <summary>
    /// Returns the current key id.
    /// </summary>
    string GetCurrentKeyId();

    /// <summary>
    /// Returns the current key bytes.
    /// </summary>
    byte[] GetCurrentKey();

    /// <summary>
    /// Try get key bytes by id; returns true if found.
    /// </summary>
    bool TryGetKey(string keyId, out byte[]? keyBytes);

    /// <summary>
    /// Returns all known keys (id -> bytes) including current and historical.
    /// </summary>
    IReadOnlyDictionary<string, byte[]> GetAllKeys();
}
