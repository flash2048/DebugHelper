using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using System.Collections.Generic;

namespace DebugHelper.AvalonEdit
{
    public class BraceFoldingStrategy : AbstractFoldingStrategy
    {
        public char OpeningBrace { get; set; }

        public char ClosingBrace { get; set; }

        public BraceFoldingStrategy()
        {
            this.OpeningBrace = '{';
            this.ClosingBrace = '}';
        }

        public override IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
        {
            firstErrorOffset = -1;
            return CreateNewFoldings(document);
        }

        public IEnumerable<NewFolding> CreateNewFoldings(ITextSource document)
        {
            var newFoldings = new List<NewFolding>();

            var startOffsets = new Stack<int>();
            var lastNewLineOffset = 0;
            var openingBrace = OpeningBrace;
            var closingBrace = ClosingBrace;
            for (var i = 0; i < document.TextLength; i++)
            {
                var c = document.GetCharAt(i);
                if (c == openingBrace)
                {
                    startOffsets.Push(i);
                }
                else if (c == closingBrace && startOffsets.Count > 0)
                {
                    int startOffset = startOffsets.Pop();
                    // don't fold if opening and closing brace are on the same line
                    if (startOffset < lastNewLineOffset)
                    {
                        newFoldings.Add(new NewFolding(startOffset, i + 1));
                    }
                }
                else if (c == '\n' || c == '\r')
                {
                    lastNewLineOffset = i + 1;
                }
            }
            newFoldings.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
            return newFoldings;
        }
    }
}
