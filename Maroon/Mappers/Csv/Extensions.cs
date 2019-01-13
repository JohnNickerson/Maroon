using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssimilationSoftware.PimData.Mappers.Csv
{
    public static class Extensions
    {
        public static List<string> Tokenise(this String line)
        {
            var tokens = new List<string>();
            var quoted = false;
			var escape = false;
            var token = new StringBuilder();
            for (int x = 0; x < line.Length; x++)
            {
				if (escape)
				{
					if (line[x] == '"')
					{
						// Escaped double quotes. Add as literal.
						token.Append(line[x]);
					}
					else if (line[x] == ',')
					{
						// That wasn't an escape.
						tokens.Add(token.ToString());
						quoted = false;
						escape = false;
					}
					else
					{
						// Well-formed strings should not end up here.
						// If we do, however, let's assume that was an erroneous escape and this next character is a literal.
						token.Append('"');
						token.Append(line[x]);
						escape = false;
					}
				}
                else if (quoted)
                {
                    if (line[x] == '"')
                    {
                        // Potentially the beginning of an escape sequence (double-double-quotes).
						escape = true;
                    }
                    else
                    {
                        // Add to token.
                        token.Append(line[x]);
                    }
                }
                else
                {
                    if (line[x] == ',')
                    {
                        // End of token.
                        tokens.Add(token.ToString());
                        token = new StringBuilder();
                    }
                    else if (line[x] == '"')
                    {
                        // Beginning of token.
                        quoted = true;
						escape = false;
                    }
                    else
                    {
                        // Add to token.
                        token.Append(line[x]);
                    }
                }
            }
            tokens.Add(token.ToString());
            return tokens;
        }
    }
}
