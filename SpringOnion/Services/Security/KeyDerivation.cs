using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace SpringOnion.Services.Security;

public static class KeyDerivation
{
    public static byte[] DeriveConversationKey(string appSecret, string userA, string userB)
    {
        var a = string.CompareOrdinal(userA, userB) <= 0 ? userA : userB;
        var b = string.CompareOrdinal(userA, userB) <= 0 ? userB : userA;

        var input = $"{appSecret}|{a}|{b}";
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return hash;
    }
}
