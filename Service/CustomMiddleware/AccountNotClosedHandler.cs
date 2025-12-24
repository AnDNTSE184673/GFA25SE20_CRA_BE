using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Repository.Constant;
using System.Security.Claims;

namespace Service.CustomMiddleware
{
    public class AccountNotClosedHandler
        : AuthorizationHandler<AccountNotClosedRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AccountNotClosedRequirement requirement)
        {
            var statusClaim = context.User.FindFirst("status")?.Value;

            if (statusClaim == ConstantEnum.Statuses.CLOSED)
            {
                // Explicitly fail
                context.Fail();
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }

}
