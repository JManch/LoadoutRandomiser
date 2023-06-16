using System.IO;
using System.Text;

namespace LoadoutRandomiser
{
    public static class Lexer
    {
        public static StreamReader Sr;
        public static string FilePath;
        private static bool _reading;
        private static int _lineNumber;
        private static char _lastChar;
        private static char _char;

        public enum TokenType
        {
            Ident,
            Colon,
            RightBracket,
            LeftBracket,
            Comma,
            Asterisk,
        }

        public struct Token
        {
            public TokenType Type { get; init; }
            public string Lexeme { get; init; }
            public int LineNo { get; init; }
            public int ColumnNo { get; init; }
        }

        public static void Reset() {
            _lineNumber = 0;
        }

        public static Token GetNextToken()
        {
            if (!_reading) {
                Sr = new StreamReader(FilePath);
                _char = (char)Sr.Read();
            }

            StringBuilder buffer = new();

            do {
                switch (_char)
                {
                    case ':':
                        return new Token
                        {
                            Type = TokenType.Colon,
                            Lexeme = "",
                            LineNo = _lineNumber,
                        };
                    case '}':
                        continue;
                    case '{':
                        continue;
                    case ',':
                        continue;
                    case '*':
                        continue;
                }

                if (c == '\n') {
                    _lineNumber++;
                    continue;
                }
                else if (c == '\r' || c == '\t' || (c == ' ' && buffer.Length == 0))
                    continue;

                _char = (char)Sr.Read();


                buffer.Append(c);

            } while (!Sr.EndOfStream);
        }
    }
}
