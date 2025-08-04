﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace AssimilationSoftware.Maroon.Mappers.Csv
{
    public static class Extensions
    {
        public static List<string> Tokenise(this string line)
        {
            var tokens = new List<string>();
            var quoted = false;
			var escape = false;
            var token = new StringBuilder();
            foreach (var c in line)
            {
                if (escape)
                {
                    switch (c)
                    {
                        case '"':
                            // Escaped double quotes. Add as literal.
                            token.Append(c);
                            break;
                        case ',':
                            // That wasn't an escape. Finish the token.
                            tokens.Add(token.ToString());
                            token = new StringBuilder();
                            quoted = false;
                            escape = false;
                            token = new StringBuilder();
                            break;
                        default:
                            // Well-formed strings should not end up here.
                            // If we do, however, let's assume that was an erroneous escape and this next character is a literal.
                            token.Append('"');
                            token.Append(c);
                            escape = false;
                            break;
                    }
                }
                else if (quoted)
                {
                    if (c == '"')
                    {
                        // Potentially the beginning of an escape sequence (double-double-quotes).
                        escape = true;
                    }
                    else
                    {
                        // Add to token.
                        token.Append(c);
                    }
                }
                else
                {
                    switch (c)
                    {
                        case ',':
                            // End of token.
                            tokens.Add(token.ToString());
                            token = new StringBuilder();
                            break;
                        case '"':
                            // Beginning of token.
                            quoted = true;
                            break;
                        default:
                            // Add to token.
                            token.Append(c);
                            break;
                    }
                }
            }
            tokens.Add(token.ToString());
            return tokens;
        }

        public static string CalculateHash(this string raw)
        {
            try
            {
                using (var cryptoProvider = new SHA1CryptoServiceProvider())
                {
                    var hash = cryptoProvider.ComputeHash(Encoding.UTF8.GetBytes(raw));
                    return BitConverter.ToString(hash);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }

        }

    }
}
