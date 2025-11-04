using System;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Dpz.Core.Web.Dashboard.Helper
{
    [Obsolete("used OIDC")]
    public class AppRouteView:RouteView
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IAuthenticationService AuthenticationService { get; set; }

        protected override void Render(RenderTreeBuilder builder)
        {
            var authorize = Attribute.GetCustomAttribute(RouteData.PageType, typeof(AuthorizeAttribute)) != null;
            if (authorize && AuthenticationService.User == null)
            {
                NavigationManager.NavigateTo("/login");
                return;
            }
            base.Render(builder);
        }

    }
}