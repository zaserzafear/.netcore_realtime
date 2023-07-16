namespace Web
{
    public class StatusCodeMiddleware
    {
        private readonly RequestDelegate _next;

        public StatusCodeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == 401 || context.Response.StatusCode == 403)
            {
                context.Response.Redirect("/");
            }
        }
    }
}
