using System;
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;

namespace DebugHelper.Utilities
{
    internal static class TextUtils
    {
        public static string GetSelectedText(IServiceProvider serviceProvider)
        {
            var textView = GetTextView(serviceProvider);

            if (textView == null)
                return string.Empty;

            if (textView.GetSelection(out var piAnchorLine, out var piAnchorCol, out var piEndLine, out var piEndCol) != VSConstants.S_OK)
            {
                var textSpan = new TextSpan[1];

                if (textView.GetWordExtent(piAnchorLine, piAnchorCol, (uint)WORDEXTFLAGS.WORDEXT_CURRENT, textSpan) != VSConstants.S_OK)
                    return string.Empty;

                var span = textSpan[0];

                piAnchorLine = span.iStartLine;
                piEndLine = span.iEndLine;
                piAnchorCol = span.iStartIndex;
                piEndCol = span.iEndIndex;
            }

            if (piAnchorLine == piEndLine && piAnchorCol == piEndCol)
                return string.Empty;

            if (textView.GetBuffer(out var buffer) != VSConstants.S_OK)
                return string.Empty;

            var (startLine, endLine, startCol, endCol) =
                NormalizeSelection(piAnchorLine, piEndLine, piAnchorCol, piEndCol);

            return buffer.GetLineText(startLine, startCol, endLine, endCol, out var selectionText)
                   != VSConstants.S_OK ? string.Empty : selectionText;

        }

        private static (int startLine, int endLine, int startCol, int endCol) NormalizeSelection(int startLine,
            int endLine, int startCol, int endCol)
        {
            var points = new (int x, int y)[]
                {
                    (startLine, startCol),
                    (endLine, endCol)
                }
                .OrderBy(p => p.x)
                .ThenBy(p => p.y)
                .ToArray();

            return (points[0].x, points[1].x, points[0].y, points[1].y);
        }

        public static IVsTextView GetTextView(IServiceProvider serviceProvider)
        {
            var textManager = (IVsTextManager)serviceProvider.GetService(typeof(SVsTextManager));
            IVsTextView textViewCurrent = null;
            textManager?.GetActiveView(1, null, out textViewCurrent);
            return textViewCurrent;
        }
    }
}
