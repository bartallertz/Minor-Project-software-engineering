using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MinorProject.Students;


using Npgsql;


namespace MinorProject
{

    class Program
    {
        public static Student[] students = { new Student("Bart", 25, 0951018) };


        static void Main(string[] args)
        {


            var sqlstring = students.SqlQuerry(s => new { s.age, s.studentNumber, s.name }, s => s.age > 25, s => s.age);


            var results = removeAccesdata(sqlstring.ToString());
            // System.Console.WriteLine(results);

            var select3 = RemoveDots(results);

            // System.Console.WriteLine(select3);



            Console.Write("> ");
            var line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }




            var parser = new Parser(select3);
            var syntaxTree = parser.Parse();

            var colour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            PrettyPrint(syntaxTree.Root);
            Console.ForegroundColor = colour;

            if (!syntaxTree.Diagnostics.Any())
            {
                var e = new Evaluator(syntaxTree.Root);
                var result = e.Evaluate();
                System.Console.WriteLine(result);

                ReadDataFromDb(result);
            }
            else
            {

                Console.ForegroundColor = ConsoleColor.DarkRed;


                foreach (var diagnositc in syntaxTree.Diagnostics)
                {
                    Console.WriteLine(diagnositc);
                }
                Console.ForegroundColor = colour;

            }



        }


        public static string removeAccesdata(string data)
        {
            if (data.Contains("new <>f__AnonymousType0`1"))
            {
                return data.Replace("new <>f__AnonymousType0`1", "");
            }
            if (data.Contains("new <>f__AnonymousType0`2"))
            {
                return data.Replace("new <>f__AnonymousType0`2", "");
            }
            if (data.Contains("new <>f__AnonymousType0`3"))
            {
                return data.Replace("new <>f__AnonymousType0`3", "");
            }
            if (data.Contains("new <>f__AnonymousType0`4"))
            {
                return data.Replace("new <>f__AnonymousType0`4", "");
            }
            return "";
        }
        public static string RemoveDots(string data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != '.')
                {
                    continue;
                }
                var result = data.Remove(i - 1, 2);

                data = result;
            }
            return data;
        }

        public static string RemoveDupes(string data)
        {

            if (data.Contains("s.age") || data.Contains("s.name") || data.Contains("s.studentNumber"))
            {
                return data.Replace(" = s.age", "").Replace(" = s.name", "").Replace(" = s.studentNumber", "");
            }
            return "";
        }


        static void PrettyPrint(SyntaxNode node, string indent = "", bool isLast = true)
        {


            // └──
            // │
            // ├──

            var marker = isLast ? "└──" : "├──";

            Console.Write(indent);
            Console.Write(marker);
            Console.Write(node.Kind);

            if (node is SyntaxToken t && t.Value != null)
            {
                Console.Write(" ");
                Console.Write(t.Value);
            }

            Console.WriteLine();




            indent += isLast ? "    " : "│   ";

            var lastChild = node.GetChildren().LastOrDefault();


            foreach (var child in node.GetChildren())
            {
                PrettyPrint(child, indent, child == lastChild);
            }
        }

        public static void ReadDataFromDb(string sqlstring)
        {
            var cs = "Host=localhost;Username=postgres;Password=25679134;Database=Minor;port=5432";

            using var con = new NpgsqlConnection(cs);
            con.Open();
            try
            {
                string sql = sqlstring;
                // initializing the NpgsqlCommand class for usage, with the connection string to the database and the sqlstring 
                var cmd = new NpgsqlCommand(sql, con);
                //initializing NpgsqlDataReader class for usage with the cmd.ExecuteReader. 
                //this sends the sqlstring u give give the NpgsqlCommand class to the connection to the database and builds a SqlDataReader and storing the data recieved in the rdr variable
                NpgsqlDataReader rdr = cmd.ExecuteReader();

                //reading the data that is stored in the rdr variable
                while (rdr.Read())
                {


                    // //writing the information in the rdr object to the console
                    Console.Write("{0} | {1} | {2} \n", rdr[0], rdr[1], rdr[2]);


                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
    }


//  the Enum SyntaxKind can be seen as labels for the characters.
// example: we have a stringToken for all string charcters, number tokens for all number characters and WhiteSpaceToken for all white spaces.
    enum SyntaxKind
    {
        NumberToken,
        WhiteSpaceToken,
        PlusToken,
        MinusToken,
        TimesToken,
        OpenParanthisisToken,
        CloseParanthisisToken,
        SlashToken,
        BadToken,
        EndOfFileToken,
        NumberExpression,
        binaryExpression,
        StringToken,
        SelectToken,
        WhereToken,
        EqualToken,
        EqualsEqualsToken,
        EqualOrGreaterToken,
        LowerOrEqualToken,
        LowerToken,
        CommaToken,
        DotToken,
        greaterThenToken,
        UnderScoreToken,
        TildeToken,
        SdotTokenm,
        greaterOrEqualToken,
        airQuotesToken
    }

    //The SyntaxToken: is what is returned to the screen, it takes in a SyntaxKind,
    //The position: is where the string is within the text
    //The Text: is the value that the token holds
    //The Object is the value that is returned
    class SyntaxToken : SyntaxNode
    {
        public SyntaxToken(SyntaxKind kind, int position, string text, object value)
        {
            Kind = kind;
            Position = position;
            Text = text;
            Value = value;
        }

        public override SyntaxKind Kind { get; }
        public int Position { get; }
        public string Text { get; }
        public object Value { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            return Enumerable.Empty<SyntaxNode>();
        }
    }

    class Lexer
    {

        private readonly string _text;

        private int _position;

        private List<string> _diagnostics = new List<string>();
        public Lexer(string text)
        {
            _text = text;
        }

        public IEnumerable<string> diagnostics => _diagnostics;
        //The function Current returns the current position within the text
        private char current
        {
            get
            {
                if (_position >= _text.Length)
                {
                    return '\0';
                }

                return _text[_position];
            }
        }
        // this function advances the lexar 1 character
        private void Next()
        {
            _position++;
        }

        // the Peek function returns the current position + a offset of characters
        // example: Peek(1) = peeking 1 character ahead from the current position.
        public char Peek(int offset)
        {
            var index = _position + offset;
            if (index >= _text.Length)
            {
                return '\0';
            }
            return _text[index];
        }
        //The Function NextToken is a function that returns all the SyntaxTokens within a string
        //
        public SyntaxToken NextToken()
        {


            if (_position >= _text.Length)
            {
                return new SyntaxToken(SyntaxKind.EndOfFileToken, _position, "\0", null);
            }
            if (char.IsLetterOrDigit(current))
            {
                var start = _position;
                while (char.IsLetterOrDigit(current))
                {
                    Next();
                }
                var length = _position - start;
                var text = _text.Substring(start, length);

                return new SyntaxToken(SyntaxKind.StringToken, start, text, text);
            }
            if (char.IsDigit(current))
            {
                var start = _position;


                while (char.IsDigit(current))
                {
                    Next();
                }
                var length = _position - start;
                var text = _text.Substring(start, length);
                if (!int.TryParse(text, out var value))
                {
                    _diagnostics.Add($"The Number {_text} isn't a valid int32.");
                }

                return new SyntaxToken(SyntaxKind.NumberToken, start, text, value);

            }
            if (char.IsWhiteSpace(current))
            {
                var start = _position;


                while (char.IsWhiteSpace(current))
                {
                    Next();
                }
                var length = _position - start;
                var text = _text.Substring(start, length);



                return new SyntaxToken(SyntaxKind.WhiteSpaceToken, start, text, null);

            }
            // this switch creates SyntaxTokens for every "extra" characters
            switch (current)
            {
                case '+':
                    return new SyntaxToken(SyntaxKind.PlusToken, _position++, "+", null);
                case '-':
                    return new SyntaxToken(SyntaxKind.MinusToken, _position++, "-", null);
                case '*':
                    return new SyntaxToken(SyntaxKind.TimesToken, _position++, "*", null);
                case '/':
                    return new SyntaxToken(SyntaxKind.SlashToken, _position++, "/", null);
                case '(':
                    return new SyntaxToken(SyntaxKind.OpenParanthisisToken, _position++, "(", null);
                case ')':
                    return new SyntaxToken(SyntaxKind.CloseParanthisisToken, _position++, ")", null);
                case ',':
                    return new SyntaxToken(SyntaxKind.CommaToken, _position++, ",", ",");
                case '"':
                    return new SyntaxToken(SyntaxKind.airQuotesToken, _position++, "'", "'");
                case '=':
                    if (Peek(1) == '=')
                    {
                        return new SyntaxToken(SyntaxKind.EqualToken, _position++, "", "");
                    }
                    else
                    {
                        return new SyntaxToken(SyntaxKind.EqualToken, _position++, "=", "=");

                    }
                case '>':

                    if (Peek(1) == '=')
                    {
                        _position += 2;
                        return new SyntaxToken(SyntaxKind.greaterOrEqualToken, _position++, ">=", ">=");
                    }
                    else
                    {
                        return new SyntaxToken(SyntaxKind.greaterThenToken, _position++, ">", ">");
                    }
                case '<':
                    if (Peek(1) == '=')
                    {
                        _position += 2;
                        return new SyntaxToken(SyntaxKind.LowerOrEqualToken, _position++, "<=", "<=");
                    }
                    else
                    {
                        return new SyntaxToken(SyntaxKind.LowerToken, _position++, "<", "<");
                    }
                case 's':
                    if (Peek(1) == '.')
                    {
                        _position += 2;
                        return new SyntaxToken(SyntaxKind.SdotTokenm, _position + 2, "s.", null);
                    }
                    else
                    {
                        return new SyntaxToken(SyntaxKind.SdotTokenm, _position++, "s.", "s.");
                    }
            }
            _diagnostics.Add($"Error: bad character in input: '{current}'");
            return new SyntaxToken(SyntaxKind.BadToken, _position++, _text.Substring(_position - 1, 1), null);


        }
    }

    abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind { get; }

        public abstract IEnumerable<SyntaxNode> GetChildren();

    }
    abstract class ExpressionSyntax : SyntaxNode
    {

    }
    sealed class StringExpressionSyntax : ExpressionSyntax
    {
        public StringExpressionSyntax(SyntaxToken stringtoken)
        {
            Stringtoken = stringtoken;
        }

        public SyntaxToken Stringtoken { get; }

        public override SyntaxKind Kind => SyntaxKind.StringToken;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Stringtoken;
        }
    }

    sealed class BinaryExpressionSyntax : ExpressionSyntax
    {
        public BinaryExpressionSyntax(ExpressionSyntax left, ExpressionSyntax right)
        {
            Left = left;
            Right = right;
        }

        public ExpressionSyntax Left { get; }

        public ExpressionSyntax Right { get; }

        public override SyntaxKind Kind => SyntaxKind.binaryExpression;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Left;
            yield return Right;
        }
    }


    sealed class SyntaxTree
    {
        public SyntaxTree(IEnumerable<string> diagnostics, ExpressionSyntax root, SyntaxToken endOfFileToken)
        {
            Diagnostics = diagnostics.ToArray();
            Root = root;
            EndOfFileToken = endOfFileToken;
        }

        public IReadOnlyList<string> Diagnostics { get; }
        public ExpressionSyntax Root { get; }
        public SyntaxToken EndOfFileToken { get; }
    }

    class Parser
    {
        private readonly SyntaxToken[] _tokens;
        private int _position;

        private List<string> _diagnostics = new List<string>();

        public Parser(string text)
        {

            var tokens = new List<SyntaxToken>();

            var lexer = new Lexer(text);
            SyntaxToken token;

            do
            {
                token = lexer.NextToken();

                if (token.Kind != SyntaxKind.WhiteSpaceToken && token.Kind != SyntaxKind.BadToken)
                {
                    tokens.Add(token);
                }


            } while (token.Kind != SyntaxKind.EndOfFileToken);

            _tokens = tokens.ToArray();
            _diagnostics.AddRange(lexer.diagnostics);



        }

        public IEnumerable<string> diagnostic => _diagnostics;

        private SyntaxToken Peek(int offset)
        {
            var index = _position + offset;
            if (index >= _tokens.Length)
            {
                return _tokens[_tokens.Length - 1];
            }

            return _tokens[index];
        }

        private SyntaxToken Current => Peek(0);


        private SyntaxToken NextToken()
        {
            var current = Current;
            _position++;
            return current;
        }
        private SyntaxToken Match(SyntaxKind kind)
        {
            if (Current.Kind == kind)
            {
                return NextToken();
            }
            else
            {
                _diagnostics.Add($"Error: unexpected token <{Current.Kind}>, expected <{kind}>");
                return new SyntaxToken(kind, Current.Position, null, null);
            }
        }

        public SyntaxTree Parse()
        {
            var expression = ParseExpression();
            var endofFileToken = Match(SyntaxKind.EndOfFileToken);
            return new SyntaxTree(_diagnostics, expression, endofFileToken);
        }

        private ExpressionSyntax ParseExpression()
        {
            var left = parsePrimaryExpression();

            while (Current.Kind == SyntaxKind.StringToken ||
                  Current.Kind == SyntaxKind.NumberToken ||
                  Current.Kind == SyntaxKind.PlusToken ||
                  Current.Kind == SyntaxKind.OpenParanthisisToken ||
                  Current.Kind == SyntaxKind.CloseParanthisisToken ||
                  Current.Kind == SyntaxKind.EqualOrGreaterToken ||
                  Current.Kind == SyntaxKind.LowerToken ||
                  Current.Kind == SyntaxKind.greaterThenToken ||
                  Current.Kind == SyntaxKind.UnderScoreToken ||
                  Current.Kind == SyntaxKind.EqualToken ||
                  Current.Kind == SyntaxKind.DotToken ||
                  Current.Kind == SyntaxKind.CommaToken ||
                  Current.Kind == SyntaxKind.LowerOrEqualToken ||
                  Current.Kind == SyntaxKind.greaterOrEqualToken ||
                  Current.Kind == SyntaxKind.EqualsEqualsToken ||
                  Current.Kind == SyntaxKind.airQuotesToken)
            {
                var right = parsePrimaryExpression();

                left = new BinaryExpressionSyntax(left, right);

            }
            return left;
        }

        private ExpressionSyntax parsePrimaryExpression()
        {
            if (Current.Kind == SyntaxKind.StringToken)
            {
                var stringtoken = Match(SyntaxKind.StringToken);
                return new StringExpressionSyntax(stringtoken);
            }
            if (Current.Kind == SyntaxKind.OpenParanthisisToken)
            {
                var OpenParanthisisToken = Match(SyntaxKind.OpenParanthisisToken);
                return new StringExpressionSyntax(OpenParanthisisToken);
            }
            if (Current.Kind == SyntaxKind.CloseParanthisisToken)
            {
                var ClosedParanthisisToken = Match(SyntaxKind.CloseParanthisisToken);
                return new StringExpressionSyntax(ClosedParanthisisToken);
            }
            if (Current.Kind == SyntaxKind.EqualOrGreaterToken)
            {
                var equalsorgreatertoken = Match(SyntaxKind.EqualOrGreaterToken);
                return new StringExpressionSyntax(equalsorgreatertoken);
            }
            if (Current.Kind == SyntaxKind.LowerToken)
            {
                var lowertoken = Match(SyntaxKind.LowerToken);
                return new StringExpressionSyntax(lowertoken);
            }
            if (Current.Kind == SyntaxKind.WhereToken)
            {
                var whereToken = Match(SyntaxKind.WhereToken);
                return new StringExpressionSyntax(whereToken);
            }
            if (Current.Kind == SyntaxKind.greaterThenToken)
            {
                var GreaterThenToken = Match(SyntaxKind.greaterThenToken);
                return new StringExpressionSyntax(GreaterThenToken);
            }
            if (Current.Kind == SyntaxKind.greaterThenToken)
            {
                var UnderscoreToken = Match(SyntaxKind.UnderScoreToken);
                return new StringExpressionSyntax(UnderscoreToken);
            }
            if (Current.Kind == SyntaxKind.EqualToken)
            {
                var equalToken = Match(SyntaxKind.EqualToken);
                return new StringExpressionSyntax(equalToken);
            }
            if (Current.Kind == SyntaxKind.DotToken)
            {
                var dotToken = Match(SyntaxKind.DotToken);
                return new StringExpressionSyntax(dotToken);
            }
            if (Current.Kind == SyntaxKind.CommaToken)
            {
                var commaToken = Match(SyntaxKind.CommaToken);
                return new StringExpressionSyntax(commaToken);
            }
            if (Current.Kind == SyntaxKind.LowerOrEqualToken)
            {
                var LoeT = Match(SyntaxKind.LowerOrEqualToken);
                return new StringExpressionSyntax(LoeT);
            }
            if (Current.Kind == SyntaxKind.greaterOrEqualToken)
            {
                var greaterOrEqual = Match(SyntaxKind.greaterOrEqualToken);
                return new StringExpressionSyntax(greaterOrEqual);
            }
            if (Current.Kind == SyntaxKind.EqualsEqualsToken)
            {
                var EqualsEquals = Match(SyntaxKind.EqualsEqualsToken);
                return new StringExpressionSyntax(EqualsEquals);
            }
            if (Current.Kind == SyntaxKind.airQuotesToken)
            {
                var airQuotes = Match(SyntaxKind.airQuotesToken);
                return new StringExpressionSyntax(airQuotes);
            }

            else
            {


                return null;
            }
        }
    }


    class Evaluator
    {


        private readonly ExpressionSyntax _root;
        public Evaluator(ExpressionSyntax root)
        {
            this._root = root;
        }
        public static string RemoveDupeWords(string input)
        {
            if (input.Contains("age =") || input.Contains("name =") || input.Contains("studentNumber = ") || input.Contains(" ' "))
            {
                var result = input.Replace("age = ", "").Replace("name = ", "").Replace("studentNumber = ", "").Replace(" ' ", "'");

                return result;
            }
            else
            {
                return input;
            }
        }
        public static string RemoveWhiteSpace(string data)
        {

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == ' ')
                {

                }
            }

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != ' ')
                {
                    continue;
                }
                var result = data.Remove(i - 1, 2);

                data = result;
            }
            return data;
        }


        public string Evaluate()
        {
            return EvaluateExpression(_root);
        }

        private string EvaluateExpression(ExpressionSyntax node)
        {
            if (node is StringExpressionSyntax s)
            {
                return (string)s.Stringtoken.Value;
            }
            if (node is BinaryExpressionSyntax b)
            {
                var left = EvaluateExpression(b.Left);
                var right = EvaluateExpression(b.Right);



                string res = left + " " + right;

                var result = RemoveDupeWords(res);

                return result;


            }

            throw new Exception($"unexpected node {node.Kind}");


        }
    }
}
