namespace Projects.Buggary.BuggaryEditor.Management
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Commons.Legacy;
    using Contracts;
    using Infrastructure;
    using Roslyn.Highlights;
    using UnityEngine;

    public class HighlightBuggaryModule
    {
        private readonly ITextEditor editor;
        private readonly HighlightModule highlightModule;

        private bool updateColorRunning = false;

        public HighlightBuggaryModule(ITextEditor editorIn, BuggaryColors colors)
        {
            this.editor = editorIn;
            this.highlightModule = new HighlightModule(colors);
        }

        public void UpdateHighlighting(bool nextFrame) => Coroutiner.Instance.StartCoroutine(this.ColorCoroutine(nextFrame));

        private IEnumerator ColorCoroutine(bool nextFrame)
        {
            if (this.updateColorRunning)
                yield break;

            this.updateColorRunning = true;

            if (nextFrame)
                yield return null;

            bool? colored = null;
            string text = this.editor.GetText(true);

            while (true)
            {
                string text1 = text;
                Task task = Task.Run(async () =>
                    {
                        List<Range> result = await this.highlightModule.Highlight(text1);
                        Dispatcher.Instance.Invoke(() => colored = this.editor.ColorSections(text1, result));
                    }
                );

                yield return new WaitUntil(() => task.IsCompleted);
                yield return new WaitForSeconds(0.2f);

                if (colored == null)
                    Debug.Log($"BS");

                if (colored != true)
                {
                    text = this.editor.GetText(true);
                    continue;
                }

                break;
            }

            this.updateColorRunning = false;
        }
    }
}