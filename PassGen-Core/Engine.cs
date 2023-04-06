using System;
using System.Linq;
using System.Security.Cryptography;

namespace PassGenCore;

public struct Engine
{
    public KeyList KeyList { get; init; }

    public string GeneratePass(Key key, string master)
    {
        if (!CheckMaster(master))
            throw new Exception("Master Password check mismatch!");

        var inputBytes = key.Label.ToUTF8();
        if (key.PasswordChanges is not (null or 0))
            inputBytes = concatBuffers(inputBytes, key.PasswordChanges.ToString().ToUTF8());

        var hashedBytes = new HMACSHA256(master.ToUTF8()).ComputeHash(inputBytes);

        var pass = key.GenMode switch
        {
            GenMode.Base64 => hashedBytes.ToBase64(),
            GenMode.Base64WithSymbol => hashedBytes.ToBase64() + "!",
            GenMode.AlphaNum => hashedBytes.ToBase64().Replace("/", "").Replace("+", "").Replace("=", ""),
            _ => throw new Exception("Unexpected GenMode"),
        };

        if (key.MaxLength != null)
            pass = pass.Substring(0, key.MaxLength.Value);

        return pass;
    }

    public bool CheckMaster(string input)
    {
        // Get Salt Bytes or Generate new one
        if (KeyList.Master.Salt is null)
        {
            var salt = RandomNumberGenerator.GetBytes(32);
            KeyList.Master.Salt = salt.ToBase64();
        }

        // Compute Input Master Hash
        var inputHash = computeMasterHash(input);

        // If this is a new Key List, save the Hash and return true
        if (KeyList.Master.Hash is null)
        {
            KeyList.Master.Hash = inputHash;
            return true;
        }

        if (KeyList.Version < KeyList.CurrVersion)
        {
            var legacyHash = KeyList.Master.Hash;

            KeyList.Version = KeyList.CurrVersion;
            KeyList.Master.Hash = computeMasterHash(input);

            return inputHash == legacyHash;
        }

        return inputHash == KeyList.Master.Hash;
    }

    private string computeMasterHash(string input)
    {
        var inputBytes = input.ToUTF8();
        var salt = KeyList.Master.Salt.FromBase64();
        var iters = KeyList.Master.IterCount;

        if (KeyList.Version is 0)
            return new HMACSHA256(inputBytes).ComputeHash("CHECK".ToUTF8()).ToBase64();

        if (KeyList.Version is 1)
        {
            var sha = SHA256.Create();
            return Enumerable.Range(0, iters).Aggregate(new byte[0], (hash, _) =>
                sha.ComputeHash(concatBuffers(hash, inputBytes, salt))).ToBase64();
        }

        if (KeyList.Version is 2)
            return new Rfc2898DeriveBytes(inputBytes, salt, iters,
                HashAlgorithmName.SHA256).GetBytes(256 / 8).ToBase64();

        throw new Exception("Unexpected Version");
    }

    
    private static byte[] concatBuffers(params byte[][] buffers)
    {
        var result = new byte[buffers.Sum(b => b.Length)];
        var offset = 0;
        foreach (var buffer in buffers)
        {
            Buffer.BlockCopy(buffer, 0, result, offset, buffer.Length);
            offset += buffer.Length;
        }

        return result;
    }
}