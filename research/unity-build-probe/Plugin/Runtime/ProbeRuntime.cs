using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Scripting;
namespace KingsSafe.Probe
{
    // Mechanism experiment only: public fixture keys, not a production key-delivery design.
    [Preserve]
    public static class ProbeRuntime
    {
        private const int Mask = 0x35A7612D;
        [Preserve] public static int Encode(int value) { return value ^ Mask; }
        [Preserve] public static int Decode(int encoded, int tag, bool initialized)
        {
            if (!initialized) return 0;
            if (tag != ~encoded) throw new InvalidOperationException("PROBE: storage integrity failed");
            return encoded ^ Mask;
        }
        // Exact overload used by this fixture. Other overloads/async paths are not covered.
        [Preserve] public static AssetBundle LoadBundle(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            if (bytes.Length < 4 || bytes[0] != 'K' || bytes[1] != 'S' || bytes[2] != 'P' || bytes[3] != '1')
                throw new InvalidDataException("PROBE: expected protected container");
            return AssetBundle.LoadFromMemory(Unwrap(bytes));
        }
        [Preserve] public static byte[] Wrap(byte[] plain)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Enumerable.Range(0, 32).Select(x => (byte)(x + 1)).ToArray();
                aes.GenerateIV();
                byte[] cipher;
                using (var enc = aes.CreateEncryptor()) cipher = enc.TransformFinalBlock(plain, 0, plain.Length);
                byte[] body = new byte[20 + cipher.Length];
                Array.Copy(new byte[]{75,83,80,49}, body, 4);
                Array.Copy(aes.IV, 0, body, 4, 16);
                Array.Copy(cipher, 0, body, 20, cipher.Length);
                using (var mac = new HMACSHA256(Enumerable.Range(0, 32).Select(x => (byte)(x + 65)).ToArray()))
                    return body.Concat(mac.ComputeHash(body)).ToArray();
            }
        }
        [Preserve] public static byte[] Unwrap(byte[] data)
        {
            if (data.Length < 84 || (data.Length - 52) % 16 != 0) throw new InvalidDataException("PROBE: invalid container");
            int length = data.Length - 32;
            using (var mac = new HMACSHA256(Enumerable.Range(0, 32).Select(x => (byte)(x + 65)).ToArray()))
            {
                byte[] expected = mac.ComputeHash(data, 0, length);
                int difference = 0;
                for (int i=0; i<32; i++) difference |= expected[i] ^ data[length+i];
                if (difference != 0) throw new InvalidDataException("PROBE: authentication failed");
            }
            using (var aes = Aes.Create())
            {
                aes.Key = Enumerable.Range(0, 32).Select(x => (byte)(x + 1)).ToArray();
                aes.IV = data.Skip(4).Take(16).ToArray();
                using (var dec = aes.CreateDecryptor()) return dec.TransformFinalBlock(data, 20, length-20);
            }
        }
    }
}
