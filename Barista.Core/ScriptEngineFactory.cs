namespace Barista
{
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Text;
    using Barista.Bundles;
    using Barista.Engine;
    using Barista.Extensions;
    using Jurassic;
    using Jurassic.Library;
    using System;

    /// <summary>
    /// Represents a class that initializes a new ScriptEngine instance.
    /// </summary>
    public abstract class ScriptEngineFactory
    {
        protected const string JavaScriptExceptionMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<HTML><HEAD>
<STYLE type=""text/css"">
#content{{ FONT-SIZE: 0.7em; PADDING-BOTTOM: 2em; MARGIN-LEFT: 30px}}
BODY{{MARGIN-TOP: 0px; MARGIN-LEFT: 0px; COLOR: #000000; FONT-FAMILY: Verdana; BACKGROUND-COLOR: white}}
P{{MARGIN-TOP: 0px; MARGIN-BOTTOM: 12px; COLOR: #000000; FONT-FAMILY: Verdana}}
PRE{{BORDER-RIGHT: #f0f0e0 1px solid; PADDING-RIGHT: 5px; BORDER-TOP: #f0f0e0 1px solid; MARGIN-TOP: -5px; PADDING-LEFT: 5px; FONT-SIZE: 1.2em; PADDING-BOTTOM: 5px; BORDER-LEFT: #f0f0e0 1px solid; PADDING-TOP: 5px; BORDER-BOTTOM: #f0f0e0 1px solid; FONT-FAMILY: Courier New; BACKGROUND-COLOR: #e5e5cc}}
.heading1{{MARGIN-TOP: 0px; PADDING-LEFT: 15px; FONT-WEIGHT: normal; FONT-SIZE: 26px; PADDING-BOTTOM: 3px; COLOR: #ffffff; PADDING-TOP: 10px; FONT-FAMILY: Tahoma; BACKGROUND-COLOR: #492B29}}
.intro{{MARGIN-LEFT: -15px}}</STYLE>
<TITLE>JavaScript Error</TITLE></HEAD><BODY>
<P class=""heading1"">JavaScript Error</P>
<DIV id=""content"">
<BR/>
<P class=""intro"">The JavaScript being executed on the server threw an exception ({0}). The exception message is '{1}'.</P>
<p class=""intro"">Function Name: <span id=""functionName"">{2}</span><br/>
Line Number: <span id=""lineNumber"">{3}</span><br/>
Source Path: <span id=""sourcePath"">{4}</span></p>
</DIV>
</BODY></HTML>
";
        protected const string JavaScriptExceptionMessageWithStackTrace = @"<?xml version=""1.0"" encoding=""utf-8""?>
<HTML><HEAD>
<STYLE type=""text/css"">
#content{{ FONT-SIZE: 0.7em; PADDING-BOTTOM: 2em; MARGIN-LEFT: 30px}}
BODY{{MARGIN-TOP: 0px; MARGIN-LEFT: 0px; COLOR: #000000; FONT-FAMILY: Verdana; BACKGROUND-COLOR: white}}
P{{MARGIN-TOP: 0px; MARGIN-BOTTOM: 12px; COLOR: #000000; FONT-FAMILY: Verdana}}
PRE{{BORDER-RIGHT: #f0f0e0 1px solid; PADDING-RIGHT: 5px; BORDER-TOP: #f0f0e0 1px solid; MARGIN-TOP: -5px; PADDING-LEFT: 5px; FONT-SIZE: 1.2em; PADDING-BOTTOM: 5px; BORDER-LEFT: #f0f0e0 1px solid; PADDING-TOP: 5px; BORDER-BOTTOM: #f0f0e0 1px solid; FONT-FAMILY: Courier New; BACKGROUND-COLOR: #e5e5cc}}
.heading1{{MARGIN-TOP: 0px; PADDING-LEFT: 15px; FONT-WEIGHT: normal; FONT-SIZE: 26px; PADDING-BOTTOM: 3px; COLOR: #ffffff; PADDING-TOP: 10px; FONT-FAMILY: Tahoma; BACKGROUND-COLOR: #492B29}}
.intro{{MARGIN-LEFT: -15px}}</STYLE>
<TITLE>JavaScript Error</TITLE></HEAD><BODY>
<P class=""heading1"">JavaScript Error</P>
<DIV id=""content"">
<BR/>
<P class=""intro"">The JavaScript being executed on the server threw an exception ({0}). The exception message is '{1}'.</P>
<p class=""intro"">Function Name: <span id=""functionName"">{2}</span><br/>
Line Number: <span id=""lineNumber"">{3}</span><br/>
Source Path: <span id=""sourcePath"">{4}</span></p>
<P class=""intro""/>
<P class=""intro"">The exception stack trace is:</P>
<P class=""intro stackTrace"">{5}</P>
</DIV>
</BODY></HTML>
";

        protected const string JavaScriptTimeoutMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<HTML><HEAD>
<STYLE type=""text/css"">
#content{{ FONT-SIZE: 0.7em; PADDING-BOTTOM: 2em; MARGIN-LEFT: 30px}}
BODY{{MARGIN-TOP: 0px; MARGIN-LEFT: 0px; COLOR: #000000; FONT-FAMILY: Verdana; BACKGROUND-COLOR: white}}
P{{MARGIN-TOP: 0px; MARGIN-BOTTOM: 12px; COLOR: #000000; FONT-FAMILY: Verdana}}
PRE{{BORDER-RIGHT: #f0f0e0 1px solid; PADDING-RIGHT: 5px; BORDER-TOP: #f0f0e0 1px solid; MARGIN-TOP: -5px; PADDING-LEFT: 5px; FONT-SIZE: 1.2em; PADDING-BOTTOM: 5px; BORDER-LEFT: #f0f0e0 1px solid; PADDING-TOP: 5px; BORDER-BOTTOM: #f0f0e0 1px solid; FONT-FAMILY: Courier New; BACKGROUND-COLOR: #e5e5cc}}
.heading1{{MARGIN-TOP: 0px; PADDING-LEFT: 15px; FONT-WEIGHT: normal; FONT-SIZE: 26px; PADDING-BOTTOM: 3px; COLOR: #ffffff; PADDING-TOP: 10px; FONT-FAMILY: Tahoma; BACKGROUND-COLOR: #A68300}}
.intro{{MARGIN-LEFT: -15px}}</STYLE>
<TITLE>Barista Script Timeout</TITLE></HEAD><BODY>
<P class=""heading1"">Barista Script Timeout</P>
<DIV id=""content"">
<BR/>
<P class=""intro"">The script being executed exceeded the maximum allowable timeout as set by an Administrator. ({0}ms).</P>
</DIV>
</BODY></HTML>
";

        protected const string ExceptionMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<HTML><HEAD>
<STYLE type=""text/css"">
#content{{ FONT-SIZE: 0.7em; PADDING-BOTTOM: 2em; MARGIN-LEFT: 30px}}
BODY{{MARGIN-TOP: 0px; MARGIN-LEFT: 0px; COLOR: #000000; FONT-FAMILY: Verdana; BACKGROUND-COLOR: white}}
P{{MARGIN-TOP: 0px; MARGIN-BOTTOM: 12px; COLOR: #000000; FONT-FAMILY: Verdana}}
PRE{{BORDER-RIGHT: #f0f0e0 1px solid; PADDING-RIGHT: 5px; BORDER-TOP: #f0f0e0 1px solid; MARGIN-TOP: -5px; PADDING-LEFT: 5px; FONT-SIZE: 1.2em; PADDING-BOTTOM: 5px; BORDER-LEFT: #f0f0e0 1px solid; PADDING-TOP: 5px; BORDER-BOTTOM: #f0f0e0 1px solid; FONT-FAMILY: Courier New; BACKGROUND-COLOR: #e5e5cc}}
.heading1{{MARGIN-TOP: 0px; PADDING-LEFT: 15px; FONT-WEIGHT: normal; FONT-SIZE: 26px; PADDING-BOTTOM: 3px; COLOR: #ffffff; PADDING-TOP: 10px; FONT-FAMILY: Tahoma; BACKGROUND-COLOR: #A60000}}
.intro{{MARGIN-LEFT: -15px}}</STYLE>
<TITLE>Request Error</TITLE></HEAD><BODY>
<P class=""heading1"">Error</P>
<DIV id=""content"">
<BR/>
<P class=""intro"">The server encountered an error processing the request. The exception message is '{0}'.</P>
<P class=""intro"">The exception stack trace is:</P>
<P class=""intro stackTrace"">{1}</P>
</DIV>
</BODY></HTML>
";

        /// <summary>
        /// Returns a new instance of a script engine object with all runtime objects available.
        /// </summary>
        /// <param name="webBundle">The web bundle. (Optional)</param>
        /// <param name="isNewScriptEngineInstance">if set to <c>true</c> [is new script engine instance].</param>
        /// <param name="errorInInitialization">if set to <c>true</c> [error in initialization].</param>
        /// <returns>IScriptEngine.</returns>
        public abstract IScriptEngine GetScriptEngine(WebBundleBase webBundle, out bool isNewScriptEngineInstance,
                                                     out bool errorInInitialization);

        public virtual void UpdateResponseWithJavaScriptExceptionDetails(IScriptEngine engine, JavaScriptException exception, BrewResponse response)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");

            if (response == null)
                throw new ArgumentNullException("response");

            string message;
            var stack = String.Empty;
            var rawStack = String.Empty;

            var javascriptExceptionMessage = JavaScriptExceptionMessage;

            if (TypeUtilities.IsString(exception.ErrorObject))
            {
                message = TypeConverter.ToString(exception.ErrorObject);
            }
            else if (exception.ErrorObject is ObjectInstance)
            {
                var errorObject = exception.ErrorObject as ObjectInstance;

                message = errorObject.GetPropertyValue("message") as string;
                if (message == null || message.IsNullOrWhiteSpace())
                {
                    message = engine.Stringify(exception.ErrorObject, null, 4);
                }
                else
                {
                    stack = errorObject.GetPropertyValue("stack") as string;
                    rawStack = stack;
                    if (stack != null && stack.IsNullOrWhiteSpace() == false)
                    {
                        stack = stack.Replace("at ", "at<br/>");
                        javascriptExceptionMessage = JavaScriptExceptionMessageWithStackTrace;
                    }
                }
            }
            else
            {
                message = exception.ErrorObject.ToString();
            }

            response.StatusCode = HttpStatusCode.BadRequest;
            response.StatusDescription = message;
            response.AutoDetectContentType = false;
            response.ContentType = "text/html";

            string exceptionName = exception.Name;
            if (exceptionName.IsNullOrWhiteSpace())
                exceptionName = "JavaScript Error";

            var resultMessage = String.Format(javascriptExceptionMessage, exceptionName, message, exception.FunctionName, exception.LineNumber, exception.SourcePath, stack);
            response.Content = Encoding.UTF8.GetBytes(resultMessage);

            response.ExtendedProperties.Add("Exception_Message", message);
            response.ExtendedProperties.Add("Exception_Name", exceptionName);
            response.ExtendedProperties.Add("Exception_FunctionName", exception.FunctionName);
            response.ExtendedProperties.Add("Exception_LineNumber", exception.LineNumber.ToString(CultureInfo.InvariantCulture));
            response.ExtendedProperties.Add("Exception_SourcePath", exception.SourcePath);
            response.ExtendedProperties.Add("Exception_StackTrace", rawStack);
        }

        public virtual void UpdateResponseWithExceptionDetails(Exception exception, BrewResponse response)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");

            if (response == null)
                throw new ArgumentNullException("response");

            if (exception is AggregateException)
            {
                var exceptions = (exception as AggregateException).InnerExceptions;

                //TODO: Make this better.
                exception = exceptions.First();
            }

            var message = exception.Message;
            var stack = exception.StackTrace;
            if (String.IsNullOrEmpty(stack) == false)
                stack = stack.Replace("at ", "at<br/>");

            response.StatusCode = HttpStatusCode.BadRequest;
            response.StatusDescription = message;
            response.AutoDetectContentType = false;
            response.ContentType = "text/html";

            var resultMessage = String.Format(ExceptionMessage, message, stack);
            response.Content = Encoding.UTF8.GetBytes(resultMessage);
        }

        public virtual void UpdateResponseWithTimeoutDetails(int executionTimeout, BrewResponse response)
        {
            response.StatusCode = HttpStatusCode.BadRequest;
            response.StatusDescription =
              "The script being executed exceeded the maximum allowable timeout as set by an Administrator. ({0}ms).";
            response.AutoDetectContentType = false;
            response.ContentType = "text/html";

            var resultMessage = String.Format(JavaScriptTimeoutMessage, executionTimeout);
            response.Content = Encoding.UTF8.GetBytes(resultMessage);
        }
    }
}
