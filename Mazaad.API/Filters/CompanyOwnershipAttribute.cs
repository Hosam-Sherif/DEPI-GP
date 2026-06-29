using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Mazaad.API.Filters
{
    /// <summary>
    /// يتأكد إن الـ companyId الموجود في الراوت بيطابق claim "companyId" بتاع اليوزر
    /// اللي مسجل دخوله (من الـ JWT). لازم يتطبق مع [Authorize] على نفس الكنترولر/الأكشن.
    ///
    /// لو مش متطابقين → 403 Forbidden (مش 404)، عشان مانأكدش حتى إن الشركة دي موجودة.
    /// لو الـ user مالوش companyId في التوكين (مثلاً Admin مش مرتبط بشركة) → 403 كمان.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CompanyOwnershipAttribute : ActionFilterAttribute
    {
        private const string RouteParamName = "companyId";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // لازم يكون فيه parameter اسمه companyId في الأكشن (من الراوت)
            if (!context.ActionArguments.TryGetValue(RouteParamName, out var routeValue)
                || routeValue is not int routeCompanyId)
            {
                context.Result = new BadRequestObjectResult(new { message = "companyId is required." });
                return;
            }

            var claimValue = context.HttpContext.User.FindFirst("companyId")?.Value;

            if (string.IsNullOrEmpty(claimValue) || !int.TryParse(claimValue, out var userCompanyId))
            {
                // اليوزر متسجل بس مالوش شركة مرتبطة بيه في التوكين
                context.Result = new ForbidResult();
                return;
            }

            if (userCompanyId != routeCompanyId)
            {
                // اليوزر بيحاول يشوف بيانات شركة تانية غير شركته (IDOR attempt)
                context.Result = new ForbidResult();
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}