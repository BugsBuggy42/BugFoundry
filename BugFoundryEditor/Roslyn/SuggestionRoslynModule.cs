namespace BugFoundry.BugFoundryEditor.Roslyn
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Completion;
    using UnityEngine;

    public class SuggestionRoslynModule
    {
        public async Task<List<string>> GetSuggestion(int indexIn, Document document, CancellationToken token)
        {
            try
            {
                int position = indexIn + 1;
                CompletionService completionService = CompletionService.GetService(document);
                if (completionService == null)
                    throw new Exception();

                CompletionList results = await completionService.GetCompletionsAsync(document, position, cancellationToken: token);
                CompletionItem[] sorted = results.Items.OrderByDescending(x => x.Rules.MatchPriority).ToArray();

                int maxSize = 10;
                int size = sorted.Length < maxSize ? sorted.Length : maxSize;

                // if (size == 0)
                //     Debug.Log($"No Suggestions");

                List<string> result = sorted.Select(x => x.DisplayText).ToList();
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError("SuggestionRoslynModule Exception: " + e);
                return new List<string>();
            }
        }
    }
}