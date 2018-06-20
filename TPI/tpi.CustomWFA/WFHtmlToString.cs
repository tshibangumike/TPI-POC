using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Text.RegularExpressions;

namespace tpi.CustomWFA
{
    public sealed class WFHtmlToString : CodeActivity
    {
        protected override void Execute(CodeActivityContext executionContext)
        {
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            string htmlInput = HtmlInput.Get<string>(executionContext);

            tracingService.Trace("String supplied = " + htmlInput, htmlInput);
            string textOutput = string.Empty;

            //string pattern = "(<div.*>)(.*)(<\\/div>)";
            //tracingService.Trace("Applying the following regex pattern: "+ pattern, pattern);
            //MatchCollection matches = Regex.Matches(htmlInput, pattern);

            //if (matches.Count > 0)
            //{
            //    foreach (Match m in matches)
            //    {
            //        StringOutput.Set(executionContext, HtmlToPlainText(m.Value));
            //        tracingService.Trace("The results returned are: "+m.Value , m.Value);
            //    }
            //}
            StringOutput.Set(executionContext, HtmlToPlainText(htmlInput));
        }

        [Input("HTML Text")]
        [RequiredArgument]
        public InArgument<String> HtmlInput { get; set; }

        [Output("Converted String")]
        public OutArgument<string> StringOutput { get; set; }


        private static string HtmlToPlainText(string html)
        {
            const string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";//matches one or more (white space or line breaks) between '>' and '<'
            const string stripFormatting = @"<[^>]*(>|$)";//match any character between '<' and '>', even when end tag is missing
            const string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>";//matches: <br>,<br/>,<br />,<BR>,<BR/>,<BR />

            var lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline);
            var stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline);
            var tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline);

            var text = html;
            //Decode html specific characters
            text = System.Net.WebUtility.HtmlDecode(text);
            //Remove tag whitespace/line breaks
            text = tagWhiteSpaceRegex.Replace(text, "><");
            //Replace <br /> with line breaks
            text = lineBreakRegex.Replace(text, Environment.NewLine);
            //Strip formatting
            text = stripFormattingRegex.Replace(text, string.Empty);

            return text;
        }
    }
}

