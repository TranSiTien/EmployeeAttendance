using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EmployeeAttendance.Auth
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RoleAuthorizationAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public RoleAuthorizationAttribute(params string[] roles)
        {
            _allowedRoles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Skip authorization if action is decorated with [AllowAnonymous]
            if (context.ActionDescriptor.EndpointMetadata.Any(em => em.GetType().Name == "AllowAnonymousAttribute"))
                return;

            // Check if the user is authenticated
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Check if user has any of the allowed roles
            if (_allowedRoles.Any() && !_allowedRoles.Any(role => 
                context.HttpContext.User.IsInRole(role)))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
} 