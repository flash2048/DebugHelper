using ICSharpCode.AvalonEdit.Folding;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;

namespace DebugHelper.AvalonEdit
{
    public abstract class AbstractFoldingStrategy
    {
        public void UpdateFoldings(FoldingManager manager, TextDocument document)
        {
            var foldings = CreateNewFoldings(document, out var firstErrorOffset);
            manager.UpdateFoldings(foldings, firstErrorOffset);
        }

        public abstract IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset);
    }
}
