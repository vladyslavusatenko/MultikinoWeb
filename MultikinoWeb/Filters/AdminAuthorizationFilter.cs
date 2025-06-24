using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MultikinoWeb.Filters
{
    public class AdminAuthorizationFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userRole = context.HttpContext.Session.GetString("UserRole");
            var userId = context.HttpContext.Session.GetInt32("UserId");

            if (userId == null || userRole != "Admin")
            {
                context.Result = new RedirectToPageResult("/Account/Login");
            }
        }
    }

    public class AdminAuthorizationAttribute : TypeFilterAttribute
    {
        public AdminAuthorizationAttribute() : base(typeof(AdminAuthorizationFilter))
        {
        }
    }
}