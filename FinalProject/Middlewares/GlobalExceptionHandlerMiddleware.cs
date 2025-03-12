namespace FinalProject.Middlewares
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception e)
            {
                // Sanitize the exception message
                string sanitizedMessage = SanitizeErrorMessage(e.Message);

                // URL-encode the sanitized message
                string encodedMessage = Uri.EscapeDataString(sanitizedMessage);

                // Redirect to the error page with the sanitized and encoded message
                context.Response.Redirect($"/home/error?errorMessage={encodedMessage}");
            }
        }

        private string SanitizeErrorMessage(string message)
        {
            // Remove newlines and other invalid characters
            return message
                .Replace("\r", "") // Remove carriage return
                .Replace("\n", "") // Remove newline
                .Replace("\t", "") // Remove tab
                .Replace("\"", "") // Remove double quotes
                .Replace("'", ""); // Remove single quotes
        }
    }
}