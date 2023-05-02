using ASP_201.Data;
using ASP_201.Data.Entity;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace ASP_201.Middleware
{
    public class SessionAuthMiddleware
    {
        /*  Для утворення ланцюга кожна ланка (об'єкт Middleware) отримує
         *  посилання на наступну ланку. Це посилання передається через конструктор
         *  і має бути збережене у об'єкті
         */
        private readonly RequestDelegate _next;

        /*  Middleware існує протягом всієї роботи програми (Singleton), тому
         *  інжекція сервісів через конструктор не здійснюється
         * 
         */
        public SessionAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /*   Обов'язковий для Middleware метод InvokeAsync або Invoke (старий підхід)
         *   Перший пераметр - завжди HttpContext context,
         *   за потреби, наступні параметри - інжекційні
         */
        public async Task InvokeAsync(HttpContext context,
            ILogger<SessionAuthMiddleware> logger,
            DataContext dataContext)
        {
            //logger.LogInformation("SessionMiddleware works");
            // Перевіряємо наявність у сесії "authUserId" (встановлюється при
            //   автентифікації у UserController::AuthUser() )
            String? userId = context.Session.GetString("authUserId");
            if(userId is not null)
            {
                try
                {
                    User? authUser=
                        dataContext.Users.Find(Guid.Parse(userId));
                    if(authUser is not null)
                    {
                        context.Items.Add("authUser", authUser);
                        /*  Передача відомостей про користувача шляхом посилання
                         *  на об`єкт-сутність (Entity) підвищує зчеплення
                         *  (залежність від реалізації), а також поширює відомості
                         *  про "технічну" сутність User, потрібну ORM, на
                         *  увесь проєкт, де потрібна авторизація.
                         *  Для уніфікації існє механізм "тверджень" (Claims).
                         *  При автентифікації користувачу задаються певні
                         *  Claims, а при авторизації перевірється наявність
                         *  потрібних з них (наприклад, вік, стать, телефон).
                         */
                        Claim[] claims = new Claim[]
                        {
                            new Claim(ClaimTypes.Sid ,userId),
                            new Claim(ClaimTypes.Name ,authUser.RealName),
                            new Claim(ClaimTypes.NameIdentifier ,authUser.Login),
                            new Claim(ClaimTypes.UserData ,authUser.Avatar??String.Empty),
                            new Claim(ClaimTypes.Email,authUser.EmailCode??String.Empty)
                        };
                        /*Створюємо власника (Principal) із даними твердженнями*/
                        var principal = new ClaimsPrincipal(
                            new ClaimsIdentity(
                                claims,
                                nameof(SessionAuthMiddleware)));

                        /*  У HttpContext є вбудоване поле User з типом ClaimsPrincipal
                         *  Встановлення його дозволить задіяти ASP механізми авторизації*/
                        context.User = principal;
                    }
                }
                catch(Exception ex)
                {
                    logger.LogWarning(ex, "SessionMiddleware");
                }
            }
            /*  Для продовження ланцюга метод має викликати наступну
             *  ланку Middleware
             */
            await _next(context);
        }
    }

    /*  Включення класу SessionAuthMiddleware у ланцюг Middleware здійснюється
     *  у Program.cs командою
     *      app.UseMiddleware<SessionAuthMiddleware>();
     *  Традиційно Middleware забезпечують розширеннями (extensions) для
     *  вижвання UseXxxxx формалізму (Xxxxx - назва Middleware)
     *      app.UseSessionAuth()
     */
    public static class SessionAuthMiddlewareExtension
    {
        public static IApplicationBuilder UseSessionAuth(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SessionAuthMiddleware>();
        }
    }
}

/*
 * Middleware - ПЗ проміжного рівня
 */