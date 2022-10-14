/*  MarkdownParser.cs
 *  Version: 1.0 (2022.10.15)
 *
 *  Contributor
 *      Arime-chan
 */

using Markdig;

namespace Project24.App
{
    public class MarkdownParser
    {
        static MarkdownParser()
        {
            Reload();
        }

        public static string ToHtml(string _markdown)
        {
            return Markdown.ToHtml(_markdown, m_Pipeline);
        }

        private static void Reload()
        {
            m_Pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        }

        private static MarkdownPipeline m_Pipeline;
    }

}
