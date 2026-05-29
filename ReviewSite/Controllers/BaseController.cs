using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

public class BaseController : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (HttpContext.Session.GetInt32("UserId") != null)
        {
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            ViewData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["IsAdmin"] = HttpContext.Session.GetString("Role") == "Admin";
        }
        base.OnActionExecuting(context);
    }
}
