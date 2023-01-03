namespace BugFoundry.BugFoundryEditor.Management
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Commons.Legacy;
    using Contracts;
    using Infrastructure;
    using Roslyn.Highlights;
    using UnityEngine;

    public class HighlightBugFoundryModule
    {
        private readonly ITextEditor editor;
        private readonly HighlightRoslynModule highlightModule;

        private bool updateColorRunning = false;

        private bool? colored = null;
        private readonly WaitUntil waitUntil;
        private readonly WaitForSeconds waitForSeconds;

        public HighlightBugFoundryModule(ITextEditor editorIn, BugFoundryColors colors)
        {
            this.editor = editorIn;
            this.highlightModule = new HighlightRoslynModule(colors);
            this.waitUntil = new WaitUntil(() => this.colored != null);
            this.waitForSeconds = new WaitForSeconds(0.2f);
        }

        public void UpdateHighlighting(bool nextFrame) => Coroutiner.Instance.StartCoroutine(this.ColorCoroutine(nextFrame));

        private IEnumerator ColorCoroutine(bool nextFrame)
        {
            if (this.updateColorRunning)
                yield break;

            this.updateColorRunning = true;

            if (nextFrame)
                yield return null;

            string text = this.editor.GetText(true);

            while (true)
            {
                string localText = text;
                Task.Run(async () =>
                    {
                        List<Range> result = await this.highlightModule.Highlight(localText);
                        Dispatcher.Instance.Invoke(() => this.colored = this.editor.ColorSections(localText, result));
                    }
                );

                yield return this.waitUntil;
                yield return this.waitForSeconds;

                if (this.colored == null)
                    Debug.LogError($"BS");

                if (this.colored != true)
                {
                    this.colored = null;
                    text = this.editor.GetText(true);
                    continue;
                }

                break;
            }

            this.updateColorRunning = false;
        }
    }
}